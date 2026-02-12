using Dapper;
using MySql.Data.MySqlClient;
using TFCiclo.Data.Exceptions;
using TFCiclo.Data.Models.TFCiclo.Data.Models;
using TFCiclo.Data.Services;

namespace TFCiclo.Data.Repositories
{
    public class RefreshTokenRepository
    {
        private readonly string decryptedConnectionString;
        private readonly Logger _logger;

        #region Constructores
        public RefreshTokenRepository(string encryptedConnection, Logger logger)
        {
            decryptedConnectionString = DecryptConnectionString(encryptedConnection);
            _logger = logger;
        }

        //TODO sustituir despues
        private string DecryptConnectionString(string encrypted)
        {
            return encrypted;
        }
        #endregion

        #region Inserts
        /// <summary>
        /// Inserta un nuevo refresh token en la base de datos.
        /// </summary>
        /// <param name="refreshToken">Objeto refreshToken con los datos a insertar.</param>
        /// <param name="cToken">CancellationToken para cancelar la operación.</param>
        /// <returns>El ID del nuevo registro insertado en la base de datos.</returns>
        public async Task InsertRefreshTokenAsync(ModelRefreshToken refreshToken, CancellationToken cToken)
        {
            //Variables y objetos
            int newId = -1;

            const string sql = @"
                INSERT INTO refresh_token (id, user_id, token_hash, created_at, expires_at, revoked_at, replaced_by_token_hash, created_by_ip, revoked_by_ip)
                VALUES (@id, @user_id, @token_hash, @created_at, @expires_at, @revoked_at, @replaced_by_token_hash, @created_by_ip, @revoked_by_ip);

                SELECT LAST_INSERT_ID();
            ";

            //Agrego los campos para operar con la DB
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@id", refreshToken.id);
            parameters.Add("@user_id", refreshToken.user_id);
            parameters.Add("@token_hash", refreshToken.token_hash);
            parameters.Add("@created_at", refreshToken.created_at);
            parameters.Add("@expires_at", refreshToken.expires_at);
            parameters.Add("@revoked_at", refreshToken.revoked_at);
            parameters.Add("@replaced_by_token_hash", refreshToken.replaced_by_token_hash);
            parameters.Add("@created_by_ip", refreshToken.created_by_ip);
            parameters.Add("@revoked_by_ip", refreshToken.revoked_by_ip);

            try
            {
                //Creo la conexión a la base de datos MySQL
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Insertar el newUser en DB
                newId = await connection.ExecuteScalarAsync<int>(sql, parameters);

                //Compruebo que se ha insertado correctamente
                if (newId <= -1)
                    throw new DatabaseOperationException("Insertar nuevo usuario en DB");
            }
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error de operación MySql", ex);
            }
        }
        #endregion

        #region Geters
        /// <summary>
        /// Obtiene un refresh token por su hash.
        /// </summary>
        /// <param name="token_hash">Hash del refresh token a buscar.</param>
        /// <param name="cToken">CancellationToken para cancelar la operación.</param>
        /// <returns>Objeto refreshToken si se encuentra, o null si no existe.</returns>
        public async Task<ModelRefreshToken> GetRefreshTokenAsync(string token_hash, CancellationToken cToken)
        {
            ModelRefreshToken? result = null;

            const string sql = @"
                SELECT *
                FROM refresh_token
                WHERE token_hash = @token_hash
                  AND (revoked_at IS NULL OR revoked_at = '0001-01-01 00:00:00')
                  AND expires_at > NOW()
                LIMIT 1;
            ";

            //Filtros
            if (string.IsNullOrWhiteSpace(token_hash) || token_hash.Length > 255)
                throw new ArgumentException("El token recibido no tiene una estructura válida");

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@token_hash", token_hash);

                //Devuelvo el objeto mapeado
                result = await connection.QueryFirstOrDefaultAsync<ModelRefreshToken>(sql, parameters);

                //Compruebo si encontro el refresh token
                if (result == null)
                    throw new RefreshTokenNotFoundException();
            }
            catch (MySqlException ex) //Mysql exception
            {
                throw new ArgumentException("Error al obtener el refresh token de la base de datos", ex);
            }

