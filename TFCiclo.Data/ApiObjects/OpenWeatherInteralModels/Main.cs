
namespace TFCiclo.Data.ApiObjects.OpenWeatherInteralModels
{
    public class Main
    {
        private double _temp = 0.0f;
        private double _feels_like = 0.0f;
        private double _temp_min = 0.0f;
        private double _temp_max = 0.0f;
        private int _pressure = 0;
        private int _humidity = 0;
        private int _sea_level = 0;
        private int _grnd_level = 0;
        private double _temp_kf = 0.0f;

        public double temp { get => _temp; set => _temp = value; }
        public double feels_like { get => _feels_like; set => _feels_like = value; }
        public double temp_min { get => _temp_min; set => _temp_min = value; }
        public double temp_max { get => _temp_max; set => _temp_max = value; }
        public int pressure { get => _pressure; set => _pressure = value; }
        public int humidity { get => _humidity; set => _humidity = value; }
        public int sea_level { get => _sea_level; set => _sea_level = value; }
        public int grnd_level { get => _grnd_level; set => _grnd_level = value; }
        public double temp_kf { get => _temp_kf; set => _temp_kf = value; }
    }
}
