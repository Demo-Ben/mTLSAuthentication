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
                message = "pong",
                subject = cert.Subject,
                thumbprint = cert.Thumbprint
            });

            //return Ok(new
            //{
            //    Message = "mTLS OK",
            //    User = User.Identity?.Name
            //});
        }
    }
}
