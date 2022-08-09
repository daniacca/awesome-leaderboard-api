using Events.Abstract;

namespace Events.Messages;

public class UpdateUserScore : EventBase
{
    public string? Username { get; set; }

    public long? NewScore { get; set; }
}