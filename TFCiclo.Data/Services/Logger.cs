using TFCiclo.Data.Models;
using TFCiclo.Data.Repositories;

namespace TFCiclo.Data.Services
{
    public class Logger
    {
        //Variables y ojetos
        private readonly ILogQueue _logQueue;
        private readonly LogEntryRepository _logEntryRepository;

        #region Constructores
        public Logger(LogEntryRepository logEntryRepository, ILogQueue logQueue)
        {
            _logEntryRepository = logEntryRepository ?? throw new ArgumentNullException(nameof(logEntryRepository));
            _logQueue = logQueue ?? throw new ArgumentNullException(nameof(logQueue));
        }
        //public Logger()//Seguramente sobra
        //{
        //    _logEntryRepository = new LogEntryRepository("Server=localhost;Database=TFCiclo;User Id=root;Password=gonzalo;");
        //    LogBackgroundService backgroundService = new LogBackgroundService(_logEntryRepository);

        //    // Usamos el backgroundService como ILogQueue; OJO: no está registrado como HostedService aquí.
        //    // En un despliegue real debes registrar LogBackgroundService en DI como HostedService.
        //    _logQueue = backgroundService;

        //    // Iniciar el worker en segundo plano como tarea "fire-and-forget" para compatibilidad local.
        //    // En producción preferible usar IHost and AddHostedService.
        //    Task _ = backgroundService.StartAsync(CancellationToken.None);
        //}
        #endregion

        #region Métodos públicos


        /// <summary>
        /// Encola el log para persistencia asíncrona en background.
        /// </summary>
        /// <param name="entry">Entrada de log (POCO)</param>
        public async Task AddAsync(log_entry entry)
        {
            if (entry == null)
            {
                Console.WriteLine("En AddAsync(log_entry entry) se ha obtenido un log con estado null");
                return;
            }

            // Delegar al ILogQueue (rápido: encola y devuelve)
            await _logQueue.EnqueueAsync(entry).ConfigureAwait(false);
        }

        /// <summary>
        /// Comienza un scope de operación que registra entrada y, al disponer, registra salida con duración.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="operation"></param>
        /// <param name="correlationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OperationLogScope BeginScope(string source, string operation, string correlationId = null, string userId = null)
        {
            OperationLogScope scope = new OperationLogScope(this, source, operation, correlationId, userId);
            return scope;
        }
        #endregion
    }
}




/// <summary>
/// 
/// </summary>
/// <param name="entry"></param>
/// <returns></returns>
/// <exception cref="ArgumentNullException"></exception>
//public async Task AddAsync(log_entry entry)
//{
//    //Compruebo que el el log extista
//    if (entry != null)
//    {
//        //Guardo la hora
//        entry.timeStamp = DateTimeOffset.UtcNow;

//        // Si no se pasó correlationId, tomarlo del scope activo
//        entry.correlationId = string.IsNullOrWhiteSpace(entry.correlationId)
//            ? OperationLogScope.CurrentCorrelationId ?? Guid.NewGuid().ToString()
//            : entry.correlationId;

//        //Lanzo el add a la DB
//        _ = await _logEntryRepository.AddLogAsync(entry);
//    }
//    else
//    {
//        Console.WriteLine("En AddAsync(log_entry entry) se ha obtenido un log con estado null");
//    }
//}