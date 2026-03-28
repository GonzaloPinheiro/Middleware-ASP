
namespace TFCiclo.Domain.Exceptions
{
    /// <summary>
    /// Se lanza cuando se intenta asignar o consultar un rol que no existe en la base de datos
    /// </summary>
    public class RoleNotFoundException : Exception
    {
        /// <summary>
        /// ID del rol que no se encontró
        /// </summary>
        public int RoleId { get; }
        

        public RoleNotFoundException(int roleId)
            : base($"El rol con ID {roleId} no existe en la base de datos")
        {
            RoleId = roleId;
        }
    }
}
