using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealEstateApp.Application.DTOs.Property;
using RealEstateApp.Application.Interfaces.Services;
using System.Security.Claims;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Agente")]
    public class PropertyController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly ISaleTypeService _saleTypeService;
        private readonly IImprovementService _improvementService;
        private readonly IWebHostEnvironment _env;

        public PropertyController(
            IPropertyService propertyService,
            IPropertyTypeService propertyTypeService,
            ISaleTypeService saleTypeService,
            IImprovementService improvementService,
            IWebHostEnvironment env)
        {
            _propertyService = propertyService;
            _propertyTypeService = propertyTypeService;
            _saleTypeService = saleTypeService;
            _improvementService = improvementService;
            _env = env;
        }

        private string GetCurrentAgentId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            var agentId = GetCurrentAgentId();
            var properties = await _propertyService.GetByAgentAsync(agentId, false);
            return View(properties);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new SavePropertyDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavePropertyDto dto, List<IFormFile> images)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(dto.PropertyTypeId, dto.SaleTypeId);
                return View(dto);
            }

            if (images == null || images.Count == 0)
            {
                ModelState.AddModelError("images", "Debe cargar al menos una imagen.");
                await LoadDropdowns(dto.PropertyTypeId, dto.SaleTypeId);
                return View(dto);
            }

            if (images.Count > 4)
            {
                ModelState.AddModelError("images", "Puede cargar un máximo de 4 imágenes.");
                await LoadDropdowns(dto.PropertyTypeId, dto.SaleTypeId);
                return View(dto);
            }

            dto.AgentId = GetCurrentAgentId();
            dto.NewImagesBase64 = await ConvertImagesToBase64(images);

            await _propertyService.CreateAsync(dto);
            TempData["SuccessMessage"] = "Propiedad creada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var property = await _propertyService.GetByIdAsync(id);
            if (property == null) return RedirectToAction(nameof(Index));

            if (property.AgentId != GetCurrentAgentId())
            {
                TempData["ErrorMessage"] = "No puedes editar una propiedad que no te pertenece.";
                return RedirectToAction(nameof(Index));
            }

            if (property.Status == "Vendida")
            {
                TempData["ErrorMessage"] = "No puedes editar una propiedad que ya fue vendida.";
                return RedirectToAction(nameof(Index));
            }

            // Cargar tipos y mantener seleccionados los valores actuales
            await LoadDropdowns(property.PropertyTypeId, property.SaleTypeId);

            // Mapeo seguro de las mejoras seleccionadas
            var allImprovements = await _improvementService.GetAllViewModel();
            var selectedImprovementIds = property.ImprovementIds?.Any() == true
                ? property.ImprovementIds
                : allImprovements
                    .Where(imp => property.Improvements != null && property.Improvements.Contains(imp.Name))
                    .Select(imp => imp.Id)
                    .ToList();

            var dto = new SavePropertyDto
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description ?? string.Empty,
                Price = property.Price,
                Size = property.Size,
                Rooms = property.Rooms,
                Bathrooms = property.Bathrooms,
                PropertyTypeId = property.PropertyTypeId,
                SaleTypeId = property.SaleTypeId,
                AgentId = property.AgentId,
                ImprovementIds = selectedImprovementIds
            };

            ViewBag.PropertyId = id;
            ViewBag.ExistingImages = property.ImagePaths;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SavePropertyDto dto, List<IFormFile> newImages, List<int> existingImageIds)
        {
            dto.Id = id; // Asegura que el DTO lleve el ID recibido por parámetro

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(dto.PropertyTypeId, dto.SaleTypeId);
                ViewBag.PropertyId = id;
                return View(dto);
            }

            var property = await _propertyService.GetByIdAsync(id);
            if (property == null) return RedirectToAction(nameof(Index));

            if (property.AgentId != GetCurrentAgentId())
            {
                TempData["ErrorMessage"] = "No puedes editar esta propiedad.";
                return RedirectToAction(nameof(Index));
            }

            dto.AgentId = GetCurrentAgentId();
            dto.ExistingImageIds = existingImageIds ?? new List<int>();

            if (newImages != null && newImages.Any())
            {
                dto.NewImagesBase64 = await ConvertImagesToBase64(newImages);
            }

            await _propertyService.UpdateAsync(id, dto);
            TempData["SuccessMessage"] = "Propiedad actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var property = await _propertyService.GetByIdAsync(id);
            if (property == null) return RedirectToAction(nameof(Index));

            if (property.AgentId != GetCurrentAgentId())
            {
                TempData["ErrorMessage"] = "No puedes eliminar esta propiedad.";
                return RedirectToAction(nameof(Index));
            }

            if (property.Status == "Vendida")
            {
                TempData["ErrorMessage"] = "No puedes eliminar una propiedad vendida.";
                return RedirectToAction(nameof(Index));
            }

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            var property = await _propertyService.GetByIdAsync(id);
            if (property == null) return RedirectToAction(nameof(Index));

            if (property.AgentId != GetCurrentAgentId())
            {
                TempData["ErrorMessage"] = "No puedes eliminar esta propiedad.";
                return RedirectToAction(nameof(Index));
            }

            await _propertyService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Propiedad eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns(int? selectedPropertyTypeId = null, int? selectedSaleTypeId = null)
        {
            var types = await _propertyTypeService.GetAllViewModel();
            var sales = await _saleTypeService.GetAllViewModel();
            var improvements = await _improvementService.GetAllViewModel();

            ViewBag.PropertyTypes = new SelectList(types, "Id", "Name", selectedPropertyTypeId);
            ViewBag.SaleTypes = new SelectList(sales, "Id", "Name", selectedSaleTypeId);
            ViewBag.Improvements = improvements;
        }

        private async Task<List<string>> ConvertImagesToBase64(List<IFormFile> files)
        {
            var result = new List<string>();
            foreach (var file in files)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());
                var ext = Path.GetExtension(file.FileName).TrimStart('.');
                result.Add($"data:image/{ext};base64,{base64}");
            }
            return result;
        }
    }
}