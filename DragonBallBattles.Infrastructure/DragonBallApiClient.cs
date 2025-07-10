using DragonBallBattles.Application.Interfaces;
using DragonBallBattles.Domain;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DragonBallBattles.Infrastructure
{
    public class DragonBallApiClient : IDragonBallApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private const string ApiBaseUrl = "https://dragonball-api.com/api/characters";

        public DragonBallApiClient(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("DragonBallApiClient inicializado.");
        }

        public async Task<List<Character>> GetCharactersAsync(int limit)
        {
            string requestUrl = $"{ApiBaseUrl}?page=1&limit={limit}";
            _logger.LogInformation($"Realizando llamada a la API: GET {requestUrl}");

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Respuesta exitosa de la API para {requestUrl}.");

                using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                {
                    if (doc.RootElement.TryGetProperty("items", out JsonElement itemsElement))
                    {
                        var characters = new List<Character>();
                        foreach (JsonElement item in itemsElement.EnumerateArray())
                        {
                            if (item.TryGetProperty("id", out JsonElement idElement) &&
                                item.TryGetProperty("name", out JsonElement nameElement))
                            {
                                characters.Add(new Character
                                {
                                    Id = idElement.GetInt32(),
                                    Name = nameElement.GetString()
                                });
                            }
                        }
                        _logger.LogInformation($"Parseados {characters.Count} personajes de la respuesta.");
                        return characters;
                    }
                }
                _logger.LogWarning("La respuesta de la API no contiene la propiedad 'items' o está vacía.");
                return new List<Character>();
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Error de red o API al consumir: {requestUrl}", e);
                throw;
            }
            catch (JsonException e)
            {
                _logger.LogError($"Error al parsear el JSON de: {requestUrl}", e);
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error inesperado en DragonBallApiClient al obtener {requestUrl}", e);
                throw;
            }
        }
    }
}