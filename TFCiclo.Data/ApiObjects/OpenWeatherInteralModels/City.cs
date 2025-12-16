
namespace TFCiclo.Data.ApiObjects.OpenWeatherInteralModels
{
    public class City
    {
        private long _id = 0;
        private string _name = string.Empty;
        private Coord _coord = new Coord();
        private string _country = string.Empty;
        private int _population = 0;
        private int _timezone = 0;
        private long _sunrise = 0;
        private long _sunset = 0;

        public long id { get => _id; set => _id = value; }
        public string name { get => _name; set => _name = value; }
        public Coord coord { get => _coord; set => _coord = value; }
        public string country { get => _country; set => _country = value; }
        public int population { get => _population; set => _population = value; }
        public int timezone { get => _timezone; set => _timezone = value; }
        public long sunrise { get => _sunrise; set => _sunrise = value; }
        public long sunset { get => _sunset; set => _sunset = value; }
    }
}
