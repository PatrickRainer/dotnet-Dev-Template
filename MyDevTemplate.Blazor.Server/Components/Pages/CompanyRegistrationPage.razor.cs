using FluentValidation;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyDevTemplate.Application.Common;
using MyDevTemplate.Application.Common.Validations;
using MyDevTemplate.Application.TenantServices;
using MyDevTemplate.Domain.Entities.TenantAggregate;
using Severity = MudBlazor.Severity;

namespace MyDevTemplate.Blazor.Server.Components.Pages;

public partial class CompanyRegistrationPage : ComponentBase
{
    MudForm _form = null!;
    bool _isFormValid;
    readonly CompanyRegistrationPageModelValidator _companyRegistrationPageModelValidator = new();
    string[] _errors = Array.Empty<string>();
    
    
    [Inject] public TenantService TenantService { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public IHttpContextAccessor HttpContextAccessor { get; set; } = null!;


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

    class CompanyRegistrationPageModel
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    class CompanyRegistrationPageModelValidator : AbstractValidator<CompanyRegistrationPageModel>
    {
        public CompanyRegistrationPageModelValidator()
        {
            RuleFor(x => x.CompanyName).ApplyCompanyNameRules();
            RuleFor(x => x.Street).ApplyStreetRules();
            RuleFor(x => x.City).ApplyCityRules();
            RuleFor(x => x.ZipCode).ApplyZipCodeRules();
            RuleFor(x => x.Country).ApplyCountryRules();
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result =
                await ValidateAsync(ValidationContext<CompanyRegistrationPageModel>.CreateWithOptions(
                    (CompanyRegistrationPageModel)model,
                    x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}