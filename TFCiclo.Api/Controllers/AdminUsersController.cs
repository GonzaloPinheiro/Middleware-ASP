using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFCiclo.Api.Controllers.Base;
using TFCiclo.Infrastructure.ApiObjects;
using TFCiclo.Domain.Entities;
using TFCiclo.Infrastructure.Repositories;
using TFCiclo.Infrastructure.Observability;

namespace TFCiclo.Api.Controllers
{
    /// <summary>
    /// Controlador administrativo responsable de la gestión de usuarios desde el punto de vista de roles y
    /// operaciones de alto impacto.
    ///
    /// Este controlador expone exclusivamente endpoints destinados a usuarios con rol de Administrador y
    /// permite realizar acciones que afectan a otros usuarios del sistema, como la consulta de usuarios y la
    /// modificación del rol asignado a un usuario.
    /// </summary>
    public class AdminUsersController : ApiControllerBase
    {
        //Variables y objetos
        private readonly Logger _logger;
        private readonly UserRepository _userRepository;
        private readonly user_rolesRepository _user_RolesRepository;

        #region Constructores
        public AdminUsersController(user_rolesRepository user_RolesRepository, UserRepository userRepository, Logger logger)
        {
            _logger = logger;
            _userRepository = userRepository;
            _user_RolesRepository = user_RolesRepository;
        }
        #endregion

        #region GetUsersList
        /// <summary>
        /// Se encarga de dar información de solo lectura sobre los usuarios registrados
        /// </summary>
        /// <param name="cToken"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin")]
        [HttpPost]
        [Route("api/admin/GetUsersList")]
        public async Task<ApiObjectResponse> GetUsersList(CancellationToken cToken)
        {
            //Variables y objetos
            string correlationId = GetCorrelationId();
            ApiObjectResponse Result = new ApiObjectResponse();
            string username = User.Identity?.Name ?? "Unknown"; // username del JWT

            // Comienza scope: registra entrada automáticamente y registrará salida al finalizar using.
            await using OperationLogScope scope = _logger.BeginScope(
                source: "TFCiclo.Api.Controllers.AdminUsersController",
                operation: "GetUsersList([FromBody] ApiObjectRequest dto, CancellationToken cToken)",
                correlationId: correlationId,
                userId: username);

            //Obtener resultado
            Result = await getUsersListAsync(cToken);

            //Devuelve el resultado
            return Result;
        }

        /// <summary>
        /// Obtiene el listado de usuarios del sistema
        /// </summary>
        /// <param name="cToken"></param>
        /// <returns></returns>
        private async Task<ApiObjectResponse> getUsersListAsync(CancellationToken cToken)
        {
            await _logger.AddAsync(new log_entry
            {
                level = "Information",
                source = "TFCiclo.Api.AdminUsersController",
                operation = "getUsersListAsync(CancellationToken cToken)",
                message = "Entrado en la función"
            });

            //Variables
            List<user_info>? usersList = new List<user_info>();

            //Obtengo el listado de usuarios
            usersList = await _userRepository.GetListOfUsersAsync(cToken);

            //Compruebo si ha habido error
            if (usersList == null)
                return new ApiObjectResponse(false, usersList, 1, "Error al obtener la lista de usarios");

            //Todo Ok
            return new ApiObjectResponse(true, usersList, 0, "");
        }
        #endregion


        #region GetUsersById
        [Authorize]
        [Authorize(Roles = "admin")]
        [HttpPost]
        [Route("api/admin/GetUsersById")]
        public async Task<ApiObjectResponse> GetUsersById(ApiObjectRequest dto, CancellationToken cToken)
        {
            //Variables y objetos
            string correlationId = GetCorrelationId();
            ApiObjectResponse Result = new ApiObjectResponse();
            string username = User.Identity?.Name ?? "Unknown"; // username del JWT

            // Comienza scope: registra entrada automáticamente y registrará salida al finalizar using.
            await using OperationLogScope scope = _logger.BeginScope(
                source: "TFCiclo.Api.Controllers.AdminUsersController",
                operation: "GetUsersById(ApiObjectRequest dto, CancellationToken cToken)",
                correlationId: correlationId,
                userId: username);

            //Obtener resultado
            Result = await getUserByIdAsync(dto, cToken);

            //Devuelve el resultado
            return Result;
        }

        /// <summary>
        /// Obtiene la información de un usuario por su Id
        /// </summary>
        /// <param name="e"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        private async Task<ApiObjectResponse> getUserByIdAsync(ApiObjectRequest e, CancellationToken cToken)
        {
            await _logger.AddAsync(new log_entry
            {
                level = "Information",
                source = "TFCiclo.Api.AdminUsersController",
                operation = "getUserByIdAsync(ApiObjectRequest e, CancellationToken cToken)",
                message = "Entrado en la función"
            });

            //Variables
            user_info? user = null;

            //Obtengo el listado de usuarios
            user = await _userRepository.GetUserByIdAsync(e.user_info, cToken);

            //Todo Ok
            return new ApiObjectResponse(true, user, 0, "");
        }
        #endregion


