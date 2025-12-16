using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using TFCiclo.Data.ApiObjects;
using TFCiclo.Data.Models;
using TFCiclo.Data.Services;

namespace TFCiclo.Data.Repositories
{
    public class WeatherRepository
    {
        //Variables y objetos
        private readonly string decryptedConnectionString;
        private readonly Logger _logger;

        #region Constructores
        public WeatherRepository(string encyptedConnection, Logger logger)
        {
            decryptedConnectionString = DecryptConnectionString(encyptedConnection);
            _logger = logger;
        }
        #endregion

        //TODO sustituir despues
        private string DecryptConnectionString(string encrypted)
        {
            return encrypted;
        }


        #region Geters
        /// <summary>
        /// Obtiene un pronóstico desde la base de datos utilizando Dapper.
        /// Valores por los que filtra: city, country.
        /// </summary>
        /// <param name="forecast"></param>
        /// <returns></returns>
        public async Task<weather_forecast?> GetWeatherForecastAsync(weather_forecast forecast, CancellationToken cToken)
        {
            //Variables y objetos
            weather_forecast? result = null;

            const string sql = @"
                SELECT *
                FROM weather_forecast
                WHERE city = @city AND country = @country
                LIMIT 1;
            ";

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync();

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@city", forecast.city);
                parameters.Add("@country", forecast.country);

                //Devuelvo el objeto mapeado
                result = await connection.QueryFirstOrDefaultAsync<weather_forecast>(sql, parameters);
            }
            catch (OperationCanceledException)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetWeatherForecastAsync(weather_forecast forecast)",
                    message = "Error al obtener datos de la Db weather_forecast"
                });
            }
            catch(MySqlException ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetWeatherForecastAsync(weather_forecast forecast)",
                    message = "Error al obtener datos de la Db weather_forecast",
                    exception = ex.ToString(),
                    metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(forecast), 60000)
                });
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetWeatherForecastAsync(weather_forecast forecast)",
                    message = "Error inesperado al obtener datos de la Db weather_forecast",
                    exception = ex.ToString(),
                    metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(forecast), 60000)
                });
            }

            //Devolver el resultado
            return result;
        }

        /// <summary>
        /// Devuelve una lista con todos los elementos de la DB weather_forecast
        /// </summary>
        /// <returns></returns>
        public async Task<List<weather_forecast>> GetListOfPlacesAsync()
        {
            //Variables y objetos
            List<weather_forecast> weatherInfo = new List<weather_forecast>();
            const string sql = "SELECT * FROM weather_forecast;";

            //Crea la conexión a la base de datos MySQL
            await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
            await connection.OpenAsync();

            //Obtengo la lista de elementos en DB weather_forecast
            try
            {
                IEnumerable<weather_forecast> forecasts = await connection.QueryAsync<weather_forecast>(sql);
                weatherInfo = forecasts.ToList();
            }
            catch (OperationCanceledException) //Operación cancelada
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetListOfPlacesAsync(string correlationId)",
                    message = "Operación cancelada por CancellationToken."
                });
            }
            catch(MySqlException ex) //Mysql error
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetListOfPlacesAsync(string correlationId)",
                    message = "Error al obtener los datos de la DB weather_forecast",
                    exception = ex.ToString()
                });
            }
            catch (Exception ex) //Error inesperado
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetListOfPlacesAsync(string correlationId)",
                    message = "Error inesperado al obtener los datos de la DB weather_forecast",
                    exception = ex.ToString()
                });
            }

            //Respuesta
            return weatherInfo;
        }
        #endregion

        #region Inserts
        /// <summary>
        /// Inserta una nueva ubicación en la DB
        /// </summary>
        /// <param name="forecast"></param>
        /// <returns></returns>
        //public async Task<int> InsertWeatherFieldAsync(weather_forecast forecast, CancellationToken cToken)
        public async Task<int> InsertWeatherFieldAsync(weather_forecast forecast)
        {
            //Variables y objetos
            int newId = -1;

            const string sql = @"
                INSERT INTO weather_forecast (city, country, requested_at, response_json, http_status)
                VALUES (@city, @country, @requested_at, @response_json, @http_status);

                SELECT LAST_INSERT_ID();
            ";

            //Creo la conexión a la base de datos MySQL
            await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
            await connection.OpenAsync();

            //Agrego los campos para operar con la DB
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@city", forecast.city);
            parameters.Add("@country", forecast.country);
            parameters.Add("@requested_at", forecast.requested_at);
            parameters.Add("@response_json", forecast.response_json);
            parameters.Add("@http_status", forecast.http_status);


            try
            {
                //Insertar el weather_forecast en DB
                newId = await connection.ExecuteScalarAsync<int>(sql, parameters);
            }
            catch (OperationCanceledException) //Operación cancelada
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "InsertWeatherFieldAsync(weather_forecast weather)",
                    message = "Operación cancelada por CancellationToken."
                });
            }
            catch (MySqlException ex) //Mysql error
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "InsertWeatherFieldAsync(weather_forecast weather)",
                    message = "Error al insertar en la DB weather_forecast",
                    exception = ex.ToString(),
                    metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(forecast), 60000)
                });
            }
            catch (Exception ex) //Error inesperado
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "InsertWeatherFieldAsync(weather_forecast weather)",
                    message = "Error inesperado al insertar en la DB weather_forecast",
                    exception = ex.ToString(),
                    metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(forecast), 60000)
                });
            }

            //Devolver resultado
            return newId;
        }
        #endregion

        #region Updates
        /// <summary>
        /// Actualiza el campo weather donde city y country coincida con lo recibido
        /// </summary>
        /// <param name="forecast"></param>
        /// <returns></returns>
        public async Task<bool> UpdateWeatherJsonAsync(forecast_weatherResponse forecast)
        {
            //Variables y objetos
            bool result = false;
            int affectedRows = 0;

            //Query
            const string sql = @"
                UPDATE weather_forecast
                SET response_json = @responseJson,
                    requested_at = @requested_at,
                    http_status = @http_status
                WHERE city = @city;
            ";

            //Creo la conexión a la base de datos MySQL
            await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
            await connection.OpenAsync();


            try
            {
                //Agrego los campos para operar con la DB
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@responseJson", JsonSerializer.Serialize(forecast.list));
                parameters.Add("@requested_at", DateTime.UtcNow);
                parameters.Add("@http_status", forecast.cod);
                parameters.Add("@city", forecast.city.name);

                //Lanzo la query
                affectedRows = await connection.ExecuteAsync(sql, parameters);

                //Comrpuebo si cambió alguna fila
                if (affectedRows > 0)
                {
                    result = true;// true si se actualizó algo
                }
            }
            catch (OperationCanceledException)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "UpdateWeatherJsonAsync(forecast_weatherResponse forecast, string correlationId)",
                    message = "Operación cancelada por CancellationToken"
                });
            }
            catch (MySqlException ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "UpdateWeatherJsonAsync(forecast_weatherResponse forecast, string correlationId)",
                    message = "Error al actualizar la DB: weather_forecast",
                    exception = ex.ToString(),
                    metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(forecast), 60000)
                });
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "UpdateWeatherJsonAsync(forecast_weatherResponse forecast, string correlationId)",
                    message = "Error critico al actualizar la DB: weather_forecast",
                    exception = ex.ToString(),
                    metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(forecast), 60000)
                });
            }

            //Devolver resultado
            return result;
        }
        #endregion
    }
}