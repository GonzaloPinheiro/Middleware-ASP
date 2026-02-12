using Microsoft.AspNetCore.Mvc;

namespace TFCiclo.Api.Controllers
{
    /// <summary>
    /// Controlador responsable de las operaciones de autoservicio del usuario autenticado.
    ///
    /// Expone endpoints que permiten al usuario consultar y actualizar su propia información personal,
    /// como datos de perfil o credenciales, así como obtener información asociada a su cuenta (por ejemplo,
    /// el rol actualmente asignado) únicamente en modo lectura.
    /// </summary>
    public class UsersController : Controller
    {

    }
}
