namespace AwesomeLeaderboard.ApiData.Users;

public class RegisterInputData
{
    public string? Username { get; set; }
    public long? InitialScore { get; set; } = null;
}