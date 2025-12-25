using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyDevTemplate.Application.SubscriptionServices;
using MyDevTemplate.Application.TenantServices;
using MyDevTemplate.Domain.Contracts.Abstractions;
using MyDevTemplate.Domain.Entities.SubscriptionAggregate;
using MyDevTemplate.Domain.Entities.TenantAggregate;

namespace MyDevTemplate.Blazor.Server.Components.Pages.Company.TenantManagement;

public partial class TenantManagementPage
{
    [Inject] private ITenantService TenantService { get; set; } = null!;
    [Inject] private ISubscriptionService SubscriptionService { get; set; } = null!;
    [Inject] private ITenantProvider TenantProvider { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    public List<TenantManagementModel> Tenants { get; set; } = new();
    public List<SubscriptionRoot> Subscriptions { get; set; } = new();
    public TenantManagementModel Model { get; set; } = new();
    public TenantManagementValidator Validator { get; set; } = new();
    public MudForm _form = null!;
    public bool _loading;
    public bool _isDialogVisible;
    public Guid? _masterTenantId;
    public readonly DialogOptions _dialogOptions = new() { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };

    protected override async Task OnInitializedAsync()
    {
        _masterTenantId = TenantProvider.GetMasterTenantId();
        await LoadTenants();
        await LoadSubscriptions();
    }

    public async Task LoadTenants()
    {
        _loading = true;
        try
        {
            var tenants = await TenantService.GetAllAsync();
            Tenants = tenants.Select(t => new TenantManagementModel
            {
                Id = t.Id,
                TenantName = t.TenantName,
                CompanyName = t.CompanyName,
                AdminEmail = t.AdminEmail,
                SubscriptionId = t.SubscriptionId,
                Street = t.Address.Street,
                City = t.Address.City,
                State = t.Address.State,
                ZipCode = t.Address.ZipCode,
                Country = t.Address.Country
            }).ToList();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading tenants: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    public async Task LoadSubscriptions()
    {
        try
        {
            Subscriptions = await SubscriptionService.GetAllAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading subscriptions: {ex.Message}", Severity.Error);
        }
    }

    public void OpenCreateDialog()
    {
        Model = new TenantManagementModel();
        _isDialogVisible = true;
    }

    public void OpenEditDialog(TenantManagementModel model)
    {
        Model = new TenantManagementModel
        {
            Id = model.Id,
            TenantName = model.TenantName,
            CompanyName = model.CompanyName,
            AdminEmail = model.AdminEmail,
            SubscriptionId = model.SubscriptionId,
            Street = model.Street,
            City = model.City,
            State = model.State,
            ZipCode = model.ZipCode,
            Country = model.Country
        };
        _isDialogVisible = true;
    }

    public void CloseDialog()
    {
        _isDialogVisible = false;
    }

    public async Task Submit()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        await SaveAsync();
    }

    public async Task SaveAsync()
    {
        try
        {
            if (Model.Id == Guid.Empty)
            {
                var tenant = new TenantRoot(Model.TenantName, Model.CompanyName, Model.AdminEmail, Model.SubscriptionId);
                tenant.AddAddress(Model.Street, Model.City, Model.State, Model.Country, Model.ZipCode);
                await TenantService.AddAsync(tenant);
                Snackbar.Add("Tenant created successfully", Severity.Success);
            }
            else
            {
                var tenant = await TenantService.GetByIdAsync(Model.Id);
                if (tenant != null)
                {
                    tenant.TenantName = Model.TenantName;
                    tenant.CompanyName = Model.CompanyName;
                    tenant.AdminEmail = Model.AdminEmail;
                    tenant.SubscriptionId = Model.SubscriptionId;
                    tenant.AddAddress(Model.Street, Model.City, Model.State, Model.Country, Model.ZipCode);
                    await TenantService.UpdateAsync(tenant);
                    Snackbar.Add("Tenant updated successfully", Severity.Success);
                }
            }
            _isDialogVisible = false;
            await LoadTenants();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error saving tenant: {ex.Message}", Severity.Error);
        }
    }

    public async Task DeleteTenant(TenantManagementModel model)
    {
        bool? result = await DialogService.ShowMessageBox(
            "Delete Tenant", 
            $"Are you sure you want to delete tenant {model.TenantName}?", 
            yesText:"Delete!", cancelText:"Cancel");

        if (result == true)
        {
            try
            {
                await TenantService.DeleteAsync(model.Id);
                Snackbar.Add("Tenant deleted successfully", Severity.Success);
                await LoadTenants();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error deleting tenant: {ex.Message}", Severity.Error);
            }
        }
    }
}