        #region ChangeRole
        [Authorize(Roles = "admin")]
        [HttpPost]
        [Route("api/admin/ChangeRole")]
        public async Task<ApiObjectResponse> ChangeRole(ApiObjectRequest dto, CancellationToken cToken)
        {
            //Variables y objetos
            string correlationId = GetCorrelationId();
            ApiObjectResponse Result = new ApiObjectResponse();
            string username = User.Identity?.Name ?? "Unknown"; // username del JWT

            // Comienza scope: registra entrada automáticamente y registrará salida al finalizar using.
            await using OperationLogScope scope = _logger.BeginScope(
                source: "TFCiclo.Api.Controllers.AdminUsersController",
                operation: "ChangeRole(ApiObjectRequest dto, CancellationToken cToken)",
                correlationId: correlationId,
                userId: username);

            //Obtener resultado
            Result = await changeRoleAsync(dto, cToken);

            //Devuelve el resultado
            return Result;
        }

        /// <summary>
        /// Cambia el rol del usuario solcitado por uno diferente(A excepción del de el propio usuario)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        private async Task<ApiObjectResponse> changeRoleAsync(ApiObjectRequest e, CancellationToken cToken)
        {
            await _logger.AddAsync(new log_entry
            {
                level = "Information",
                source = "TFCiclo.Api.AdminUsersController",
                operation = "changeRoleAsync(ApiObjectRequest e, CancellationToken cToken)",
                message = "Entrado en la función"
            });

            //Variables
            int userId = getUserId();
            roles? userRole = null;

            //Compruebo que no se intente cambiar el rol del propio usuario
            if (userId == e.user_info.id)
                return new ApiObjectResponse(false, null, 1, "No se puede cambiar el rol del propio usuario");

            //Obtengo el rol actual del usuario
            userRole = await _user_RolesRepository.GetRoleFromUserAsync(e.user_info.id, cToken);

            //Compruebo que no se cambie el rol al mismo que ya tiene
            if (userRole!.id == e.admin_user_Operation.newRoleId)
                throw new ArgumentException("El nuevo rol es el mismo que el actual");

            //Actualizo el rol del usuario
            await _user_RolesRepository.UpdateRoleAsync(e.user_info.id, e.admin_user_Operation.newRoleId, cToken);

            //Todo Ok
            return new ApiObjectResponse(true, null, 0, "Rol actualizado correctamente");
        }
        #endregion

        #region EliminateUser
        [Authorize(Roles = "admin")]
        [HttpPost]
        [Route("api/admin/EliminateUser")]
        public async Task<ApiObjectResponse> EliminateUser(ApiObjectRequest dto, CancellationToken cToken)
        {
            //Variables y objetos
            string correlationId = GetCorrelationId();
            ApiObjectResponse Result = new ApiObjectResponse();
            string username = User.Identity?.Name ?? "Unknown"; // username del JWT

            // Comienza scope: registra entrada automáticamente y registrará salida al finalizar using.
            await using OperationLogScope scope = _logger.BeginScope(
                source: "TFCiclo.Api.Controllers.AdminUsersController",
                operation: "EliminateUser(ApiObjectRequest dto, CancellationToken cToken)",
                correlationId: correlationId,
                userId: username);

            //Obtener resultado
            Result = await eliminateUserAsync(dto, cToken);

            //Devuelve el resultado
            return Result;
        }

        private async Task<ApiObjectResponse> eliminateUserAsync(ApiObjectRequest e, CancellationToken cToken)
        {
            await _logger.AddAsync(new log_entry
            {
                level = "Information",
                source = "TFCiclo.Api.AdminUsersController",
                operation = "eliminateUserAsync(ApiObjectRequest e, CancellationToken cToken)",
                message = "Entrado en la función"
            });

            //Variables
            int userId = getUserId();
            roles targetUserRole;

            //Filtros
            if (e?.user_info == null || e.user_info.id <= 0)
                throw new ArgumentException("Datos del usuario inválidos");

            //Compruebo si el usuario a eliminar es el propio usuario
            if (userId == e.user_info.id)
                throw new ArgumentException("No se puede elminar el propio usuario");

            //Obtengo el rol del usuario a eliminar
            targetUserRole = await _user_RolesRepository.GetRoleFromUserAsync(e.user_info.id, cToken);

            //Commpruebo que el usuario a eliminar no es un admin
            if (targetUserRole.id == 3)
                throw new ArgumentException("No se puede eliminar un usuario con rol de admin");

            //Elimino el usuario
            await _userRepository.DeleteUserByIdAsync(e.user_info.id, cToken);

            //Todo Ok
            return new ApiObjectResponse(true, null, 0, string.Empty);
        }
        #endregion
    }
}
