using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MtlsWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecureController : ControllerBase
    {
        [HttpGet("data")]
        public async Task<IActionResult> GetData()
        {
            var clientCert = await HttpContext.Connection.GetClientCertificateAsync();
            if (clientCert == null)
            {
                return Unauthorized("Client certificate missing.");
            }
            return Ok($"Hello from secure API! Authenticated via certificate: {clientCert.Subject}");
        }
    }
}
