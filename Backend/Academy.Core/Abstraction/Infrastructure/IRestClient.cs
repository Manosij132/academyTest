namespace Academy.Core.Abstraction.Infrastructure
{
    public interface IRestClient
    {
        Task<Dictionary<string, object>> SendAsync(string endpoint, HttpMethod httpMethod, string token, HttpContent content = null);
    }
}
