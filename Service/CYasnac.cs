﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace MOTOCOM
{
    public class CYasnac
    {
        private short m_Handle = -1;
        private string m_IPAddress = "";
        private System.Timers.Timer StatusTimer = new System.Timers.Timer();
        private short m_oldStatusD1 = -1, m_oldStatusD2 = -1;
        private static Object m_FileAccessDirLock = new Object();
        private Object m_YasnacAccessLock = new Object();
        private static string m_CommDir;
        private string m_Path;
        private bool m_IsError;
        private bool m_IsCommandRemote;
        private bool m_IsStep;
        private bool m_Is1Cycle;
        private bool m_IsAuto;
        private bool m_IsOperating;
        private bool m_IsSafeSpeed;
        private bool m_IsTeach;
        private bool m_IsPlay;
        private bool m_IsPlaybackBoxHold;
        private bool m_IsPPHold;
        private bool m_IsExternalHold;
        private bool m_IsCommandHold;
        private bool m_IsAlarm;
        private bool m_IsServoOn;
        private bool m_AutoStatusUpdate = false;

        public event EventHandler StatusChanged;


        public CYasnac(string IPAddress, string Path)
        {
            m_IPAddress = IPAddress;
            m_Path = Path;

            Console.WriteLine("IP Address: " + m_IPAddress);
            //** Initialize communication **
            short ret;
            Console.WriteLine("CommDir: " + m_CommDir);
            //try to get a handle
            m_Handle = CMotocom.BscOpen(m_CommDir, 256);
            Console.WriteLine("Handle: " + m_Handle);

            if (m_Handle >= 0)
            {
                //set IP Address
                ret = CMotocom.BscSetEServer(m_Handle, m_IPAddress);
                if (ret != 1)
                    throw new Exception("Could not set IP address !");
            }
            else
                throw new Exception("Could not get a handle. Check Hardware key !");
        }

        static CYasnac()
        {
            m_CommDir = Directory.GetCurrentDirectory();
            if (m_CommDir.Substring(m_CommDir.Length - 1, 1) != "\\")
            {
                m_CommDir = m_CommDir + "\\";
            }
        }

        ~CYasnac()
        {
            short ret;

            //close if there is a valid handle
            if (m_Handle >= 0)
                ret = CMotocom.BscClose(m_Handle);
        }

        /**
        Reads status of the controller
        param name=d1
        param name=d2 
        */
        public void UpdateStatus(ref short d1, ref short d2)
        {
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscGetStatus(m_Handle, ref d1, ref d2);

                if (ret != 0)
                    throw new Exception("Error getting status !");
                else
                {
                    //check bits and set properties
                    if (d1 != m_oldStatusD1 || d2 != m_oldStatusD2)
                    {
                        m_IsStep = (d1 & (1 << 0)) > 0 ? true : false;
                        m_Is1Cycle = (d1 & (1 << 1)) > 0 ? true : false;
                        m_IsAuto = (d1 & (1 << 2)) > 0 ? true : false;
                        m_IsOperating = (d1 & (1 << 3)) > 0 ? true : false;
                        m_IsSafeSpeed = (d1 & (1 << 4)) > 0 ? true : false;
                        m_IsTeach = (d1 & (1 << 5)) > 0 ? true : false;
                        m_IsPlay = (d1 & (1 << 6)) > 0 ? true : false;
                        m_IsCommandRemote = (d1 & (1 << 7)) > 0 ? true : false;

                        m_IsPlaybackBoxHold = (d2 & (1 << 0)) > 0 ? true : false;
                        m_IsPPHold = (d2 & (1 << 1)) > 0 ? true : false;
                        m_IsExternalHold = (d2 & (1 << 2)) > 0 ? true : false;
                        m_IsCommandHold = (d2 & (1 << 3)) > 0 ? true : false;
                        m_IsAlarm = (d2 & (1 << 4)) > 0 ? true : false;
                        m_IsError = (d2 & (1 << 5)) > 0 ? true : false;
                        m_IsServoOn = (d2 & (1 << 6)) > 0 ? true : false;

                        m_oldStatusD1 = d1;
                        m_oldStatusD2 = d2;

                        //Raise StatusChanged event to notify clients
                        if (StatusChanged != null)
                            StatusChanged(this, null);
                    }
                }
            }
        }

        public void UpdateStatus()
        {
            short d1 = 0, d2 = 0;
            UpdateStatus(ref d1, ref d2);
        }

        /**
        des: Retrieves joblist from controller
        param name=d1
        param name=d2 
        return: Number of jobs in joblist
        */
        public int GetJobList(ArrayList JobList)
        {
            short ret;
            StringBuilder jobname = new StringBuilder(CMotocom.MaxJobNameLength + 1);

            lock (m_YasnacAccessLock)
            {

                JobList.Clear();

                ret = CMotocom.BscFindFirst(m_Handle, jobname, CMotocom.MaxJobNameLength + 1);
                if (ret < -1)
                    throw new Exception("Error reading job list !");
                if (ret == 0)
                {
                    JobList.Add(jobname.ToString());
                    do
                    {
                        ret = CMotocom.BscFindNext(m_Handle, jobname, CMotocom.MaxJobNameLength + 1);
                        if (ret < -1)
                            throw new Exception("Error reading job list !");
                        if (ret == 0)
                            JobList.Add(jobname.ToString());
                    }
                    while (ret == 0);
                }
            }
            return JobList.Count;
        }

        /**
        des: Deletes a job on the controller
        param name=JobName :Name of job to delete
        */
        public void DeleteJob(string JobName)
        {
            short ret;

            if (!JobName.ToLower().Contains(".jbi"))
                throw new Exception("Error *.jbi jobname extension is missing !");

            lock (m_YasnacAccessLock)
            {

                ret = CMotocom.BscSelectJob(m_Handle, JobName);
                if (ret == 0)
                {
                    ret = CMotocom.BscDeleteJob(m_Handle);
                    if (ret != 0)
                        throw new Exception("Error deleting job !");
                }
                else
                    throw new Exception("Error selecting job !");
            }
        }

        /**
        des: Retrieves alarm and error status of the controller
        param name=error
        param name=alarmlist 
        return: Number of active alarms
        */      
        public int GetAlarm(CErrorData error, ArrayList alarmlist)
        {
            short ret;
            short errorno = -1;
            StringBuilder errormsg = new StringBuilder(256);
            short alarmsubno = -1;
            StringBuilder alarmmsg = new StringBuilder(256);

            lock (m_YasnacAccessLock)
            {
                alarmlist.Clear();

                ret = CMotocom.BscReadAlarmS(m_Handle, ref errorno, errormsg);
                if (ret != 0)
                    throw new Exception("Error reading error/alarm information !");

                error.ErrorNo = errorno;
                error.ErrorMsg = errormsg.ToString();

                ret = CMotocom.BscGetFirstAlarmS(m_Handle, ref alarmsubno, alarmmsg);
                if (ret < 0)
                    throw new Exception("Error reading alarms !");

                if (ret > 0)
                {

                    alarmlist.Add(new CAlarmHistoryItem(ret, alarmsubno, alarmmsg.ToString()));

                    do
                    {
                        ret = CMotocom.BscGetNextAlarmS(m_Handle, ref alarmsubno, alarmmsg);
                        if (ret < 0)
                            throw new Exception("Error reading alarms !");
                        if (ret > 0)
                            alarmlist.Add(new CAlarmHistoryItem(ret, alarmsubno, alarmmsg.ToString()));
                    }
                    while (ret > 0);
                }
            }
            return alarmlist.Count;
        }


        /**
        Reset alarm
        */
        public void ResetAlarm()
        {
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscReset(m_Handle);
                if (ret != 0)
                    throw new Exception("Error executing BscReset");
            }
        }


        /**
        Set teach mode
        */
        public void SetTeachMode()
        {
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscSelectMode(m_Handle, 1);
                if (ret != 0)
                    throw new Exception("Error executing BscSelectMode");
            }
        }


        /**
        Set Play mode
        */
        public void SetPlayMode()
        {
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscSelectMode(m_Handle, 2);
                if (ret != 0)
                    throw new Exception("Error executing BscSelectMode");
            }
        }

        /**
        Set Serve on
        */
        public void SetServoOn()
        {
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscServoOn(m_Handle);
                if (ret != 0)
                    throw new Exception("Error executing BscServoON");
            }
        }

        /**
        Set Serve off
        */
        public void SetServoOff()
        {
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscServoOff(m_Handle);
                if (ret != 0)
                    throw new Exception("Error executing BscServoOff");
            }
        }

        /**
        Set hold on
        */
        public void SetHoldOn()
        {
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscHoldOn(m_Handle);
                if (ret != 0)
                    throw new Exception("Error executing BscHoldOn");
            }
        }

        /**
        Set Hold off
        */
        public void SetHoldOff()
        {
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscHoldOff(m_Handle);
                if (ret != 0)
                    throw new Exception("Error executing BscHoldOff");
            }
        }


        /**
        Starts operation from the current line of current job
        */
        public void Start()
        {
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscContinueJob(m_Handle);
                if (ret != 0)
                    throw new Exception("Error executing BscContinueJob");
            }
        }


        /**
        dest: Reads multiple variables of simple data type
        param name=SimpleVar
        */
        public void ReadSimpleTypeVariable(CSimpleTypeVarList SimpleVar)
        {
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscHostGetVarDataM(m_Handle, (short)SimpleVar.VarType, SimpleVar.StartIndex, SimpleVar.ListSize, ref SimpleVar.VarListArray[0]);
                if (ret != 0)
                    throw new Exception("Error executing BscHostGetVarDataM");
            }
        }

     /**
        des: Reads position variable
        param name="Index">Index of variable to read
        param name="PosVar">Object receiving results
        */

        /**
        public void ReadPositionVariable(short Index, CRobPosVar PosVar)
        {
            lock (m_YasnacAccessLock)
            {
                StringBuilder StringVal = new StringBuilder(256);
                double[] PosVarArray = new double[12];
                short ret = CMotocom.BscHostGetVarData(m_Handle, 4, Index, ref PosVarArray[0], StringVal);
                if (ret != 0)
                    throw new Exception("Error executing BscHostGetVarData");
                PosVar.HostGetVarDataArray = PosVarArray;
            }
        }
        */

        /**
        des: Write position variable
        param name="Index">Index of variable to read
        param name="PosVar">Object contain value to write
        */
        /**
        public void WritePositionVariable(short Index, CRobPosVar PosVar)
        {
            lock (m_YasnacAccessLock)
            {
                StringBuilder StringVal = new StringBuilder(256);
                double[] PosVarArray = PosVar.HostGetVarDataArray;
                short ret = CMotocom.BscHostPutVarData(m_Handle, 4, Index, ref PosVarArray[0], StringVal);
                if (ret != 0)
                    throw new Exception("Error executing BscHostPutVarData");
            }
        }
        */

        /**
        dest: Writes multiple simple type variables
        param: name=SimpleVar
        */
        public void WriteSimpleTypeVariable(CSimpleTypeVarList SimpleVar)
        {
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscHostPutVarDataM(m_Handle, (short)SimpleVar.VarType, SimpleVar.StartIndex, SimpleVar.ListSize, ref SimpleVar.VarListArray[0]);
                if (ret != 0)
                    throw new Exception("Error executing BscHostPutVarDataM");
            }
        }

        /**
        dest: Download a file from controller
        param name=Filetitle: Name of the file
        param name=Path: Folder to store the file
        */
        public void ReadFile(string Filetitle, string Path)
        {
            StringBuilder _Filetitle = new StringBuilder(Filetitle, 255);
            short ret;
            if (Path.Substring(Path.Length - 1, 1) != "\\")
                Path = Path + "\\";
            lock (m_FileAccessDirLock)
            {
                lock (m_YasnacAccessLock)
                {
                    ret = CMotocom.BscUpLoad(m_Handle, _Filetitle);
                }
                if (ret != 0)
                    throw new Exception("Error executing BscUpLoadEx");
                else
                    File.Copy(m_CommDir + Filetitle, Path + Filetitle, true);
            }
        }


        /**
        dest Reads file and stores it to default folder
        param name="Filetitle"
        */
        public void ReadFile(string Filetitle)
        {
            ReadFile(Filetitle, m_Path);
        }


        /*
        des Writes one single IO
        /// <param name="Address">Address of IO</param>
        /// <param name="value">Value of IO</param>
        */
        public void WriteSingleIO(int Address, bool value)
        {
            int BaseAddress = Address / 10 * 10;
            short iovalue;
            if (value)
                iovalue = (short)(1 << (Address - BaseAddress));
            else
                iovalue = 0;
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscWriteIO2(m_Handle, Address, 1, ref iovalue);
                if (ret != 0)
                    throw new Exception("Error executing BscWriteIO2");
            }
        }

        /**
        des Reads multiple IO groups
        param name="StartAddress">Address  of first group
        param name="NumberOfGroups">Number of groups to read
        return: Array of binary codes representing each group
        */
        public short[] ReadIOGroups(int StartAddress, short NumberOfGroups)
        {
            int BaseAddress = (int)StartAddress / 10 * 10;

            if (StartAddress != BaseAddress)
                throw new Exception("Start address has to be first address of a group");
            if (NumberOfGroups > 32)
                throw new Exception("Maximum group number to read is 32");

            short[] ioValues = new short[32];
            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscReadIO2(m_Handle, StartAddress, (short)(NumberOfGroups * 8), ref ioValues[0]);
                if (ret != 0)
                    throw new Exception("Error executing BscReadIO2");
            }
            return ioValues;
        }

        /// <summary>
        /// Writes multiple IO groups
        /// </summary>
        /// <param name="StartAddress">Address  of first group</param>
        /// <param name="NumberOfGroups">Number of groups to write</param>
        /// <param name="IOGroupValues">Values of each group</param>
        public void WriteIOGroups(int StartAddress, short NumberOfGroups, short[] IOGroupValues)
        {
            int BaseAddress = (int)StartAddress / 10 * 10;

            if (StartAddress != BaseAddress)
                throw new Exception("Start address has to be first address of a group");
            if (NumberOfGroups > 32)
                throw new Exception("Maximum group number to write is 32");

            lock (m_YasnacAccessLock)
            {
                short ret = CMotocom.BscWriteIO2(m_Handle, StartAddress, (short)(NumberOfGroups * 8), ref IOGroupValues[0]);
                if (ret != 0)
                    throw new Exception("Error executing BscWriteIO2");
            }
        }

        /// <summary>
        /// Reads one single IO
        /// </summary>
        /// <param name="Address">Address of IO to read</param>
        /// <returns>IO status</returns>
        public bool ReadSingleIO(int Address)
        {
            short ret;
            short IOVal = 0;

            lock (m_YasnacAccessLock)
            {
                ret = CMotocom.BscReadIO2(m_Handle, Address, 1, ref IOVal);
                if (ret != 0)
                    throw new Exception("Error reading IO !");
            }
            return (IOVal > 0 ? true : false);
        }

        /// <summary>
        /// Uploads file to the controller
        /// </summary>
        /// <param name="Filename">Filename including path</param>
        public void WriteFile(string Filename)
        {
            StringBuilder _Filetitle = new StringBuilder(Path.GetFileName(Filename), 255);
            short ret;
            lock (m_FileAccessDirLock)
            {
                if (Filename != m_CommDir + _Filetitle)
                    File.Copy(Filename, m_CommDir + _Filetitle, true);

                lock (m_YasnacAccessLock)
                {
                    ret = CMotocom.BscDownLoad(m_Handle, _Filetitle);
                }
                if (ret != 0)
                    throw new Exception("Error executing BscDownLoad");
            }
        }

        /// <summary>
        /// Calls and executes specified jon
        /// </summary>
        /// <param name="JobName">Jobname to execute</param>
        public void StartJob(string JobName)
        {
            short ret;

            if (!JobName.ToLower().Contains(".jbi"))
                throw new Exception("Error *.jbi jobname extension is missing !");

            lock (m_YasnacAccessLock)
            {

                ret = CMotocom.BscSelectJob(m_Handle, JobName);
                if (ret == 0)
                {
                    ret = CMotocom.BscStartJob(m_Handle);
                    if (ret != 0)
                        throw new Exception("Error starting job !");
                }
                else
                    throw new Exception("Error selecting job !");
            }
        }

        /// <summary>
        /// Returns executing job of task 0
        /// </summary>
        /// <returns>Jobname</returns>
        public string GetCurrentJob()
        {
            return GetCurrentJob(0);
        }

        /// <summary>
        /// Returns executing job of specified task
        /// </summary>
        /// <param name="TaskID">Task ID</param>
        /// <returns>Jobname</returns>
        public string GetCurrentJob(short TaskID)
        {
            short ret;
            StringBuilder jobname = new StringBuilder(255);

            lock (m_YasnacAccessLock)
            {
                if (TaskID > 0)
                {
                    ret = CMotocom.BscChangeTask(m_Handle, TaskID);
                    if (ret != 0)
                    {
                        throw new Exception("Error changing task !");
                    }
                }
                ret = CMotocom.BscIsJobName(m_Handle, jobname, 255);
                if (ret != 0)
                    throw new Exception("Error getting current job name !");
            }
            return jobname.ToString();
        }

        /// <summary>
        /// Sets executing job and cursor
        /// </summary>
        /// <param name="TaskID">Task ID</param>
        /// <param name="JobName">Jobname</param>
        /// <param name="linenumber">Line number</param>
        public void SetCurrentJob(short TaskID, string JobName, short linenumber)
        {
            short ret;

            if (!JobName.ToLower().Contains(".jbi"))
                throw new Exception("Error *.jbi jobname extension is missing !");

            lock (m_YasnacAccessLock)
            {
                if (TaskID > 0)
                {
                    ret = CMotocom.BscChangeTask(m_Handle, TaskID);
                    if (ret != 0)
                    {
                        throw new Exception("Error changing task !");
                    }
                }
                ret = CMotocom.BscSelectJob(m_Handle, JobName);
                if (ret == 0)
                {
                    ret = CMotocom.BscSetLineNumber(m_Handle, linenumber);
                    if (ret != 0)
                        throw new Exception("Error setting current job line !");
                }
            }
        }

        /// <summary>
        /// Returns current job line of specified task
        /// </summary>
        /// <param name="TaskID">Task ID</param>
        /// <returns>Job line number</returns>
        public short GetCurrentLine(short TaskID)
        {
            short ret;

            lock (m_YasnacAccessLock)
            {
                if (TaskID > 0)
                {
                    ret = CMotocom.BscChangeTask(m_Handle, TaskID);
                    if (ret != 0)
                    {
                        throw new Exception("Error changing task !");
                    }
                }
                ret = CMotocom.BscIsJobLine(m_Handle);
                if (ret == -1)
                    throw new Exception("Error getting current job line !");
            }
            return ret;
        }

        void StatusTimer_Tick(object sender, EventArgs e)
        {
            short d1 = 0, d2 = 0;
            try
            {
                UpdateStatus(ref d1, ref d2);
            }
            catch (Exception)
            {

            }
        }

        public static string CommDir
        {
            get { return CYasnac.m_CommDir; }
            set { CYasnac.m_CommDir = value; }
        }

        public bool IsStep
        {
            get { return m_IsStep; }
        }

        public bool Is1Cycle
        {
            get { return m_Is1Cycle; }
        }

        public bool IsAuto
        {
            get { return m_IsAuto; }
        }

        public bool IsOperating
        {
            get { return m_IsOperating; }
        }

        public bool IsSafeSpeed
        {
            get { return m_IsSafeSpeed; }
        }

        public bool IsTeach
        {
            get { return m_IsTeach; }
        }

        public bool IsPlay
        {
            get { return m_IsPlay; }
        }

        public bool IsCommandRemote
        {
            get { return m_IsCommandRemote; }
        }

        public bool IsPlaybackBoxHold
        {
            get { return m_IsPlaybackBoxHold; }
        }

        public bool IsPPHold
        {
            get { return m_IsPPHold; }
        }

        public bool IsExternalHold
        {
            get { return m_IsExternalHold; }
        }

        public bool IsCommandHold
        {
            get { return m_IsCommandHold; }
        }

        public bool IsAlarm
        {
            get { return m_IsAlarm; }
        }

        public bool IsError
        {
            get { return m_IsError; }
        }

        public bool IsServoOn
        {
            get { return m_IsServoOn; }
        }

        public bool IsHold
        {
            get
            {
                if (m_IsPlaybackBoxHold || m_IsPPHold || m_IsExternalHold || m_IsCommandHold)
                    return true;
                else
                    return false;
            }
        }

    public bool AutoStatusUpdate
    {
        get
        {
            return m_AutoStatusUpdate;
        }
        set
        {
            m_AutoStatusUpdate = value;
            if (m_AutoStatusUpdate)
            {
                // Initialize status timer
                StatusTimer.Interval = 500; // Interval in milliseconds
                StatusTimer.Elapsed += StatusTimer_Tick; // Attach event handler
                StatusTimer.AutoReset = true; // Ensure the timer repeats
                StatusTimer.Start(); // Start timer
            }
            else
            {
                StatusTimer.Stop(); // Stop timer
                StatusTimer.Elapsed -= StatusTimer_Tick; // Detach event handler
            }
        }
    }

// Timer event handler
    private void StatusTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
    {
        short d1 = 0, d2 = 0;
        try
        {
            UpdateStatus(ref d1, ref d2); // Update the robot's status
        }
        catch (Exception ex)
        {
            // Handle exception if necessary (e.g., log it)
        }
    }

}

}