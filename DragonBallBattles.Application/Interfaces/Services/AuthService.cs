using DragonBallBattles.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Collections.Generic;

namespace DragonBallBattles.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        // Inyectamos IConfiguration para leer los valores de appsettings.json
        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Implementación del método GenerateToken de IAuthService
        public string GenerateToken(string username, string password)
        {
            
            // Credenciales de prueba de Postman:
            const string ValidUsername = "admin";
            const string ValidPassword = "batallas123";

            if (username != ValidUsername || password != ValidPassword)
            {
                return null; 
            }

            // Obtener configuración JWT de appsettings.json
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new InvalidOperationException("Configuración JWT incompleta en appsettings.json.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, jwtIssuer),
                new Claim(JwtRegisteredClaimNames.Aud, jwtAudience),
            };

            // Crear el descriptor del token
            var tokenDescriptor = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // Token válido por 2 horas
                signingCredentials: credentials);

            // Generar el token como una cadena
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}