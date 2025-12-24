namespace MyDevTemplate.Blazor.Server.Components.Pages;

internal class UserIdentityException : Exception
{
    public UserIdentityException() : base("Logged in user not found")
    {
    }

    public UserIdentityException(string message) : base(message)
    {
    }

    public UserIdentityException(string message, Exception innerException) : base(message, innerException)
    {
    }
}