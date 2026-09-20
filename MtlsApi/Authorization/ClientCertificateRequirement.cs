using Microsoft.AspNetCore.Authorization;

namespace MtlsApi.Authorization
{
    public class ClientCertificateRequirement : IAuthorizationRequirement
    {
        // No properties needed — this is just a marker requirement.
    }
}
