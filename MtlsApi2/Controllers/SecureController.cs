using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MtlsApi2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecureController : ControllerBase
    {

        [HttpGet("ping")]
        //[Authorize]
        //[Authorize(Policy = "ClientCertificate")]
        public IActionResult Ping()
        {
            // ** Verify that the client certificate is present and valid

            //var cert = HttpContext.Connection.ClientCertificate;

            //if (cert == null)
            //    return Unauthorized("No client certificate received");

            return Ok(new
            {
                message = "mTLS Ping Successful!"
                //subject = cert.Subject,
                //thumbprint = cert.Thumbprint
            });
        }
    }
}
