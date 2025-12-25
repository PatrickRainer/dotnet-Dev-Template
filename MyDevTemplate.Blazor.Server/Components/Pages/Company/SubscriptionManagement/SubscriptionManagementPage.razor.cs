using System.Reflection;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyDevTemplate.Application.SubscriptionServices;
using MyDevTemplate.Domain.Entities.SubscriptionAggregate;

namespace MyDevTemplate.Blazor.Server.Components.Pages.Company.SubscriptionManagement;

public partial class SubscriptionManagementPage : ComponentBase
{
    [Inject] private ISubscriptionService SubscriptionService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<SubscriptionRoot> Subscriptions { get; set; } = new();
    private bool _loading = true;
    private bool _isDialogVisible;
    private SubscriptionModel Model { get; set; } = new();
    private SubscriptionModelValidator Validator { get; } = new();
    private MudForm _form = default!;

    private readonly DialogOptions _dialogOptions = new() { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };

    private readonly List<string> AllFeatures = typeof(SubscriptionFeatures)
        .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
        .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
        .Select(x => (string)x.GetRawConstantValue()!)
        .ToList();

    protected override async Task OnInitializedAsync()
    {
        await LoadSubscriptions();
    }

    private async Task LoadSubscriptions()
    {
        _loading = true;
        try
        {
            Subscriptions = await SubscriptionService.GetAllAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading subscriptions: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private void OpenCreateDialog()
    {
        Model = new SubscriptionModel();
        _isDialogVisible = true;
    }

    private void OpenEditDialog(SubscriptionRoot subscription)
    {
        Model = new SubscriptionModel
        {
            Id = subscription.Id,
            Name = subscription.Name,
            Description = subscription.Description,
            SelectedFeatures = subscription.Features.ToList()
        };
        _isDialogVisible = true;
    }

    private void CloseDialog()
    {
        _isDialogVisible = false;
    }

    private void OnFeatureToggled(string feature, bool isSelected)
    {
        if (isSelected)
        {
            if (!Model.SelectedFeatures.Contains(feature))
                Model.SelectedFeatures.Add(feature);
        }
        else
        {
            Model.SelectedFeatures.Remove(feature);
        }
    }

    private async Task Submit()
    {
        await _form.Validate();

        if (_form.IsValid)
        {
            try
            {
                if (Model.Id == Guid.Empty)
                {
                    var subscription = new SubscriptionRoot(Model.Name, Model.Description)
                    {
                        Features = Model.SelectedFeatures
                    };
                    await SubscriptionService.AddAsync(subscription);
                    Snackbar.Add("Subscription created successfully", Severity.Success);
                }
                else
                {
                    var subscription = await SubscriptionService.GetByIdAsync(Model.Id);
                    if (subscription != null)
                    {
                        subscription.Name = Model.Name;
                        subscription.Description = Model.Description;
                        subscription.Features = Model.SelectedFeatures;
                        await SubscriptionService.UpdateAsync(subscription);
                        Snackbar.Add("Subscription updated successfully", Severity.Success);
                    }
                }

                _isDialogVisible = false;
                await LoadSubscriptions();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error saving subscription: {ex.Message}", Severity.Error);
            }
        }
    }

    private async Task DeleteSubscription(SubscriptionRoot subscription)
    {
        bool? result = await DialogService.ShowMessageBox(
            "Delete Subscription",
            $"Are you sure you want to delete subscription '{subscription.Name}'?",
            yesText: "Delete!", cancelText: "Cancel");

        if (result == true)
        {
            try
            {
                await SubscriptionService.DeleteAsync(subscription.Id);
                Snackbar.Add("Subscription deleted successfully", Severity.Success);
                await LoadSubscriptions();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error deleting subscription: {ex.Message}", Severity.Error);
            }
        }
    }
}
