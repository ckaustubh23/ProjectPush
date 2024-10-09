using VendorBilling.Application.Common.DTO.Role;

namespace VendorBilling.Application.Common.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<RoleDTO>> GetAllRolesAsync();
        Task<int> AddRoleWithPermissionAsync(string roleName, List<int> permissionIds);
        Task UpdateRolePermissionsAsync(int roleId, List<int> permissionIds);
        Task DeleteRoleAsync(int roleId);
    }
}
