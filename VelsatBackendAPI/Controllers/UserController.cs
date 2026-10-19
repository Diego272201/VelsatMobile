using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model;
using VelsatMobile.Data.Repositories;

namespace VelsatBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IReadOnlyUnitOfWork _readOnlyUow; // ✅ Cambiar a ReadOnly
        private readonly IUnitOfWork _uow;

        public UserController(IReadOnlyUnitOfWork readOnlyUow, IUnitOfWork uow) // ✅ Cambiar
        {
            _readOnlyUow = readOnlyUow;
            _uow = uow;
        }

        [HttpGet("MobileDetailsUser")]
        public async Task<IActionResult> GetDetails([FromQuery] string accountID, [FromQuery] char tipo)
        {
            if (string.IsNullOrEmpty(accountID))
            {
                return BadRequest("El accountID es requerido");
            }

            if (tipo != 'n' && tipo != 'p' && tipo != 'c')
            {
                return BadRequest("Tipo de usuario no válido. Use 'n', 'p' o 'c'");
            }

            try
            {
                var details = await _readOnlyUow.UserRepository.GetDetails(accountID, tipo);

                if (details == null)
                {
                    return NotFound(new { mensaje = "Usuario no encontrado" });
                }

                return Ok(details);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut("MobileUpdateUser")]
        public async Task<IActionResult> UpdateUser([FromQuery] char tipo, [FromBody] Account account)
        {
            if (account == null)
            {
                return BadRequest("Datos inválidos");
            }

            if (tipo != 'n' && tipo != 'p' && tipo != 'c')
            {
                return BadRequest("Tipo de usuario no válido");
            }

            try
            {
                var success = await _uow.UserRepository.UpdateUser(account, tipo);

                _uow.SaveChanges();

                if (success)
                {
                    return Ok(new { mensaje = "Usuario actualizado correctamente" });
                }

                return NotFound(new { mensaje = "Usuario no encontrado" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = "Error al actualizar usuario" });
            }
        }


        [HttpPut("MobileUpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromQuery] string username, [FromQuery] string password, [FromQuery] char tipo)
        {
            if (username == null || password == null)
            {
                return BadRequest("Datos inválidos");
            }

            if (tipo != 'n' && tipo != 'p' && tipo != 'c')
            {
                return BadRequest("Tipo de usuario no válido");
            }

            try
            {
                var success = await _uow.UserRepository.UpdatePassword(username, password, tipo);

                _uow.SaveChanges();

                if (success)
                {
                    return Ok(new { mensaje = "Contraseña actualizada correctamente" });
                }

                return NotFound(new { mensaje = "Usuario no encontrado" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = "Error al actualizar usuario" });
            }
        }
    }
}
