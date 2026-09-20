using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MtlsApi.Authorization;

public class ClientCertificateHandler : AuthorizationHandler<ClientCertificateRequirement>
{
    protected override Task HandleRequirementAsync( AuthorizationHandlerContext context,
                                                    ClientCertificateRequirement requirement)
    {
        // .NET 8: Resource is HttpContext directly
        var httpContext = context.Resource as HttpContext;

        if (httpContext == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        //if httpContext.Connection.ClientCertificate is null, that means the TLS handshake succeeded,
        //but no client certificate was actually sent to the server.
        var cert = httpContext.Connection.ClientCertificate;

        if (cert == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        Console.WriteLine("Client Cert Subject: " + cert.Subject);
        Console.WriteLine("Client Cert Thumbprint: " + cert.Thumbprint);

        if (cert.Subject.Contains("CN=MtlsClient"))
        {
            // context.Success();
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}



//using Microsoft.AspNetCore.Authorization;

//namespace MtlsApi.Authorization
//{
//    public class ClientCertificateHandler : AuthorizationHandler<ClientCertificateRequirement>
//    {
//        protected override Task HandleRequirementAsync(
//            AuthorizationHandlerContext context,
//            ClientCertificateRequirement requirement)
//        {
//            var httpContext = (context.Resource as DefaultHttpContext);
//            var cert = httpContext?.Connection.ClientCertificate;

//            if (cert == null)
//            {
//                context.Fail();
//                return Task.CompletedTask;
//            }

//            Console.WriteLine("Client Cert Subject: " + cert.Subject);
//            Console.WriteLine("Client Cert Thumbprint: " + cert.Thumbprint);

//            if (cert.Subject.Contains("CN=MtlsClient"))
//            {
//                context.Success();
//            }
//            else
//            {
//                context.Fail();
//            }

//            return Task.CompletedTask;
//        }
//    }

//}
