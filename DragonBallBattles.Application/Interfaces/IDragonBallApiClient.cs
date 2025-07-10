using DragonBallBattles.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DragonBallBattles.Application.Interfaces
{
    public interface IDragonBallApiClient
    {
        Task<List<Character>> GetCharactersAsync(int limit);
    }
}