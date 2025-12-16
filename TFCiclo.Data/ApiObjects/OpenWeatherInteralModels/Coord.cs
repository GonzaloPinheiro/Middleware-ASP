
namespace TFCiclo.Data.ApiObjects.OpenWeatherInteralModels
{
    public class Coord
    {
        private double _lon = 0.0f;
        private double _lat = 0.0f;

        public double lon { get => _lon; set => _lon = value; }
        public double lat { get => _lat; set => _lat = value; }
    }
}
