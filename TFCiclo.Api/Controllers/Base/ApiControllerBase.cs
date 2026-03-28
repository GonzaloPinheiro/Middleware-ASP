using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TFCiclo.Api.Controllers.Base
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Obtiene el correlationId del request actual (generado por el middleware)
        /// </summary>
        protected string GetCorrelationId()
        {
            return HttpContext.Items["CorrelationId"]?.ToString();
        }

        /// <summary>
        /// Devuelve el userId del jwt del usuario autenticado.
        /// El campo "sub" con el id esta validado en el program.cs, por lo que se asume que siempre estará presente y es un entero válido.
        /// </summary>
        /// <returns></returns>
        protected int getUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value); //Sub se valida en program.cs
        }
    }
}
