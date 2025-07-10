using DragonBallBattles.Application.Interfaces;
using DragonBallBattles.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DragonBallBattles.Application.Services
{
    public class BattleSchedulingService
    {
        private readonly IDragonBallApiClient _apiClient;
        private readonly ILogger _logger;

        public BattleSchedulingService(IDragonBallApiClient apiClient, ILogger logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("BattleSchedulingService inicializado.");
        }

        public async Task<List<ClientBattleOutput>> GenerateSchedule(int numberOfParticipants)
        {
            _logger.LogInformation($"Iniciando la generación del cronograma para {numberOfParticipants} participantes.");

            // 1. Validación de Entrada
            if (numberOfParticipants <= 0 || numberOfParticipants > 16 || numberOfParticipants % 2 != 0)
            {
                _logger.LogError($"Validación fallida: Número de participantes ({numberOfParticipants}) no es válido.");
                throw new ArgumentException("El número de participantes debe ser un número par positivo y menor o igual que 16.");
            }
            _logger.LogInformation($"Número de participantes validado: {numberOfParticipants}.");

            // 2. Obtener Personajes a través de la interfaz
            List<Character> characters;
            try
            {
                characters = await _apiClient.GetCharactersAsync(numberOfParticipants);
            }
            catch (Exception ex)
            {
                _logger.LogError("Fallo al obtener personajes de la API externa.", ex);
                throw new InvalidOperationException("No se pudieron obtener los personajes debido a un error en la comunicación con la API.", ex);
            }

            if (characters == null || characters.Count == 0)
            {
                _logger.LogError("La API devolvió una lista de personajes nula o vacía.");
                throw new InvalidOperationException("No se pudieron obtener los personajes de la API o la lista está vacía.");
            }

            // Ajustar el número de participantes si la API devuelve menos de lo solicitado
            if (characters.Count < numberOfParticipants)
            {
                _logger.LogWarning($"La API devolvió {characters.Count} personajes, menos de los {numberOfParticipants} solicitados.");
                numberOfParticipants = characters.Count;
                if (numberOfParticipants % 2 != 0)
                {
                    numberOfParticipants--;
                    characters = characters.Take(numberOfParticipants).ToList();
                    _logger.LogWarning($"El número de participantes se ajustó a {numberOfParticipants} (impar original).");
                }
            }
            if (numberOfParticipants < 2)
            {
                 _logger.LogError($"No hay suficientes personajes disponibles ({numberOfParticipants}) para crear batallas.");
                 throw new InvalidOperationException("No hay suficientes personajes disponibles para crear batallas.");
            }
            _logger.LogInformation($"Obtenidos {characters.Count} personajes de la API.");


            // 3. Emparejamientos Aleatorios
            _logger.LogInformation("Realizando emparejamientos aleatorios.");
            var random = new Random();
            var shuffledCharacters = characters.OrderBy(c => random.Next()).ToList();

            var allBattles = new List<Battle>();
            for (int i = 0; i < shuffledCharacters.Count; i += 2)
            {
                if (i + 1 < shuffledCharacters.Count)
                {
                    allBattles.Add(new Battle
                    {
                        Fighter1 = shuffledCharacters[i],
                        Fighter2 = shuffledCharacters[i + 1]
                    });
                }
            }
            _logger.LogInformation($"Se generaron {allBattles.Count} batallas individuales.");

            // 4. Asignación de Fechas y Construcción del Resultado Final
            var finalClientOutput = new List<ClientBattleOutput>();
            DateTime currentBattleDate = DateTime.Now.AddDays(30);
            _logger.LogInformation($"Fecha de inicio de batallas calculada: {currentBattleDate.ToShortDateString()}.");

            int battleIndex = 0;
            while (battleIndex < allBattles.Count)
            {
                // Primera batalla del día
                var battle1 = allBattles[battleIndex];
                finalClientOutput.Add(new ClientBattleOutput
                {
                    batalla = $"{battle1.Fighter1.Name} vs {battle1.Fighter2.Name}",
                    fecha = currentBattleDate.ToString("yyyy/MM/dd")
                });
                _logger.LogInformation($"Agregada batalla: {battle1.Fighter1.Name} vs {battle1.Fighter2.Name} para la fecha {currentBattleDate.ToShortDateString()}.");
                battleIndex++;

                // Segunda batalla del día (si existe)
                if (battleIndex < allBattles.Count)
                {
                    var battle2 = allBattles[battleIndex];
                    finalClientOutput.Add(new ClientBattleOutput
                    {
                        batalla = $"{battle2.Fighter1.Name} vs {battle2.Fighter2.Name}",
                        fecha = currentBattleDate.ToString("yyyy/MM/dd")
                    });
                    _logger.LogInformation($"Agregada batalla: {battle2.Fighter1.Name} vs {battle2.Fighter2.Name} para la fecha {currentBattleDate.ToShortDateString()}.");
                    battleIndex++;
                }

                currentBattleDate = currentBattleDate.AddDays(1);
            }

            _logger.LogInformation("Generación del cronograma completada exitosamente.");
            return finalClientOutput;
        }
    }
}