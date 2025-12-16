
namespace TFCiclo.Data.ApiObjects.OpenWeatherInteralModels
{
    public class Wind
    {
        private double _speed = 0.0f;
        private int _deg = 0;
        private double _gust = 0.0f;

        public double speed { get => _speed; set => _speed = value; }
        public int deg { get => _deg; set => _deg = value; }
        public double gust { get => _gust; set => _gust = value; }
    }
}
