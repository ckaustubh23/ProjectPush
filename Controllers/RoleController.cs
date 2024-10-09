using Microsoft.AspNetCore.Mvc;
using VendorBilling.Application.Common.DTO.Role;
using VendorBilling.Application.Common.Interfaces;
using VendorBilling.Infrastructure.Repository;

namespace VendorBilling.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController(IUnitOfWork unitofWork) : ControllerBase
    {
        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await unitofWork.RoleRepository.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpPost("AddRoleWithPermissions")]
        public async Task<IActionResult> AddRoleWithPermissions([FromBody] RoleDTO role)
        {
            if (string.IsNullOrEmpty(role.RoleName))
            {
                return BadRequest("Role name is required.");
            }

            var roleId = await unitofWork.RoleRepository.AddRoleWithPermissionAsync(role.RoleName, role.PermissionIds);
            return CreatedAtAction(nameof(GetAllRoles), new { roleId }, role);
        }
        [HttpPut("UpdateRolePermissions/{roleId}")]
        public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] RoleDTO role)
        {
            await unitofWork.RoleRepository.UpdateRolePermissionsAsync(roleId, role.PermissionIds);
            return NoContent();
        }

        [HttpDelete("DeleteRole/{roleId}")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            await unitofWork.RoleRepository.DeleteRoleAsync(roleId);
            return NoContent();
        }
    }
}
