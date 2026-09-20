using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MtlsApi.Authorization;

public class ClientCertificateHandler 
            : AuthorizationHandler<ClientCertificateRequirement>
{
    // HandleRequirementAsync is triggered automatically by ASP.NET Core at a very specific moment in the request pipeline
    // — not when the client sends the certificate, not during TLS, not when the controller starts,
    // but exactly when the authorization middleware evaluates your policy.

    //It runs only when all three conditions are true:
    //      The request reached your API endpoint
    //      The endpoint has[Authorize(Policy = "ClientCertificate")]
    //      ASP.NET Core’s authorization middleware is evaluating that policy


    //What causes the handler to run?
    //      The request reached your API endpoint: [Authorize(Policy = "ClientCertificate")]
    //      Authorization middleware is active: app.UseAuthorization();
    //      Your policy is registered: 
    //              options.AddPolicy("ClientCertificate", policy =>
    //              {
    //                  policy.Requirements.Add(new ClientCertificateRequirement());
    //              });
    //      Your handler is registered: builder.Services.AddSingleton<IAuthorizationHandler, ClientCertificateHandler>();
    protected override Task HandleRequirementAsync( AuthorizationHandlerContext context,
                                                    ClientCertificateRequirement requirement)
    {
        // .NET 8: Resource is HttpContext directly
        var httpContext = context.Resource as HttpContext;

        Console.WriteLine("🔵 Handler triggered");   // ← Log #1: handler is running

        if (httpContext == null)
        {
            Console.WriteLine("❌ HttpContext is null");   // ← Log #2
            context.Fail();
            return Task.CompletedTask;
        }

        //if httpContext.Connection.ClientCertificate is null, that means the TLS handshake succeeded,
        //but no client certificate was actually sent to the server.
        var cert = httpContext.Connection.ClientCertificate;

        if (cert == null)
        {
            Console.WriteLine("❌ No client certificate received");  // ← Log #3
            context.Fail();
            return Task.CompletedTask;
        }

        // Log certificate details
        Console.WriteLine("🔐 Certificate Subject: " + cert.Subject);       // ← Log #4
        Console.WriteLine("🔐 Certificate Thumbprint: " + cert.Thumbprint); // ← Log #5
        Console.WriteLine("🔐 Certificate Issuer: " + cert.Issuer);         // ← Log #6

        if (cert.Subject.Contains("CN=MtlsClient"))
        {
            // context.Success();
            Console.WriteLine("✅ Certificate accepted");   // ← Log #7
            context.Succeed(requirement);
        }
        else
        {
            Console.WriteLine("❌ Certificate rejected");   // ← Log #8
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
