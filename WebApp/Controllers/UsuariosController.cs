using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;


public class UsuariosController : Controller
{
    private readonly IApiService _apiService;
    private readonly string _endpoint = "api/usuarios";

    public UsuariosController(IApiService apiService)
    {
        _apiService = apiService;

    }
    [Authorize(Roles = "Admin,Admin AutoReport")]
    public async Task<IActionResult> Index()
    {
        var usuarios = await _apiService.GetAsync<Usuario>(_endpoint);
        return View(usuarios);
    }

    [Authorize(Roles = "Admin,Admin AutoReport")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Empresas = await _apiService.GetAsync<Empresa>("api/empresas");
        ViewBag.Roles = await _apiService.GetAsync<Rol>("api/roles");
        return View();
    }
    [Authorize(Roles = "Admin,Admin AutoReport")]
    [HttpPost]
    public async Task<IActionResult> Create(Usuario usuario)
    {
        if (ModelState.IsValid)
        {
            await _apiService.PostAsync(_endpoint, usuario);
            TempData["Mensaje"] = "Usuario  creado correctamente.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }
        TempData["Mensaje"] = "Ocurrio un error!";
        TempData["TipoMensaje"] = "error";
        return View(usuario);
    }
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _apiService.GetByIdAsync<Usuario>(_endpoint, id);
        ViewBag.Empresas = await _apiService.GetAsync<Empresa>("api/empresas");
        ViewBag.Roles = await _apiService.GetAsync<Rol>("api/roles");
        return View(usuario);
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Edit( Usuario usuario)
    {
        ViewBag.Empresas = await _apiService.GetAsync<Empresa>("api/empresas");
        ViewBag.Roles = await _apiService.GetAsync<Rol>("api/roles");

        if (ModelState.IsValid)
        {
            await _apiService.PutAsync(_endpoint, usuario);
            TempData["Mensaje"] = "Usuario Modificado correctamente.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }
        TempData["Mensaje"] = "Ocurrio un error!";
        TempData["TipoMensaje"] = "error";
        return View(usuario);
    }
    [Authorize(Roles = "Admin,Admin AutoReport")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _apiService.GetByIdAsync<Usuario>(_endpoint, id);
        return View(usuario);
    }
    [Authorize(Roles = "Admin,Admin AutoReport")]
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int UsuarioId)
    {
        var request = new eUsuarioRequest
        {
            UsuarioId = UsuarioId,
            Activar = false // o true si lo quieres reactivar
        };
        TempData["Mensaje"] = "Usuario eliminado correctamente.";
        TempData["TipoMensaje"] = "success";
        await _apiService.PostAsync($"{_endpoint}/del", request); // "/del" es el endpoint que tienes en tu API
        return RedirectToAction(nameof(Index), "Usuarios");
    }
}
