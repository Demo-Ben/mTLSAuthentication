using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MtlsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecureController : ControllerBase
    {
        [HttpGet("ping")]
        //[Authorize]
        [Authorize(Policy = "ClientCertificate")]
        public IActionResult Ping()
        {
            // It is not necessary to check for the certificate here, because the authorization handler
            // will have already validated it.
            // However, if you want to inspect the certificate details, you can do so.
            var cert = HttpContext.Connection.ClientCertificate;

            if (cert == null)
                return Unauthorized("No client certificate received");

            return Ok(new
            {
                message = "mTLS Ping Successful!",
                subject = cert.Subject,
                thumbprint = cert.Thumbprint
            });

            //return Ok(new
            //{
            //    Message = "mTLS OK",
            //    User = User.Identity?.Name
            //});
        }

        [HttpGet("debug-cert")]
        public IActionResult DebugCert()
        {
            var cert = HttpContext.Connection.ClientCertificate;

            if (cert == null)
                return Ok("NO CLIENT CERT RECEIVED");

            return Ok(new
            {
                subject = cert.Subject,
                issuer = cert.Issuer,
                thumbprint = cert.Thumbprint,
                publickey =cert.GetPublicKeyString()
            });
        }
    }
}


//Your API must validate:
//    Certificate chain
//    Certificate thumbprint
//    Certificate subject
//    Enhanced Key Usage (EKU)