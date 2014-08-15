using System;
using Gtk;
using MacInterop;

namespace RestrictionTrackerGTK
{

	class MainClass
	{

		public static frmAbout fAbout;
    public static dlgAlertSelection fAlertSelection;
		public static dlgConfig fConfig;
		public static dlgCustomColors fCustomColors;
		public static frmHistory fHistory;
		public static frmMain fMain;
		public static void Main(string[] args)
    {
      Application.Init();
      try
      {
        string sUpdatePath = modFunctions.AppData + System.IO.Path.DirectorySeparatorChar + "Setup";
        if (CurrentOS.IsMac)
          sUpdatePath += ".dmg";
        else if (CurrentOS.IsLinux)
          sUpdatePath += ".bz2.sh";
        if (System.IO.File.Exists(sUpdatePath))
          System.IO.File.Delete(sUpdatePath);
      }
      catch {}
      fMain = new frmMain();
      if (CurrentOS.IsMac)
      {
        ApplicationEvents.Quit += delegate (object sender, ApplicationQuitEventArgs e)
        {
          Application.Quit();
          e.Handled = true;
        };
        ApplicationEvents.Reopen += delegate (object sender, ApplicationEventArgs e)
        {
          fMain.ShowFromTray();
          e.Handled = true;
        };
      }
      fMain.Show();
      Application.Run();
    }
  }
}
