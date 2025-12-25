namespace MyDevTemplate.Application.Common.Interfaces;

public interface IFeatureService
{
    Task<bool> HasFeatureAsync(string featureName);
    Task<List<string>> GetSubscribedFeaturesAsync();
    Task<bool> IsFeatureSubscribedAsync(string featureName);
}
