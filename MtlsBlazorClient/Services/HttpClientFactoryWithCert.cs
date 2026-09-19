using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace MtlsBlazorClient.Services;

public static class HttpClientFactoryWithCert
{
    public static HttpClient Create()
    {
        // Load client certificate from file (dev)
        var cert = new X509Certificate2(
            "client.pfx",
            "ClientCertPassword!",
            X509KeyStorageFlags.MachineKeySet |
            X509KeyStorageFlags.Exportable);

        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(cert);
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator; // dev only

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:5001")
        };

        return client;
    }
}
