using VelsatBackendAPI.Data.Repositories;
using VelsatMobile.Data.Repositories;
using VelsatMobile.Model.NotificacionesMobile;
using VelsatMobile.Services.FirebaseService;

namespace VelsatMobile.Jobs
{
    public class DocumentosNotificacionJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<DocumentosNotificacionJob> _logger;

        // Horas de notificación (hora peruana)
        private readonly TimeSpan[] _horariosNotificacion = new[]
        {
            new TimeSpan(7, 0, 0),   // 6:00 AM
            new TimeSpan(18, 0, 0)   // 6:00 PM
        };

        public DocumentosNotificacionJob(IServiceScopeFactory scopeFactory,
                                         IFirebaseService firebaseService,
                                         ILogger<DocumentosNotificacionJob> logger)
        {
            _scopeFactory = scopeFactory;
            _firebaseService = firebaseService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DocumentosNotificacionJob iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Hora actual peruana
                var ahora = DateTime.UtcNow.AddHours(-5);
                var proximaEjecucion = CalcularProximaEjecucion(ahora);
                var espera = proximaEjecucion - ahora;

                _logger.LogInformation($"Próxima notificación: {proximaEjecucion:dd/MM/yyyy HH:mm}");

                await Task.Delay(espera, stoppingToken);

                await EnviarNotificacionesAsync();
            }
        }

        private DateTime CalcularProximaEjecucion(DateTime ahora)
        {
            // Buscar el próximo horario del día actual
            foreach (var horario in _horariosNotificacion.OrderBy(h => h))
            {
                var candidato = ahora.Date.Add(horario);
                if (candidato > ahora)
                    return candidato;
            }

            // Si ya pasaron todos los horarios de hoy → mañana al primero
            return ahora.Date.AddDays(1).Add(_horariosNotificacion.Min());
        }

        private async Task EnviarNotificacionesAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var readOnlyUow = scope.ServiceProvider.GetRequiredService<IReadOnlyUnitOfWork>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // 1. Obtener todos los usuarios con FCMToken registrado
                var usuarios = await uow.AplicativoRepository.GetAllUserFCMTokensAsync();

                _logger.LogInformation($"Procesando {usuarios.Count()} usuarios...");

                foreach (var usuario in usuarios)
                {
                    try
                    {
                        // 2. Consultar documentos por vencer del usuario
                        // Reutilizando tu repositorio existente
                        var documentos = await readOnlyUow.AplicativoRepository.DocumentosPorVencer(usuario.UserId);

                        if (documentos == null || !documentos.Any())
                            continue;

                        // 3. Agrupar por estado para armar mensaje
                        var vencidos = documentos.Count(d => d.Estado == "Vencido");
                        var porVencer = documentos.Count(d => d.Estado == "Por Vencer");
                        var proximoVencer = documentos.Count(d => d.Estado == "Próximo a Vencer");

                        string titulo = "Documentos - Velsat";
                        string mensaje = ArmarMensaje(documentos);

                        // 4. Enviar push notification
                        bool enviado = await _firebaseService.SendPushNotificationAsync(
                            usuario.FCMToken,
                            titulo,
                            mensaje
                        );

                        if (enviado)
                            _logger.LogInformation($"Notificación enviada a {usuario.UserId}");
                        else
                            _logger.LogWarning($"Fallo notificación para {usuario.UserId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error procesando usuario {usuario.UserId}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error general en job: {ex.Message}");
            }
        }

        private string ArmarMensaje(IEnumerable<Documento> documentos)
        {
            var lista = documentos.ToList();
            int total = lista.Count;

            // Solo 1 documento → mensaje personalizado con el nombre
            if (total == 1)
            {
                var doc = lista.First();
                return doc.Estado switch
                {
                    "Vencido" => $"Tu {doc.Nombre_documento} está vencido. Actualízalo.",
                    "Por Vencer" => $"Tu {doc.Nombre_documento} vence en menos de 15 días.",
                    "Próximo a Vencer" => $"Tu {doc.Nombre_documento} vence en menos de 30 días.",
                    _ => $"Revisa tu {doc.Nombre_documento}."
                };
            }

            // Más de 1 documento → mensaje agrupado como antes
            var vencidos = lista.Count(d => d.Estado == "Vencido");
            var porVencer = lista.Count(d => d.Estado == "Por Vencer");
            var proximoVencer = lista.Count(d => d.Estado == "Próximo a Vencer");

            var partes = new List<string>();
            if (vencidos > 0) partes.Add($"{vencidos} vencido(s)");
            if (porVencer > 0) partes.Add($"{porVencer} por vencer (≤15 días)");
            if (proximoVencer > 0) partes.Add($"{proximoVencer} próximo(s) a vencer (≤30 días)");

            return $"Tienes {total} documentos: {string.Join(", ", partes)}. Revísalos.";
        }
    }
}
