using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

[Authorize(Roles = "Admin,Admin AutoReport")]
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
            TempData["Mensaje"] = "Rol  creado correctamente.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }
        TempData["Mensaje"] = "Ocurrio un error!";
        TempData["TipoMensaje"] = "error";
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
            TempData["Mensaje"] = "Rol editado correctamente.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }
        TempData["Mensaje"] = "Ocurrio un error!";
        TempData["TipoMensaje"] = "error";
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
        TempData["Mensaje"] = "Rol eliminado correctamente.";
        TempData["TipoMensaje"] = "success";
        await _apiService.PostAsync($"{_endpoint}/del", request);
        return RedirectToAction(nameof(Index), "Roles");
    }

}
