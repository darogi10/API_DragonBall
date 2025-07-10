using DragonBallBattles.Application.Services;
using DragonBallBattles.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using DragonBallBattles.Application.Interfaces;

namespace DragonBallBattles.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ScheduleController : ControllerBase
    {
        private readonly BattleSchedulingService _battleSchedulingService;
        private readonly DragonBallBattles.Application.Interfaces.ILogger _logger;

        public ScheduleController(BattleSchedulingService battleSchedulingService, DragonBallBattles.Application.Interfaces.ILogger logger)
        {
            _battleSchedulingService = battleSchedulingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ClientBattleOutput>>> GetSchedule([FromQuery] int numberOfParticipants)
        {
            _logger.LogInformation($"Solicitud de cronograma recibida para {numberOfParticipants} participantes.");
            try
            {
                var schedule = await _battleSchedulingService.GenerateSchedule(numberOfParticipants);
                _logger.LogInformation("Cronograma generado y enviado al cliente.");
                return Ok(schedule);
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogError($"Error de validación al obtener cronograma: {ex.Message}", ex);
                return BadRequest(new { Message = ex.Message });
            }
            catch (System.InvalidOperationException ex)
            {
                _logger.LogError($"Error operacional al obtener cronograma: {ex.Message}", ex);
                return StatusCode(500, new { Message = ex.Message });
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error inesperado al obtener cronograma: {ex.Message}", ex);
                return StatusCode(500, new { Message = "Ocurrió un error interno del servidor." });
            }
        }
    }
}