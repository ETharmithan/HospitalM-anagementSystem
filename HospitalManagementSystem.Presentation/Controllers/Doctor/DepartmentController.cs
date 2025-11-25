using HospitalManagementSystem.Application.DTOs.Doctor.Request_Dto;
using HospitalManagementSystem.Application.IServices.Doctor_IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers.Doctor
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET: api/departments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _departmentService.GetAllAsync();
            return Ok(departments);
        }

        // GET: api/departments/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
                return NotFound(new { message = "Department not found" });

            return Ok(department);
        }

        // POST: api/departments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DepartmentRequestDto departmentRequestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _departmentService.CreateAsync(departmentRequestDto);

            // Return 201 Created with location header
            return CreatedAtAction(nameof(GetById), new { id = created.DepartmentId }, created);
        }

        // PUT: api/departments/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DepartmentRequestDto departmentRequestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _departmentService.UpdateAsync(id, departmentRequestDto);
            if (!success)
                return NotFound(new { message = "Department not found" });

            return NoContent();
        }

        // DELETE: api/departments/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _departmentService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Department not found" });

            return NoContent();
        }
    }
}
