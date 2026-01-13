using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace TFCiclo.Data.Security
{
    public static class JWTValidation
    {
        /// <summary>
        /// Genera un JWT firmado usando HMAC SHA256
        /// </summary>
        /// <param name="username"></param>
        /// <param name="role"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static string GenerateHashJwt(string username, string role, string secretKey)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            //Genero el token
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: "tfciclo",
                audience: "tfciclo",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            //Devolver resultado
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static ClaimsPrincipal ValidateJwt(string token, string secretKey)
        {
            //Variables y objetos
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.UTF8.GetBytes(secretKey);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "tfciclo",
                    ValidAudience = "tfciclo",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Opcional: para evitar problemas de sincronización de tiempo
                };

                //Valida el token
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                //Devuelve el ClaimsPrincipal si el token es válido
                return principal;
            }
            catch
            {
                // Token inválido
                return null;
            }
        }
    }
}