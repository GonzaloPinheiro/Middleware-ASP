using Dapper;
using MySql.Data.MySqlClient;
using System.Text.Json;
using TFCiclo.Data.ApiObjects;
using TFCiclo.Data.Models;
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
        public async Task<int> InsertRefreshTokenAsync(ModelRefreshToken refreshToken, CancellationToken cToken)
        {
            //Variables y objetos
            int newId = -1;

            const string sql = @"
                INSERT INTO refresh_token (id, user_id, token_hash, created_at, expires_at, revoked_at, replaced_by_token_hash, created_by_ip, revoked_by_ip)
                VALUES (@id, @user_id, @token_hash, @created_at, @expires_at, @revoked_at, @replaced_by_token_hash, @created_by_ip, @revoked_by_ip);

                SELECT LAST_INSERT_ID();
            ";

            //Creo la conexión a la base de datos MySQL
            await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
            await connection.OpenAsync(cToken);

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
                //Insertar el newUser en DB
                newId = await connection.ExecuteScalarAsync<int>(sql, parameters);
            }
            catch (OperationCanceledException)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "InsertRefreshTokenAsync(refreshToken refreshToken, CancellationToken cToken)",
                    message = "Operación cancelada por CancellationToken."
                });
            }
            catch (MySqlException ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "InsertRefreshTokenAsync(refreshToken refreshToken, CancellationToken cToken)",
                    message = "Error al insertar en la DB refresh_token",
                    exception = ex.ToString(),
                    metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(refreshToken), 60000)
                });
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "InsertRefreshTokenAsync(refreshToken refreshToken, CancellationToken cToken)",
                    message = "Error inesperado al insertar en la DB refresh_token",
                    exception = ex.ToString(),
                    metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(refreshToken), 60000)
                });
            }

            //Devolver resultado
            return newId;
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
            {
                return null;
            }

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@token_hash", token_hash);

                //Devuelvo el objeto mapeado
                result = await connection.QueryFirstOrDefaultAsync<ModelRefreshToken>(sql, parameters);
            }
            catch (OperationCanceledException) //Operación cancelada
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetRefreshTokenByTokenHashAsync(string token_hash, CancellationToken cToken)",
                    message = "Operación cancelada por CancellationToken."
                });
            }
            catch (MySqlException ex) //Mysql exception
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetRefreshTokenByTokenHashAsync(string token_hash, CancellationToken cToken)",
                    message = $"Error al obtener datos de la Db refresh_token.",
                    exception = ex.ToString()
                });
            }
            catch (Exception ex) //Excepción inesperada
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetRefreshTokenByTokenHashAsync(string token_hash, CancellationToken cToken)",
                    message = "Error inesperado al obtener refresh_token",
                    exception = ex.ToString()
                });
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
            {
                return result;
            }

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@user_id", userId);

                result = await connection.QueryAsync<ModelRefreshToken>(sql, parameters);
            }
            catch (OperationCanceledException)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetActiveRefreshTokensByUserIdAsync(int userId, CancellationToken cToken)",
                    message = "Operación cancelada por CancellationToken."
                });
            }
            catch (MySqlException ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetActiveRefreshTokensByUserIdAsync(int userId, CancellationToken cToken)",
                    message = "Error al obtener refresh tokens activos.",
                    exception = ex.ToString()
                });
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetActiveRefreshTokensByUserIdAsync(int userId, CancellationToken cToken)",
                    message = "Error inesperado.",
                    exception = ex.ToString()
                });
            }

            //Devolver resultado
            return result;
        }

        public async Task<string?> GetUsernameById(int userId, CancellationToken cToken)
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
                return null;

            try
            {
                await using MySqlConnection connection = new MySqlConnection(decryptedConnectionString);
                await connection.OpenAsync(cToken);

                //Agrego las propiedades que uso en la query
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@user_id", userId);

                //Devuelvo el objeto mapeado
                result = await connection.QueryFirstOrDefaultAsync<string>(sql, parameters);
            }
            catch (OperationCanceledException) //Operación cancelada
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetActiveRefreshTokensByUserIdAsync(int userId, CancellationToken cToken)",
                    message = "Operación cancelada por CancellationToken."
                });
            }
            catch (MySqlException ex) //Mysql exception
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetActiveRefreshTokensByUserIdAsync(int userId, CancellationToken cToken)",
                    message = $"Error al obtener datos de la Db.",
                    exception = ex.ToString()
                });
            }
            catch (Exception ex) //Excepción inesperada
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "GetActiveRefreshTokensByUserIdAsync(int userId, CancellationToken cToken)",
                    message = "Error inesperado al obtener username",
                    exception = ex.ToString()
                });
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
        public async Task<bool> RevokeRefreshTokenAsync(string tokenHash, string revokedByIp, string replacedByTokenHash, CancellationToken cToken)
        {
            //Variables y objetos
            bool result = false;
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
            {
                return false;
            }

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
                if (affectedRaws >= 0)
                {
                    result = true;
                }
            }
            catch (OperationCanceledException) //Operación cancelada
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "RevokeRefreshTokenAsync(string tokenHash, string revokedByIp, string replacedByTokenHash, CancellationToken cToken)",
                    message = "Operación cancelada por CancellationToken."
                });
            }
            catch (MySqlException ex) //Mysql exception
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "RevokeRefreshTokenAsync(string tokenHash, string revokedByIp, string replacedByTokenHash, CancellationToken cToken)",
                    message = "Error al revocar refresh token.",
                    exception = ex.ToString()
                });
            }
            catch (Exception ex) //Error inesperado
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "RevokeRefreshTokenAsync(string tokenHash, string revokedByIp, string replacedByTokenHash, CancellationToken cToken)",
                    message = "Error inesperado.",
                    exception = ex.ToString()
                });
            }

            //Devolver resultado
            return result;
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
            {
                return result;
            }

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
                {
                    result = true;
                }
            }
            catch (OperationCanceledException)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "RevokeAllRefreshTokensByUserIdAsync(int userId, string revokedByIp, CancellationToken cToken)",
                    message = "Operación cancelada por CancellationToken."
                });
            }
            catch (MySqlException ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "RevokeAllRefreshTokensByUserIdAsync(int userId, string revokedByIp, CancellationToken cToken)",
                    message = "Error al revocar todos los refresh tokens.",
                    exception = ex.ToString()
                });
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "RevokeAllRefreshTokensByUserIdAsync(int userId, string revokedByIp, CancellationToken cToken)",
                    message = "Error inesperado.",
                    exception = ex.ToString()
                });
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
            }
            catch (OperationCanceledException)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "DeleteExpiredRefreshTokensAsync(CancellationToken cToken)",
                    message = "Operación cancelada por CancellationToken."
                });
            }
            catch (MySqlException ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "DeleteExpiredRefreshTokensAsync(CancellationToken cToken)",
                    message = "Error al eliminar refresh tokens expirados.",
                    exception = ex.ToString()
                });
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "DeleteExpiredRefreshTokensAsync(CancellationToken cToken)",
                    message = "Error inesperado.",
                    exception = ex.ToString()
                });
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
            }
            catch (OperationCanceledException)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Data.Repositories",
                    operation = "DeleteRevokedRefreshTokensAsync(DateTime revokedBefore, CancellationToken cToken)",
                    message = "Operación cancelada por CancellationToken."
                });
            }
            catch (MySqlException ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Data.Repositories",
                    operation = "DeleteRevokedRefreshTokensAsync(DateTime revokedBefore, CancellationToken cToken)",
                    message = "Error al eliminar refresh tokens revocados.",
                    exception = ex.ToString()
                });
            }
            catch (Exception ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Data.Repositories",
                    operation = "DeleteRevokedRefreshTokensAsync(DateTime revokedBefore, CancellationToken cToken)",
                    message = "Error inesperado.",
                    exception = ex.ToString()
                });
            }

            //Devolver resultado
            return affectedRows;
        }
        #endregion
    }
}