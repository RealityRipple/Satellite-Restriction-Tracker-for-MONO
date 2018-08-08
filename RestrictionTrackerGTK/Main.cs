using System;
using System.IO;
using Gtk;
using MacInterop;
using MantisBugTracker;
using RestrictionLibrary;
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
    public static void Main()
    {
      Application.Init();
      if (!modFunctions.RunningLock())
        return;
      GLib.ExceptionManager.UnhandledException += unhandledException;
      System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(1033);
      if (CurrentOS.IsMac)
      {
        try
        {
          string sOldConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), modFunctions.CompanyName);
          if (Directory.Exists(sOldConfig))
          {
            modFunctions.MoveDirectory(sOldConfig, Path.Combine(modFunctions.LocalAppData, modFunctions.CompanyName));
            AppSettings cSettings = new AppSettings();
            cSettings.HistoryDir = modFunctions.AppData;
            cSettings.Save();
            Directory.Delete(sOldConfig, true);
          }
        }
        catch (Exception)
        {
        }
      }
      try
      {
        string sUpdatePath = Path.Combine(modFunctions.AppData, "Setup");
        if (CurrentOS.IsMac)
          sUpdatePath += ".dmg";
        else if (CurrentOS.IsLinux)
          sUpdatePath += ".bz2.sh";
        if (File.Exists(sUpdatePath))
          File.Delete(sUpdatePath);
      }
      catch (Exception)
      {
      }
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
        ApplicationEvents.Prefs += delegate (object sender, ApplicationEventArgs e)
        {
          fMain.cmdConfig_Click(new object(), new EventArgs());
          e.Handled = true;
        };
        IgeMacMenuGroup appGroup = IgeMacMenu.AddAppMenuGroup();
        MenuItem mnuAbout = new MenuItem();
        mnuAbout.Activated += delegate (object sender, EventArgs e)
        {
          fMain.cmdAbout_Click(new object(), new EventArgs());
        };
        appGroup.AddMenuItem(mnuAbout, "About " + modFunctions.ProductName);
        appGroup.AddMenuItem(new MenuItem(), "-");
        MenuItem mnuHistory = new MenuItem();
        mnuHistory.Activated += delegate (object sender, EventArgs e)
        {
          fMain.cmdHistory_Click(new object(), new EventArgs());
        };
        appGroup.AddMenuItem(mnuHistory, "Usage History");
        MenuItem mnuConfig = new MenuItem();
        mnuConfig.Activated += delegate (object sender, EventArgs e)
        {
          fMain.cmdConfig_Click(new object(), new EventArgs());
        };
        appGroup.AddMenuItem(mnuConfig, "Preferences...");

        MenuBar srtMenu = new MenuBar();
        Menu helpMenu = new Menu();
        MenuItem helpMenuItem = new MenuItem("Help");

        MenuItem mnuFAQ = new MenuItem("Frequently Asked Questions");
        mnuFAQ.Activated += delegate (object sender, EventArgs e)
        {
          System.Diagnostics.Process.Start("http://srt.realityripple.com/faq.php");
        };
        helpMenu.Append(mnuFAQ);
        mnuFAQ.Show();

        MenuItem mnuChanges = new MenuItem("What's New in " + modFunctions.ProductName + " v" + modFunctions.ProductVersion);
        mnuChanges.Activated += delegate (object sender, EventArgs e)
        {
          System.Diagnostics.Process.Start("http://srt.realityripple.com/For-MONO/changes.php");
        };
        helpMenu.Append(mnuChanges);
        mnuChanges.Show();

        MenuItem mnuHelpSpace1 = new MenuItem("-");
        helpMenu.Append(mnuHelpSpace1);
        mnuHelpSpace1.Show();

        MenuItem mnuWebsite = new MenuItem("Visit the " + modFunctions.ProductName + " Home Page");
        mnuWebsite.Activated += delegate (object sender, EventArgs e)
        {
          System.Diagnostics.Process.Start("http://srt.realityripple.com/For-MONO/mac.php");
        };
        helpMenu.Append(mnuWebsite);
        mnuWebsite.Show();

        MenuItem mnuBug = new MenuItem("Report a Bug");
        mnuBug.Activated += delegate (object sender, EventArgs e)
        {
          System.Diagnostics.Process.Start("http://bugs.realityripple.com/bug_report_page.php?project_id=2");
        };
        helpMenu.Append(mnuBug);
        mnuBug.Show();

        helpMenuItem.Submenu = helpMenu;
        helpMenu.Show();
        srtMenu.Append(helpMenuItem);
        helpMenuItem.Show();
        IgeMacMenu.MenuBar = srtMenu;
        srtMenu.Show();
      }
      fMain.Show();
      Application.Run();
    }
    static void unhandledException(GLib.UnhandledExceptionArgs args)
    {
      Exception ex = (Exception)args.ExceptionObject;
      if (ex.Message == "Exception has been thrown by the target of an invocation.")
        ex = ex.InnerException;
      ResponseType ret = showErrDialog(ex, !args.IsTerminating);
      if (ret == ResponseType.Ok)
      {
        string sRet = MantisReporter.ReportIssue(ex);
        if (sRet == "OK")
        {
          if (modFunctions.ShowMessageBox(null, "Thank you for reporting the error.\nYou can find details on the Bug Report page.\n\nDo you wish to visit the Bug Report?", modFunctions.ProductName + " Error Report Sent!", (DialogFlags)0, MessageType.Question, ButtonsType.YesNo) == ResponseType.Yes)
            System.Diagnostics.Process.Start("http://bugs.realityripple.com/set_project.php?project_id=2");
        }
        else
        {
          string sErrRep = "http://bugs.realityripple.com/set_project.php?project_id=2&make_default=no&ref=bug_report_page.php";
          if (CurrentOS.Is32bit)
          {
            sErrRep += "?platform=x86";
          }
          else if (CurrentOS.Is64bit)
          {
            if (CurrentOS.Is32BitProcess)
            {
              sErrRep += "?platform=x86-64";
            }
            else if (CurrentOS.Is64BitProcess)
            {
              sErrRep += "?platform=x64";
            }
            else
            {
              sErrRep += "?platform=x86 OS, Unknown Process";
            }
          }
          else
          {
            sErrRep += "?platform=Unknown";
          }
          sErrRep += "%2526os=" + DoubleEncode(CurrentOS.Name);
          sErrRep += "%2526os_build=" + DoubleEncode(Environment.OSVersion.VersionString);
          sErrRep += "%2526product_version=" + DoubleEncode(modFunctions.ProductVersion);
          string sDesc = ex.Message;
          string sSum = sDesc;
          if (sSum.Length > 80)
            sSum = sSum.Substring(0, 77) + "...";
          sErrRep += "%2526summary=" + DoubleEncode(sSum);
          if (!string.IsNullOrEmpty(ex.StackTrace))
          {
            sDesc += "\n" + ex.StackTrace.Substring(0, ex.StackTrace.IndexOf("\n"));
          }
          else
          {
            if (!string.IsNullOrEmpty(ex.Source))
            {
              sDesc += "\n @ " + ex.Source;
              if (ex.TargetSite != null)
                sDesc += "." + ex.TargetSite.Name;
            }
            else
            {
              if (ex.TargetSite != null)
                sDesc += "\n @ " + ex.TargetSite.Name;
            }
          }
          sErrRep += "%2526description=" + DoubleEncode(sDesc);
          if (modFunctions.ShowMessageBox(null, sRet + "\n\nWould you like to report the error manually?", modFunctions.ProductName + " Error Report Failed!", (DialogFlags)0, MessageType.Error, ButtonsType.YesNo) == ResponseType.Yes)
          {
            System.Diagnostics.Process.Start(sErrRep);
          }
        }
      }
      else if (ret == ResponseType.Reject)
      {
        Application.Quit();
      }
    }
    private static string DoubleEncode(string inString)
    {
      return srlFunctions.PercentEncode(srlFunctions.PercentEncode(inString));
    }
    static void SizeAllocateLabel(object o, SizeAllocatedArgs e)
    {
      ((Gtk.Widget)o).SetSizeRequest(e.Allocation.Width, -1);
    }
    private static Gtk.ResponseType showErrDialog(Exception e, bool canContinue)
    {
      Gtk.Table pnlError;
      Gtk.Image pctError;
      Gtk.Label lblError;
      Gtk.ScrolledWindow scrError;
      Gtk.TextView txtError;
      Gtk.Button cmdReport;
      Gtk.Button cmdIgnore;
      Gtk.Button cmdExit;

      pnlError = new Gtk.Table(2, 2, false);
      pnlError.Name = "pnlError";
      pnlError.RowSpacing = 6;
      pnlError.ColumnSpacing = 6;

      pctError = new Gtk.Image();
      pctError.Name = "pctError";
      pctError.Xpad = 8;
      pctError.Ypad = 8;
      if (CurrentOS.IsMac)
        pctError.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_nettest_error.png");
      else
        pctError.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_error.png");
      pnlError.Attach(pctError, 0, 1, 0, 1, AttachOptions.Fill, AttachOptions.Fill, 0, 0);

      lblError = new Gtk.Label();
      lblError.Name = "lblError";
      lblError.Xalign = 0F;
      lblError.Yalign = 0.5F;
      if (e.TargetSite == null)
      {
        lblError.LabelProp = "<span size=\"12000\" weight=\"bold\">" + modFunctions.ProductName + " has Encountered an Error</span>";
      }
      else
      {
        lblError.LabelProp = "<span size=\"12000\" weight=\"bold\">" + modFunctions.ProductName + " has Encountered an Error in " + e.TargetSite.Name + "</span>";
      }
      var signal = GLib.Signal.Lookup(lblError, "size-allocate", typeof(SizeAllocatedArgs));
      signal.AddDelegate(new EventHandler<SizeAllocatedArgs>(SizeAllocateLabel));
      lblError.LineWrap = true;
      lblError.UseMarkup = true;
      pnlError.Attach(lblError, 1, 2, 0, 1, AttachOptions.Shrink | AttachOptions.Fill | AttachOptions.Expand, AttachOptions.Fill, 0, 0);

      scrError = new Gtk.ScrolledWindow();
      scrError.Name = "scrError";
      scrError.VscrollbarPolicy = PolicyType.Automatic;
      scrError.HscrollbarPolicy = PolicyType.Never;
      scrError.ShadowType = ShadowType.In;

      txtError = new Gtk.TextView();
      txtError.CanFocus = true;
      txtError.Name = "txtError";
      txtError.Editable = false;
      txtError.AcceptsTab = false;
      txtError.WrapMode = WrapMode.Word;

      scrError.Add(txtError);
      pnlError.Attach(scrError, 1, 2, 1, 2, AttachOptions.Shrink | AttachOptions.Fill | AttachOptions.Expand, AttachOptions.Shrink | AttachOptions.Fill | AttachOptions.Expand, 0, 0);

      txtError.Buffer.Text = "Error: " + e.Message;
      if (!string.IsNullOrEmpty(e.StackTrace))
      {
        if (e.StackTrace.Contains("\n"))
          txtError.Buffer.Text += "\n" + e.StackTrace.Substring(0, e.StackTrace.IndexOf("\n"));
        else
          txtError.Buffer.Text += "\n" + e.StackTrace;
      }
      else
      {
        if (!string.IsNullOrEmpty(e.Source))
        {
          txtError.Buffer.Text += "\n @ " + e.Source;
          if (e.TargetSite != null)
            txtError.Buffer.Text += "." + e.TargetSite.Name;
        }
        else
        {
          if (e.TargetSite != null)
            txtError.Buffer.Text += "\n @ " + e.TargetSite.Name;
        }
      }

      cmdReport = new Gtk.Button();
      cmdReport.CanDefault = true;
      cmdReport.CanFocus = true;
      cmdReport.Name = "cmdReport";
      cmdReport.UseUnderline = false;
      cmdReport.Label = "Report Error";

      if (canContinue)
      {
        cmdIgnore = new Gtk.Button();
        cmdIgnore.CanDefault = true;
        cmdIgnore.CanFocus = true;
        cmdIgnore.Name = "cmdIgnore";
        cmdIgnore.UseUnderline = false;
        cmdIgnore.Label = "Ignore and Continue";
      }
      else
      {
        cmdIgnore = null;
      }

      cmdExit = new global::Gtk.Button();
      cmdExit.CanFocus = true;
      cmdExit.Name = "cmdExit";
      cmdExit.UseUnderline = true;
      cmdExit.Label = global::Mono.Unix.Catalog.GetString("Exit Application");

      Gtk.Dialog dlgErr = new Gtk.Dialog("Error in " + modFunctions.ProductName, null, DialogFlags.Modal | DialogFlags.DestroyWithParent, cmdReport);

      dlgErr.TypeHint = Gdk.WindowTypeHint.Dialog;
      dlgErr.WindowPosition = WindowPosition.CenterAlways;
      dlgErr.SkipPagerHint = true;
      dlgErr.SkipTaskbarHint = true;
      dlgErr.AllowShrink = true;
      dlgErr.AllowGrow = true;

      VBox pnlErrorDialog = dlgErr.VBox;
      pnlErrorDialog.Name = "pnlErrorDialog";
      pnlErrorDialog.BorderWidth = 2;
      pnlErrorDialog.Add(pnlError);

      Box.BoxChild pnlError_BC = (Box.BoxChild)pnlErrorDialog[pnlError];
      pnlError_BC.Position = 0;

      Gtk.HButtonBox dlgErrorAction = dlgErr.ActionArea;
      dlgErrorAction.Name = "dlgErrorAction";
      dlgErrorAction.Spacing = 10;
      dlgErrorAction.BorderWidth = 5;
      dlgErrorAction.LayoutStyle = ButtonBoxStyle.End;

      dlgErr.AddActionWidget(cmdReport, ResponseType.Ok);
      if (canContinue)
        dlgErr.AddActionWidget(cmdIgnore, ResponseType.No);
      dlgErr.AddActionWidget(cmdExit, ResponseType.Reject);

      dlgErr.ShowAll();
      Gdk.Geometry minGeo = new Gdk.Geometry();
      minGeo.MinWidth = dlgErr.Allocation.Width;
      minGeo.MinHeight = dlgErr.Allocation.Height;
      if (minGeo.MinWidth > 1 & minGeo.MinHeight > 1)
        dlgErr.SetGeometryHints(null, minGeo, Gdk.WindowHints.MinSize);

      Gtk.ResponseType dRet;
      do
      {
        dRet = (Gtk.ResponseType)dlgErr.Run();
      } while (dRet == ResponseType.None);
      dlgErr.Hide();
      dlgErr.Destroy();
      dlgErr = null;
      return dRet;
    }
  }
}
