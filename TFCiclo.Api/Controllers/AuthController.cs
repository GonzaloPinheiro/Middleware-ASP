using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Cryptography;
using System.Text.Json;
using TFCiclo.Data.ApiObjects;
using TFCiclo.Data.Models;
using TFCiclo.Data.Models.TFCiclo.Data.Models;
using TFCiclo.Data.Repositories;
using TFCiclo.Data.Security;
using TFCiclo.Data.Services;

namespace TFCiclo.Api.Controllers
{
    [ApiController]
    //[Route("api/[controller]")]
    public class AuthController : Controller
    {
        //Variables y objetos
        private readonly UserRepository _userRepository;
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly Logger _logger;
        private readonly IConfiguration _configuration;

        #region Constructores
        public AuthController(UserRepository userRepository, RefreshTokenRepository refreshTokenRepository, Logger logger, IConfiguration config)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
            _configuration = config;
        }
        #endregion

        #region api/Auth/Login
        [HttpPost]
        [Route("api/Auth/Login")]
        [EnableRateLimiting("strict-auth")]
        public async Task<ApiObjectResponse> Login([FromBody] ApiObjectRequest dto, CancellationToken cToken)
        {
            //Variables y objetos
            string correlationId = Guid.NewGuid().ToString();
            ApiObjectResponse Result = new ApiObjectResponse();


            // Comienza scope: registra entrada automáticamente y registrará salida al finalizar using.
            await using OperationLogScope scope = _logger.BeginScope(
                source: "TFCiclo.Api.Controllers.AuthController",
                operation: "Login([FromBody] ApiObjectRequest dto)",
                correlationId: correlationId,
                userId: dto.user_info.id.ToString());

            //Obtener resultado
            Result = await doTheLoginJWT(dto, cToken);

            //Devuelve el resultado
            return Result;
        }

        private async Task<ApiObjectResponse> doTheLoginJWT(ApiObjectRequest e, CancellationToken cToken)
        {
            //Log
            await _logger.AddAsync(new log_entry
            {
                level = "Information",
                source = "TFCiclo.Api.AuthController",
                operation = "doTheLogin(ApiObjectRequest e)",
                message = "Entrado en la función"
#if DEBUG 
                ,metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(e), 60000)
#endif
            });

            //Variables y objetos
            user_info? user = null;
            string jwtToken = string.Empty;

            //Obtengo el usuario
            user = await _userRepository.GetUserByUserNameAsync(e.user_info.username, cToken);

