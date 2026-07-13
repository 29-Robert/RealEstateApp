using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.PropertyTypes;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PropertyTypesController : Controller
    {
        private readonly IPropertyTypeService _propertyTypeService;

        public PropertyTypesController(IPropertyTypeService propertyTypeService)
        {
            _propertyTypeService = propertyTypeService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _propertyTypeService.GetAllViewModel();
            return View(list);
        }

        public IActionResult Create()
        {
            return View(new SavePropertyTypeViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(SavePropertyTypeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var list = await _propertyTypeService.GetAllViewModel();
            if (list.Any(x => x.Name.ToLower().Trim() == vm.Name.ToLower().Trim()))
            {
                ModelState.AddModelError("Name", "Ya existe un tipo de propiedad registrado con este nombre.");
                return View(vm);
            }

            await _propertyTypeService.Add(vm);
            TempData["SuccessMessage"] = "El tipo de propiedad fue creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _propertyTypeService.GetByIdSaveViewModel(id);
            if (vm == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SavePropertyTypeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var list = await _propertyTypeService.GetAllViewModel();
            if (list.Any(x => x.Name.ToLower().Trim() == vm.Name.ToLower().Trim() && x.Id != vm.Id))
            {
                ModelState.AddModelError("Name", "Ya existe otro tipo de propiedad registrado con este nombre.");
                return View(vm);
            }

            await _propertyTypeService.Update(vm);
            TempData["SuccessMessage"] = "El tipo de propiedad fue actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var vm = await _propertyTypeService.GetByIdSaveViewModel(id);
            if (vm == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            await _propertyTypeService.Delete(id);
            TempData["SuccessMessage"] = "El tipo de propiedad fue eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}