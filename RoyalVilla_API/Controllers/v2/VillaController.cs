using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.Dto;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Services.IServices;
using System.Text;

namespace RoyalVilla_API.Controllers.v2
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    //[Authorize(Roles ="TopAdmin")]
    public class VillaController(AppDbContext dbContext, IMapper mapper, IImageService imageService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDto>>>> GetVillas(
            [FromQuery] string? filterBy,
            [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var villasQuery = dbContext.Villas.AsQueryable();

            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterQuery))
            {
                switch (filterBy.ToLower())
                {
                    case "name":
                        villasQuery = villasQuery.Where(u => u.Name.ToLower().Contains(filterQuery.ToLower()));
                        break;
                    case "details":
                        villasQuery = villasQuery.Where(u => u.Details.ToLower().Contains(filterQuery.ToLower()));
                        break;
                    case "rate":
                        if (double.TryParse(filterQuery, out double rate))
                        {
                            villasQuery = villasQuery.Where(u => u.Rate == rate);
                        }
                        break;
                    case "minrate":
                        if (double.TryParse(filterQuery, out double minRate))
                        {
                            villasQuery = villasQuery.Where(u => u.Rate >= minRate);
                        }
                        break;
                    case "maxrate":
                        if (double.TryParse(filterQuery, out double maxRate))
                        {
                            villasQuery = villasQuery.Where(u => u.Rate <= maxRate);
                        }
                        break;
                    case "occupancy":
                        if (int.TryParse(filterQuery, out int occupancy))
                        {
                            villasQuery = villasQuery.Where(u => u.Occupancy == occupancy);
                        }
                        break;
                }
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                var isDescending = sortOrder?.ToLower() == "desc";

                villasQuery = sortBy.ToLower() switch
                {
                    "name" => isDescending ? villasQuery.OrderByDescending(v => v.Name)
                        : villasQuery.OrderBy(v => v.Name),
                    "details" => isDescending ? villasQuery.OrderByDescending(v => v.Details)
                        : villasQuery.OrderBy(v => v.Details),
                    "rate" => isDescending ? villasQuery.OrderByDescending(v => v.Rate)
                        : villasQuery.OrderBy(v => v.Rate),
                    "occupancy" => isDescending ? villasQuery.OrderByDescending(v => v.Occupancy)
                        : villasQuery.OrderBy(v => v.Occupancy),
                    "sqft" => isDescending ? villasQuery.OrderByDescending(v => v.Sqft)
                        : villasQuery.OrderBy(v => v.Sqft),
                    "id" => isDescending ? villasQuery.OrderByDescending(v => v.Id)
                        : villasQuery.OrderBy(v => v.Id),
                    _ => villasQuery.OrderBy(v => v.Id)
                };
            }
            else
            {
                villasQuery = villasQuery.OrderBy(v => v.Id);
            }

            var skip = (page - 1) * pageSize;

            var totalCount = await villasQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var villas = await villasQuery.Skip(skip).Take(pageSize).ToListAsync();
            var dtoResponseVilla = mapper.Map<List<VillaDto>>(villas);

            var messageBuilder = new StringBuilder();

            messageBuilder.Append($"Successfully retrieved {dtoResponseVilla.Count} villa(s)");
            messageBuilder.Append($"Page {page} of {totalPages}, {totalCount} total records");

            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterQuery))
            {
                messageBuilder.Append($" Filtered by {filterBy}: '{filterQuery}'");

            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                messageBuilder.Append($" Sorted by {sortBy}: '{sortOrder?.ToLower() ?? "asc"}'");
            }

            Response.Headers.Append("X-Pagination-CurrentPage", page.ToString());
            Response.Headers.Append("X-Pagination-PageSize", pageSize.ToString());
            Response.Headers.Append("X-Pagination-TotalCount", totalCount.ToString());
            Response.Headers.Append("X-Pagination-TotalPages", totalPages.ToString());

            var response = ApiResponse<IEnumerable<VillaDto>>.Ok(dtoResponseVilla, messageBuilder.ToString());
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VillaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDto>>> GetVillaById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Villa ID must be greater than 0."));
                }

                var villa = await dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);

                if (villa is null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} not found."));
                }

                return Ok(ApiResponse<VillaDto>.Ok(mapper.Map<VillaDto>(villa), "Record retrieved successfully."));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "Internal server error", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]

        [ProducesResponseType(typeof(ApiResponse<VillaDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDto>>> CreateVilla([FromForm] VillaCreateDto villaDto)
        {
            try
            {
                if (villaDto is null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa data is required"));
                }

                var dublicateVilla = await dbContext.Villas.FirstOrDefaultAsync(v => v.Name.ToLower() == villaDto.Name.ToLower());

                if (dublicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict("A villa with the same name already exists"));
                }

                Villa villa = mapper.Map<Villa>(villaDto);

                if (villaDto.Image != null)
                {
                    if (!imageService.ValidateImage(villaDto.Image))
                    {
                        return BadRequest(ApiResponse<object>.BadRequest("Invalid image file. Allowed formats: jpg, jpeg, png. Max size: 5MB"));
                    }
                    villa.ImageUrl = await imageService.UploadImageAsync(villaDto.Image);
                }

                await dbContext.Villas.AddAsync(villa);
                await dbContext.SaveChangesAsync();

                var response = ApiResponse<VillaDto>.CreatedAt(mapper.Map<VillaDto>(villa), "Villa created successfully.");

                return CreatedAtAction(nameof(GetVillaById), new { id = villa.Id }, response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "Internal server error", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<VillaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDto>>> UpdateVilla(int id, [FromForm] VillaUpdateDto villaDto)
        {
            try
            {
                if (villaDto is null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa data is required"));
                }

                if (id != villaDto.Id)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa ID mismatch"));
                }

                if (villaDto.Image != null && !imageService.ValidateImage(villaDto.Image))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Invalid image file. Allowed formats: jpg, jpeg, png. Max size: 5MB"));
                }

                var existingVilla = await dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);

                if (existingVilla is null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"Villa with ID {id} not found"));
                }

                var dublicateVilla = await dbContext.Villas.FirstOrDefaultAsync(v => v.Name.ToLower() == villaDto.Name.ToLower()
                && v.Id != id);

                if (dublicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict("A villa with the same name already exists"));
                }

                var oldImageUrl = existingVilla.ImageUrl;

                mapper.Map(villaDto, existingVilla);

                existingVilla.UpdatedAt = DateTime.Now;

                if (villaDto.Image != null)
                {
                    existingVilla.ImageUrl = await imageService.UploadImageAsync(villaDto.Image);
                    villaDto.ImageUrl = existingVilla.ImageUrl;
                    if (!string.IsNullOrEmpty(oldImageUrl) && oldImageUrl != existingVilla.ImageUrl)
                    {
                        await imageService.DeleteImageAsync(oldImageUrl);
                    }
                }

                await dbContext.SaveChangesAsync();

                var response = ApiResponse<VillaDto>.Ok(mapper.Map<VillaDto>(villaDto), "Villa updated successfully");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "Internal server error", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVilla(int id)
        {
            try
            {
                var existingVilla = await dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);

                if (existingVilla is null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"Villa with ID {id} not found."));
                }

                if (!string.IsNullOrEmpty(existingVilla.ImageUrl))
                {
                    await imageService.DeleteImageAsync(existingVilla.ImageUrl);
                }

                dbContext.Villas.Remove(existingVilla);

                await dbContext.SaveChangesAsync();

                return Ok(ApiResponse<object>.NoContent("Villa deleted successfully."));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "Internal server error", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
