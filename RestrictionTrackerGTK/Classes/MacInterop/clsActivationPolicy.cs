using System;
using System.Runtime.InteropServices;

namespace RestrictionTrackerGTK
{
  public class ActivationPolicy
  {
    [DllImport("libobjc.dylib", EntryPoint = "sel_registerName")]
    static extern IntPtr GetSelector(string name);
    [DllImport("libobjc.dylib", EntryPoint = "objc_getClass")]
    static extern IntPtr GetClass(string name);
    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    static extern bool SendBool(IntPtr receiver, IntPtr selector, int int1);
    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    static extern IntPtr SendIntPtr(IntPtr receiver, IntPtr selector);
    public static bool setPolicy(ApplicationActivationPolicy policy)
    {
      IntPtr nsapp = GetClass("NSApplication");
      if (nsapp.ToInt64() == 0)
       return false;
      IntPtr shapp = GetSelector("sharedApplication");
      if (shapp.ToInt64() == 0)
       return false;
      IntPtr h = SendIntPtr(nsapp, shapp);
      if (h.ToInt64() == 0)
       return false;
      IntPtr sap = GetSelector("setActivationPolicy:");
      if (sap.ToInt64() == 0)
       return false;
      return SendBool(h, sap, (int)policy);
    }
  }
}
