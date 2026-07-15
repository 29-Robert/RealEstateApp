using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.DTOs.SaleTypes;
using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleTypesController : ControllerBase
    {
        private readonly ISaleTypeService _saleTypeService;

        public SaleTypesController(ISaleTypeService saleTypeService)
        {
            _saleTypeService = saleTypeService;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Desarrollador")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<SaleTypeDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List()
        {
            var list = await _saleTypeService.GetAllDto();
            if (list == null || list.Count == 0)
            {
                return NoContent();
            }
            return Ok(list);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Desarrollador")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SaleTypeDto))]
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

            var dto = await _saleTypeService.GetByIdDto(id);
            if (dto == null)
            {
                return NotFound("El tipo de venta solicitado no existe.");
            }

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SaleTypeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] SaveSaleTypeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Los datos enviados no son válidos.");
            }

            var list = await _saleTypeService.GetAllDto();
            if (list.Any(x => x.Name.ToLower().Trim() == dto.Name.ToLower().Trim()))
            {
                return BadRequest("Ya existe un tipo de venta registrado con este nombre.");
            }

            var createdDto = await _saleTypeService.AddDto(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdDto.Id }, createdDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SaleTypeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] SaveSaleTypeDto dto)
        {
            if (!ModelState.IsValid || id != dto.Id || id <= 0)
            {
                return BadRequest("Los datos enviados no son válidos.");
            }

            var existing = await _saleTypeService.GetByIdDto(id);
            if (existing == null)
            {
                return NotFound("El tipo de venta solicitado no existe.");
            }

            var list = await _saleTypeService.GetAllDto();
            if (list.Any(x => x.Name.ToLower().Trim() == dto.Name.ToLower().Trim() && x.Id != id))
            {
                return BadRequest("Ya existe otro tipo de venta registrado con este nombre.");
            }

            await _saleTypeService.UpdateDto(dto);
            var updatedDto = await _saleTypeService.GetByIdDto(id);
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

            var existing = await _saleTypeService.GetByIdDto(id);
            if (existing == null)
            {
                return NotFound("El tipo de venta solicitado no existe.");
            }

            await _saleTypeService.Delete(id);
            return NoContent();
        }
    }
}