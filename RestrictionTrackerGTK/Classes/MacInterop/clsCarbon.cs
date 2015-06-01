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
}
