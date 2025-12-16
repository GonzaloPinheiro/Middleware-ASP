using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TFCiclo.Connector;

namespace TimerModule
{
    public class TimedHostedService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private int _intervaloSegundos;
        private readonly IServiceProvider _serviceProvider;

        public TimedHostedService(ILogger<TimedHostedService> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            // Leer valor del intervalo desde la configuración
            string? valor = _configuration["TimerSettings:IntervaloSegundos"];
            _intervaloSegundos = int.TryParse(valor, out int result) ? result : 3600;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //Crear un nuevo scope para los servicios para eviar usar servicios singleton(WeatherRepository)
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    // Obtener instancia de TimerConnector
                    TimerConnector timerConnector = scope.ServiceProvider.GetRequiredService<TimerConnector>();
                    //Obtiene los datos de OpenWeather y los guarda en la DB
                    await timerConnector.SaveOpenWeatherData("Timer");
                }

                //Esperar el intervalo
                await Task.Delay(TimeSpan.FromSeconds(_intervaloSegundos), stoppingToken);
            }
        }
    }
}
