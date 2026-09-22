namespace MtlsBlazorClient.Services
{
    public interface IJwtTokenService
    {
        Task<string> GetTokenAsync();
    }
}
