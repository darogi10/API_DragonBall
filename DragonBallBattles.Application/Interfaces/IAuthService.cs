using DragonBallBattles.Domain;
using System.Threading.Tasks;

namespace DragonBallBattles.Application.Interfaces
{
    public interface IAuthService
    {
        string GenerateToken(string username, string password);
    }

}