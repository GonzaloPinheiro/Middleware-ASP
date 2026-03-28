using Dapper;
using MySql.Data.MySqlClient;
using TFCiclo.Domain.Exceptions;
using TFCiclo.Domain.Entities;
using TFCiclo.Infrastructure.Observability;

namespace TFCiclo.Infrastructure.Repositories
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
        public async Task<user_info?> GetUserByUserNameAsync(string username, CancellationToken cToken)
        {
            //Variables y objetos
            user_info? result = null;

            const string sql = @"
                SELECT *
                FROM user_info
                WHERE username = @username
                LIMIT 1;
            ";

            //Filtros
            if (string.IsNullOrWhiteSpace(username) || username.Length > 50)
                throw new ArgumentException("El usuario es null, vacio o tiene una logintud mayor a 50 caracteres");

            await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
            await connection.OpenAsync(cToken);

            //Agrego las propiedades que uso en la query
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@username", username);

            //Devuelvo el objeto mapeado
            result = await connection.QueryFirstOrDefaultAsync<user_info>(sql, parameters);

            //Compruebo que haya encontrado un usuario con el nombre solicitado
            if (result == null)
                throw new UserNotFoundException(username);

            //Devolver resultado
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> CheckExistingUserAsync(string username, CancellationToken cToken)
        {
            //Variables y objetos
            user_info? result = null;

            const string sql = @"
                SELECT *
                FROM user_info
                WHERE username = @username
                LIMIT 1;
            ";

            //Filtros
            if (string.IsNullOrWhiteSpace(username) || username.Length > 50)
                throw new ArgumentException("El usuario es null, vacio o tiene una logintud mayor a 50 caracteres");

            await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
            await connection.OpenAsync(cToken);

            //Agrego las propiedades que uso en la query
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@username", username);

            //Devuelvo el objeto mapeado
            result = await connection.QueryFirstOrDefaultAsync<user_info>(sql, parameters);

            //No encontro un usuario con ese nombre
            if (result == null)
                return false;

            //Encontro un usuario con ese nombre
            return true;
        }

        /// <summary>
        /// Devuelve el objeto user_info en base al id de usuario proporcionado
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="UserNotFoundException"></exception>
        public async Task<user_info?> GetUserByIdAsync(user_info user, CancellationToken cToken)
        {
            //Variables y objetos
            user_info? result = null;

            const string sql = @"
                SELECT id, username, email, created_at
                FROM user_info
                WHERE id = @userId
                LIMIT 1;
            ";

            //Filtros
            if (user == null || user.id <= -1)
                throw new ArgumentException("El objeto user no puede ser null y su campo id debe ser mayor o igual a 0");

            await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
            await connection.OpenAsync(cToken);

            //Agrego las propiedades que uso en la query
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@userId", user.id);

            //Devuelvo el objeto mapeado
            result = await connection.QueryFirstOrDefaultAsync<user_info>(sql, parameters);

            //Compruebo que haya encontrado un usuario con ese id
            if (result == null)
                throw new UserNotFoundException(user.id);

            //Devolver resultado
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="UserNotFoundException"></exception>
        public async Task<string> GetUsernameByIdAsync(int userId, CancellationToken cToken)
        {
            //Variables y objetos
            string? result = null;

            const string sql = @"
                SELECT username
                FROM user_info
                WHERE id = @userId
                LIMIT 1;
            ";

            //Filtros
            if (userId <= -1)
                throw new ArgumentException("El id debe ser mayor o igual a 0");

            await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
            await connection.OpenAsync(cToken);

            //Agrego las propiedades que uso en la query
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@userId", userId);

            //Devuelvo el objeto mapeado
            result = await connection.QueryFirstOrDefaultAsync<string>(sql, parameters);

            //Compruebo que haya encontrado un usuario con ese id
            if (result == null)
                throw new UserNotFoundException(userId);

            //Devolver resultado
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="UserNotFoundException"></exception>
        public async Task<string> GetPasswordByIdAsync(int userId, CancellationToken cToken)
        {
            //Variables y objetos
            string? result = null;

            const string sql = @"
                SELECT user_password
                FROM user_info
                WHERE id = @userId
                LIMIT 1;
            ";

            //Filtros
            if (userId <= -1)
                throw new ArgumentException("El campo id debe ser mayor o igual a 0");

            await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
            await connection.OpenAsync(cToken);

            try
            {
                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@userId", userId);

                //Devuelvo el objeto mapeado
                result = await connection.QueryFirstOrDefaultAsync<string>(sql, parameters);

                //Compruebo que haya encontrado un usuario con ese id
                if (result == null)
                    throw new UserNotFoundException(userId);

                //Devolver resultado
                return result;
            }
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error en la query de operación MySql", ex);
            }

        }

        /// <summary>
        /// Devuelve la lista de todos los usuarios en user_info
        /// </summary>
        /// <param name="cToken"></param>
        /// <returns></returns>
        /// <exception cref="UserNotFoundException"></exception>
        public async Task<List<user_info>> GetListOfUsersAsync(CancellationToken cToken)
        {
            //Variables y objetos
            List<user_info>? result = null;
            const string sql = @"
                SELECT id, username, email, created_at
                FROM user_info;
            ";
            await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
            await connection.OpenAsync(cToken);

            //Devuelvo el objeto mapeado
            var queryResult = await connection.QueryAsync<user_info>(sql);


            if (queryResult != null) //Encontro usuario
            {
                result = queryResult.ToList();
            }
            else //No encontro usuarios
            {
                throw new UserNotFoundException("No se han encontrado usuarios en la base de datos");
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
        /// <param name="cToken"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseOperationException"></exception>
        public async Task<int> InsertUserAsync(user_info newUser, CancellationToken cToken)
        {
            //Variables y objetos
            int newId;

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
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error de operación MySql", ex);
            }

            //Devolver resultado
            return newId;
        }
        #endregion

        #region Updates
        /// <summary>
        /// Actualiza la contraseña del usuario equivalente a su id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="HashPassword"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InternalInfoNotFoundException"></exception>
        /// <exception cref="UserNotFoundException"></exception>
        /// <exception cref="DatabaseOperationException"></exception>
        public  async Task UpdatePasswordByIdAsync(int userId, string HashPassword,CancellationToken cToken)
        {
            //Variables y objetos
            int affectedRows = -1;

            const string sql = @"
                UPDATE user_info
                SET user_password = @HashPassword
                WHERE id = @userId
                LIMIT 1
            ";

            //Filtros
            if (userId <= 0)
                throw new ArgumentException("El user id debe ser mayor a 0 ", nameof(userId));

            if (String.IsNullOrEmpty(HashPassword))
                throw new InternalInfoNotFoundException("Intento de insertar contraseña vacía o null");

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@HashPassword", HashPassword);
                parameters.Add("@userId", userId);

                //Ejecuto la query
                affectedRows = await connection.ExecuteAsync(sql, parameters);

                //Compruebo las filas afectadas por si no encontró el ususario
                if (affectedRows == 0)
                    throw new UserNotFoundException(userId);
            }
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error en la query de operación MySql", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="username"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InternalInfoNotFoundException"></exception>
        /// <exception cref="UserNotFoundException"></exception>
        /// <exception cref="DatabaseOperationException"></exception>
        public async Task UpdateUsernameByIdAsync(int userId, string username, CancellationToken cToken)
        {
            //Variables y objetos
            int affectedRows = -1;

            const string sql = @"
                UPDATE user_info
                SET username = @username
                WHERE id = @userId
                LIMIT 1
            ";

            //Filtros
            if (userId <= 0)
                throw new ArgumentException("El user id debe ser mayor a 0 ", nameof(userId));

            if (String.IsNullOrEmpty(username))
                throw new InternalInfoNotFoundException("Intento de insertar nombre de usuario vacío o null");

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@username", username);
                parameters.Add("@userId", userId);

                //Ejecuto la query
                affectedRows = await connection.ExecuteAsync(sql, parameters);

                //Compruebo las filas afectadas por si no encontró el ususario
                if (affectedRows == 0)
                    throw new UserNotFoundException(userId);
            }
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error en la query de operación MySql", ex);
            }
        }
        #endregion

        #region Deletes
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="UserNotFoundException"></exception>
        /// <exception cref="DatabaseOperationException"></exception>
        public async Task DeleteUserByIdAsync(int userId, CancellationToken cToken)
        {
            //Variables y objetos
            int affectedRows = -1;

            const string sql = @"
                DELETE FROM user_info
                WHERE id = @user_id;
            ";

            //Filtros
            if (userId <= -1)
                throw new ArgumentException("El user id debe ser mayor o igual a 0");

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@user_id", userId);

                //Ejecutar el delete
                affectedRows = await connection.ExecuteAsync(sql, parameters);

                //Compruebo las filas afectadas por si no existía el usuario
                if (affectedRows <= 0) throw new UserNotFoundException(userId);
            }
            catch (MySqlException ex) //Mysql exception
            {
                throw new DatabaseOperationException("Error en la query al eliminar el usuario", ex);
            }
        }
        #endregion
    }
}