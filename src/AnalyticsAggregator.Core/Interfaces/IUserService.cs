namespace AnalyticsAggregator.Core.Interfaces;

public interface IUserService
{
    Task<string> RegisterAsync(string email, string password, string name);
    Task<string?> LoginAsync(string email, string password);
}

