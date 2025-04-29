using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

[Authorize]
public class UsuariosController : Controller
{
    private readonly IApiService _apiService;
    private readonly string _endpoint = "api/usuarios";

    public UsuariosController(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = await _apiService.GetAsync<Usuario>(_endpoint);
        return View(usuarios);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Empresas = await _apiService.GetAsync<Empresa>("api/empresas");
        ViewBag.Roles = await _apiService.GetAsync<Rol>("api/roles");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Usuario usuario)
    {
        if (ModelState.IsValid)
        {
            await _apiService.PostAsync(_endpoint, usuario);
            return RedirectToAction(nameof(Index));
        }

        return View(usuario);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _apiService.GetByIdAsync<Usuario>(_endpoint, id);
        ViewBag.Empresas = await _apiService.GetAsync<Empresa>("api/empresas");
        ViewBag.Roles = await _apiService.GetAsync<Rol>("api/roles");
        return View(usuario);
    }

    [HttpPost]
    public async Task<IActionResult> Edit( Usuario usuario)
    {
        if (ModelState.IsValid)
        {
            await _apiService.PutAsync(_endpoint, usuario);
            return RedirectToAction(nameof(Index));
        }

        return View(usuario);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _apiService.GetByIdAsync<Usuario>(_endpoint, id);
        return View(usuario);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int UsuarioId)
    {
        var request = new eUsuarioRequest
        {
            UsuarioId = UsuarioId,
            Activar = false // o true si lo quieres reactivar
        };

        await _apiService.PostAsync($"{_endpoint}/del", request); // "/del" es el endpoint que tienes en tu API
        return RedirectToAction(nameof(Index), "Usuarios");
    }
}
