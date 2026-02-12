
namespace TFCiclo.Data.Exceptions
{
    public class RefreshTokenNotFoundException : Exception
    {
        /// <summary>
        /// Refresh token que no se encontró en DB o no es válido
        /// </summary>
        public string RefreshToken { get; }

        public RefreshTokenNotFoundException(string refreshToken)
            : base($"El refresh token {refreshToken} no es válido")
        {
            RefreshToken = refreshToken;
        }

        public RefreshTokenNotFoundException()
            : base($"El refresh token recibido no es válido")
        {
        }
    }
}
