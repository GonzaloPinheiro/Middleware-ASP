
namespace TFCiclo.Data.ApiObjects.OpenWeatherInteralModels
{
    public class ForecastItem
    {
        private long _dt = 0;
        private Main _main = new Main();
        private List<Weather> _weather = new List<Weather>();
        private Clouds _clouds = new Clouds();
        private Wind _wind = new Wind();
        private int _visibility = 0;
        private double _pop = 0.0f;
        private Rain _rain = new Rain();
        private Sys _sys = new Sys();
        private string _dt_txt = string.Empty;

        public long dt { get => _dt; set => _dt = value; }
        public Main main { get => _main; set => _main = value; }
        public List<Weather> weather { get => _weather; set => _weather = value; }
        public Clouds clouds { get => _clouds; set => _clouds = value; }
        public Wind wind { get => _wind; set => _wind = value; }
        public int visibility { get => _visibility; set => _visibility = value; }
        public double pop { get => _pop; set => _pop = value; }
        public Rain rain { get => _rain; set => _rain = value; }
        public Sys sys { get => _sys; set => _sys = value; }
        public string dt_txt { get => _dt_txt; set => _dt_txt = value; }
    }
}
