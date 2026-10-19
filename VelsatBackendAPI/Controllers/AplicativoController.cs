using Microsoft.AspNetCore.Mvc;
using VelsatBackendAPI.Data.Repositories;
using VelsatMobile.Data.Repositories;
using VelsatMobile.Model;
using VelsatMobile.Model.NotificacionesMobile;
using VelsatMobile.Services.FirebaseService;

namespace VelsatMobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AplicativoController : Controller
    {
        private readonly IReadOnlyUnitOfWork _readOnlyUow;  // ✅ Para GET
        private readonly IUnitOfWork _uow;

        public AplicativoController(IReadOnlyUnitOfWork readOnlyUow, IUnitOfWork uow)
        {
            _readOnlyUow = readOnlyUow;
            _uow = uow;
        }

        [HttpGet("unidades/{accountID}")]
        public async Task<IActionResult> GetUnidadesUsuario(string accountID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountID))
                {
                    return BadRequest("El accountID es requerido");
                }

                var unidades = await _readOnlyUow.AplicativoRepository.GetUnidadesUsuario(accountID);

                if (unidades == null || !unidades.Any())
                {
                    return NotFound("No se encontraron unidades para el día de hoy");
                }

                return Ok(unidades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("notifications/{accountID}")]
        public async Task<IActionResult> GetNotifications(string accountID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountID))
                {
                    return BadRequest("El accountID es requerido");
                }

                var notifications = await _readOnlyUow.AplicativoRepository.GetNotifications(accountID);

                if (notifications == null || !notifications.Any())
                {
                    return NotFound("No se encontraron notificaciones para el día de hoy");
                }

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("notifications/{accountID}/{deviceID}")]
        public async Task<IActionResult> GetNotifDevice(string accountID, string deviceID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountID) || string.IsNullOrWhiteSpace(deviceID))
                {
                    return BadRequest("El accountID y deviceID es requerido");
                }

                var notifications = await _readOnlyUow.AplicativoRepository.GetNotifDevice(accountID, deviceID);

                if (notifications == null || !notifications.Any())
                {
                    return NotFound("No se encontraron notificaciones para el día de hoy");
                }

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("vehiculo/{login}/{placa}")]
        public async Task<IActionResult> ObtenerDatosVehiculo(string login, string placa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(placa))
                {
                    return BadRequest("El login y la placa son requeridos");
                }

                var datosVehiculo = await _readOnlyUow.DatosCargainicialService.ObtenerDatosVehiculoAsync(login, placa);

                if (datosVehiculo.Vehiculo == null)
                {
                    return NotFound("No se encontró el vehículo o no pertenece al usuario");
                }

                return Ok(datosVehiculo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //NOTIFICACIONES
        [HttpPost("save-token")]
        public async Task<IActionResult> SaveToken([FromBody] UserFCMToken request)
        {
            try
            {
                // Validar entrada
                if (request == null)
                    return BadRequest("Los datos son requeridos.");

                if (string.IsNullOrWhiteSpace(request.UserId))
                    return BadRequest("El campo userId es obligatorio.");

                if (string.IsNullOrWhiteSpace(request.FCMToken))
                    return BadRequest("El campo fcmtoken es obligatorio.");

                if (string.IsNullOrWhiteSpace(request.Platform) ||
                    (request.Platform != "android" && request.Platform != "ios"))
                    return BadRequest("El campo Platform debe ser 'android' o 'ios'.");

                // Guardar o actualizar token
                var resultado = await _uow.AplicativoRepository.SaveFCMTokenAsync(request.UserId, request.FCMToken, request.Platform);

                _uow.SaveChanges();

                if (!resultado)
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "No se pudo guardar el token."
                    });

                return Ok(new
                {
                    success = true,
                    message = "Token FCM guardado correctamente."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }

        [HttpPost("ActualizarDeviceServicio")]
        public async Task<IActionResult> ActualizarDeviceServicio([FromQuery] string codservicio, [FromQuery] string deviceID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codservicio) || string.IsNullOrWhiteSpace(deviceID))
                    return BadRequest("El codservicio y el deviceID son requeridos");

                int filasAfectadas = await _uow.AplicativoRepository.ActualizarDeviceServicio(deviceID, codservicio);
                _uow.SaveChanges();

                if (filasAfectadas > 0)
                {
                    return Ok(new
                    {
                        message = "Device actualizado correctamente.",
                        filasActualizadas = filasAfectadas
                    });
                }
                else
                {
                    return Ok("Device no actualizado.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("ActualizarDeviceFinServicio")]
        public async Task<IActionResult> ActualizarDeviceFinServicio([FromQuery] string deviceID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(deviceID))
                    return BadRequest("El deviceID es requerido");

                int filasAfectadas = await _uow.AplicativoRepository.ActualizarDeviceFinServicio(deviceID);
                _uow.SaveChanges();

                if (filasAfectadas > 0)
                {
                    return Ok(new
                    {
                        message = "Device actualizado correctamente.",
                        filasActualizadas = filasAfectadas
                    });
                }
                else
                {
                    return Ok("Device no actualizado.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
