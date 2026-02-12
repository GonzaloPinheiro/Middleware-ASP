using TFCiclo.Data.ApiObjects;
using TFCiclo.Data.Exceptions;
using TFCiclo.Data.Models;
using TFCiclo.Data.Services;

namespace TFCiclo.Api.Middleware
{
    /// <summary>
    /// Middleware global que captura todas las excepciones no manejadas
    /// y las convierte en respuestas HTTP apropiadas
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        //Delegado que apunta al siguiente middleware en el pipeline
        private readonly RequestDelegate _next;

        //Logger para registrar las excepciones
        private readonly Logger _logger;

        #region Constructores
        public GlobalExceptionHandlerMiddleware(RequestDelegate next, Logger logger)
        {
            _next = next;
            _logger = logger;
        }
        #endregion

        /// <summary>
        /// Método que ASP.NET Core llama automáticamente para cada request
        /// </summary>
        /// <param name="context">Contexto HTTP con toda la info del request/response</param>
        public async Task InvokeAsync(HttpContext context)
        {
            // Variables y objetos
            string? correlationId;
            string path = context.Request.Path;
            string method = context.Request.Method;


            // Verificar si ya existe un correlationId
            if (context.Items.ContainsKey("CorrelationId") && context.Items["CorrelationId"] != null)
            {
                // Ya existe, usarlo
                correlationId = context.Items["CorrelationId"]!.ToString();
            }
            else
            {
                // No existe, generar uno nuevo
                correlationId = Guid.NewGuid().ToString();
                context.Items["CorrelationId"] = correlationId;
            }


            try
            {
                //Esto es lo que hace que el request siga su camino normal a través del pipeline
                await _next(context);
            }
            catch (RoleNotFoundException ex) //Se intentó asignar un rol que no existe  
            {
                // 1. Loguear (nivel Warning porque es un error esperado)
                await _logger.AddAsync(new log_entry
                {
                    level = "Warning",
                    source = "TFCiclo.Api.Middleware.GlobalExceptionHandlerMiddleware",
                    operation = "InvokeAsync",
                    message = $"Rol no encontrado. RoleId: {ex.RoleId}, Path: {path}, Method: {method}",
                    exception = ex.ToString(),
                    correlationId = correlationId!
                });

                // 2. Preparar respuesta para el cliente
                var response = new ApiObjectResponse(
                    result: false,
                    data: null,
                    error_code: 1, // Error de cliente (datos inválidos)
                    error_message: $"El rol con ID {ex.RoleId} no existe"
                );

                // 3. Enviar respuesta HTTP
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status400BadRequest, // HTTP 400 Bad Request
                    response
                );
            }
            catch (UserNotFoundException ex)//El usuario objetivo no existe o no tiene rol 
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Warning",
                    source = "TFCiclo.Api.Middleware.GlobalExceptionHandlerMiddleware",
                    operation = "InvokeAsync",
                    message = $"Usuario no encontrado. UserId: {ex.UserId}, Path: {path}, Method: {method}",
                    correlationId = correlationId!,
                    exception = ex.ToString()
                });

                var response = new ApiObjectResponse(
                    result: false,
                    data: null,
                    error_code: 1,
                    error_message: $"El usuario con ID {ex.UserId} no tiene rol asignado o no existe"
                );

                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status404NotFound, // HTTP 404 Not Found
                    response
                );
            }
            catch (ArgumentException ex) //Parámetros inválidos (IDs <= 0, nulls, etc.)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Warning",
                    source = "TFCiclo.Api.Middleware.GlobalExceptionHandlerMiddleware",
                    operation = "InvokeAsync",
                    message = $"Argumentos inválidos. ParamName: {ex.ParamName}, Message: {ex.Message}, Path: {path}",
                    correlationId = correlationId!,
                    exception = ex.ToString()
                });

                var response = new ApiObjectResponse(
                    result: false,
                    data: null,
                    error_code: 1,
                    error_message: "Los datos proporcionados son inválidos. Message: " + ex.Message
                );

                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status400BadRequest, // HTTP 400
                    response
                );
            }
            catch (DatabaseOperationException ex) //Error de infraestructura de BD
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Error",
                    source = "TFCiclo.Api.Middleware.GlobalExceptionHandlerMiddleware",
                    operation = "InvokeAsync",
                    message = $"Error de base de datos. Operation: {ex.Operation}, Path: {path}, Method: {method}",
                    correlationId = correlationId!,
                    exception = ex.ToString()
                });

                var response = new ApiObjectResponse(
                    result: false,
                    data: null,
                    error_code: 2, // Error de servidor
                    error_message: "Error interno del servidor. Por favor intente más tarde"
                );

                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status500InternalServerError, // HTTP 500
                    response
                );
            }
            catch (OperationCanceledException ex) //Cliente canceló la petición 
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Api.Middleware.GlobalExceptionHandlerMiddleware",
                    operation = "InvokeAsync",
                    message = $"Operación cancelada. Path: {path}, Method: {method}",
                    correlationId = correlationId!,
                    exception = ex.ToString()
                });

                var response = new ApiObjectResponse(
                    result: false,
                    data: null,
                    error_code: 1,
                    error_message: "La operación fue cancelada"
                );

                // HTTP 499 "Client Closed Request"
                await HandleExceptionAsync(
                    context,
                    499,
                    response
                );
            }
            catch (WeatherNotFoundException ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Api.Middleware.WeatherNotFoundExceprion",
                    operation = "InvokeAsync",
                    message = $"Ubicación no encontrada en base de datos- City -> {ex.City}. Country -> {ex.Country}",
                    correlationId = correlationId!,
                    exception = ex.ToString()
                });

                var response = new ApiObjectResponse(
                    result: false,
                    data: null,
                    error_code: 1,
                    error_message: $"Ubicación no encontrada en base de datos- City -> {ex.City}. Country -> {ex.Country}"
                );

                // HTTP 400
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    response
                );
            }
            catch (RefreshTokenNotFoundException ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Info",
                    source = "TFCiclo.Api.Middleware.RefreshTokenNotFoundException",
                    operation = "InvokeAsync",
                    message = "Refresh token no encontrado",
                    correlationId = correlationId!,
                    exception = ex.ToString()
                });

                var response = new ApiObjectResponse(
                    result: false,
                    data: null,
                    error_code: 1,
                    error_message: "Refresh token no encontrado"
                );

                // HTTP 404
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status404NotFound,
                    response
                );
            }
            catch (InternalInfoNotFoundException ex)
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Api.Middleware.InternalInfoNotFoundException",
                    operation = "InvokeAsync",
                    message = $"Error -> {ex.Error}.  Path: {path}, Method: {method}",
                    correlationId = correlationId!,
                    exception = ex.ToString()
                });

                var response = new ApiObjectResponse(
                    result: false,
                    data: null,
                    error_code: 500,
                    error_message: $"Ineternal server error"
                );

                // HTTP 500
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    response
                );
            }
            catch (Exception ex) //Cualquier otra excepción no manejada  
            {
                await _logger.AddAsync(new log_entry
                {
                    level = "Critical",
                    source = "TFCiclo.Api.Middleware.GlobalExceptionHandlerMiddleware",
                    operation = "InvokeAsync",
                    message = $"Excepción no controlada. Type: {ex.GetType().Name}, Path: {path}, Method: {method}, " +
                    $"Message: {ex.Message}",
                    correlationId = correlationId!,
                    exception = ex.ToString()
                });

                var response = new ApiObjectResponse(
                    result: false,
                    data: null,
                    error_code: 2,
                    error_message: "Error inesperado del servidor"
                );

                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    response
                );
            }
        }

        #region Helpers
        /// <summary>
        /// Escribe la respuesta JSON al cliente
        /// </summary>
        /// <param name="context">Contexto HTTP</param>
        /// <param name="statusCode">Código HTTP a devolver (400, 404, 500, etc.)</param>
        /// <param name="response">Objeto ApiObjectResponse a serializar como JSON</param>
        private static async Task HandleExceptionAsync(HttpContext context, int statusCode, ApiObjectResponse response)
        {
            //Headers y status de la respuesta
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            //Sertializar y escribir la respuesta JSON
            await context.Response.WriteAsJsonAsync(response);
        }
        #endregion
    }
}