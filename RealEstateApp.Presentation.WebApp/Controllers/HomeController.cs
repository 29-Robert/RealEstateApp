using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IPropertyTypeService _propertyTypeService;

        public HomeController(IPropertyService propertyService, IPropertyTypeService propertyTypeService)
        {
            _propertyService = propertyService;
            _propertyTypeService = propertyTypeService;
        }

        public async Task<IActionResult> Index(
            int? propertyTypeId,
            decimal? minPrice,
            decimal? maxPrice,
            int? rooms,
            int? bathrooms,
            int? code)
        {
            // Búsqueda por código exacto
            if (code.HasValue)
            {
                var found = await _propertyService.GetByCodeAsync(code.Value);
                if (found != null)
                {
                    return RedirectToAction(nameof(Details), new { id = found.Id });
                }
                TempData["SearchError"] = $"No se encontró ninguna propiedad con el código {code.Value}.";
            }

            var properties = await _propertyService.GetFilteredAsync(
                propertyTypeId, minPrice, maxPrice, rooms, bathrooms);

            var propertyTypes = await _propertyTypeService.GetAllViewModel();

            ViewBag.PropertyTypes = propertyTypes;
            ViewBag.SelectedTypeId = propertyTypeId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Rooms = rooms;
            ViewBag.Bathrooms = bathrooms;
            ViewBag.Code = code;

            return View(properties);
        }

        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyService.GetByIdAsync(id);
            if (property == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(property);
        }
    }
}
