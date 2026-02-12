using Microsoft.AspNetCore.Mvc;

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
    }
}
