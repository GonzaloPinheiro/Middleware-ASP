using System.Text.Json;
using TFCiclo.Data.ApiObjects;
using TFCiclo.Data.Models;
using TFCiclo.Data.Services;

//Función encargada de guardar en db los datos de la api openWeather
public class GetWeatherFromApi
{
    //Variables y objetos
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly Logger _logger;
    private readonly string _correlationId;

    #region Constructores
    // Constructor con la API key
    public GetWeatherFromApi(string apiKey, Logger logger, string correlationId)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _logger = logger;
        _correlationId = correlationId;
    }
    #endregion

    #region Funciones públicas
    /// <summary>
    /// Llama al endpoint /data/2.5/forecast y devuelve el JSON completo como string
    /// </summary>
    /// <param name="city"></param>
    /// <param name="country"></param>
    /// <returns></returns>
    public async Task<forecast_weatherResponse?> GetForecastJsonAsync(string city, string country)
    {
        //Variables y objetos
        forecast_weatherResponse? result = null;
        string url = $"https://api.openweathermap.org/data/2.5/forecast?q={city},{country}&units=metric&lang=es&appid={_apiKey}";

        try
        {
            //Obtengo el forecast de la ubicación solicitada
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            //Me aseguro de respuesta correcta
            if (!response.IsSuccessStatusCode) throw new Exception("Error al solicitar el forecast de openWeather");

            //Guardo el json deserializado
            var json = await response.Content.ReadAsStringAsync();
            result = JsonSerializer.Deserialize<forecast_weatherResponse>(json);
        }
        catch (Exception ex)
        {
            //Crear log
            await _logger.AddAsync(new log_entry
            {
                level = "Error",
                source = "TFCiclo.Connector",
                operation = "GetForecastJsonAsync(string city,string country)",
                message = "City -> " + city + ", country -> " + country,
                correlationId = _correlationId,
                exception = ex.ToString()
            });
        }

        //Devolver resultado
        return result;
    }
    #endregion
}