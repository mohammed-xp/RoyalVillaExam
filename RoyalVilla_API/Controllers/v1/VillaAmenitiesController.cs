using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.Dto;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;

namespace RoyalVilla_API.Controllers.v1
{
    [Route("api/v{version:apiVersion}/villa-amenitie")]
    [ApiVersion("1.0")]
    [ApiController]
    public class VillaAmenitiesController(AppDbContext dbContext, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaAmenitiesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaAmenitiesDto>>>> GetVillaAmenities()
        {
            var villasAmenities = await dbContext.VillaAmenities.Include(v => v.Villa).ToListAsync();
            var villaAmenitiesDto = mapper.Map<List<VillaAmenitiesDto>>(villasAmenities);
            var response = ApiResponse<IEnumerable<VillaAmenitiesDto>>.Ok(villaAmenitiesDto, "Villa amenities retrieved successfully.");
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDto>>> GetVillaAmenityById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Villa amenity ID must be greader than 0"));
                }

                var villasAmenity = await dbContext.VillaAmenities
                    .Include(v => v.Villa)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (villasAmenity is null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa amenity with {id} was not found"));
                }

                var villaAmenityDto = mapper.Map<VillaAmenitiesDto>(villasAmenity);
                var response = ApiResponse<VillaAmenitiesDto>.Ok(villaAmenityDto, "Villa amenity retrieved successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while get villa amenity by id", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDto>>> CreateVillaAmenity(VillaAmenitiesCreateDto villaAmenitiesCreateDto)
        {
            try
            {
                if (villaAmenitiesCreateDto is null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa amenity data is required"));
                }

                var villaExists = await dbContext.Villas.FirstOrDefaultAsync(v => v.Id == villaAmenitiesCreateDto.VillaId);

                if (villaExists is null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"Villa with the ID {villaAmenitiesCreateDto.VillaId} does not exists"));
                }

                VillaAmenities villaAmenities = mapper.Map<VillaAmenities>(villaAmenitiesCreateDto);

                villaAmenities.CreatedDate = DateTime.Now;

                await dbContext.VillaAmenities.AddAsync(villaAmenities);
                await dbContext.SaveChangesAsync();

                var villaAmenityDto = mapper.Map<VillaAmenitiesDto>(villaAmenities);
                var response = ApiResponse<VillaAmenitiesDto>.CreatedAt(villaAmenityDto, "Villa amenity created successfully.");
                return CreatedAtAction(nameof(CreateVillaAmenity), new { id = villaAmenityDto.Id }, response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while create villa amenity", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDto>>> UpdateVillaAmenity(int id, VillaAmenitiesUpdateDto villaAmenitiesUpdateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Villa amenity ID must be greader than 0"));
                }

                if (villaAmenitiesUpdateDto is null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa amenity data is required"));
                }

                if (id != villaAmenitiesUpdateDto.Id)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"VillaAmenity with ID {id} was not found"));
                }

                var villaExists = await dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);

                if (villaExists == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa with the ID {villaAmenitiesUpdateDto.VillaId} does not found"));
                }

                var villaAmenityExists = await dbContext.VillaAmenities.FirstOrDefaultAsync(v => v.Id == id);

                if (villaAmenityExists == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa amenity with the ID {id} does not found"));
                }


                mapper.Map(villaAmenitiesUpdateDto, villaAmenityExists);

                villaAmenityExists.UpdatedDate = DateTime.Now;

                await dbContext.SaveChangesAsync();

                var villaAmenityDto = mapper.Map<VillaAmenitiesDto>(villaAmenityExists);
                var response = ApiResponse<VillaAmenitiesDto>.Ok(villaAmenityDto, "Villa amenity updated successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while update villa amenity", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVillaAmenity(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Villa amenity ID must be greader than 0"));
                }

                var villaAmenity = await dbContext.VillaAmenities.FirstOrDefaultAsync(v => v.Id == id);

                if (villaAmenity == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa amenity with the ID {id} does not found"));
                }

                dbContext.VillaAmenities.Remove(villaAmenity);

                await dbContext.SaveChangesAsync();

                var response = ApiResponse<object>.NoContent("Villa amenity deleted successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while delete villa amenity", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

    }
}
