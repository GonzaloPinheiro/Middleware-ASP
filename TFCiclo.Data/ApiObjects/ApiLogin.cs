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
        private string _token = string.Empty;
        #endregion

        #region Métodos públicos
        [Write(false)] //para que al heredar dapper ignore el campo
        public string username { get => _username; set => _username = value; }

        [Write(false)] //para que al heredar dapper ignore el campo
        public string role { get => _role; set => _role = value; }

        [Write(false)] //para que al heredar dapper ignore el campo
        public string token { get => _token; set => _token = value; }
        
        #endregion

        #region Constructores
        #endregion

    }
}
