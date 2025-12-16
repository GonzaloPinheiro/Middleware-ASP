
namespace TFCiclo.Data.ApiObjects.OpenWeatherInteralModels
{
    public class Weather
    {
        private int _id = 0;
        private string _main = string.Empty;
        private string _description = string.Empty;
        private string _icon = string.Empty;

        public int id { get => _id; set => _id = value; }
        public string main { get => _main; set => _main = value; }
        public string description { get => _description; set => _description = value; }
        public string icon { get => _icon; set => _icon = value; }
    }
}
