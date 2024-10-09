using Dapper;
using IdentityModel.Client;
using Microsoft.Data.SqlClient;
using System.Data;
using VendorBilling.Application.Common.DTO.Role;
using VendorBilling.Application.Common.Interfaces;

namespace VendorBilling.Infrastructure.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly string _connectionString;

        public RoleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<RoleDTO>> GetAllRolesAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("flag", "GetAllRoles");

                var roles = await db.QueryAsync<RoleDTO>("stp_RoleManagement", parameters, commandType: CommandType.StoredProcedure);
                return roles;
            }
        }

        public async Task<int> AddRoleWithPermissionAsync(string roleName, List<int> permissionIds)
        {
            using(IDbConnection  db = new SqlConnection(_connectionString))
            {
                var permissionIdsString = string.Join(",", permissionIds);
                var parameters = new DynamicParameters();
                parameters.Add("flag", "AddRoleWithPermissions");
                parameters.Add("RoleName", roleName);
                parameters.Add("PermissionIds", permissionIdsString);

                var result = await db.ExecuteScalarAsync<int>("stp_RoleManagement", parameters, commandType: CommandType.StoredProcedure);
                return result;
            }
        }

        public async Task UpdateRolePermissionsAsync(int roleId, List<int> permissionIds)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var permissionIdsString = string.Join(",", permissionIds);
                var parameters = new DynamicParameters();
                parameters.Add("flag", "UpdateRolePermissions");
                parameters.Add("RoleId", roleId);
                parameters.Add("PermissionIds", permissionIdsString);

                await db.ExecuteAsync("stp_RoleManagement", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task DeleteRoleAsync(int roleId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {   
                var parameters = new DynamicParameters();
                parameters.Add("flag", "DeleteRole");
                parameters.Add("RoleId", roleId);

                await db.ExecuteAsync("stp_RoleManagement", parameters, commandType: CommandType.StoredProcedure);
            }
        }
    }   
}
