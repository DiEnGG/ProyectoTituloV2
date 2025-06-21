using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

[Authorize(Roles = "Admin,Admin AutoReport")]
public class EmpresasController : Controller
{
    private readonly IApiService _apiService;
    private readonly string _endpoint = "api/empresas";

    public EmpresasController(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        var empresas = await _apiService.GetAsync<Empresa>(_endpoint);
        if (empresas == null || empresas.Count == 0)
        {
            //ModelState.AddModelError("", "No se encontraron empresas.");
            TempData["Mensaje"] = "No se encontraron empresas.";
            TempData["TipoMensaje"] = "error";
            return View(new List<Empresa>());
        }

        return View(empresas);
    }
    public async Task<IActionResult> Panel(int id)
    {
        var empresa = await _apiService.GetByIdAsync<Empresa>(_endpoint, id);
        if (empresa == null)
            return NotFound();

        return View(empresa); // Esto buscará Views/Empresas/Panel.cshtml
    }
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Empresa empresa)
    {
        if (ModelState.IsValid)
        {
            await _apiService.PostAsync(_endpoint, empresa);
            TempData["Mensaje"] = "Empresa creada correctamente.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }
        TempData["Mensaje"] = "Ocurrio un error!";
        TempData["TipoMensaje"] = "error";
        return View(empresa);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var empresa = await _apiService.GetByIdAsync<Empresa>(_endpoint, id);
        return View(empresa);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Empresa empresa)
    {
        if (ModelState.IsValid)
        {
            await _apiService.PutAsync(_endpoint, empresa);
            TempData["Mensaje"] = "Empresa modificada correctamente.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }
        TempData["Mensaje"] = "Ocurrio un error!";
        TempData["TipoMensaje"] = "error";
        return View(empresa);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var empresa = await _apiService.GetByIdAsync<Empresa>(_endpoint, id);
        return View(empresa);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int EmpresaId)
    {
        var request = new eEmpresaRequest
        {
            EmpresaId = EmpresaId,
            Activar = false // o true si lo quieres reactivar
        };
        TempData["Mensaje"] = "Empresa eliminada correctamente.";
        TempData["TipoMensaje"] = "success";
        await _apiService.PostAsync($"{_endpoint}/del", request); // "/del" es el endpoint que tienes en tu API
        return RedirectToAction(nameof(Index), "Empresas");
    }


}
