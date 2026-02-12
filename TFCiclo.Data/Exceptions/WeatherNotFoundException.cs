
namespace TFCiclo.Data.Exceptions
{
    public class WeatherNotFoundException : Exception
    {
        public string City { get; }
        public string Country { get; }

        public WeatherNotFoundException(string city, string country)
            : base()
        {
            City = city;
            Country = country;
        }
    }
}
