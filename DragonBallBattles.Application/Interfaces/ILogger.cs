namespace DragonBallBattles.Application.Interfaces
{
    public interface ILogger
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogError(string message, System.Exception exception);
    }
}