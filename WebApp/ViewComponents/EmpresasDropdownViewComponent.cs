using Microsoft.AspNetCore.Mvc;
using WebApp.Models;      // Ajusta el namespace si es necesario


public class EmpresasDropdownViewComponent : ViewComponent
{
    private readonly IApiService _apiService;
    private readonly string _endpoint = "api/empresas";

    public EmpresasDropdownViewComponent(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var empresas = await _apiService.GetAsync<Empresa>(_endpoint) ?? new List<Empresa>();
        return View("Dropdown", empresas);
    }
}
