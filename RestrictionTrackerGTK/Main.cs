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
      if (!modFunctions.RunningLock())
        return;
      try
      {
        string sUpdatePath = System.IO.Path.Combine(modFunctions.AppData, "Setup");
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
        ApplicationEvents.Prefs += delegate(object sender, ApplicationEventArgs e) 
        {
          fMain.cmdConfig_Click(new object(), new EventArgs());
          e.Handled=true;
        };
        IgeMacMenuGroup appGroup = IgeMacMenu.AddAppMenuGroup();
        MenuItem mnuAbout = new MenuItem();
        mnuAbout.Activated += delegate(object sender, EventArgs e)
        {
          fMain.cmdAbout_Click(new object(), new EventArgs());
        };
        appGroup.AddMenuItem(mnuAbout, "About " + modFunctions.ProductName);
        appGroup.AddMenuItem(new MenuItem(), "-");
        MenuItem mnuHistory = new MenuItem();
        mnuHistory.Activated += delegate(object sender, EventArgs e)
        {
          fMain.cmdHistory_Click(new object(), new EventArgs());
        };
        appGroup.AddMenuItem(mnuHistory, "Usage History");

        MenuItem mnuConfig = new MenuItem();
        mnuConfig.Activated += delegate(object sender, EventArgs e)
        {
          fMain.cmdConfig_Click(new object(), new EventArgs());
        };
        appGroup.AddMenuItem(mnuConfig, "Preferences...");
      }
      fMain.Show();
      Application.Run();
    }
  }
}
