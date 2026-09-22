using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.Dto;
using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Controllers
{
    [Route("api/auth")]
    [ApiVersionNeutral]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> Register(RegistrationRequestDto registrationRequestDto)
        {
            try
            {
                if (registrationRequestDto == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registration data is required"));
                }

                if (await _authService.IsEmailExistsAsync(registrationRequestDto.Email))
                {
                    return Conflict(ApiResponse<object>.Conflict($"User with email '{registrationRequestDto.Email}' already exists"));
                }

                var userDto = await _authService.RegisterAsync(registrationRequestDto);

                if (userDto == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registration failed"));
                }

                var response = ApiResponse<UserDto>.CreatedAt(userDto, "User registered successfully");
                return CreatedAtAction(nameof(Register), response);

            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred during registration", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }


        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TokenDto>>> Login(LoginRequestDto loginRequestDto)
        {
            try
            {
                if (loginRequestDto == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Login data is required"));
                }

                var loginResponse = await _authService.LoginAsync(loginRequestDto);

                if (loginResponse == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Login failed, Please check your credentials"));
                }

                var response = ApiResponse<TokenDto>.Ok(loginResponse, "User logged in successfully");
                return Ok(response);

            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred during login", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> RefreshAccessToken(RefreshTokenRequestDto refreshTokenRequestDto)
        {
            try
            {
                if (refreshTokenRequestDto == null || string.IsNullOrEmpty(refreshTokenRequestDto.RefreshToken))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Refresh token is required"));
                }

                var tokenResponse = await _authService.RefreshAccessTokenAsync(refreshTokenRequestDto);

                if(tokenResponse == null)
                {
                    var errorResponse = ApiResponse<object>.Error(401, "Invalid or expired refresh token. If token reused was detected, all your sissions have been terminate");
                    return Unauthorized(errorResponse);
                }


                var response = ApiResponse<TokenDto>.Ok(tokenResponse, "Token refresh successfully");
                return Ok(response);

            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred during token refresh", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

    }
}
