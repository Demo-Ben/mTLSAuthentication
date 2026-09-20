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
                thumbprint = cert.Thumbprint,
                issuer = cert.Issuer
            });
        }
    }
}
