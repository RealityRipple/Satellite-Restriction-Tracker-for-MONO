using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;

namespace MacInterop
{
  internal delegate CarbonEventHandlerStatus EventDelegate (IntPtr callRef, IntPtr eventRef, IntPtr userData);

  internal static class Carbon
  {
    public const string CarbonLib = "/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon";

    [DllImport (CarbonLib)]
    public static extern IntPtr GetApplicationEventTarget ();

    #region Event handler installation
    
    [DllImport (CarbonLib)]
    static extern EventStatus InstallEventHandler (IntPtr target, EventDelegate handler, uint count, CarbonEventTypeSpec [] types, IntPtr user_data, out IntPtr handlerRef);
    
    [DllImport (CarbonLib)]
    public static extern EventStatus RemoveEventHandler (IntPtr handlerRef);
    
    public static IntPtr InstallEventHandler (IntPtr target, EventDelegate handler, CarbonEventTypeSpec [] types)
    {
      IntPtr handlerRef;
      CheckReturn (InstallEventHandler (target, handler, (uint)types.Length, types, IntPtr.Zero, out handlerRef));
      return handlerRef;
    }
    
    public static IntPtr InstallEventHandler (IntPtr target, EventDelegate handler, CarbonEventTypeSpec type)
    {
      return InstallEventHandler (target, handler, new CarbonEventTypeSpec[] { type });
    }

    public static IntPtr InstallApplicationEventHandler (EventDelegate handler, CarbonEventTypeSpec type)
    {
      return InstallEventHandler (GetApplicationEventTarget (), handler, new CarbonEventTypeSpec[] { type });
    }
    
    #endregion

    public static void CheckReturn (EventStatus status)
    {
      int intStatus = (int) status;
      if (intStatus < 0)
        throw new EventStatusException (status);
    }

  }

  internal enum CarbonEventHandlerStatus 
  {
    Handled = 0,
    NotHandled = -9874,
    UserCancelled = -128
  }

  internal enum CarbonEventClass : uint
  {
    AppleEvent = 1634039412
  }

  internal enum CarbonEventApple
  {
    ReopenApplication = 1918988400,
    QuitApplication =  1903520116,
    ShowPreferences = 1886545254
  }
  
  [StructLayout(LayoutKind.Sequential, Pack = 2)]
  struct CarbonEventTypeSpec
  {
    public CarbonEventClass EventClass;
    public uint EventKind;

    public CarbonEventTypeSpec (CarbonEventClass eventClass, UInt32 eventKind)
    {
      this.EventClass = eventClass;
      this.EventKind = eventKind;
    }

    public CarbonEventTypeSpec (CarbonEventApple kind) : this (CarbonEventClass.AppleEvent, (uint) kind)
    {
    }

    public static implicit operator CarbonEventTypeSpec (CarbonEventApple kind)
    {
      return new CarbonEventTypeSpec (kind);
    }
  }
  
  class EventStatusException : SystemException
  {
    public EventStatusException (EventStatus status)
    {
      StatusCode = status;
    }
    
    public EventStatus StatusCode {
      get; private set;
    }
  }
  
  enum EventStatus 
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
