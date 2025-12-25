using FluentValidation;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyDevTemplate.Application.RegistrationServices;
using MyDevTemplate.Application.RegistrationServices.Dtos;
using MyDevTemplate.Application.SubscriptionServices;
using MyDevTemplate.Domain.Entities.SubscriptionAggregate;
using Severity = MudBlazor.Severity;

namespace MyDevTemplate.Blazor.Server.Components.Pages.Company.CompanyRegistration;

public partial class CompanyRegistrationPage : ComponentBase
{
    MudForm _form = null!;
    bool _isFormValid;
    string[] _errors = Array.Empty<string>();
    
    
    [Inject] public IRegistrationService RegistrationService { get; set; } = null!;
    [Inject] public ISubscriptionService SubscriptionService { get; set; } = null!;
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

                var registrationDto = new RegistrationDto(
                    model.CompanyName,
                    model.Street,
                    model.City,
                    model.ZipCode,
                    model.Country,
                    subscriptionId);

                var result = await RegistrationService.RegisterAsync(
                    registrationDto, 
                    loggedInUser ?? throw new UserIdentityException());

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