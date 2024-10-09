using VendorBilling.Application.Common.DTO;
using VendorBilling.Application.Common.DTO.User;

namespace VendorBilling.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<int> CreateUserAsync(createUserDTO user); 
        Task<int> UpdateUserAsync(updateUserDTO user);
        Task<createUserDTO?> GetUserByIdAsync(int id);
        Task<IEnumerable<createUserDTO>> GetAllUsersAsync();

        Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO);
        Task RevokeRefreshToken(TokenDTO tokenDTO);
    }
}
