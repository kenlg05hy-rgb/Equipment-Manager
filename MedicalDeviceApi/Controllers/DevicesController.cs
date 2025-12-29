using Microsoft.AspNetCore.Mvc;
using MedicalDeviceApi.Models;
using MedicalDeviceApi.Interfaces; // using Interface
using MedicalDeviceApi.Repositories;

namespace MedicalDeviceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceRepository _repository; // Chỉ giao tiếp với Interface

        // Constructor Injection
        public DevicesController(IDeviceRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult GetDevices()
        {
            var data = _repository.GetAllDevices();
            return Ok(data);
        }

        [HttpPost]
        public IActionResult CreateDevice([FromBody] CreateDeviceDto deviceDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                _repository.CreateDevice(deviceDto);
                return StatusCode(201, "Tạo thiết bị thành công");
            }
            catch (System.Data.DuplicateNameException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateDevice(int id, [FromBody] UpdateDeviceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                _repository.UpdateDevice(id, dto);
                return Ok(new { Message = "Cập nhật thiết bị thành công" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message, DeviceID = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message, Error = ex.ToString() });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult SoftDeleteDevice(int id)
        {
            try
            {
                bool result = _repository.SoftDeleteDevice(id);
                if (result)
                {
                    return Ok(new { Message = "Xóa thiết bị thành công" });
                }
                else
                {
                    return NotFound(new { Message = $"Device with ID {id} not found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message, Error = ex.ToString() });
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult GetDeviceById(int id)
        {
            try
            {
                var device = _repository.GetDeviceById(id);
                if (device == null)
                {
                    return NotFound(new { Message = $"Device with ID {id} not found." });
                }
                return Ok(device);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message, Error = ex.ToString() });
            }
        }

        [HttpGet("search")]
        public IActionResult SearchDevices([FromQuery] string? status, [FromQuery] string? keyword)
        {
            try
            {
                var devices = _repository.SearchDevices(status, keyword);
                return Ok(devices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message, Error = ex.ToString() });
            }
        }

        [HttpGet("{id}/maintenance")]
        public IActionResult GetMaintenance(int id)
        {
            try
            {
                var history = _repository.GetMaintenanceHistory(id);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi Server: " + ex.Message);
            }
        }

        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            try
            {
                var stats = _repository.GetDashboardStats();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi: " + ex.Message);
            }
        }
    }
}