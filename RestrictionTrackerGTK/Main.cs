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
