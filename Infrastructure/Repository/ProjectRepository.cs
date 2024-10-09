using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using VendorBilling.Application.Common.DTO.Project;
using VendorBilling.Application.Common.Interfaces;

namespace VendorBilling.Infrastructure.Repository
{
    public class ProjectRepository(IConfiguration configuration, IWebHostEnvironment env) : IProjectRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection");
        private readonly IWebHostEnvironment _env = env;
        private const string spName = "stp_ProjectManagement";

        public async Task<int> AddProjectAsync(createProjectDTO project)
        {
            string? filePath = null;

            if (project.excelFile != null && project.excelFile.Length > 0)
            {
                var folderPath = Path.Combine(_env.ContentRootPath, "C://UploadedFiles");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(project.excelFile.FileName)}";
                filePath = Path.Combine(folderPath, uniqueFileName);

                try
                {
                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await project.excelFile.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred while uploading the file.", ex);
                }
            }

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ProjectName", project.ProjectName);
                parameters.Add("@ProjectCode", project.ProjectCode);
                parameters.Add("@StartDate", project.StartDate);
                parameters.Add("@EndDate", project.EndDate);
                parameters.Add("@Status", project.Status);
                parameters.Add("@ClientType", project.ClientType);
                parameters.Add("@ApprovalExtraWall", project.Approval);
                parameters.Add("@ExcelFilePath", filePath); 
                parameters.Add("@flag", "CreateProject");

                return await db.ExecuteScalarAsync<int>(spName, parameters, commandType: CommandType.StoredProcedure);
            }
        }


        public async Task<IEnumerable<createProjectDTO>> GetAllProjectsAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@flag", "GetAllProjects");

                return await db.QueryAsync<createProjectDTO>(spName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<createProjectDTO?> GetProjectByIdAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ProjectCode", id);
                parameters.Add("@flag", "GetProjectById");

                return await db.QueryFirstOrDefaultAsync<createProjectDTO>(spName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task UpdateProjectAsync(createProjectDTO project)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ProjectName", project.ProjectName);
                parameters.Add("@ProjectCode", project.ProjectCode);
                parameters.Add("@StartDate", project.StartDate);
                parameters.Add("@EndDate", project.EndDate);
                parameters.Add("@Status", project.Status);
                parameters.Add("@ClientType", project.ClientType);
                parameters.Add("@ApprovalExtraWall", project.Approval);

                if (project.excelFile != null)
                {
                    var folderPath = Path.Combine(_env.ContentRootPath, "C://UploadedFiles");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var filePath = Path.Combine(folderPath, project.excelFile.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await project.excelFile.CopyToAsync(stream);
                    }

                    parameters.Add("@ExcelFilePath", filePath);
                }
                else
                {
                    parameters.Add("@ExcelFilePath", null);
                }

                parameters.Add("@flag", "UpdateProject");

                await db.ExecuteAsync(spName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task DeleteProjectAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ProjectCode", id);
                parameters.Add("@flag", "DeleteProject");

                await db.ExecuteAsync(spName, parameters, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
