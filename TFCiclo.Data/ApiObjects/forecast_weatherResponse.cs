using System.Text.Json;
using TFCiclo.Data.ApiObjects.OpenWeatherInteralModels;

namespace TFCiclo.Data.ApiObjects
{
    public class forecast_weatherResponse
    {
        #region Métodos privados
        private string _cod = string.Empty;
        private int _message = 0;
        private int _cnt = 0;
        private List<ForecastItem> _list = new List<ForecastItem>();
        private City _city = new City();
        #endregion

        #region Métodos públicos
        public string cod { get => _cod; set => _cod = value; }
        public int message { get => _message; set => _message = value; }
        public int cnt { get => _cnt; set => _cnt = value; }
        public List<ForecastItem> list { get => _list; set => _list = value; }
        public City city { get => _city; set => _city = value; }
        #endregion

        #region Overrides
        //Override de ToString para devolver JSON
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
        #endregion
    }
}