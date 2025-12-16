using Dapper.Contrib.Extensions;
using TFCiclo.Data.ApiObjects;

namespace TFCiclo.Data.Models
{
    [Table("weather_forecast")]
    public class weather_forecast : ApiLogin
    {
        #region Métodos privados
        private int _id = 0;
        private string _city = string.Empty;
        private string _country = string.Empty;
        private DateTime _requested_at = DateTime.UtcNow;
        private string _response_json = string.Empty;
        private int _http_status = 0;
        #endregion

        #region Métodos públicos
        [Key]
        public int id { get => _id; set => _id = value; }
        public string city { get => _city; set => _city = value; }
        public string country { get => _country; set => _country = value; }
        public DateTime requested_at { get => _requested_at; set => _requested_at = value; }
        public string response_json { get => _response_json; set => _response_json = value; }
        public int http_status { get => _http_status; set => _http_status = value; }
        #endregion

        #region Constructores
        public weather_forecast() { }
        #endregion
    }
}
