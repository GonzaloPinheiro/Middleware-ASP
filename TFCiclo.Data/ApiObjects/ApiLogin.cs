using Dapper.Contrib.Extensions;

namespace TFCiclo.Data.ApiObjects
{
    //Clase encargada de almacenar los datos del login del usuario
    //Dapper debe ignorar los elementos de la clase para evitar errores al insertar en DB
    public class ApiLogin
    {
        #region Métodos privados
        private string _username = string.Empty;
        private string _role = string.Empty;
        private string _accesToken = string.Empty;
        private string _refreshToken = string.Empty;
        #endregion

        #region Métodos públicos
        [Write(false)] //para que al heredar dapper ignore el campo
        public string Username { get => _username; set => _username = value; }

        [Write(false)] //para que al heredar dapper ignore el campo
        public string Role { get => _role; set => _role = value; }

        [Write(false)] //para que al heredar dapper ignore el campo
        public string AccesToken { get => _accesToken; set => _accesToken = value; }
        [Write(false)] //para que al heredar dapper ignore el campo
        public string RefreshToken { get => _refreshToken; set => _refreshToken = value; }

        #endregion

        #region Constructores
        #endregion

    }
}
