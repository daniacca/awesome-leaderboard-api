using System.Net;
using System.Net.Mime;
using System.Text.Json;
using AwesomeLeaderboard.ApiData.Users;
using Common.DistribuitedCache.Interfaces;
using Common.DistribuitedCache.Manager.Commands;
using Events.Abstract;
using Events.Messages;
using Events.Types;
using MessageBus.RabbitMq.AbsClasses;
using Microsoft.AspNetCore.Mvc;

namespace AwesomeLeaderboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    IDistributedCacheManager CacheManager { get; }

    public UserController(IDistributedCacheManager cacheManager)
    {
        CacheManager = cacheManager;
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Register([FromBody] RegisterInputData data, [FromServices] RabbitProducerBase<RegisterUser> rabbitProducer)
    {
        if (data is null || data.Username is null)
            return BadRequest(new { Message = "User data not specified" });

        var eventId = Guid.NewGuid();
        var registerUserEvent = new RegisterUser
        {
            Id = eventId,
            Username = data.Username,
            InitialScore = data.InitialScore
        };

        var setCommand = new SetCommand<RegisterUser>(new SetCommandPayload<RegisterUser>
        {
            Key = eventId.ToString(),
            Data = registerUserEvent
        });
        await CacheManager.ExecuteAsync(setCommand);

        rabbitProducer.Publish(registerUserEvent);
        return Accepted(new { eventId });
    }

    [HttpPut("{userName}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Update(string userName, [FromBody] UpdateInputData data, [FromServices] RabbitProducerBase<UpdateUserScore> rabbitProducer)
    {
        if(userName is null || data is null)
            return BadRequest(new { Message = "User data not specified" });

        var eventId = Guid.NewGuid();
        var updateUserEvent = new UpdateUserScore
        {
            Id = eventId,
            Username = userName,
            NewScore = data.NewScore
        };
        
        var setCommand = new SetCommand<UpdateUserScore>(new SetCommandPayload<UpdateUserScore>
        {
            Key = eventId.ToString(),
            Data = updateUserEvent
        });
        await CacheManager.ExecuteAsync(setCommand);

        rabbitProducer.Publish(updateUserEvent);
        return Accepted(new { eventId });
    }

    [HttpGet("[action]/{type}/{eventId}")]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status102Processing)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EventResult(string type, string eventId)
    {
        if(string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(eventId))
            return BadRequest("Input data are missing");

        if(type is not "register" || type is not "update")
            return BadRequest("Type specified is not supported");

        var getCommand = new GetCommand(eventId);
        await CacheManager.ExecuteAsync(getCommand);

        if(string.IsNullOrWhiteSpace(getCommand.Result))
            return StatusCode((int)HttpStatusCode.Gone);

        EventBase? eventInfo = type switch
        {
            "register" => JsonSerializer.Deserialize<RegisterUser>(getCommand.Result),
            "update" => JsonSerializer.Deserialize<UpdateUserScore>(getCommand.Result),
            _ => null
        };

        if (eventInfo is null)
            return BadRequest("Type specified is not supported");

        if(eventInfo.State == EventState.Done)
            return Ok(new { Message = "The request were executed with success!" });

        if (eventInfo.State == EventState.InProgress)
            return StatusCode((int)HttpStatusCode.Processing);

        return Conflict();
    }
}

