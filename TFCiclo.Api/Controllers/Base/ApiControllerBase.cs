using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TFCiclo.Api.Controllers.Base
{
    [ApiController]
    [Authorize]
    [EnableRateLimiting("jwt-user")]
    public abstract class ApiControllerBase : ControllerBase
    {
    }
}
