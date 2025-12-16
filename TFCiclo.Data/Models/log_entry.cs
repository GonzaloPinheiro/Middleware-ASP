using Dapper.Contrib.Extensions;

namespace TFCiclo.Data.Models
{
    [Table("log_entry")]
    public class log_entry
    {
        #region Métodos privados
        private int _id = 0;
        private DateTimeOffset _timeStamp = DateTimeOffset.MinValue;
        private string _level = string.Empty;
        private string _source = string.Empty;
        private string _operation = string.Empty;
        private string _message = string.Empty;
        private string _exception = string.Empty;
        private string _userId = string.Empty;
        private string _correlationId = string.Empty;
        private long _durationMs = 0;
        private string _metadataJson = string.Empty;
        #endregion

        #region Métodos públicos
        [Key]
        public int id { get => _id; set => _id = value; }                                      // PK (identity)
        public DateTimeOffset timeStamp { get => _timeStamp; set => _timeStamp = value; }      // momento del registro (UTC)
        public string level { get => _level; set => _level = value; }                          // Info, Warning, Error, Critical
        public string source { get => _source; set => _source = value; }                       // "TFCiclo.Api.Controllers.X"
        public string operation { get => _operation; set => _operation = value; }              // Nombre de la operación/método
        public string message { get => _message; set => _message = value; }                    // Mensaje principal
        public string exception { get => _exception; set => _exception = value; }              // StackTrace / excepción (nullable)
        public string userId { get => _userId; set => _userId = value; }                       // id del usuario si aplica (nullable)
        public string correlationId { get => _correlationId; set => _correlationId = value; }  // id para correlacionar peticiones (GUID)
        public long durationMs { get => _durationMs; set => _durationMs = value; }             // Duración en ms (nullable, si aplica)
        public string metadataJson { get => _metadataJson; set => _metadataJson = value; }     // JSON con datos extra (nullable)
        #endregion
    }
}
