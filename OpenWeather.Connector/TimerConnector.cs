using MySql.Data.MySqlClient;
using TFCiclo.Data.ApiObjects;
using TFCiclo.Data.Models;
using TFCiclo.Data.Repositories;
using TFCiclo.Data.Services;

namespace TFCiclo.Connector
{
    public class TimerConnector
    {
        //Variables de entorno
        static readonly string apiKey = Environment.GetEnvironmentVariable("ApiKeys__OpenWeather")!;

        //Variables y objetos
        private readonly Logger _logger;
        private readonly WeatherRepository _weatherRepository;

        #region Constructores
        public TimerConnector(Logger logger, WeatherRepository weatherRepository)
        {
            _logger = logger;
            _weatherRepository = weatherRepository;
        }
        #endregion

        #region Métodos Públicos
        /// <summary>
        /// Método que se encarga de guardar en DB los datos obtenidos de la api OpenWeather (false si una de las hubiacaciones no lo lee de la api)
        /// </summary>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public async Task SaveOpenWeatherData(string correlationId)
        {
            await using OperationLogScope scope = _logger.BeginScope(
                source: "TFCiclo.Connector",
                operation: "SaveOpenWeatherData(string correlationId)",
                correlationId: correlationId);

            //Varaibles y objetos
            //string cadenaConexion = "Server=localhost;Database=TFCiclo;User Id=root;Password=gonzalo;"; //TODO eliminar
            bool resultadoOpercion = false;

            GetWeatherFromApi getWeatherFromApi = new GetWeatherFromApi(apiKey, _logger, "Timer");
            //WeatherRepository weatherRepository = new WeatherRepository(connectionString, _logger);

            List<weather_forecast> weather_placesList = new List<weather_forecast>();
            forecast_weatherResponse? weatherForecastResponse = null;


            //Obtener lista de ciudades almacenadas en db
            weather_placesList = await _weatherRepository.GetListOfPlacesAsync();

            //Recorro la lista de ciudades de las cuales hace falta pedir el tiempo
            foreach (var weather_places in weather_placesList)
            {
                try
                {
                    //Consigo el json de la api openWeather
                    weatherForecastResponse = await getWeatherFromApi.GetForecastJsonAsync(weather_places.city, weather_places.country);

                    //Compruebo si se ha obtenido el json de la api
                    if (weatherForecastResponse == null)
                        throw new Exception($"Error al obtener el tiempo de la api: City:{weather_places.city}, Country: {weather_places.country}");

                    //Guardo el json en la Db
                    resultadoOpercion = await _weatherRepository.UpdateWeatherJsonAsync(weatherForecastResponse);

                    //Compruebo que se haya insertado correctamente
                    if (resultadoOpercion == false)
                        throw new Exception($"Error al actualizar tiempo en DB. Objeto: {weatherForecastResponse.ToString()}");

                }
                catch(MySqlException ex) //Error mysql
                {
                    await _logger.AddAsync(new log_entry
                    {
                        level = "Error",
                        source = "TFCiclo.Connector",
                        operation = "SaveOpenWeatherData(string correlationId)",
                        message = "Error al operar con la base de datos",
                        correlationId = correlationId,
                        exception = ex.ToString()
                    });
                }
                catch (Exception ex) //Error inesperado
                {
                    await _logger.AddAsync(new log_entry
                    {
                        level = "Critical",
                        source = "TFCiclo.Connector",
                        operation = "SaveOpenWeatherData(string correlationId)",
                        message = "Error inesperado",
                        correlationId = correlationId,
                        exception = ex.ToString()
                    });
                }
            }
        }
        #endregion
    }
}