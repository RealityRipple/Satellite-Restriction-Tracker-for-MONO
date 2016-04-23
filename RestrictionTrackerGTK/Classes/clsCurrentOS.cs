using System;
namespace RestrictionTrackerGTK
{
  public static class CurrentOS
  {
    public static bool IsWindows { get; private set; }
    public static bool IsUnix { get; private set; }
    public static bool IsMac { get; private set; }
    public static bool IsLinux { get; private set; }
    public static bool IsUnknown { get; private set; }
    public static bool Is32bit { get; private set; }
    public static bool Is64bit { get; private set; }
    public static bool Is64BitProcess { get { return (IntPtr.Size == 8); } }
    public static bool Is32BitProcess { get { return (IntPtr.Size == 4); } }
    public static string Name { get; private set; }
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Winapi)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool IsWow64Process([System.Runtime.InteropServices.In] IntPtr hProcess, [System.Runtime.InteropServices.Out] out bool wow64Process);
    private static bool Is64bitWindows
    {
      get
      {
        if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) || Environment.OSVersion.Version.Major >= 6)
        {
          using (System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess())
          {
            bool retVal;
            if (!IsWow64Process(p.Handle, out retVal))
            {
              return false;
            }
            return retVal;
          }
        }
        else
        {
          return false;
        }
      }
    }
    static CurrentOS()
    {
      IsWindows = System.IO.Path.DirectorySeparatorChar == '\\';
      if (IsWindows)
      {
        Name = Environment.OSVersion.VersionString;
 
        Name = Name.Replace("Microsoft ", "");
        Name = Name.Replace("  ", " ");
        Name = Name.Replace(" )", ")");
        Name = Name.Trim();
 
        Name = Name.Replace("NT 10.", "10 %bit 10.");
        Name = Name.Replace("NT 6.3", "8.1 %bit 6.3");
        Name = Name.Replace("NT 6.2", "8 %bit 6.2");
        Name = Name.Replace("NT 6.1", "7 %bit 6.1");
        Name = Name.Replace("NT 6.0", "Vista %bit 6.0");
        Name = Name.Replace("NT 5.", "XP %bit 5.");
        Name = Name.Replace("%bit", (Is64bitWindows ? "64bit" : "32bit"));
 
        if (Is64bitWindows)
        {
          Is64bit = true;
        }
        else
        {
          Is32bit = true;
        }
      }
      else
      {
        string UnixName = ReadProcessOutput("uname");
        if (UnixName.Contains("Darwin"))
        {
          IsUnix = true;
          IsMac = true;
 
          Name = "MacOS X " + ReadProcessOutput("sw_vers", "-productVersion");
          Name = Name.Trim();
 
          string machine = ReadProcessOutput("uname", "-m");
          if (machine.Contains("x86_64"))
          {
            Is64bit = true;
          }
          else
          {
            Is32bit = true;
          }
 
          Name += " " + (Is32bit ? "32bit" : "64bit");
        }
        else if (UnixName.Contains("Linux"))
        {
          IsUnix = true;
          IsLinux = true;
 
          Name = ReadProcessOutput("lsb_release", "-d");
          Name = Name.Substring(Name.IndexOf(":") + 1);
          Name = Name.Trim();
 
          string machine = ReadProcessOutput("uname", "-m");
          if (machine.Contains("x86_64"))
          {
            Is64bit = true;
          }
          else
          {
            Is32bit = true;
          }
 
          Name += " " + (Is32bit ? "32bit" : "64bit");
        }
        else if (!String.IsNullOrEmpty(UnixName))
        {
          IsUnix = true;
        }
        else
        {
          IsUnknown = true;
        }
      }
    }
    private static string ReadProcessOutput(string name)
    {
      return ReadProcessOutput(name, null);
    }
    private static string ReadProcessOutput(string name, string args)
    {
      try
      {
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        if (!String.IsNullOrEmpty(args))
        {
          p.StartInfo.Arguments = " " + args;
        }
        p.StartInfo.FileName = name;
        p.Start();
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        if (output == null)
        {
          output = "";
        }
        output = output.Trim();
        return output;
      }
      catch
      {
        return "";
      }
    }
  }
}
