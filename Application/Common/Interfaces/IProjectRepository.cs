using VendorBilling.Application.Common.DTO.Project;

namespace VendorBilling.Application.Common.Interfaces
{
    public interface IProjectRepository
    {
        Task<int> AddProjectAsync(createProjectDTO project);
        Task<IEnumerable<createProjectDTO>> GetAllProjectsAsync();
        Task<createProjectDTO?> GetProjectByIdAsync(int id);
        Task UpdateProjectAsync(createProjectDTO project);
        Task DeleteProjectAsync(int id);
    }
}
