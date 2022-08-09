using Events.Abstract;

namespace Events.Messages;

public class RegisterUser : EventBase
{
    public string? Username { get; set; }

    public long? InitialScore { get; set; } = null;
}
