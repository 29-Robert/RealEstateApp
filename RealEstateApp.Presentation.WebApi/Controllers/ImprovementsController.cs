using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.DTOs.Improvements;
using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImprovementsController : ControllerBase
    {
        private readonly IImprovementService _improvementService;

        public ImprovementsController(IImprovementService improvementService)
        {
            _improvementService = improvementService;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Desarrollador")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ImprovementDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List()
        {
            var list = await _improvementService.GetAllDto();
            if (list == null || list.Count == 0)
            {
                return NoContent();
            }
            return Ok(list);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Desarrollador")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ImprovementDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("El Id enviado no tiene un formato válido.");
            }

            var dto = await _improvementService.GetByIdDto(id);
            if (dto == null)
            {
                return NotFound("La mejora solicitada no existe.");
            }

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ImprovementDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] SaveImprovementDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Los datos enviados no son válidos.");
            }

            var list = await _improvementService.GetAllDto();
            if (list.Any(x => x.Name.ToLower().Trim() == dto.Name.ToLower().Trim()))
            {
                return BadRequest("Ya existe una mejora registrada con este nombre.");
            }

            var createdDto = await _improvementService.AddDto(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdDto.Id }, createdDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ImprovementDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] SaveImprovementDto dto)
        {
            if (!ModelState.IsValid || id != dto.Id || id <= 0)
            {
                return BadRequest("Los datos enviados no son válidos.");
            }

            var existing = await _improvementService.GetByIdDto(id);
            if (existing == null)
            {
                return NotFound("La mejora solicitada no existe.");
            }

            var list = await _improvementService.GetAllDto();
            if (list.Any(x => x.Name.ToLower().Trim() == dto.Name.ToLower().Trim() && x.Id != id))
            {
                return BadRequest("Ya existe otra mejora registrada con este nombre.");
            }

            await _improvementService.UpdateDto(dto);
            var updatedDto = await _improvementService.GetByIdDto(id);
            return Ok(updatedDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("El Id enviado no tiene un formato válido.");
            }

            var existing = await _improvementService.GetByIdDto(id);
            if (existing == null)
            {
                return NotFound("La mejora solicitada no existe.");
            }

            await _improvementService.Delete(id);
            return NoContent();
        }
    }
}