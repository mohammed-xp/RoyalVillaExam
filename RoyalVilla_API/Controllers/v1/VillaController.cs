using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.Dto;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;

namespace RoyalVilla_API.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class VillaController(AppDbContext dbContext, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDto>>>> GetVillas()
        {
            var villas = await dbContext.Villas.ToListAsync();
            var dtoResponseVilla = mapper.Map<List<VillaDto>>(villas);
            var response = ApiResponse<IEnumerable<VillaDto>>.Ok(dtoResponseVilla, "Villas retrieved successfully.");
            return Ok(response);
        }

        [HttpGet("{id:int}")]
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
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(ApiResponse<VillaDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDto>>> CreateVilla(VillaCreateDto villaDto)
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
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(ApiResponse<VillaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDto>>> UpdateVilla(int id, VillaUpdateDto villaDto)
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

                var existingVilla = await dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);

                if (existingVilla is null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"Villa with ID {id} not found."));
                }

                var dublicateVilla = await dbContext.Villas.FirstOrDefaultAsync(v => v.Name.ToLower() == villaDto.Name.ToLower()
                && v.Id != id);

                if (dublicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict("A villa with the same name already exists"));
                }

                mapper.Map(villaDto, existingVilla);

                existingVilla.UpdatedAt = DateTime.Now;

                await dbContext.SaveChangesAsync();

                var response = ApiResponse<VillaDto>.Ok(mapper.Map<VillaDto>(villaDto), "Villa updated successfully.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "Internal server error", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
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
