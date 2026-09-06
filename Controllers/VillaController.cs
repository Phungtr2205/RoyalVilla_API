using Microsoft.AspNetCore.Mvc;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using RoyalVilla_API.Models.DTO;
using AutoMapper;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaController(ApplicationDbContext db,IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> GetVillas()
        {
            var villas = await _db.Villa.ToListAsync();
            var dtoResponseVilla = _mapper.Map<IEnumerable<VillaDTO>>(villas);
            var response = ApiResponse<IEnumerable<VillaDTO>>.Ok(_mapper.Map<IEnumerable<VillaDTO>>(villas), "Villas retrieved successfully");
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> GetVillaById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Villa ID must be greater than 0"));
                }
                var villa = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);
                if(villa == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} was not found"));
                }
                return Ok(ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(villa), "Villa retrieved successfully"));

            }
            catch(Exception ex) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
    $"An error occurred while retrieving villa with Id {id} : {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<VillaDTO>> CreateVilla(VillaCreateDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return BadRequest("Villa Data is required");
                }


                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDTO.Name.ToLower());

                if (duplicateVilla != null)
                {
                    return Conflict($"A villa  with the name '{villaDTO.Name}' already exists");
                }

                Villa villa = _mapper.Map<Villa>(villaDTO);


                await _db.Villa.AddAsync(villa);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(CreateVilla), new { id = villa.Id }, _mapper.Map<VillaDTO>(villa));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                $"An error occurred while creating villa with Id : {ex.Message}");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<VillaUpdateDTO>> UpdateVilla(int id, VillaUpdateDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return BadRequest("Villa Data is required");
                }

                if (id != villaDTO.Id)
                {
                    return BadRequest("Villa ID in URL does not match Villa ID in the request body");
                }

                var existingVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);
                
                if (existingVilla == null)
                {
                    return NotFound($"Villa with ID {id} was not found");
                }

                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDTO.Name.ToLower()
                && u.Id != id);

                if(duplicateVilla != null)
                {
                    return Conflict($"A villa  with the name '{villaDTO.Name}' already exists");
                }

                _mapper.Map(villaDTO,existingVilla);
                existingVilla.UpdateDate = DateTime.Now;
                await _db.SaveChangesAsync();

                return Ok(villaDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                $"An error occurred while updating villa with Id : {ex.Message}");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteVilla(int id)
        {
            try
            {
                var existingVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);

                if (existingVilla == null)
                {
                    return NotFound($"Villa with ID {id} was not found");
                }

                _db.Villa.Remove(existingVilla); 
                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                $"An error occurred while deleting villa with Id : {ex.Message}");
            }
        }   

    }
}
