using TFCiclo.Data.Models;

namespace TFCiclo.Data.ApiObjects
{
    public class ApiObjectRequest : ApiLogin
    {
        #region Propiedades privadas
        //Propiedades usadas para identificar al usuario(Login/Register)
        private user_info _user_info = new user_info();
        //Propiedades usadas para identificar al usuario(Login/Register)

        //Propiedades usadas para OpenWeather
        private weather_forecast _weather_forecast = new weather_forecast();
        //Propiedades usadas para OpenWeather
        #endregion

        #region Propiedades públicas
        //Propiedades usadas para OpenWeather
        public weather_forecast weather_forecast { get => _weather_forecast; set => _weather_forecast = value; }
        //Propiedades usadas para OpenWeather

        //Propiedades usadas para identificar al usuario(Login/Register)
        public user_info user_info { get => _user_info; set => _user_info = value; }
        //Propiedades usadas para identificar al usuario(Login/Register)
        #endregion
    }
}
