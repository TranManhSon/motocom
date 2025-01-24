using System;
using System.Collections;
using MOTOCOM;

namespace MyRobotAPI.Services
{
    public class RobotService
    {
        private CYasnac _yasnac;

        // Kết nối đến robot
        public void Connect(string ipAddress, string startupPath)
        {
            Console.WriteLine("Connecting to robot..." + startupPath);
            _yasnac = new CYasnac(ipAddress, startupPath);
        }

        // Lấy danh sách công việc (Jobs)
        public ArrayList GetJobList()
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");

            var jobList = new ArrayList();
            _yasnac.GetJobList(jobList);
            return jobList;
        }
        
        // Xóa một job
        public void DeleteJob(string jobName)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.DeleteJob(jobName);
        }

        // Lấy danh sách báo động (Alarms)
        public ArrayList GetAlarms()
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");

            var alarms = new ArrayList();
            var errorData = new CErrorData();
            _yasnac.GetAlarm(errorData, alarms);
            return alarms;
        }

        // Bật Servo
        public void SetServoOn()
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.SetServoOn();
        }

        // Tắt Servo
        public void SetServoOff()
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.SetServoOff();
        }

        // Chế độ Play
        public void SetPlayMode()
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.SetPlayMode();
        }

        // Chế độ Teach
        public void SetTeachMode()
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.SetTeachMode();
        }

        // Cập nhật trạng thái (ref short d1, ref short d2)
        public void UpdateStatus(ref short d1, ref short d2)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.UpdateStatus(ref d1, ref d2);
        }

        // Cập nhật trạng thái
        public void UpdateStatus()
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not12 connected.");
            _yasnac.UpdateStatus();
        }

        // Reset Alarm
        public void ResetAlarm()
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.ResetAlarm();
        }

        // Bật Hold
        public void SetHoldOn()
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.SetHoldOn();
        }

        // Tắt Hold
        public void SetHoldOff()
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.SetHoldOff();
        }

        // Start chương trình
        public void Start()
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.Start();
        }

        // Đọc biến kiểu đơn giản
        public CSimpleTypeVarList ReadSimpleTypeVariable(CSimpleTypeVarList simpleVar)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.ReadSimpleTypeVariable(simpleVar);
            return simpleVar;
        }

/**
        // Đọc biến vị trí
        public CRobPosVar ReadPositionVariable(short index)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            var posVar = new CRobPosVar();
            _yasnac.ReadPositionVariable(index, posVar);
            return posVar;
        }


        // Ghi biến vị trí
        public void WritePositionVariable(short index, CRobPosVar posVar)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.WritePositionVariable(index, posVar);
        }
        */

        // Ghi biến kiểu đơn giản
        public void WriteSimpleTypeVariable(CSimpleTypeVarList simpleVar)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.WriteSimpleTypeVariable(simpleVar);
        }

                public string ReadFile(string fileTitle, string path)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");

            string filePath = Path.Combine(path, fileTitle);
            _yasnac.ReadFile(fileTitle, path);
            return filePath;
        }

        // Đọc file từ robot với tiêu đề
        public string ReadFile(string fileTitle)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");

            string tempPath = Path.Combine(Path.GetTempPath(), fileTitle);
            _yasnac.ReadFile(fileTitle, tempPath);
            return tempPath;
        }

        // Ghi một IO đơn lẻ
        public void WriteSingleIO(int address, bool value)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.WriteSingleIO(address, value);
        }

        // Đọc nhóm IO
        public short[] ReadIOGroups(int startAddress, short numberOfGroups)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            return _yasnac.ReadIOGroups(startAddress, numberOfGroups);
        }

        // Ghi nhóm IO
        public void WriteIOGroups(int startAddress, short numberOfGroups, short[] ioGroupValues)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.WriteIOGroups(startAddress, numberOfGroups, ioGroupValues);
        }

        // Đọc một IO đơn lẻ
        public bool ReadSingleIO(int address)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            return _yasnac.ReadSingleIO(address);
        }

                // Bắt đầu một job
        public void StartJob(string jobName)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.StartJob(jobName);
        }

        // Lấy job hiện tại
        public string GetCurrentJob()
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            return _yasnac.GetCurrentJob();
        }

        // Đặt job hiện tại
        public void SetCurrentJob(short taskId, string jobName, short lineNumber)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            _yasnac.SetCurrentJob(taskId, jobName, lineNumber);
        }

        // Lấy dòng hiện tại của task
        public short GetCurrentLine(short taskId)
        {
            if (_yasnac == null) throw new InvalidOperationException("Robot is not connected.");
            return _yasnac.GetCurrentLine(taskId);
        }

    }
}
