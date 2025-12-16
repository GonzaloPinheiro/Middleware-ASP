using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TFCiclo.Data.ApiObjects;
using TFCiclo.Data.Models;
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
        private readonly Logger _logger;
        private readonly IConfiguration _configuration;

        #region Constructores
        public AuthController(UserRepository userRepository, Logger logger, IConfiguration config)
        {
            _userRepository = userRepository;
            _logger = logger;
            _configuration = config;
        }
        #endregion

        #region api/Auth/Login
        [HttpPost]
        [Route("api/Auth/Login")]
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
                    jwtToken = JWTValidation.GenerateJwt(user.username, "User", secret);

                    //Todo Ok
                    return new ApiObjectResponse(true, jwtToken, 0, string.Empty);
                }
            }

            //Credenciales no son correctas
            return new ApiObjectResponse(false, string.Empty, 401, "Unauthorized");
        }
        #endregion

        #region api/Auth/Register
        [HttpPost]
        [Route("api/Auth/Register")]
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
    }
}