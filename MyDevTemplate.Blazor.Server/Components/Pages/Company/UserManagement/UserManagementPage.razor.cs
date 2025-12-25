using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyDevTemplate.Application.UserServices;
using MyDevTemplate.Domain.Entities.Common;
using MyDevTemplate.Domain.Entities.UserAggregate;

namespace MyDevTemplate.Blazor.Server.Components.Pages.Company.UserManagement;

public partial class UserManagementPage
{
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    public List<UserManagementModel> Users { get; set; } = new();
    public UserManagementModel Model { get; set; } = new();
    public UserManagementValidator Validator { get; set; } = new();
    public MudForm _form = null!;
    public bool _loading;
    public bool _isDialogVisible;
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };

    protected override async Task OnInitializedAsync()
    {
        await LoadUsers();
    }

    protected async Task LoadUsers()
    {
        _loading = true;
        try
        {
            var users = await UserService.GetAllAsync();
            Users = users.Select(u => new UserManagementModel
            {
                Id = u.Id,
                Email = u.Email.Value,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IdentityProviderId = u.IdentityProviderId
            }).ToList();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading users: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    protected void OpenCreateDialog()
    {
        Model = new UserManagementModel();
        _isDialogVisible = true;
    }

    protected void OpenEditDialog(UserManagementModel model)
    {
        Model = new UserManagementModel
        {
            Id = model.Id,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            IdentityProviderId = model.IdentityProviderId
        };
        _isDialogVisible = true;
    }

    protected void CloseDialog()
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
                var user = new UserRoot(new EmailAddress(Model.Email), Model.FirstName, Model.LastName, Model.IdentityProviderId);
                await UserService.AddAsync(user);
                Snackbar.Add("User created successfully", Severity.Success);
            }
            else
            {
                var user = await UserService.GetByIdAsync(Model.Id);
                if (user != null)
                {
                    user.Email = new EmailAddress(Model.Email);
                    user.FirstName = Model.FirstName;
                    user.LastName = Model.LastName;
                    user.IdentityProviderId = Model.IdentityProviderId;
                    await UserService.UpdateAsync(user);
                    Snackbar.Add("User updated successfully", Severity.Success);
                }
            }
            _isDialogVisible = false;
            await LoadUsers();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error saving user: {ex.Message}", Severity.Error);
        }
    }

    protected async Task DeleteUser(UserManagementModel model)
    {
        bool? result = await DialogService.ShowMessageBox(
            "Delete User", 
            $"Are you sure you want to delete user {model.Email}?", 
            yesText:"Delete!", cancelText:"Cancel");

        if (result == true)
        {
            try
            {
                await UserService.DeleteAsync(model.Id);
                Snackbar.Add("User deleted successfully", Severity.Success);
                await LoadUsers();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error deleting user: {ex.Message}", Severity.Error);
            }
        }
    }
}
