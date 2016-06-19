using System;
using System.Runtime.InteropServices;
namespace MacInterop
{
  [StructLayout(LayoutKind.Sequential, Pack = 2)]
  public struct CarbonEventTypeSpec
  {
    public CarbonEventClass EventClass;
    public uint EventKind;
    public CarbonEventTypeSpec(CarbonEventClass eventClass, UInt32 eventKind)
    {
      this.EventClass = eventClass;
      this.EventKind = eventKind;
    }
    public CarbonEventTypeSpec(CarbonEventApple kind) :
      this(CarbonEventClass.AppleEvent, (uint) kind)
    {
    }
    public static implicit operator CarbonEventTypeSpec(CarbonEventApple kind)
    {
      return new CarbonEventTypeSpec(kind);
    }
  }
}
