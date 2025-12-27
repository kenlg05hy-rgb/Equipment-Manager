using Microsoft.AspNetCore.Mvc;
using MedicalDeviceApi.Models;
using MedicalDeviceApi.Interfaces; // Nhớ using Interface

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
                return NotFound(new { Message = ex.Message, DeviceId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message, Error = ex.ToString() });
            }
        }
    }
}