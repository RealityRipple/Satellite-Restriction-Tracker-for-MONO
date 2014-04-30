using System;
using System.Collections.Generic;

namespace MacInterop
{
  public static class ApplicationEvents
  {
    static object lockObj = new object();
    
    #region Quit
    
    static EventHandler<ApplicationQuitEventArgs> quit;
    static IntPtr quitHandlerRef = IntPtr.Zero;
    
    public static event EventHandler<ApplicationQuitEventArgs> Quit
    {
      add
      {
        lock (lockObj)
        {
          quit += value;
          if (quitHandlerRef == IntPtr.Zero)
          {
            quitHandlerRef = Carbon.InstallApplicationEventHandler(HandleQuit, CarbonEventApple.QuitApplication);
          }
        }
      }
      remove
      {
        lock (lockObj)
        {
          quit -= value;
          if (quit == null && quitHandlerRef != IntPtr.Zero)
          {
            Carbon.RemoveEventHandler(quitHandlerRef);
            quitHandlerRef = IntPtr.Zero;
          }
        }
      }
    }
    
    static CarbonEventHandlerStatus HandleQuit(IntPtr callRef, IntPtr eventRef, IntPtr user_data)
    {
      var args = new ApplicationQuitEventArgs();
      quit(null, args);
      return args.UserCancelled ? CarbonEventHandlerStatus.UserCancelled : args.HandledStatus;
    }
    
    #endregion
    
    #region Reopen
    
    static EventHandler<ApplicationEventArgs> reopen;
    static IntPtr reopenHandlerRef = IntPtr.Zero;
    
    public static event EventHandler<ApplicationEventArgs> Reopen
    {
      add
      {
        lock (lockObj)
        {
          reopen += value;
          if (reopenHandlerRef == IntPtr.Zero)
          {
            reopenHandlerRef = Carbon.InstallApplicationEventHandler(HandleReopen, CarbonEventApple.ReopenApplication);
          }
        }
      }
      remove
      {
        lock (lockObj)
        {
          reopen -= value;
          if (reopen == null && reopenHandlerRef != IntPtr.Zero)
          {
            Carbon.RemoveEventHandler(reopenHandlerRef);
            reopenHandlerRef = IntPtr.Zero;
          }
        }
      }
    }
    
    static CarbonEventHandlerStatus HandleReopen(IntPtr callRef, IntPtr eventRef, IntPtr user_data)
    {
      var args = new ApplicationEventArgs();
      reopen(null, args);
      return args.HandledStatus;
    }
    
    #endregion
  }
  
  public class ApplicationEventArgs : EventArgs
  {
    public bool Handled { get; set; }
    
    internal CarbonEventHandlerStatus HandledStatus
    {
      get
      {
        return Handled ? CarbonEventHandlerStatus.Handled : CarbonEventHandlerStatus.NotHandled;
      }
    }
  }
  
  public class ApplicationQuitEventArgs : ApplicationEventArgs
  {
    public bool UserCancelled { get; set; }
  }
}

