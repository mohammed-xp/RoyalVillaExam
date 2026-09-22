using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.Dto;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;
using System.Diagnostics;

namespace RoyalVillaWeb.Controllers
{
    public class HomeController(IVillaService _villaService, IMapper _mapper) : Controller
    {
        public async Task<IActionResult> Index()
        {
            List<VillaDto> villaList = new();

            try
            {
                var response = await _villaService.GetAllAsync<ApiResponse<List<VillaDto>>>();
                if (response != null && response.Success && response.Data != null)
                {
                    villaList = response.Data;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return View(villaList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
