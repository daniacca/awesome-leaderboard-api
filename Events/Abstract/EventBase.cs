﻿using Events.Interfaces;
using Events.Types;

namespace Events.Abstract;

public abstract class EventBase : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public EventState State { get; set; } = EventState.Created;
}