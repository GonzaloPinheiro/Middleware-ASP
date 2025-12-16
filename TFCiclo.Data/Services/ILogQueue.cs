using TFCiclo.Data.Models;

namespace TFCiclo.Data.Services
{
    // Interfaz que expone la operación rápida de encolar logs
    public interface ILogQueue
    {
        // Encola un log de forma rápida; no espera a la persistencia.
        Task EnqueueAsync(log_entry entry);
    }
}
