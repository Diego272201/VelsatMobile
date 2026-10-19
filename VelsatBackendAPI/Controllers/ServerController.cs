using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using VelsatBackendAPI.Data.Repositories;
using VelsatMobile.Data.Repositories;

namespace VelsatBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly IReadOnlyUnitOfWork _readOnlyUow;

        public ServerController(IReadOnlyUnitOfWork readOnlyUow) // ✅ Cambiar
        {
            _readOnlyUow = readOnlyUow;
        }

        [HttpGet("MobileServer/{accountID}")]
        public async Task<IActionResult> GetServidor(string accountID)
        {
            var server = await _readOnlyUow.ServidorRepository.GetServidor(accountID);

            if (server == null)
            {
                return Ok(new { mensaje = "usuario incorrecto" });
            }

            return Ok(server);
        }
    }
}
