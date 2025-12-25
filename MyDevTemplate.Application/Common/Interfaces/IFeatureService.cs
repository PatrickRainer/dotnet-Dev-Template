namespace MyDevTemplate.Application.Common.Interfaces;

public interface IFeatureService
{
    Task<bool> HasFeatureAsync(string featureName);
}
