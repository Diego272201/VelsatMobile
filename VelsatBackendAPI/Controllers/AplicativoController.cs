using Microsoft.AspNetCore.Mvc;
using VelsatBackendAPI.Data.Repositories;

namespace VelsatMobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AplicativoController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AplicativoController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

                var notifications = await _unitOfWork.AplicativoRepository.GetNotifications(accountID);

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

        [HttpGet("kilometers/{deviceID}")]
        public async Task<IActionResult> GetKilometersDay(string deviceID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(deviceID))
                {
                    return BadRequest("El deviceID es requerido");
                }

                var kilometers = await _unitOfWork.AplicativoRepository.GetKilometersDay(deviceID);

                return Ok(new
                {
                    kilometersToday = kilometers,
                });
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

                var notifications = await _unitOfWork.AplicativoRepository.GetNotifDevice(accountID, deviceID);

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

                var datosVehiculo = await _unitOfWork.DatosCargainicialService.ObtenerDatosVehiculoAsync(login, placa);

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

        [HttpGet("serviciosPasajero/{codcliente}")]
        public async Task<IActionResult> ServiciosPasajeros(string codcliente)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codcliente))
                {
                    return BadRequest("El código de cliente es requerido");
                }

                var servicios = await _unitOfWork.AplicativoRepository.ServiciosPasajeros(codcliente);

                if (servicios == null || !servicios.Any())
                {
                    return NotFound("No se encontraron servicios para el día de hoy");
                }

                return Ok(servicios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("detalleDestino/{codcliente}")]
        public async Task<IActionResult> GetDetalleDestino(string codcliente)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codcliente))
                {
                    return BadRequest("El código de cliente es requerido");
                }

                var destinos = await _unitOfWork.AplicativoRepository.GetDetalleDestino(codcliente);

                if (destinos == null || !destinos.Any())
                {
                    return NotFound("No se encontraron destinos activos para el cliente");
                }

                return Ok(destinos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("detalleConductor/{codtaxi}")]
        public async Task<IActionResult> GetDetalleConductor(string codtaxi)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codtaxi))
                {
                    return BadRequest("El código de taxi es requerido");
                }

                var conductor = await _unitOfWork.AplicativoRepository.GetDetalleConductor(codtaxi);

                if (conductor == null)
                {
                    return NotFound("No se encontró información del conductor");
                }

                return Ok(conductor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
