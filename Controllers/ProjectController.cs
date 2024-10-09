using Microsoft.AspNetCore.Mvc;
using VendorBilling.Application.Common.DTO.Project;
using VendorBilling.Application.Common.Interfaces;
using VendorBilling.Infrastructure.Repository;

namespace VendorBilling.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController(IUnitOfWork unitOfWork) : ControllerBase
    {
        [HttpPost("Add")]
        public async Task<IActionResult> AddProject([FromForm] createProjectDTO projectDTO)
        {

            try
            {
                var projectCode = await unitOfWork.ProjectRepository.AddProjectAsync(projectDTO);
                return Ok(new { Message = "Project added successfully", ProjectCode = projectCode });
            }
            catch (ArgumentException ex)
            {
                // Return bad request for validation errors
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex });
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllProjects()
        {
            try
            {
                var projects = await unitOfWork.ProjectRepository.GetAllProjectsAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching projects.", Error = ex.Message });
            }
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            try
            {
                var project = await unitOfWork.ProjectRepository.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound("Project Not Found");
                }
                return Ok(project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the project.", Error = ex.Message });
            }
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateProject([FromForm] createProjectDTO projectDTO)
        {
            try
            {
                await unitOfWork.ProjectRepository.UpdateProjectAsync(projectDTO);
                return Ok(new { Message = "Project updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the project.", Error = ex.Message });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                await unitOfWork.ProjectRepository.DeleteProjectAsync(id);
                return Ok(new { Message = "Project deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the project.", Error = ex.Message });
            }
        }
    }
}
