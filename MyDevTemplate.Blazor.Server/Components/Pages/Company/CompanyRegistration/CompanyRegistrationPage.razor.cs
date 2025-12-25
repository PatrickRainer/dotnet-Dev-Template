using FluentValidation;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyDevTemplate.Application.Common;
using MyDevTemplate.Application.Common.Validations;
using MyDevTemplate.Application.SubscriptionServices;
using MyDevTemplate.Application.TenantServices;
using MyDevTemplate.Domain.Entities.SubscriptionAggregate;
using MyDevTemplate.Domain.Entities.TenantAggregate;
using Severity = MudBlazor.Severity;

namespace MyDevTemplate.Blazor.Server.Components.Pages.Company.CompanyRegistration;

public partial class CompanyRegistrationPage : ComponentBase
{
    MudForm _form = null!;
    bool _isFormValid;
    string[] _errors = Array.Empty<string>();
    
    
    [Inject] public TenantService TenantService { get; set; } = null!;
    [Inject] public SubscriptionService SubscriptionService { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public IHttpContextAccessor HttpContextAccessor { get; set; } = null!;
    [Inject] public CompanyRegistrationPageModelValidator ModelValidator { get; set; } = null!;

    [Parameter] public string? SubscriptionId { get; set; }


    CompanyRegistrationPageModel Model { get; set; } = new();
    SubscriptionRoot? SelectedSubscription { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(SubscriptionId) && Guid.TryParse(SubscriptionId, out var id))
        {
            SelectedSubscription = await SubscriptionService.GetByIdAsync(id);
            if (SelectedSubscription != null)
            {
                Model.ChosenSubscription = SelectedSubscription.Name;
            }
        }
    }


    async Task Register(CompanyRegistrationPageModel model)
    {
        await _form.Validate();
        if (_isFormValid)
        {
            try
            {
                var loggedInUser = HttpContextAccessor.HttpContext?.User.Identity?.Name;
                var subscriptionId = SelectedSubscription?.Id;
                
                var newTenant = new TenantRoot(model.CompanyName, model.CompanyName,
                    loggedInUser ?? throw new UserIdentityException(), subscriptionId);
                newTenant.AddAddress(model.Street, model.City, model.ZipCode, model.Country, string.Empty);

                var result = await TenantService.AddAsync(newTenant);

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
            catch (Exception e)
            {
                Snackbar.Add("An unexpected error occurred", Severity.Error);
            }
        }
    }
}