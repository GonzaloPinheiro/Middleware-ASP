using Microsoft.AspNetCore.Mvc;
using TFCiclo.Api.Controllers.Base;

namespace TFCiclo.Api.Controllers
{
    /// <summary>
    /// Controlador para comprobar conexión
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ApiControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = 200,
                message = "Api TFCiclo funciona correctamente"
            }
            );
        }
    }
}
