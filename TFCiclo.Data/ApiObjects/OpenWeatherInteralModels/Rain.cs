using Newtonsoft.Json;

namespace TFCiclo.Data.ApiObjects.OpenWeatherInteralModels
{
    public class Rain
    {
        private double _3h = 0.0f;

        [JsonProperty("3h")]
        public double threeHours { get => _3h; set => _3h = value; } 
    }
}

