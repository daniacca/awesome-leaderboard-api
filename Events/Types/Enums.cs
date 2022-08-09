using System;
namespace Events.Types;

public enum EventState
{
    Created = 0,
    InProgress = 10,
    Done = 20,
    FinishWithError = 30
}