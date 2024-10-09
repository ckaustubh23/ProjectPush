namespace VendorBilling.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IRoleRepository RoleRepository { get; }
        IProjectRepository ProjectRepository { get; }

    }
}
