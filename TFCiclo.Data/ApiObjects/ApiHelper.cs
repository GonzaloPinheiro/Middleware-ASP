
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;

namespace TFCiclo.Data.ApiObjects
{
    public class ApiHelper
    {
        #region Métodos static
        public static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value ?? string.Empty;

            return value.Length <= maxLength
                ? value
                : value.Substring(0, maxLength);
        }

        /// <summary>
        /// Genera un nuevo refresh token seguro
        /// </summary>
        /// <returns></returns>
        public static string GenerateNewToken()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
            string newRefreshToken = Convert.ToBase64String(randomBytes);

            return newRefreshToken;
        }
        #endregion
    }
}
