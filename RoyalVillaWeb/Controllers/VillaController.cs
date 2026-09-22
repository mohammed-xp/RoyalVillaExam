using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.Dto;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;
using System.Diagnostics;

namespace RoyalVillaWeb.Controllers
{
    public class VillaController(IVillaService _villaService, IMapper _mapper) : Controller
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

        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VillaCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return View(createDto);
            }

            try
            {
                var response = await _villaService.CreateAsync<ApiResponse<VillaDto>>(createDto);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Villa created seccessfully";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return View(createDto);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "Invalid Villa ID";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var response = await _villaService.GetAsync<ApiResponse<VillaDto>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Villa Reseved seccessfully";
                    return View(response.Data);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(VillaDto dto)
        {
            try
            {
                var response = await _villaService.DeleteAsync<ApiResponse<object>>(dto.Id);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Villa Deleted seccessfully";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "Invalid Villa ID";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var response = await _villaService.GetAsync<ApiResponse<VillaDto>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Villa recived seccessfully";
                    return View(_mapper.Map<VillaUpdateDto>(response.Data));
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VillaUpdateDto dto)
        {
            try
            {
                var response = await _villaService.UpdateAsync<ApiResponse<object>>(dto);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Villa Edited seccessfully";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
