using Dapper;
using MySql.Data.MySqlClient;
using System.Text.Json;
using TFCiclo.Data.ApiObjects;
using TFCiclo.Data.Models;
using TFCiclo.Data.Services;

namespace TFCiclo.Data.Repositories
{
    public class UserRepository
    {
        //Variables y objetos
        private readonly string decryptedConnectionString;
        private readonly Logger _logger;

        public UserRepository(string encyptedConnection, Logger logger)
        {
            decryptedConnectionString = DecryptConnectionString(encyptedConnection);
            _logger = logger;

        }
        //TODO sustituir despues
        private string DecryptConnectionString(string encrypted)
        {
            return encrypted;
        }


        #region Geters
        /// <summary>
        /// Devuelve el objeto user_info buscando por el username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        //public async Task<user_info?> GetUserByUserNameAsync(string username, CancellationToken cToken)
        public async Task<user_info?> GetUserByUserNameAsync(string username, CancellationToken cToken)
        {
            //Variables y objetos
            user_info? result = null;

            const string sql = @"
                SELECT *
                FROM user_info
                WHERE username = @username
                LIMIT 1
            ";

            //Filtros
            if (string.IsNullOrWhiteSpace(username) || username.Length > 50)
            {
                return null;
            }

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@username", username);

                //Devuelvo el objeto mapeado
                result = await connection.QueryFirstOrDefaultAsync<user_info>(sql, parameters);
            }
            catch(OperationCanceledException) //Operación token
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetUserByUserNameAsync(string username)",
                    message = "Operación cancelada por CancellationToken."
                });
            }
            catch (MySqlException ex) //Mysql exception
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetUserByUserNameAsync(string username)",
                    message = $"Error al obtener datos de la Db user_info. Username recibido(10 carac) -> {username[..Math.Min(10, username.Length)]}",
                    exception = ex.ToString()
                });
            }
            catch (Exception ex) //Excepción inesperada
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetUserByUserNameAsync(string username)",
                    message = "Error inesperado al obtener user_info",
                    exception = ex.ToString()
                });
            }

            //Devolver resultado
            return result;
        }
        #endregion

        #region Inserts
        /// <summary>
        /// Inserta el usuario que se le pasa por parámetro en user_info
        /// </summary>
        /// <param name="newUser"></param>
        /// <returns></returns>
        //public async Task<int> InsertUserAsync(user_info newUser, CancellationToken cToken)
        public async Task<int> InsertUserAsync(user_info newUser, CancellationToken cToken)
        {
            //Variables y objetos
            int newId = -1;

            const string sql = @"
                INSERT INTO user_info (username, email, user_password, created_at)
                VALUES (@username, @email, @user_password, @created_at);

                SELECT LAST_INSERT_ID();
            ";

            //Creo la conexión a la base de datos MySQL
            await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
            await connection.OpenAsync(cToken);

            //Agrego los campos para operar con la DB
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@username", newUser.username);
            parameters.Add("@email", newUser.email);
            parameters.Add("@user_password", newUser.user_password);
            parameters.Add("@created_at", newUser.created_at);

            try
            {
                //Insertar el newUser en DB
                newId = await connection.ExecuteScalarAsync<int>(sql, parameters);
            }
            catch (OperationCanceledException)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "InsertUserAsync(user_info newUser)",
                    message = "Operación cancelada por CancellationToken."
                });
            }
            catch(MySqlException ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "InsertUserAsync(user_info newUser)",
                    message = "Error al insertar en la DB user_info",
                    exception = ex.ToString(),
                    metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(newUser), 60000)
                });
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "InsertUserAsync(user_info newUser)",
                    message = "Error inesperado al insertar en la DB user_info",
                    exception = ex.ToString(),
                    metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(newUser), 60000)
                });
            }

            //Devolver resultado
            return newId;
        }
        #endregion
    }
}