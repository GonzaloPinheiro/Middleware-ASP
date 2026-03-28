
namespace TFCiclo.Domain.Exceptions
{
    /// <summary>
    /// Se lanza cuando ocurre un error de infraestructura en la base de datos
    /// (timeout, conexión perdida, deadlock, etc.) que no es recuperable
    /// </summary>
    public class DatabaseOperationException : Exception
    {
        /// <summary>
        /// Operación que se estaba ejecutando cuando ocurrió el error
        /// </summary>
        public string Operation { get; }

        public DatabaseOperationException(string operation)
            : base($"Error de base de datos durante la operación: {operation}")
                {
                    Operation = operation;
                }

        public DatabaseOperationException(string operation, Exception innerException)
            : base($"Error de base de datos durante la operación: {operation}", innerException)
        {
            Operation = operation;
        }
    }
}
