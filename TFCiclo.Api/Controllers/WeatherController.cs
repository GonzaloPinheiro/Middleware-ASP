using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TFCiclo.Api.Controllers.Base;
using TFCiclo.Data.ApiObjects;
using TFCiclo.Data.Models;
using TFCiclo.Data.Repositories;
using TFCiclo.Data.Services;

namespace TFCiclo.Api.Controllers
{
    /// <summary>
    /// Controlador encargado de enviar la información del tiempo solicitada por la app
    /// </summary>
    [ApiController]
    //[Route("[controller]")]
    public class WeatherController : ApiControllerBase
    {
        //Variables y objetos
        private readonly Logger _logger;
        private readonly WeatherRepository _weatherRepository;
        //private readonly UserRepository _userRepository;

        #region Constructores
        public WeatherController(WeatherRepository weatherRepository, /*UserRepository userRepository,*/ Logger logger)
        {
            _logger = logger;
            _weatherRepository = weatherRepository;
            //_userRepository = userRepository;
        }
        #endregion

        #region GetWeatherForecastController
        /// <summary>
        /// Devuelve el forecast de la ubicación solicitada en el body
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("api/GetWeatherForecast")]
        public async Task<ApiObjectResponse> GetWeatherForecast([FromBody] ApiObjectRequest dto, CancellationToken cToken)
        {
            //Variables y objetos
            string correlationId = Guid.NewGuid().ToString();
            ApiObjectResponse Result = new ApiObjectResponse();
            string username = User.Identity.Name; // username del JWT

            // Comienza scope: registra entrada automáticamente y registrará salida al finalizar using.
            await using OperationLogScope scope = _logger.BeginScope(
                source: "TFCiclo.Api.Controllers.WeatherController",
                operation: "GetWeatherForecast([FromBody] ApiObjectRequest dto)",
                correlationId: correlationId,
                userId: username);

            //Obtener resultado
            Result = await getOrCreateWeatherAsync(dto, cToken);

            //Devuelve el resultado
            return Result;
        }

        /// <summary>
        /// Devuelve data del forecast de ubicación recibida, lo crea en DB si no existe
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task<ApiObjectResponse> getOrCreateWeatherAsync(ApiObjectRequest e, CancellationToken cToken)
        {
            await _logger.AddAsync(new log_entry
            {
                level = "Information",
                source = "TFCiclo.Api.WeatherController",
                operation = "getOrCreateWeatherAsync(ApiObjectRequest e)",
                message = "Entrado en la función"
#if DEBUG
                ,metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(e), 60000)
#endif
            });

            //Variables y objetos
            int idWeather = -1;
            weather_forecast? weatherResult = null;

            //Obtengo el weather de la DB
            weatherResult = await _weatherRepository.GetWeatherForecastAsync(e.weather_forecast, cToken);

            //Si no existe en DB la ubicación solicitada crearla
            if (weatherResult == null)
            {
                //Inserto ubicación DB
                idWeather = await _weatherRepository.InsertWeatherFieldAsync(e.weather_forecast);

                //Compruebo si se ha insertado correctamente en DB
                if (idWeather <= -1)//Fallo en la inserción
                {
                    return new ApiObjectResponse(false, weatherResult, 0, "no se ha insertado en db");
                }

                //Obtengo el registro recién insertado
                weatherResult = await _weatherRepository.GetWeatherForecastAsync(e.weather_forecast, cToken);
                return new ApiObjectResponse(false, weatherResult, 999, "exito: creado en db");
            }

            //Encontró en DB la ubicación
            return new ApiObjectResponse(true, weatherResult, 0, string.Empty);
        }
        #endregion
    }
}