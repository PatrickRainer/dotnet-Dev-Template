using FluentValidation;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyDevTemplate.Application.Common;
using MyDevTemplate.Application.Common.Validations;
using MyDevTemplate.Application.TenantServices;
using MyDevTemplate.Domain.Entities.TenantAggregate;
using Severity = MudBlazor.Severity;

namespace MyDevTemplate.Blazor.Server.Components.Pages.Company.CompanyRegistration;

public partial class CompanyRegistrationPage : ComponentBase
{
    MudForm _form = null!;
    bool _isFormValid;
    string[] _errors = Array.Empty<string>();
    
    
    [Inject] public TenantService TenantService { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public IHttpContextAccessor HttpContextAccessor { get; set; } = null!;
    [Inject] public CompanyRegistrationPageModelValidator ModelValidator { get; set; } = null!;


    CompanyRegistrationPageModel Model { get; set; } = new();


    async Task Register(CompanyRegistrationPageModel model)
    {
        await _form.Validate();
        if (_isFormValid)
        {
            try
            {
                var loggedInUser = HttpContextAccessor.HttpContext?.User.Identity?.Name;
                var newTenant = new TenantRoot(model.CompanyName, model.CompanyName,
                    loggedInUser ?? throw new UserIdentityException());
                newTenant.AddAddress(model.Street, model.City, model.ZipCode, model.Country, string.Empty);

                var result = await TenantService.AddTenantAsync(newTenant);

                if (result != Guid.Empty)
                {
                    Snackbar.Add("Tenant created successfully", Severity.Success);
                }
            }
            catch (ValidationException ex)
            {
                foreach (var error in ex.Errors)
                {
                    Snackbar.Add(error.ErrorMessage, Severity.Error);
                }
            }
            catch (Exception)
            {
                Snackbar.Add("An unexpected error occurred", Severity.Error);
            }
        }
    }
}