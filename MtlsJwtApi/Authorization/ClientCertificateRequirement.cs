using Microsoft.AspNetCore.Authorization;

namespace MtlsJwtApi.Authorization
{
    public class ClientCertificateRequirement : IAuthorizationRequirement
    {
        // No properties needed — this is just a marker requirement.
    }
}