            if (user != null) //Encontró un usuario
            {
                //Compruebo al contraseña
                if (PasswordHelper.VerifyPassword(e.user_info.user_password, user.user_password))
                {
                    string secret = _configuration["JwtSecret"]!;

                    //Me aseguro de no generar un jwt no válido
                    if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(user.username))
                        return new ApiObjectResponse(false, string.Empty, 500, "Internal server error");

                    //Genera el jwt a devolver
                    jwtToken = JWTValidation.GenerateHashJwt(user.username, "User", secret);

                    //Genero el refresh token
                    string refreshToken = ApiHelper.GenerateNewToken();

                    //Guardo el refresh token hasheado en base de datos
                    string refreshTokenHash = PasswordHelper.HashRefreshToken(refreshToken);

                    //Creo el objeto refresh token
                    ModelRefreshToken newRefreshTokenObj = new ModelRefreshToken()
                    {
                        user_id = user.id,
                        created_at = DateTime.UtcNow,
                        expires_at = DateTime.UtcNow.AddDays(7),
                        created_by_ip = "ipAddress",
                        token_hash = refreshTokenHash
                    };

                    //Guardo el refresh token
                    int insertResult = await _refreshTokenRepository.InsertRefreshTokenAsync(newRefreshTokenObj, cToken);

                    //Compruebo si fallo el insert
                    if (insertResult <= 1)
                        return new ApiObjectResponse(false, null, 500, "Error al guardar el refresh token");

                    TokensResponse response = new TokensResponse()
                    {
                        accessToken = jwtToken,
                        refreshToken = refreshToken
                    };

                    //Todo Ok TODO crear objeto con token y refresh token
                    return new ApiObjectResponse(true, response, 0, string.Empty);
                }
            }

            //Credenciales no son correctas
            return new ApiObjectResponse(false, string.Empty, 401, "Unauthorized");
        }
        #endregion

        #region api/Auth/Register
        [HttpPost]
        [Route("api/Auth/Register")]
        [EnableRateLimiting("strict-auth")]
        public async Task<ApiObjectResponse> Register([FromBody] ApiObjectRequest dto, CancellationToken cToken)
        {
            //Variables y objetos
            string correlationId = Guid.NewGuid().ToString();
            ApiObjectResponse Result = new ApiObjectResponse();


            // Comienza scope: registra entrada automáticamente y registrará salida al finalizar using.
            await using OperationLogScope scope = _logger.BeginScope(
                source: "TFCiclo.Api.Controllers.AuthController",
                operation: "Register([FromBody] ApiObjectRequest dto)",
                correlationId: correlationId,
                userId: User?.Identity?.Name);

            //Obtener resultado
            Result = await doTheRegister(dto, cToken);

            //Devuelve el resultado
            return Result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task<ApiObjectResponse> doTheRegister(ApiObjectRequest e, CancellationToken cToken)
        {
            //Log
            await _logger.AddAsync(new log_entry
            {
                level = "Information",
                source = "TFCiclo.Api.AuthController",
                operation = "doTheRegister(ApiObjectRequest e)",
                message = "Entrado en la función"
#if DEBUG
                ,
                metadataJson = ApiHelper.Truncate(JsonSerializer.Serialize(e), 60000)
#endif
            });


            //Variables y objetos
            string encryptedPassword = string.Empty;
            int newId = 0;
            user_info? user = null;

            //Compruebo si ya existe un usuario con el mismo nombre
            user = await _userRepository.GetUserByUserNameAsync(e.user_info.username, cToken);

            if (user != null) //Encontró un usuario
                return new ApiObjectResponse(false, string.Empty, 409, "Conflict");

            //Hasheo la contraseña recibida
            encryptedPassword = PasswordHelper.HashPassword(e.user_info.user_password);

            //Genero el usuario a insertar en DB
            user = new user_info()
            {
                username = e.user_info.username,
                user_password = encryptedPassword,
                email = e.user_info.email,
                created_at = DateTime.UtcNow,
            };

            //Genero el nuevo usuario
            newId = await _userRepository.InsertUserAsync(user, cToken);

            //Compruebo si fallo el insert
            if (newId <= -1)
                return new ApiObjectResponse(false, string.Empty, 500, "Error al crear el nuevo usuario");

            //Todo Ok
            return new ApiObjectResponse(true, string.Empty, 0, string.Empty);
        }
        #endregion

        #region api/Auth/RefreshToken
        [HttpPost]
        [Route("api/Auth/RefreshToken")]
        [EnableRateLimiting("strict-auth")]
        public async Task<ApiObjectResponse> RefreshToken([FromBody] ApiObjectRequest dto, CancellationToken cToken)
        {
            //Variables y objetos
            string correlationId = Guid.NewGuid().ToString();
            ApiObjectResponse Result = new ApiObjectResponse();


            // Comienza scope: registra entrada automáticamente y registrará salida al finalizar using.
            await using OperationLogScope scope = _logger.BeginScope(
                source: "TFCiclo.Api.Controllers.AuthController",
                operation: "RefreshToken([FromBody] ApiObjectRequest dto, CancellationToken cToken)",
                correlationId: correlationId,
                userId: User?.Identity?.Name);

            //Obtener resultado
            Result = await doTheRefreshToken(dto, cToken);

            //Devuelve el resultado
            return Result;
        }

        /// <summary>
        /// Renueva el token de acceso (JWT) utilizando un refresh token válido.
        /// 
        /// El método valida el refresh token contra la base de datos, revoca el token
        /// anterior y genera un nuevo par de tokens (access token y refresh token).
        /// </summary>
        /// <param name="e">
        /// DTO que contiene el refresh token y, opcionalmente, el access token actual.
        /// </param>
        /// <param name="cToken">
        /// Token de cancelación para abortar la operación si la petición es cancelada.
        /// </param>
        /// <returns>
        /// Resultado de la operación de renovación de tokens, incluyendo los nuevos tokens
        /// en caso de éxito o el error correspondiente en caso de fallo.
        /// </returns>
        private async Task<ApiObjectResponse> doTheRefreshToken(ApiObjectRequest e, CancellationToken cToken)
        {
            //Log
            await _logger.AddAsync(new log_entry
            {
                level = "Information",
                source = "TFCiclo.Api.AuthController",
                operation = "doTheRefreshToken(ApiObjectRequest e, CancellationToken cToken)",
                message = "Entrado en la función"
            });

            //Varibales y objetos
            ModelRefreshToken refreshToken = null;
            bool exitoRevoke = false;
            string secret = _configuration["JwtSecret"]!;

            //Comrpuebo si falta algo
            if (string.IsNullOrEmpty(e.RefreshToken))
            {
                return new ApiObjectResponse(false, string.Empty, 400, "Bad Request");
            }

            //Hasheo el refresh token recibido para buscarlo en DB
            string refreshTokenHash = PasswordHelper.HashRefreshToken(e.RefreshToken);

            //Busco el refresh token en DB activo
            refreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(refreshTokenHash, cToken);

            //Si no es válido devuelvo Unauthorized
            if (refreshToken == null)
            {
                //Registro el intento de uso de un refresh token no válido
                await _logger.AddAsync(new log_entry
                {
                    level = "Warning",
                    source = "TFCiclo.Api.AuthController",
                    operation = "doTheRefreshToken(ApiObjectRequest e, CancellationToken cToken)",
                    message = "Intento de uso de refresh token no válido",
                    userId = e.user_info.id.ToString()
                });

                return new ApiObjectResponse(false, string.Empty, 401, "Unauthorized");
            }

            //Obtengo el username de la DB
            string? username = await  _refreshTokenRepository.GetUsernameById(refreshToken.user_id, cToken);

            //Me aseguro de no generar un jwt no válido
            if (username == null)
                return new ApiObjectResponse(false, string.Empty, 401, "Unauthorized");

            //Genera el jwt temporal nuevo a devolver(2 horas)
            string newJwtHash = JWTValidation.GenerateHashJwt(username, "User", secret);

            //Genero el nuevo refresh token
            string newRefreshToken = ApiHelper.GenerateNewToken();

            //Hasheo el nuevo refresh token
            string newRefreshTokenHash = PasswordHelper.HashRefreshToken(newRefreshToken);

            ModelRefreshToken newRefreshTokenObj = new ModelRefreshToken()
            {
                user_id = refreshToken.user_id,
                created_at = DateTime.UtcNow,
                expires_at = DateTime.UtcNow.AddDays(7),
                created_by_ip = "ipAddress",
                token_hash = newRefreshTokenHash
            };

            //Guardo el nuevo refresh token
            int exitoInsert = await _refreshTokenRepository.InsertRefreshTokenAsync(newRefreshTokenObj, cToken);

            //Actualizo el refresh token
            exitoRevoke = await _refreshTokenRepository.RevokeRefreshTokenAsync(refreshToken.token_hash, "revokedByIp", newRefreshTokenHash, cToken);


            if (exitoRevoke == true && exitoInsert >= 0) //Todo Ok
            {
                //Si todo ok devuelvo el nuevo refresh token junto a el token normal
                TokensResponse response = new TokensResponse()
                {
                    accessToken = newJwtHash,
                    refreshToken = newRefreshToken
                };

                return new ApiObjectResponse(true, response, 0, string.Empty);
            }
            else //Fallo algo
            {
                return new ApiObjectResponse(false, string.Empty, 0, string.Empty);
            }
        }
        #endregion
    }
}