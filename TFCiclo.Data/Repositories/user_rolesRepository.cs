using Dapper;
using MySql.Data.MySqlClient;
using TFCiclo.Data.Exceptions;
using TFCiclo.Data.Models;
using TFCiclo.Data.Services;

namespace TFCiclo.Data.Repositories
{
    public class user_rolesRepository
    {
        //Variables y objetos
        private readonly string decryptedConnectionString;
        private readonly Logger _logger;

        public user_rolesRepository(string encyptedConnection, Logger logger)
        {
            decryptedConnectionString = DecryptConnectionString(encyptedConnection);
            _logger = logger;

        }
        //TODO sustituir despues
        private string DecryptConnectionString(string encrypted)
        {
            return encrypted;
        }

        #region Inserts
        /// <summary>
        /// Asigna un rol específico a un usuario insertando una relación en la tabla <c>user_roles</c>.
        /// </summary>
        /// <param name="userId">Identificador único del usuario al cual se le asignará el rol. Debe ser mayor o igual a 0.</param>
        /// <param name="roleId">Identificador único del rol que se asignará al usuario. Debe ser mayor o igual a 0.</param>
        /// <param name="cToken">Token de cancelación que permite abortar la operación de manera segura.</param>
        /// <returns>
        /// Retorna el identificador generado de la nueva relación usuario-rol insertada en la base de datos.
        /// Retorna <c>-1</c> si ocurre un error o si los parámetros de entrada son inválidos.
        /// </returns>
        public async Task<int> InsertRoleToUserAsync(int userId, int roleId, CancellationToken cToken)
        {
            //Variables y objetos
            int newId = -0;

            const string sql = @"
                INSERT INTO user_roles (user_id, role_id)
                VALUES (@user_id, @role_id);

                SELECT LAST_INSERT_ID();
            ";

            //Creo la conexión a la base de datos MySQL
            await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
            await connection.OpenAsync(cToken);

            //Agrego los campos para operar con la DB
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@user_id", userId);
            parameters.Add("@role_id", roleId);

            try
            {
                //Insertar el newUser en DB
                newId = await connection.ExecuteScalarAsync<int>(sql, parameters);

                if (newId <= 0)
                    throw new DatabaseOperationException("Error al asignar el rol al nuevo usuario");
            }
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error al asignar el rol al nuevo usuario", ex);
            }

            //Devolver resultado
            return newId;
        }
        #endregion

