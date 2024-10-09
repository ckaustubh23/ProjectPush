using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VendorBilling.Application.Common.DTO;
using VendorBilling.Application.Common.DTO.User;
using VendorBilling.Application.Common.Interfaces;
using VendorBilling.Entities.Models;
using VendorBilling.Infrastructure.Data;
using VendorBilling.Infrastructure.Data.DataAccess;

namespace VendorBilling.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        private readonly ApplicationDbContext _db;
        private readonly ISqlDataAccess _sqlDataAccess;
        private readonly string _secretKey;
        private const string spName = "stp_User_Management";
        private readonly IConfiguration _configuration;

        public UserRepository(ApplicationDbContext db, IConfiguration configuration, ISqlDataAccess sqlDataAccess)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _db = db;
            _sqlDataAccess = sqlDataAccess;
            _secretKey = configuration.GetValue<string>("JWT:Secret");
            _configuration = configuration;
        }

        public async Task<int> CreateUserAsync(createUserDTO user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Username", user.Username);
                parameters.Add("@FirstName", user.FirstName);
                parameters.Add("@LastName", user.LastName);
                parameters.Add("@Password", user.Password);
                parameters.Add("@Role", user.Role);
                parameters.Add("@Email", user.Email);
                parameters.Add("@MobileNo", user.MobileNo);
                parameters.Add("@Status", user.Status);
                parameters.Add("@flag", "CreateUser");

                await connection.ExecuteAsync("stp_User_Management", parameters, commandType: CommandType.StoredProcedure);

                return 0;
            }
        }

        public async Task<int> UpdateUserAsync(updateUserDTO user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", user.Id);
                parameters.Add("@Username", user.Username);
                parameters.Add("@FirstName", user.FirstName);
                parameters.Add("@LastName", user.LastName);
                parameters.Add("@Role", user.Role);
                parameters.Add("@Email", user.Email);
                parameters.Add("@MobileNo", user.MobileNo);
                parameters.Add("@Status", user.Status);
                parameters.Add("@flag", "UpdateUser");

                return await connection.ExecuteAsync("stp_User_Management", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<IEnumerable<createUserDTO>> GetAllUsersAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@flag", "GetUser");

                var users = await connection.QueryAsync<createUserDTO>("stp_User_Management", parameters, commandType: CommandType.StoredProcedure);
                return users;
            }
        }

        public async Task<createUserDTO?> GetUserByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<createUserDTO>(
                    "SELECT Id, Username, FirstName, LastName, Role, Email, MobileNo, Status FROM tbl_Users WHERE Id = @Id", new { Id = id });
            }
        }

        public async Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@flag", "Login", DbType.String);
            parameters.Add("@Username", loginRequestDTO.UserName, DbType.String);
            parameters.Add("@Password", loginRequestDTO.Password, DbType.String);

            var user = await _sqlDataAccess.GetData<createUserDTO, DynamicParameters>(spName, parameters);

            if (user == null)
            {
                return new TokenDTO()
                {
                    AccessToken = ""
                };
            }

            var jwtTokenId = $"JTI{Guid.NewGuid()}";
            var accessToken = await GetAccessToken(user, jwtTokenId);
            var refreshToken = await CreateNewRefreshToken(user.Id.ToString(), jwtTokenId);

            return new TokenDTO()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private async Task<string> GetAccessToken(createUserDTO user, string jwtTokenId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.FirstName.ToString()),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, jwtTokenId),
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO)
        {
            var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(u => u.Refresh_Token == tokenDTO.RefreshToken);
            if (existingRefreshToken == null)
            {
                return new TokenDTO();
            }

            var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            if (!isTokenValid || existingRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                await MarkTokenAsInvalid(existingRefreshToken);
                return new TokenDTO();
            }

            var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            await MarkTokenAsInvalid(existingRefreshToken);

            var parameters = new DynamicParameters();
            parameters.Add("@flag", "GetUser", DbType.String);
            parameters.Add("@Id", existingRefreshToken.UserId, DbType.String);

            var applicationUser = await _sqlDataAccess.GetData<createUserDTO, DynamicParameters>(spName, parameters);

            if (applicationUser == null)
                return new TokenDTO();

            var newAccessToken = await GetAccessToken(applicationUser, existingRefreshToken.JwtTokenId);

            return new TokenDTO()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }

        public async Task RevokeRefreshToken(TokenDTO tokenDTO)
        {
            var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(_ => _.Refresh_Token == tokenDTO.RefreshToken);

            if (existingRefreshToken == null)
                return;

            var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            if (!isTokenValid)
            {
                return;
            }

            await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
        }

        private async Task<string> CreateNewRefreshToken(string userId, string tokenId)
        {
            RefreshToken refreshToken = new()
            {
                IsValid = true,
                UserId = userId,
                JwtTokenId = tokenId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(2),
                Refresh_Token = Guid.NewGuid() + "-" + Guid.NewGuid(),
            };

            await _db.RefreshTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();
            return refreshToken.Refresh_Token;
        }

        private bool GetAccessTokenData(string accessToken, string expectedUserId, string expectedTokenId)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);
                var jwtTokenId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti).Value;
                var userId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;
                return userId == expectedUserId && jwtTokenId == expectedTokenId;
            }
            catch
            {
                return false;
            }
        }

        private async Task MarkAllTokenInChainAsInvalid(string userId, string tokenId)
        {
            await _db.RefreshTokens.Where(u => u.UserId == userId && u.JwtTokenId == tokenId)
                                   .ExecuteUpdateAsync(u => u.SetProperty(refreshToken => refreshToken.IsValid, false));
        }

        private Task MarkTokenAsInvalid(RefreshToken refreshToken)
        {
            refreshToken.IsValid = false;
            return _db.SaveChangesAsync();
        }
    }
}
