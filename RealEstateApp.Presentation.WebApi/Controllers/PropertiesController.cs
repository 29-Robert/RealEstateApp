using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.DTOs.Property;
using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApi.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PropertiesController : ControllerBase
    {
        private readonly IPropertyService _service;

        public PropertiesController(IPropertyService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<PropertyDto>>> List()
        {
            var result = await _service.GetAllAsync();
            return result.Count == 0 ? NoContent() : Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PropertyDetailDto>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("code/{code:int}")]
        public async Task<ActionResult<PropertyDetailDto>> GetByCode(int code)
        {
            var result = await _service.GetByCodeAsync(code);
            return result is null ? NotFound() : Ok(result);
        }
    }
}