        #region Geters
        /// <summary>
        /// Obtiene todos los roles asignados a un usuario específico.
        /// </summary>
        /// <param name="userId">Identificador único del usuario para filtrar los roles. Debe ser mayor o igual a 0.</param>
        /// <param name="cToken">Token de cancelación que permite abortar la operación de manera segura.</param>
        /// <returns>
        /// Retorna una colección de objetos <see cref="roles"/> correspondientes a los roles asociados al usuario especificado.
        /// Devuelve <c>null</c> si <paramref name="userId"/> es inválido o si ocurre un error durante la ejecución.
        /// </returns>
        public async Task<IEnumerable<roles>?> GetRolesFromUserAsync(int userId, CancellationToken cToken)
        {
            IEnumerable<roles>? result = null;

            const string sql = @"
                SELECT r.id, r.name
                FROM user_roles ur
                JOIN roles r ON r.id = ur.role_id
                WHERE ur.user_id = @id;
            ";

            //Filtros
            if (userId <= 1)
                throw new ArgumentException("UserId debe ser mayor o igual a 0");

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@id", userId);

                //Devuelvo el objeto mapeado
                result = await connection.QueryAsync<roles>(sql, parameters);

                if (result == null)
                    throw new UserNotFoundException(userId);
            }
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error de operación MySql", ex);
            }

            //Devolver resultado
            return result;
        }

        /// <summary>
        /// Obtiene el rol asignado a un usuario específico.
        /// </summary>
        /// <param name="userId">ID del usuario del cual obtener el rol.</param>
        /// <param name="cToken">Token de cancelación para interrumpir la operación asíncrona.</param>
        /// <returns>Objeto roles si el usuario tiene un rol asignado, null en caso contrario.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="UserNotFoundException"></exception>
        public async Task<roles> GetRoleFromUserAsync(int userId, CancellationToken cToken)
        {
            //Variables y objetos
            roles? result = null;

            const string sql = @"
                SELECT r.id, r.name
                FROM user_roles ur
                JOIN roles r ON r.id = ur.role_id
                WHERE ur.user_id = @id
                LIMIT 1;
            ";

            //Filtros
            if (userId <= -1)
                throw new ArgumentException("El userId debe ser mayor o igual a 0", nameof(userId));

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@id", userId);

                //Devuelvo el objeto mapeado
                result = await connection.QueryFirstOrDefaultAsync<roles>(sql, parameters);

                //Si es null no encontró un usuario con el rol indicado
                if (result == null)
                    throw new UserNotFoundException(userId);
            }
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error de operación MySql", ex);
            }

            //Devolver resultado
            return result;
        }

        /// <summary>
        /// Obtiene el identificador de un rol a partir de su nombre.
        /// </summary>
        /// <param name="roleName">Nombre del rol a buscar. No debe ser nulo ni vacío.</param>
        /// <param name="cToken">Token de cancelación que permite abortar la operación de manera segura.</param>
        /// <returns>
        /// Retorna el identificador del rol encontrado.
        /// Devuelve <c>-1</c> si el nombre del rol es inválido, no se encuentra un registro o si ocurre un error durante la ejecución.
        /// </returns>
        public async Task<int> GetRoleByNameAsync(string roleName, CancellationToken cToken)
        {
            int result = 0;

            const string sql = @"
                SELECT id
                FROM roles
                WHERE name = @roleName
                LIMIT 1;
            ";

            //Filtros
            if (string.IsNullOrEmpty(roleName))
                throw new InternalInfoNotFoundException("Rol recibido tiene un valor invalido");

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@roleName", roleName);

                //Devuelvo el objeto mapeado
                result = await connection.QueryFirstOrDefaultAsync<int>(sql, parameters);

                if(result <= 0)
                    throw new InternalInfoNotFoundException($"No se ha encontrado un el rol solicitado: {roleName}");
            }
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error al obtener el id del rol por defecto", ex); //TODO crear excepción específica para cuando falla una query pero por datos que no vienen del usuario
            }

            //Devolver resultado
            return result;
        }

        /// <summary>
        /// Obtiene todos los usuarios que tienen asignado un rol específico.
        /// </summary>
        /// <param name="roleId">Identificador único del rol para filtrar los usuarios. Debe ser mayor o igual a 0.</param>
        /// <param name="cToken">Token de cancelación que permite abortar la operación de manera segura.</param>
        /// <returns>
        /// Retorna una colección de objetos <see cref="user_info"/> correspondientes a los usuarios asociados al rol especificado.
        /// Devuelve <c>null</c> si <paramref name="roleId"/> es inválido o si ocurre un error durante la ejecución.
        /// </returns>
        public async Task<IEnumerable<user_info>?> GetUsersByRoleAsync(int roleId, CancellationToken cToken)
        {
            IEnumerable<user_info>? result = null;

            const string sql = @"
                SELECT u.id, u.username, u.user_password, u.email, u.created_at
                FROM user_roles ur
                JOIN user_info u ON u.id = ur.user_id
                WHERE ur.role_id = @role_id;
            ";

            //Filtros
            if (roleId <= -1)
            {
                return result;
            }

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@role_id", roleId);

                //Devuelvo el objeto mapeado
                result = await connection.QueryAsync<user_info>(sql, parameters);
            }
            catch (MySqlException ex) //Mysql exception
            {
                throw new DatabaseOperationException("Error en la query al obtener los usuarios por rol", ex);
            }

            return result;
        }
        #endregion

        #region Updates
        /// <summary>
        /// Actualiza el rol de un usuario en la tabla user_roles
        /// </summary>
        /// <param name="userId">ID del usuario a actualizar</param>
        /// <param name="newUserRoleId">ID del nuevo rol a asignar</param>
        /// <param name="cToken">Token de cancelación</param>
        /// <exception cref="ArgumentException">Si userId o newUserRoleId son inválidos</exception>
        /// <exception cref="UserNotFoundException">Si el usuario no existe en user_roles</exception>
        /// <exception cref="RoleNotFoundException">Si el rol no existe en la tabla roles</exception>
        /// <exception cref="DatabaseOperationException">Si ocurre un error de base de datos</exception>
        /// <exception cref="OperationCanceledException">Si se cancela la operación</exception>
        public async Task UpdateRoleAsync(int userId, int newUserRoleId, CancellationToken cToken)
        {
            //Variables y objetos
            int affectedRows = -1;

            const string sql = @"
                UPDATE user_roles
                SET role_id = @newUserRoleId
                WHERE user_id = @userId
            ";

            //Filtros
            if (userId <= 0)
                throw new ArgumentException("El user id debe ser mayor a 0 ", nameof(userId));

            if (newUserRoleId <= 0)
                throw new ArgumentException("El newUserRoleId debe ser mayor a 0", nameof(newUserRoleId));

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@newUserRoleId", newUserRoleId);
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
        /// Elimina la relación de un rol específico de un usuario en la base de datos.
        /// </summary>
        /// <param name="userId">Identificador único del usuario del cual se desea eliminar el rol. Debe ser mayor o igual a 0.</param>
        /// <param name="roleId">Identificador único del rol que se desea eliminar del usuario. Debe ser mayor o igual a 0.</param>
        /// <param name="cToken">Token de cancelación que permite abortar la operación de manera segura.</param>
        /// <returns>
        /// Retorna un entero que indica el número de filas afectadas por la operación.
        /// -1 si los parámetros de entrada son inválidos o si ocurre un error durante la ejecución.
        /// </returns>
        public async Task<int> RemoveRoleFromUserAsync(int userId, int roleId, CancellationToken cToken)
        {
            //Variables y objetos
            int affectedRows = -1;

            const string sql = @"
                DELETE FROM user_roles
                WHERE user_id = @user_id AND role_id = @role_id;
            ";

            //Filtros
            if (userId <= -1 || roleId <= -1)
                throw new ArgumentException("El user/role id debe ser mayor o igual a 0");
            

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@user_id", userId);
                parameters.Add("@role_id", roleId);

                //Ejecutar el delete
                affectedRows = await connection.ExecuteAsync(sql, parameters);
            }
            catch (MySqlException ex) //Mysql exception
            {
                throw new DatabaseOperationException("Error en la query al eliminar el rol del usuario", ex);
            }

            return affectedRows;
        }
        #endregion
    }
}