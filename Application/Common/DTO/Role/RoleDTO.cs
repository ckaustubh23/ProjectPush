namespace VendorBilling.Application.Common.DTO.Role
{
    public class RoleDTO
    {
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public List<int>? PermissionIds { get; set; }
    }
}
