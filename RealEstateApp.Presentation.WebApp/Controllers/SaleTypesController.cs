using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.SaleTypes;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SaleTypesController : Controller
    {
        private readonly ISaleTypeService _saleTypeService;

        public SaleTypesController(ISaleTypeService saleTypeService)
        {
            _saleTypeService = saleTypeService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _saleTypeService.GetAllViewModel();
            return View(list);
        }

        public IActionResult Create()
        {
            return View(new SaveSaleTypeViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveSaleTypeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var list = await _saleTypeService.GetAllViewModel();
            if (list.Any(x => x.Name.ToLower().Trim() == vm.Name.ToLower().Trim()))
            {
                ModelState.AddModelError("Name", "Ya existe un tipo de venta registrado con este nombre.");
                return View(vm);
            }

            await _saleTypeService.Add(vm);
            TempData["SuccessMessage"] = "El tipo de venta fue creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _saleTypeService.GetByIdSaveViewModel(id);
            if (vm == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveSaleTypeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var list = await _saleTypeService.GetAllViewModel();
            if (list.Any(x => x.Name.ToLower().Trim() == vm.Name.ToLower().Trim() && x.Id != vm.Id))
            {
                ModelState.AddModelError("Name", "Ya existe otro tipo de venta registrado con este nombre.");
                return View(vm);
            }

            await _saleTypeService.Update(vm);
            TempData["SuccessMessage"] = "El tipo de venta fue actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var vm = await _saleTypeService.GetByIdSaveViewModel(id);
            if (vm == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            await _saleTypeService.Delete(id);
            TempData["SuccessMessage"] = "El tipo de venta fue eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}