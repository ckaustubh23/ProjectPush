using VendorBilling.Application.Common.Interfaces;
using VendorBilling.Infrastructure.Data;
using VendorBilling.Infrastructure.Data.DataAccess;

namespace VendorBilling.Infrastructure.Repository
{
    public sealed class UnitOfWork(ISqlDataAccess sqlDataAccess, ApplicationDbContext _db,
        IConfiguration _configuration, IWebHostEnvironment _env) : IUnitOfWork
    {
        private readonly Lazy<IUserRepository> _LazyUserRepository = new Lazy<IUserRepository>(() => new UserRepository(_db, _configuration, sqlDataAccess));
        private readonly Lazy<IRoleRepository> _LazyRoleRepository = new Lazy<IRoleRepository>(() => new RoleRepository(_configuration));
        private readonly Lazy<IProjectRepository> _LazyProjectRepository = new Lazy<IProjectRepository>(() => new ProjectRepository(_configuration, _env));


        public IUserRepository UserRepository => _LazyUserRepository.Value;
        public IRoleRepository RoleRepository => _LazyRoleRepository.Value;
        public IProjectRepository ProjectRepository => _LazyProjectRepository.Value;
        
    }
}
