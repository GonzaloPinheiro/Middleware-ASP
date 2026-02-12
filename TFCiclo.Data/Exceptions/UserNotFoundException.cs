
namespace TFCiclo.Data.Exceptions
{
    /// <summary>
    /// Se lanza cuando se intenta operar sobre un usuario que no existe 
    /// o no tiene registro en la tabla user_roles
    /// </summary>
    public class UserNotFoundException : Exception
    {
        /// <summary>
        /// ID del usuario que no se encontró
        /// </summary>
        public int UserId { get; }

        public UserNotFoundException(int userId)
            : base($"El usuario con ID {userId} no existe o no tiene rol asignado")
        {
            UserId = userId;
        }

        public UserNotFoundException(string message)
            : base(message)
        {
        }
    }
}
