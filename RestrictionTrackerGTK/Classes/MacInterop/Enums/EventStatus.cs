using System;
namespace MacInterop
{
  public enum EventStatus
  {
    Ok = 0,
    EventAlreadyPostedErr = -9860,
    EventTargetBusyErr = -9861,
    EventClassInvalidErr = -9862,
    EventClassIncorrectErr = -9864,
    EventHandlerAlreadyInstalledErr = -9866,
    EventInternalErr = -9868,
    EventKindIncorrectErr = -9869,
    EventParameterNotFoundErr = -9870,
    EventNotHandledErr = -9874,
    EventLoopTimedOutErr = -9875,
    EventLoopQuitErr = -9876,
    EventNotInQueueErr = -9877,
    EventHotKeyExistsErr = -9878,
    EventHotKeyInvalidErr = -9879
  }
}
