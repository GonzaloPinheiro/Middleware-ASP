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
        public static string GenerateJwt(string username, string role, string secretKey)
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
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            //Devolver resultado
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
