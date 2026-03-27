using HospitalManagementSystem.Core.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace HospitalManagementSystem.Web.Helper
{
    public class JWTTokenManager
    {
        public static string GenerateJWTToken(JWTokenDTO claims)
        {
            // Local variable declaration
            string result = null;

            // Validate input parameters
            if (null == claims)
            {
                return result;
            }

            try
            {
                byte[] privateKeyRaw = { };
                var privateKeyPem = System.IO.File.ReadAllText(@"privatekey.pem");
                privateKeyPem = privateKeyPem.Replace("-----BEGIN PRIVATE KEY-----", "");
                privateKeyPem = privateKeyPem.Replace("-----END PRIVATE KEY-----", "");
                privateKeyRaw = Convert.FromBase64String(privateKeyPem);

                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.ImportPkcs8PrivateKey(new ReadOnlySpan<byte>(privateKeyRaw), out _);
                RsaSecurityKey rsaSecurityKey = new RsaSecurityKey(provider);

                var now = DateTime.Now;
                //var utctime = now.ToUniversalTime();
                var reducedTime = now.AddHours(-5);
                var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();

                var userClaims = new List<Claim>();
                userClaims.Add(new Claim(JwtRegisteredClaimNames.Iat,
                    unixTimeSeconds.ToString()));
                userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()));

                if (null != claims.Issuer)
                    userClaims.Add(new Claim(JwtRegisteredClaimNames.Iss,
                    claims.Issuer));
                if (null != claims.Audience)
                    userClaims.Add(new Claim(JwtRegisteredClaimNames.Aud,
                    claims.Audience));
                if (null != claims.Subject)
                    userClaims.Add(new Claim(JwtRegisteredClaimNames.Sub,
                    claims.Subject));
                if (null != claims.RedirecUri)
                    userClaims.Add(new Claim("redirect_uri", claims.RedirecUri));
                if (null != claims.ResponseType)
                    userClaims.Add(new Claim("response_type", claims.ResponseType));
                if (null != claims.Scope)
                    userClaims.Add(new Claim("scope", claims.Scope));
                if (null != claims.Nonce)
                    userClaims.Add(new Claim("nonce", claims.Nonce));
                if (null != claims.State)
                    userClaims.Add(new Claim("state", claims.State));

                var jwt = new JwtSecurityToken(
                    claims: userClaims.AsEnumerable(),
                    notBefore: reducedTime,
                    expires: now.AddMinutes(claims.Expiry),
                    signingCredentials: new SigningCredentials(rsaSecurityKey,
                    SecurityAlgorithms.RsaSha256)
                );

                result = new JwtSecurityTokenHandler().WriteToken(jwt);
            }
            catch (Exception)
            {
                return null;
            }

            return result;
        }
    }
}
