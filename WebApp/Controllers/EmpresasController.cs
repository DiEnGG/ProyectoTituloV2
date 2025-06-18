using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

[Authorize]
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
            ModelState.AddModelError("", "No se encontraron empresas.");
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
            return RedirectToAction(nameof(Index));
        }

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
            return RedirectToAction(nameof(Index));
        }

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

        await _apiService.PostAsync($"{_endpoint}/del", request); // "/del" es el endpoint que tienes en tu API
        return RedirectToAction(nameof(Index), "Empresas");
    }


}
