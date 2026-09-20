namespace MtlsBlazorClient
{
    public class DebugHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Console.WriteLine("DebugHandler invoked");

            return await base.SendAsync(request, token);
        }
    }

}
