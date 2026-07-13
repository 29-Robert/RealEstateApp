using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Improvements;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ImprovementsController : Controller
    {
        private readonly IImprovementService _improvementService;

        public ImprovementsController(IImprovementService improvementService)
        {
            _improvementService = improvementService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _improvementService.GetAllViewModel();
            return View(list);
        }

        public IActionResult Create()
        {
            return View(new SaveImprovementViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveImprovementViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var list = await _improvementService.GetAllViewModel();
            if (list.Any(x => x.Name.ToLower().Trim() == vm.Name.ToLower().Trim()))
            {
                ModelState.AddModelError("Name", "Ya existe una mejora registrada con este nombre.");
                return View(vm);
            }

            await _improvementService.Add(vm);
            TempData["SuccessMessage"] = "La mejora fue creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _improvementService.GetByIdSaveViewModel(id);
            if (vm == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveImprovementViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var list = await _improvementService.GetAllViewModel();
            if (list.Any(x => x.Name.ToLower().Trim() == vm.Name.ToLower().Trim() && x.Id != vm.Id))
            {
                ModelState.AddModelError("Name", "Ya existe otra mejora registrada con este nombre.");
                return View(vm);
            }

            await _improvementService.Update(vm);
            TempData["SuccessMessage"] = "La mejora fue actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var vm = await _improvementService.GetByIdSaveViewModel(id);
            if (vm == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            await _improvementService.Delete(id);
            TempData["SuccessMessage"] = "La mejora fue eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}