using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

[Authorize]
public class RolesController : Controller
{
    private readonly IApiService _apiService;
    private readonly string _endpoint = "api/roles";

    public RolesController(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        var roles = await _apiService.GetAsync<Rol>(_endpoint);
        return View(roles);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Rol rol)
    {
        if (ModelState.IsValid)
        {
            await _apiService.PostAsync(_endpoint, rol);
            return RedirectToAction(nameof(Index));
        }

        return View(rol);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var rol = await _apiService.GetByIdAsync<Rol>(_endpoint, id);
        return View(rol);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Rol rol)
    {
        if (ModelState.IsValid)
        {
            await _apiService.PutAsync(_endpoint, rol);
            return RedirectToAction(nameof(Index));
        }

        return View(rol);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var rol = await _apiService.GetByIdAsync<Rol>(_endpoint, id);
        return View(rol);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int RolId)
    {
        var request = new eRolRequest
        {
            RolId = RolId,
            Activar = false // o true si lo quieres reactivar
        };

        await _apiService.PostAsync($"{_endpoint}/del", request); // "/del" es el endpoint que tienes en tu API
        return RedirectToAction(nameof(Index), "Roles");
    }

}
