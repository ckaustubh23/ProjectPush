using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using VendorBilling.Application.Common.DTO;
using VendorBilling.Application.Common.DTO.User;
using VendorBilling.Application.Common.Interfaces;
using VendorBilling.Entities.Models;
using VendorBilling.Infrastructure.Repository;

namespace VendorBilling.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUnitOfWork unitOfWork) : ControllerBase
    {
        APIResponse _response = new();
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var tokenDto = await unitOfWork.UserRepository.Login(model);
            if (tokenDto == null || string.IsNullOrEmpty(tokenDto.AccessToken))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username or password is incorrect");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = tokenDto;
            return Ok(_response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> GetNewTokenFromRefreshToken([FromBody] TokenDTO tokenDTO)
        {
            if (ModelState.IsValid)
            {
                var tokenDTOResponse = await unitOfWork.UserRepository.RefreshAccessToken(tokenDTO);
                if (tokenDTOResponse == null || string.IsNullOrEmpty(tokenDTOResponse.AccessToken))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Token Invalid");
                    return BadRequest(_response);
                }
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = tokenDTOResponse;
                return Ok(_response);
            }
            else
            {
                _response.IsSuccess = false;
                _response.Result = "Invalid Input";
                return BadRequest(_response);
            }

        }


        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] TokenDTO tokenDTO)
        {

            if (ModelState.IsValid)
            {
                await unitOfWork.UserRepository.RevokeRefreshToken(tokenDTO);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            _response.IsSuccess = false;
            _response.Result = "Invalid Input";
            return BadRequest(_response);
        }




        // POST: api/user/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] createUserDTO userDto)
        {
            if (userDto == null)
            {
                return BadRequest("User data is null.");
            }

            var newUserId = await unitOfWork.UserRepository.CreateUserAsync(userDto);
            return CreatedAtAction(nameof(GetUserById), new { id = newUserId }, userDto);
        }

        // PUT: api/user/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] updateUserDTO userDto)
        {
            if (userDto == null || userDto.Id <= 0)
            {
                return BadRequest("Invalid user data.");
            }

            var result = await unitOfWork.UserRepository.UpdateUserAsync(userDto);
            if (result == 0)
            {
                return NotFound("User not found.");
            }

            return NoContent(); 
        }

   
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await unitOfWork.UserRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }

        [Authorize]
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await unitOfWork.UserRepository.GetAllUsersAsync();
            return Ok(users);
        }
    }
}
