using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System.Collections;
using MOTOCOM;
using MyRobotAPI.Models;

namespace MyRobotAPI.Services
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigins")] // Áp dụng chính sách CORS
    public class RobotController : ControllerBase
    {
        private readonly RobotService _robotService;

        public RobotController()
        {
            _robotService = new RobotService();
        }

        // Kết nối đến robot
        [HttpPost("connect")]
        public IActionResult Connect([FromBody] string ipAddress)
        {
            try
            {
                _robotService.Connect(ipAddress, Environment.CurrentDirectory);
                return Ok($"Connected to robot at {ipAddress}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Lấy danh sách công việc
        [HttpGet("jobs")]
        public IActionResult GetJobList()
        {
            try
            {
                var jobs = _robotService.GetJobList();
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Xóa một Job
        [HttpDelete("jobs/{jobName}")]
        public IActionResult DeleteJob(string jobName)
        {
            try
            {
                _robotService.DeleteJob(jobName);
                return Ok($"Job {jobName} has been deleted.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Lấy danh sách báo động
        [HttpGet("alarms")]
        public IActionResult GetAlarms()
        {
            try
            {
                var alarms = _robotService.GetAlarms();
                var result = new ArrayList();
                foreach (CAlarmHistoryItem item in alarms)
                {
                    result.Add(new AlarmItem
                    {
                        Code = $"{item.Code}-{item.Subcode}",
                        Message = item.Message
                    });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Bật Servo
        [HttpPost("servo/on")]
        public IActionResult SetServoOn()
        {
            try
            {
                _robotService.SetServoOn();
                return Ok("Servo is turned ON.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Tắt Servo
        [HttpPost("servo/off")]
        public IActionResult SetServoOff()
        {
            try
            {
                _robotService.SetServoOff();
                return Ok("Servo is turned OFF.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Chế độ Play
        [HttpPost("mode/play")]
        public IActionResult SetPlayMode()
        {
            try
            {
                _robotService.SetPlayMode();
                return Ok("Robot is set to Play mode.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Chế độ Teach
        [HttpPost("mode/teach")]
        public IActionResult SetTeachMode()
        {
            try
            {
                _robotService.SetTeachMode();
                return Ok("Robot is set to Teach mode.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Cập nhật trạng thái (ref short d1, ref short d2)
        [HttpGet("status/update-detailed")]
        public IActionResult UpdateStatusDetailed()
        {
            try
            {
                short d1 = 0, d2 = 0;
                _robotService.UpdateStatus(ref d1, ref d2);
                return Ok(new { StatusD1 = d1, StatusD2 = d2 });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Cập nhật trạng thái (ngắn gọn)
        [HttpGet("status/update")]
        public IActionResult UpdateStatus()
        {
            try
            {
                _robotService.UpdateStatus();
                return Ok("Status updated.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Reset Alarm
        [HttpPost("alarm/reset")]
        public IActionResult ResetAlarm()
        {
            try
            {
                _robotService.ResetAlarm();
                return Ok("Alarm has been reset.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Bật Hold
        [HttpPost("hold/on")]
        public IActionResult SetHoldOn()
        {
            try
            {
                _robotService.SetHoldOn();
                return Ok("Hold is turned ON.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Tắt Hold
        [HttpPost("hold/off")]
        public IActionResult SetHoldOff()
        {
            try
            {
                _robotService.SetHoldOff();
                return Ok("Hold is turned OFF.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Start chương trình
        [HttpPost("start")]
        public IActionResult StartProgram()
        {
            try
            {
                _robotService.Start();
                return Ok("Robot program started.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // Đọc biến kiểu đơn giản
        [HttpPost("variables/simple/read")]
        public IActionResult ReadSimpleTypeVariable([FromBody] CSimpleTypeVarList simpleVar)
        {
            try
            {
                var result = _robotService.ReadSimpleTypeVariable(simpleVar);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

/**
        // Đọc biến vị trí
        [HttpGet("variables/position/{index}")]
        public IActionResult ReadPositionVariable(short index)
        {
            try
            {
                var result = _robotService.ReadPositionVariable(index);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        */

/**
        // Ghi biến vị trí
        [HttpPost("variables/position/write")]
        public IActionResult WritePositionVariable([FromBody] WritePositionVariableRequest request)
        {
            try
            {
                _robotService.WritePositionVariable(request.Index, request.PosVar);
                return Ok("Position variable written successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

*/

        // Ghi biến kiểu đơn giản
        [HttpPost("variables/simple/write")]
        public IActionResult WriteSimpleTypeVariable([FromBody] CSimpleTypeVarList simpleVar)
        {
            try
            {
                _robotService.WriteSimpleTypeVariable(simpleVar);
                return Ok("Simple type variable written successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

                // Đọc file với đường dẫn cụ thể
        [HttpGet("file/read/{fileTitle}")]
        public IActionResult ReadFileWithPath(string fileTitle, [FromQuery] string path)
        {
            try
            {
                var filePath = _robotService.ReadFile(fileTitle, path);
                return Ok(new { FilePath = filePath });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Đọc file chỉ với tiêu đề
        [HttpGet("file/read-temp/{fileTitle}")]
        public IActionResult ReadFileToTemp(string fileTitle)
        {
            try
            {
                var filePath = _robotService.ReadFile(fileTitle);
                return Ok(new { TempFilePath = filePath });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Ghi một IO đơn lẻ
        [HttpPost("io/write-single")]
        public IActionResult WriteSingleIO([FromBody] SingleIORequest request)
        {
            try
            {
                _robotService.WriteSingleIO(request.Address, request.Value);
                return Ok("Single IO written successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Đọc nhóm IO
        [HttpGet("io/read-groups")]
        public IActionResult ReadIOGroups([FromQuery] int startAddress, [FromQuery] short numberOfGroups)
        {
            try
            {
                var groups = _robotService.ReadIOGroups(startAddress, numberOfGroups);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Ghi nhóm IO
        [HttpPost("io/write-groups")]
        public IActionResult WriteIOGroups([FromBody] WriteIOGroupsRequest request)
        {
            try
            {
                _robotService.WriteIOGroups(request.StartAddress, request.NumberOfGroups, request.IOGroupValues);
                return Ok("IO groups written successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Đọc một IO đơn lẻ
        [HttpGet("io/read-single/{address}")]
        public IActionResult ReadSingleIO(int address)
        {
            try
            {
                var value = _robotService.ReadSingleIO(address);
                return Ok(new { Address = address, Value = value });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

                // Bắt đầu một job
        [HttpPost("job/start")]
        public IActionResult StartJob([FromBody] string jobName)
        {
            try
            {
                _robotService.StartJob(jobName);
                return Ok($"Job {jobName} started successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Lấy job hiện tại
        [HttpGet("job/current")]
        public IActionResult GetCurrentJob()
        {
            try
            {
                var currentJob = _robotService.GetCurrentJob();
                return Ok(new { CurrentJob = currentJob });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Đặt job hiện tại
        [HttpPost("job/set-current")]
        public IActionResult SetCurrentJob([FromBody] SetCurrentJobRequest request)
        {
            try
            {
                _robotService.SetCurrentJob(request.TaskID, request.JobName, request.LineNumber);
                return Ok($"Job {request.JobName} set as current for TaskID {request.TaskID} at Line {request.LineNumber}.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Lấy dòng hiện tại của task
        [HttpGet("task/{taskId}/current-line")]
        public IActionResult GetCurrentLine(short taskId)
        {
            try
            {
                var currentLine = _robotService.GetCurrentLine(taskId);
                return Ok(new { TaskID = taskId, CurrentLine = currentLine });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
/**
    // Yêu cầu cho ghi biến vị trí
    public class WritePositionVariableRequest
    {
        public short Index { get; set; }
        public CRobPosVar PosVar { get; set; }
    }
    */

    // Yêu cầu ghi IO đơn lẻ
    public class SingleIORequest
    {
        public int Address { get; set; }
        public bool Value { get; set; }
    }

    // Yêu cầu ghi nhóm IO
    public class WriteIOGroupsRequest
    {
        public int StartAddress { get; set; }
        public short NumberOfGroups { get; set; }
        public short[] IOGroupValues { get; set; }
    }

    // Yêu cầu để đặt job hiện tại
    public class SetCurrentJobRequest
    {
        public short TaskID { get; set; }
        public string JobName { get; set; }
        public short LineNumber { get; set; }
    }
}
