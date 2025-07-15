namespace ECommerce.Gateway.Handlers;

using System.Net.Http.Headers;

public class TokenBlacklistHandler : DelegatingHandler
{
    private readonly HttpClient _httpClient;

    public TokenBlacklistHandler(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization != null && 
            request.Headers.Authorization.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            var token = request.Headers.Authorization.Parameter;

            var response = await _httpClient.GetAsync($"/api/auth/is-blacklisted?token={token}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Blacklist service error")
                };
            }

            var isBlacklisted = bool.Parse(await response.Content.ReadAsStringAsync());

            if (isBlacklisted)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Token is blacklisted.")
                };
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
