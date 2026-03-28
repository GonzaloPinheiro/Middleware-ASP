using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFCiclo.Api.Controllers.Base;
using TFCiclo.Infrastructure.ApiObjects;
using TFCiclo.Domain.Entities;
using TFCiclo.Infrastructure.Repositories;
using TFCiclo.Infrastructure.Security;
using TFCiclo.Infrastructure.Observability;

namespace TFCiclo.Api.Controllers
{
    /// <summary>
    /// Controlador responsable de las operaciones de autoservicio del usuario autenticado.
    ///
    /// Expone endpoints que permiten al usuario consultar y actualizar su propia información personal,
    /// como datos de perfil o credenciales, así como obtener información asociada a su cuenta (por ejemplo,
    /// el rol actualmente asignado) únicamente en modo lectura.
    /// </summary>
    public class UsersController : ApiControllerBase
    {
        //Variables y objetos
        private readonly Logger _logger;
        private readonly UserRepository _userRepository;

        #region Constructores
        public UsersController(UserRepository userRepository, Logger logger)
        {
            _logger = logger;
            _userRepository = userRepository;
        }
        #endregion

        #region ChangeUsername
        [Authorize(Roles = "user,premium_user,admin")]
        [HttpPost]
        [Route("api/user/ChangeUsername")]
        public async Task<ApiObjectResponse> ChangeUsername([FromBody] ApiObjectRequest dto, CancellationToken cToken)
        {
            //Variables y objetos
            string correlationId = GetCorrelationId();
            ApiObjectResponse Result = new ApiObjectResponse();
            int userId = getUserId();

            // Comienza scope: registra entrada automáticamente y registrará salida al finalizar using.
            await using OperationLogScope scope = _logger.BeginScope(
                source: "TFCiclo.Api.Controllers.UsersController",
                operation: "ChangeUsername([FromBody] ApiObjectRequest dto, CancellationToken cToken)",
                correlationId: correlationId,
                userId: userId.ToString());

            //Obtener resultado
            Result = await changeUsernameAsync(dto, cToken);

            //Devuelve el resultado
            return Result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task<ApiObjectResponse> changeUsernameAsync(ApiObjectRequest e, CancellationToken cToken)
        {
            await _logger.AddAsync(new log_entry
            {
                level = "Information",
                source = "TFCiclo.Api.UsersController",
                operation = "changePasswordAsync(CancellationToken cToken)",
                message = "Entrado en la función"
            });

            //Variables
            string username;
            int userId = getUserId();

            //Obtengo el nombre actual del usuario
            username = await _userRepository.GetUsernameByIdAsync(userId, cToken);


            //Compruebo que el nuevo nombre no sea el mismo que el antiguo
            if (username == e.user_info.username)
                throw new ArgumentException("El nombre de usuario es el mismo que el actual");

            //Actualizo el nombre
            await _userRepository.UpdateUsernameByIdAsync(userId, e.user_info.username, cToken);

            //Revoco los tokens antiguos? TODO

            //Todo Ok
            return new ApiObjectResponse(true, null, 0, "");
        }
        #endregion

        #region  ChangePassword
        [Authorize(Roles = "user,premium_user,admin")]
        [HttpPost]
        [Route("api/user/ChangePassword")]
        public async Task<ApiObjectResponse> ChangePassword([FromBody] ApiObjectRequest dto, CancellationToken cToken)
        {
            //Variables y objetos
            string correlationId = GetCorrelationId();
            ApiObjectResponse Result = new ApiObjectResponse();
            int userId = getUserId();

            // Comienza scope: registra entrada automáticamente y registrará salida al finalizar using.
            await using OperationLogScope scope = _logger.BeginScope(
                source: "TFCiclo.Api.Controllers.UsersController",
                operation: "ChangePassword([FromBody] ApiObjectRequest dto, CancellationToken cToken)",
                correlationId: correlationId,
                userId: userId.ToString());

            //Obtener resultado
            Result = await changePasswordAsync(dto, cToken);

            //Devuelve el resultado
            return Result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        private async Task<ApiObjectResponse> changePasswordAsync(ApiObjectRequest e, CancellationToken cToken)
        {
            await _logger.AddAsync(new log_entry
            {
                level = "Information",
                source = "TFCiclo.Api.UsersController",
                operation = "changePasswordAsync(CancellationToken cToken)",
                message = "Entrado en la función"
            });

            //Variables
            string? encryptedPassword = null;
            int userId = getUserId();

            //Hashear la contraseña
            encryptedPassword = PasswordHelper.HashPassword(e.user_info.user_password);

            //Cambiar contraseña
            await _userRepository.UpdatePasswordByIdAsync(userId, encryptedPassword, cToken);

            //Todo Ok
            return new ApiObjectResponse(true, null, 0, "");
        }
        #endregion
    }
}