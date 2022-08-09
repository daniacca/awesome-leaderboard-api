using AwesomeLeaderboard.ApiData.Leaderboard;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models.Db;

namespace AwesomeLeaderboard.Controllers;

[Route("api/[controller]")]
public class LeaderboardController : Controller
{
    [HttpGet]
    public async Task<IEnumerable<LeaderboardEntryOutputData>> Get([FromQuery] int? top, [FromQuery] int? minScore, [FromServices] IUserRepository userRepository)
    {
        var users = minScore is null
            ? await userRepository.GetListAsync(x => true)
            : await userRepository.GetListAsync(x => x.Score >= minScore);

        return users
            .OrderByDescending(x => x.Score)
            .Take(top ?? 10)
            .Select(u => new LeaderboardEntryOutputData
            {
                Username = u.Username,
                Score = u.Score ?? 0
            });
    }
}
