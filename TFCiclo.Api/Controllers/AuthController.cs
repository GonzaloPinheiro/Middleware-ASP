using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TFCiclo.Api.Controllers.Base;
using TFCiclo.Data.ApiObjects;
using TFCiclo.Data.Exceptions;
using TFCiclo.Data.Models;
using TFCiclo.Data.Models.TFCiclo.Data.Models;
using TFCiclo.Data.Repositories;
using TFCiclo.Data.Security;
using TFCiclo.Data.Services;

namespace TFCiclo.Api.Controllers
{
    [ApiController]
    public class AuthController : ApiControllerBase
    {
        //Variables y objetos
        private readonly UserRepository _userRepository;
        private readonly user_rolesRepository _user_rolesRepository;
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly Logger _logger;
        private readonly IConfiguration _configuration;

        #region Constructores
        public AuthController(UserRepository userRepository, user_rolesRepository user_rolesRepository, RefreshTokenRepository refreshTokenRepository, Logger logger, IConfiguration config)
        {
            _userRepository = userRepository;
            _user_rolesRepository = user_rolesRepository;
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
            string correlationId = GetCorrelationId();
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
            });

            //Variables y objetos
            user_info? user = null;
            string jwtToken = string.Empty;

            //Obtengo el usuario
            user = await _userRepository.GetUserByUserNameAsync(e.user_info.username, cToken);

            //Compruebo la contraseña
            if (!PasswordHelper.VerifyPassword(e.user_info.user_password, user!.user_password))
                return new ApiObjectResponse(false, string.Empty, 401, "Unauthorized");


            //Obtengo el secret para generar el jwt
            string secret = _configuration["JwtSecret"]!;

            //Me aseguro de no generar un jwt no válido
            if (string.IsNullOrEmpty(secret))
                throw new InternalInfoNotFoundException("Secreto con valor inválido");

            if (string.IsNullOrEmpty(user.username))
                throw new ArgumentException("El username recibido es null o vacío");


            //Obtengo los roles del usuario
            IEnumerable<roles>? userRoles = await _user_rolesRepository.GetRolesFromUserAsync(user.id, cToken);

            //Genera el jwt a devolver
            jwtToken = JWTValidation.GenerateJwt(user.id, userRoles!.Select(r => r.name), secret);

            //Genero el refresh token
            string refreshToken = ApiHelper.GenerateNewToken();

            //Creo el refresh token hasheado para guardar en DB
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
            await _refreshTokenRepository.InsertRefreshTokenAsync(newRefreshTokenObj, cToken);

            TokensResponse response = new TokensResponse()
            {
                accessToken = jwtToken,
                refreshToken = refreshToken
            };

            //Todo Ok TODO crear objeto con token y refresh token
            return new ApiObjectResponse(true, response, 0, string.Empty);
        }
        #endregion

        #region api/Auth/Register
        [HttpPost]
        [Route("api/Auth/Register")]
        [EnableRateLimiting("strict-auth")]
        public async Task<ApiObjectResponse> Register([FromBody] ApiObjectRequest dto, CancellationToken cToken)
        {
            //Variables y objetos
            string correlationId = GetCorrelationId();
            ApiObjectResponse Result = new ApiObjectResponse();


            // Comienza scope: registra entrada automáticamente y registrará salida al finalizar using.
            await using OperationLogScope scope = _logger.BeginScope(
                source: "TFCiclo.Api.Controllers.AuthController",
                operation: "Register([FromBody] ApiObjectRequest dto)",
                correlationId: correlationId,
                userId: User?.Identity?.Name ?? "Unknown");

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
            });

            //Variables y objetos
            string encryptedPassword = string.Empty;
            int newId = 0;
            int defaultRoleId = -1;
            user_info? user = null;

            //Compruebo si ya existe un usuario con el mismo nombre
            if (await _userRepository.CheckExistingUserAsync(e.user_info.username, cToken))
                throw new ArgumentException("Ya existe un usuario con el nombre solicitado");

            //TODO falta comprobar la información recibida

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

            //Obtengo el rol por defecto
            defaultRoleId = await _user_rolesRepository.GetRoleByNameAsync("user", cToken);

            //Asigno el rol por defecto al nuevo usuario
            int resultadoInsert = await _user_rolesRepository.InsertRoleToUserAsync(newId, defaultRoleId, cToken);

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
            string correlationId = GetCorrelationId();
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
            //bool exitoRevoke = false;
            string secret = _configuration["JwtSecret"]!;

            //Comrpuebo si falta algo
            if (string.IsNullOrEmpty(e.RefreshToken) || e.RefreshToken.Length > 255)
                throw new ArgumentException("Es requerido recibir el RefreshToken");

            //Hasheo el refresh token recibido para buscarlo en DB
            string refreshTokenHash = PasswordHelper.HashRefreshToken(e.RefreshToken);

            //Busco el refresh token en DB activo
            refreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(refreshTokenHash, cToken);

            //TODO: Asegurarme de que no hace falta en el flujo del controlador
            //Obtengo el username de la DB
            //string? username = await _refreshTokenRepository.GetUsernameById(refreshToken.user_id, cToken);

            ////Me aseguro de no generar un jwt no válido
            //if (username == null)
            //    return new ApiObjectResponse(false, string.Empty, 401, "Unauthorized");

            //Obtengo los roles del usuario
            IEnumerable<roles>? userRoles = await _user_rolesRepository.GetRolesFromUserAsync(refreshToken.user_id, cToken);

            //Compruebo si tiene roles asignados
            if (!userRoles!.Any())
            {
                throw new UserNotFoundException(refreshToken.user_id);
            }

            //Genera el jwt temporal nuevo a devolver
            string newJwt = JWTValidation.GenerateJwt(refreshToken.user_id, userRoles.Select(r => r.name), secret);

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
            await _refreshTokenRepository.InsertRefreshTokenAsync(newRefreshTokenObj, cToken);

            //Actualizo el refresh token
            await _refreshTokenRepository.RevokeRefreshTokenAsync(refreshToken.token_hash, "revokedByIp", newRefreshTokenHash, cToken);


            //Si todo ok devuelvo el nuevo refresh token junto a el token normal
            TokensResponse response = new TokensResponse()
            {
                accessToken = newJwt,
                refreshToken = newRefreshToken
            };

            return new ApiObjectResponse(true, response, 0, string.Empty);
        }
        #endregion
    }
}