            //Devolver resultado
            return result;
        }

        /// <summary>
        /// Obtiene todos los refresh tokens activos de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="cToken">CancellationToken para cancelar la operación.</param>
        /// <returns>Lista de refreshToken activos del usuario.</returns>
        public async Task<IEnumerable<ModelRefreshToken>> GetActiveRefreshTokensByUserIdAsync(int userId, CancellationToken cToken)
        {
            IEnumerable<ModelRefreshToken>? result = null;

            const string sql = @"
                SELECT *
                FROM refresh_token
                WHERE user_id = @user_id
                  AND revoked_at IS NULL
                  AND expires_at > UTC_TIMESTAMP();
            ";

            //Filtros
            if (userId <= 0)
                throw new ArgumentException("El id del usuario debe ser mayor a 0");
            

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@user_id", userId);

                result = await connection.QueryAsync<ModelRefreshToken>(sql, parameters);
            }
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error al obtener el activo token del usuario", ex);
            }

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
        public async Task<string> GetUsernameById(int userId, CancellationToken cToken)
        {
            string? result = null;

            const string sql = @"
                SELECT username
                FROM user_info t1
                JOIN refresh_token t2 
                    ON t1.id = t2.user_id
                WHERE t1.id = @user_id           
                LIMIT 1;
            ";

            //Filtros
            if (userId <= -1)
                throw new ArgumentException("Id de usuario recibido no válido");

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@user_id", userId);

                //Devuelvo el objeto mapeado
                result = await connection.QueryFirstOrDefaultAsync<string>(sql, parameters);

                if (result == null)
                    throw new ArgumentException("No se ha encontrado un usuario con el id indicado");
            }
            catch (MySqlException ex) //Mysql exception
            {
                throw new DatabaseOperationException("Error al obtener el nombre de usuario en base al id", ex);
            }

            //Devolver resultado
            return result;
        }
        #endregion

        #region Revokes(Updates)
        /// <summary>
        /// Revoca un refresh token específico, marcándolo como revocado y opcionalmente reemplazado.
        /// </summary>
        /// <param name="tokenHash">Hash del refresh token a revocar.</param>
        /// <param name="revokedByIp">IP desde la cual se revoca el token.</param>
        /// <param name="replacedByTokenHash">Hash del token que reemplaza al actual (opcional).</param>
        /// <param name="cToken">CancellationToken para cancelar la operación.</param>
        /// <returns>True si se revocó correctamente, false en caso contrario.</returns>
        /// <returns></returns>
        public async Task RevokeRefreshTokenAsync(string tokenHash, string revokedByIp, string replacedByTokenHash, CancellationToken cToken)
        {
            //Variables y objetos
            int affectedRaws = 0;

            const string sql = @"
                UPDATE refresh_token
                SET revoked_at = UTC_TIMESTAMP(),
                    revoked_by_ip = @revoked_by_ip,
                    replaced_by_token_hash = @replaced_by_token_hash
                WHERE token_hash = @token_hash
                  AND (revoked_at IS NULL OR revoked_at = '0001-01-01 00:00:00');
            ";

            //Filtros
            if (string.IsNullOrWhiteSpace(tokenHash))
                throw new InternalInfoNotFoundException($"Token Hash con valor inválido,{nameof(tokenHash)}");

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@token_hash", tokenHash);
                parameters.Add("@revoked_by_ip", revokedByIp);
                parameters.Add("@replaced_by_token_hash", replacedByTokenHash);

                //Ejecuto la query obteniendo las líenas afectadas
                affectedRaws = await connection.ExecuteAsync(sql, parameters);

                //Compruebo si cambió alguna línea
                if (affectedRaws <= 0)
                    throw new InternalInfoNotFoundException("No se ha logrado actualizar el refresh token");
            }
            catch (MySqlException ex) //Mysql exception
            {
                throw new DatabaseOperationException("Error al actualizar el refresh token", ex);
            }
        }

        /// <summary>
        /// Revoca todos los refresh tokens activos de un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario cuyos tokens serán revocados.</param>
        /// <param name="revokedByIp">IP desde la cual se realiza la revocación.</param>
        /// <param name="cToken">CancellationToken para cancelar la operación.</param>
        /// <returns>True si se revocaron tokens, false si no se afectó ningún registro.</returns>
        public async Task<bool> RevokeAllRefreshTokensByUserIdAsync(int userId, string revokedByIp, CancellationToken cToken)
        {
            //Variables y objetos
            int affectedRows = 0;
            bool result = false;

            const string sql = @"
                UPDATE refresh_token
                SET revoked_at = UTC_TIMESTAMP(),
                    revoked_by_ip = @revoked_by_ip
                WHERE user_id = @user_id
                  AND revoked_at IS NULL;
            ";

            //Filtros
            if (userId <= 0)
                throw new ArgumentException("El id de usuario debe ser mayor o igual a 1");
            
            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@user_id", userId);
                parameters.Add("@revoked_by_ip", revokedByIp);

                //Ejecuto la query obteniendo las línes afectadas
                affectedRows = await connection.ExecuteAsync(sql, parameters);

                //Compruebo si ha cambiado alguna línea
                if (affectedRows > 0)
                    throw new ArgumentException("No ha sido posible eliminar los refresh tokens del usuario indicado");

            }
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error al eliminar los refresh tokens del usuario indicado");
            }

            //Devolver resultado
            return result;
        }
        #endregion

        #region Deletes
        /// <summary>
        /// Elimina todos los refresh tokens que han expirado.
        /// </summary>
        /// <param name="cToken">CancellationToken para cancelar la operación.</param>
        /// <returns>El número de registros eliminados.</returns>
        public async Task<int> DeleteExpiredRefreshTokensAsync(CancellationToken cToken)
        {
            //Variables y objetos
            int affectedRows = 0;

            const string sql = @"
                    DELETE FROM refresh_token
                    WHERE expires_at < UTC_TIMESTAMP();
                ";

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                affectedRows = await connection.ExecuteAsync(sql);

                //Compruebo si se ha borrado el refresh token
                if (affectedRows <= 0)
                    throw new ArgumentException("Error al borrar el refresh token");
            }
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error al intentar borrar el refresh token");
            }

            //Devolver resultado
            return affectedRows;
        }

        /// <summary>
        /// Elimina todos los refresh tokens que fueron revocados antes de una fecha determinada.
        /// </summary>
        /// <param name="revokedBefore">Fecha límite: se eliminarán tokens revocados antes de esta fecha.</param>
        /// <param name="cToken">CancellationToken para cancelar la operación.</param>
        /// <returns>El número de registros eliminados.</returns>
        public async Task<int> DeleteRevokedRefreshTokensAsync(DateTime revokedBefore, CancellationToken cToken)
        {
            int affectedRows = 0;

            const string sql = @"
                DELETE FROM refresh_token
                WHERE revoked_at IS NOT NULL
                  AND revoked_at < @revoked_before;
            ";

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@revoked_before", revokedBefore);

                affectedRows = await connection.ExecuteAsync(sql, parameters);

                if (affectedRows <= 0)
                    throw new ArgumentException("No ha sido posible borrar ningún refresh token en base a la fecha dada");
            }
            catch (MySqlException ex)
            {
                throw new DatabaseOperationException("Error al intentar borrar los refresh tokens antiguos", ex);
            }

            //Devolver resultado
            return affectedRows;
        }
        #endregion
    }
}