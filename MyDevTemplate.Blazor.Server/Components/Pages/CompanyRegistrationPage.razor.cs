using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyDevTemplate.Application.TenantServices;
using MyDevTemplate.Domain.Entities.TenantAggregate;

namespace MyDevTemplate.Blazor.Server.Components.Pages;

public partial class CompanyRegistrationPage : ComponentBase
{
    MudForm _form = null!;
    bool _isFormValid;
    string[] _errors = Array.Empty<string>();
    [Inject] public TenantService TenantService { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public IHttpContextAccessor HttpContextAccessor { get; set; } = null!;

    CompanyRegistrationPageModel Model { get; set; } = new();

    class CompanyRegistrationPageModel
    {
        public string CompanyName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }

    async Task Register(CompanyRegistrationPageModel model)
    {
        await _form.Validate();
        
        if (_isFormValid)
        {
            var loggedInUser = HttpContextAccessor.HttpContext.User.Identity?.Name;
            var newTenant = new TenantRoot(model.CompanyName, model.CompanyName, loggedInUser);
            newTenant.AddAddress(model.Street, model.City, model.ZipCode, model.Country, string.Empty);


            var result = await TenantService.AddTenantAsync(newTenant);
            
            if(result != Guid.Empty)
            {
                Snackbar.Add("Tenant created successfully", Severity.Success);
            }
        }
    }
}