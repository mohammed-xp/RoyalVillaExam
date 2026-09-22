using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.Dto;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RoyalVillaWeb.Controllers
{
    public class AuthController(IAuthService _authService, IMapper _mapper, ITokenProvider tokenProvider) : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            try
            {
                var response = await _authService.LoginAsync<ApiResponse<TokenDto>>(loginRequestDto);
                if (response != null && response.Success && response.Data != null)
                {
                    var principal = tokenProvider.CreatePrincipalFromJwtToken(response.Data.AccessToken);

                    if (principal != null)
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        tokenProvider.SetToken(response.Data.AccessToken, response.Data.RefreshToken);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["error"] = "Invalid token received. Please try again.";
                    }
                }
                else
                {
                    TempData["error"] = response.Message;
                }
                return View(loginRequestDto);
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegistrationRequestDto
            {
                Email = string.Empty,
                Name = string.Empty,
                Password = string.Empty,
                Role = "Customer"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationRequestDto registrationRequestDto)
        {
            try
            {
                ApiResponse<UserDto>? response = await _authService.RegisterAsync<ApiResponse<UserDto>>(registrationRequestDto);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Registration successful! Please login with your credentials.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    TempData["error"] = response?.Message ?? "Registration field. Please try again.";
                    return View(registrationRequestDto);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return View(registrationRequestDto);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            tokenProvider.ClearToken();

            return RedirectToAction("Index", "Home");
        }
    }
}
