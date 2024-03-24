using System;
using System.IO;
using Gtk;
using RestrictionLibrary;
using System.Drawing;
using System.Collections.Generic;
using System.Security.Policy;
using RestrictionTrackerGTK;
using System.Configuration;
using System.Threading;
using GLib;
namespace RestrictionTrackerGTK
{
  public partial class frmMain:
    Gtk.Window
  {
    public static StatusIcon trayIcon;
    public static AppIndicator.ApplicationIndicator appIcon;
    private bool mTrayState = false;
    public bool TrayState
    {
      get
      {
        if (!TraySupported)
          mTrayState = false;
        return mTrayState;
      }
      set
      {
        if (!TraySupported)
          mTrayState = false;
        else
          mTrayState = value;
        if (mTrayState == false)
          HideTrayIcon();
        else
          ShowTrayIcon();
      }
    }
    private string trayResource;
    public bool TraySupported
    {
      get
      {
        return (mTraySupport != TraySupport.Off);
      }
    }
    private enum TraySupport
    {
      Off = 0,
      Standard = 1,
      AppIndicator = 2
    }
    private TraySupport mTraySupport = TraySupport.Off;
    private int trayRes = 16;
    private string _IconFolder = "";
    private string IconFolder
    {
      get
      {
        if (string.IsNullOrEmpty(_IconFolder))
        {
          _IconFolder = System.IO.Path.Combine(modFunctions.AppData, "trayIcons");
        }
        if (!Directory.Exists(_IconFolder))
        {
          Directory.CreateDirectory(_IconFolder);
        }
        return _IconFolder;
      }
    }
    private string c_PauseActivity;
    public string PauseActivity
    {
      get
      {
        return c_PauseActivity;
      }
      set
      {
        c_PauseActivity = value;
      }
    }
    private uint tmrUpdate;
    private uint tmrIcon;
    private uint tmrSpeed;
    private uint tmrStatus;
    private uint tmrShow;
    private uint tmrShowConfig;
    private delegate void MethodInvoker();
    private delegate void ParamaterizedInvoker(object parameter);
    private clsUpdate updateChecker;
    private void updateCheckerEvent(bool Add)
    {
      if (Add)
      {
        updateChecker.CheckResult += updateChecker_CheckResult;
        updateChecker.DownloadingUpdate += updateChecker_DownloadingUpdate;
        updateChecker.DownloadResult += updateChecker_DownloadResult;
        updateChecker.UpdateProgressChanged += updateChecker_UpdateProgressChanged;
      }
      else
      {
        updateChecker.CheckResult -= updateChecker_CheckResult;
        updateChecker.DownloadingUpdate -= updateChecker_DownloadingUpdate;
        updateChecker.DownloadResult -= updateChecker_DownloadResult;
        updateChecker.UpdateProgressChanged -= updateChecker_UpdateProgressChanged;
      }
    }
    private TaskbarNotifier taskNotifier;
    private RestrictionLibrary.Local.SiteConnection localData;
    private void localDataEvent(bool Add)
    {
      if (Add)
      {
        localData.ConnectionStatus += localData_ConnectionStatus;
        localData.ConnectionFailure += localData_ConnectionFailure;
        localData.ConnectionResult += localData_ConnectionResult;
      }
      else
      {
        localData.ConnectionStatus -= localData_ConnectionStatus;
        localData.ConnectionFailure -= localData_ConnectionFailure;
        localData.ConnectionResult -= localData_ConnectionResult;
      }
    }
    private const string sDISPLAY = "Usage Levels (%lt)";
    private const string sDISPLAY_LT_NONE = "No History";
    private const string sDISPLAY_LT_BUSY = "Please Wait";
    private const string sDISPLAY_TT_NEXT = "Next Update in %t.";
    private const string sDISPLAY_TT_LATE = "Update Should've Happened %t Ago!";
    private const string sDISPLAY_TT_ERR = "%e\n%m";
    private const string sDISPLAY_TT_T_SOON = "a Moment";
    private const int MBPerGB = 1000;
    private LoadStates myState = LoadStates.Loading;
    private string sDisp = sDISPLAY.Replace("%lt", sDISPLAY_LT_NONE);
    private string sDisp_LT = sDISPLAY_LT_NONE;
    private string sDispTT = sDISPLAY_TT_NEXT.Replace("%t", sDISPLAY_TT_T_SOON);
    private string sDisp_TT_M = sDISPLAY_TT_NEXT.Replace("%t", sDISPLAY_TT_T_SOON);
    private string sDisp_TT_T = sDISPLAY_TT_T_SOON;
    private string sDisp_TT_E = "";
    private string sUpdatePath = System.IO.Path.Combine(modFunctions.AppDataPath, "Setup");
    private AppSettings mySettings;
    private string sAccount;
    private string sPassword;
    private bool imSlowed;
    private bool imFree = false;
    private long NextGrabTick;
    private int GrabAttempt;
    private bool ClosingTime;
    private byte bAlert;
    private long uCache_used, uCache_lim;
    private long lastBalloon;
    private bool firstRestore;
    private Dictionary<string, string> markupList = new Dictionary<string, string>();
    private Gtk.Menu mnuTray;
    private Gtk.MenuItem mnuRestore;
    private Gtk.MenuItem mnuAbout;
    private Gtk.SeparatorMenuItem mnuTraySpace;
    private Gtk.MenuItem mnuExit;
    private Gtk.Menu mnuGraph;
    private Gtk.MenuItem mnuGraphRefresh;
    private Gtk.SeparatorMenuItem mnuGraphSpace;
    private Gtk.MenuItem mnuGraphColors;
    #region "Form Events"
    public frmMain() :
      base(Gtk.WindowType.Toplevel)
    {
      sUpdatePath = modFunctions.AppDataPath + "Setup";
      if (CurrentOS.IsMac)
        sUpdatePath += ".dmg";
      else if (CurrentOS.IsLinux)
        sUpdatePath += ".bz2.sh";
      Gdk.Geometry minGeo = new Gdk.Geometry();
      minGeo.MinWidth = 450;
      minGeo.MinHeight = 200;
      this.SetGeometryHints(null, minGeo, Gdk.WindowHints.MinSize);

      mnuTray = new Menu();
      mnuRestore = new MenuItem("_Restore");
      mnuAbout = new MenuItem("_About");
      mnuTraySpace = new SeparatorMenuItem();
      mnuExit = new MenuItem("E_xit");
      mnuTray.Append(mnuRestore);
      mnuTray.Append(mnuAbout);
      mnuTray.Append(mnuTraySpace);
      mnuTray.Append(mnuExit);
      mnuTray.ShowAll();
      mnuRestore.Activated += mnuRestore_Click;
      mnuAbout.Activated += mnuAbout_Click;
      mnuExit.Activated += mnuExit_Click;

      mnuGraph = new Menu();
      mnuGraphRefresh = new MenuItem("_Refresh");
      mnuGraphSpace = new SeparatorMenuItem();
      mnuGraphColors = new MenuItem("_Customize Colors");
      mnuGraph.Append(mnuGraphRefresh);
      mnuGraph.Append(mnuGraphSpace);
      mnuGraph.Append(mnuGraphColors);
      mnuGraph.ShowAll();
      mnuGraphRefresh.Activated += mnuGraphRefresh_Click;
      mnuGraphColors.Activated += mnuGraphColors_Click;
      this.Opacity = 0;
      Build();

      this.WindowStateEvent += Form_WindowStateChanged;
      this.ConfigureEvent += Form_SizeConfigured;
      this.SizeAllocated += Form_SizeAllocated;
      this.DeleteEvent += Form_Closed;

      if (mySettings == null)
      {
        ReLoadSettings();
      }
      System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;

      NextGrabTick = long.MinValue;
      cmdRefresh.ButtonReleaseEvent += cmdRefresh_Click;
      cmdHistory.Clicked += cmdHistory_Click;
      cmdConfig.Clicked += cmdConfig_Click;
      cmdAbout.Clicked += cmdAbout_Click;
      evnUsage.ButtonReleaseEvent += evnGraph_Click;

      pbMainStatus.Visible = false;
      lblMainStatus.Visible = false;

      DisplayResults(0, 0);

      this.Resize(mySettings.MainSize.Width, mySettings.MainSize.Height);
      Main_SizeChanged(null, null);

      mTraySupport = TraySupport.Off;
      trayIcon = new StatusIcon();
      trayIcon.SizeChanged += trayIcon_SizeChanged;
      tmrShow = GLib.Timeout.Add(2000, trayIcon_NoGo);
    }
    private void trayIcon_SizeChanged(object sender, SizeChangedArgs e)
    {
      if (mTraySupport == TraySupport.Off)
      {
        if (trayIcon.Embedded)
        {
          if (e.Size > 7)
          {
            mTraySupport = TraySupport.Standard;
            trayRes = e.Size;
            MakeIconListing();
            if (tmrShow != 0)
            {
              GLib.Source.Remove(tmrShow);
              tmrShow = 0;
              trayIcon.Activate += OnTrayIconActivate;
              trayIcon.PopupMenu += OnTrayIconPopup;
              SetTrayText(modFunctions.ProductName);
              firstRestore = false;
              StartupCleanup();
            }
          }
        }
      }
      else if (mTraySupport == TraySupport.AppIndicator)
      {
        appIcon.Status = AppIndicator.Status.Passive;
        appIcon.Dispose();
        appIcon = null;
        if (trayIcon.Embedded)
        {
          if (e.Size > 7)
          {
            mTraySupport = TraySupport.Standard;
            trayRes = e.Size;
            MakeIconListing();
            if (tmrShow != 0)
            {
              GLib.Source.Remove(tmrShow);
              tmrShow = 0;
            }
            trayIcon.Activate += OnTrayIconActivate;
            trayIcon.PopupMenu += OnTrayIconPopup;
            SetTrayText(modFunctions.ProductName);
            if (mySettings.TrayIconStyle == TrayStyles.Always)
              mTrayState = true;
            else if (mySettings.TrayIconStyle == TrayStyles.Minimized)
            {
              if ((this.GdkWindow.State & Gdk.WindowState.Iconified) == Gdk.WindowState.Iconified)
                mTrayState = true;
              else
                mTrayState = false;
            }
            else
              mTrayState = false;
            SetTrayIcon(trayResource);
          }
        }
      }
      else if (mTraySupport == TraySupport.Standard)
      {
        if (trayIcon.Embedded)
        {
          if (e.Size > 7)
          {
            trayRes = e.Size;
            MakeIconListing();
          }
        }
      }
    }
    private bool trayIcon_NoGo()
    {
      if (tmrShow != 0)
      {
        GLib.Source.Remove(tmrShow);
        tmrShow = 0;
        if (trayIcon.Embedded)
        {
          mTraySupport = TraySupport.Standard;
          trayRes = trayIcon.Size;
          if (trayRes < 8)
            trayRes = 8;
          MakeIconListing();
          trayIcon.Activate += OnTrayIconActivate;
          trayIcon.PopupMenu += OnTrayIconPopup;
          SetTrayText(modFunctions.ProductName);
          firstRestore = false;
        }
        else
        {
          trayRes = 16;
          MakeIconListing();
          try
          {
            appIcon = new AppIndicator.ApplicationIndicator("restriction-tracker", "norm", AppIndicator.Category.Communications, IconFolder + System.IO.Path.DirectorySeparatorChar);
            mTraySupport = TraySupport.AppIndicator;
            appIcon.Menu = mnuTray;
            SetTrayText(modFunctions.ProductName);
            firstRestore = false;
          }
          catch (Exception)
          {
            Directory.Delete(IconFolder, true);
            mTraySupport = TraySupport.Off;
            firstRestore = true;
          }
        }
        StartupCleanup();
      }
      return false;
    }
    private void MakeIconListing()
    {
      if (trayRes < 8)
        trayRes = 8;
      string[] sFiles = Directory.GetFiles(IconFolder);
      foreach (string sFile in sFiles)
      {
        if (String.Compare(System.IO.Path.GetExtension(sFile),".png", StringComparison.OrdinalIgnoreCase) == 0)
          File.Delete(sFile);
      }
      string[][] icoNames = new string[][] { new string[] { "norm.ico", "norm.png" }, new string[] { "free.ico", "free.png" }, new string[] { "restricted.ico", "restricted.png" }, new string[] { "error.png", "error.png" }, new string[] { "throbsprite.0.ico", "throb_0.png" }, new string[] { "throbsprite.1.ico", "throb_1.png" }, new string[] { "throbsprite.2.ico", "throb_2.png" }, new string[] { "throbsprite.3.ico", "throb_3.png" }, new string[] { "throbsprite.4.ico", "throb_4.png" }, new string[] { "throbsprite.5.ico", "throb_5.png" }, new string[] { "throbsprite.6.ico", "throb_6.png" }, new string[] { "throbsprite.7.ico", "throb_7.png" }, new string[] { "throbsprite.8.ico", "throb_8.png" }, new string[] { "throbsprite.9.ico", "throb_9.png" }, new string[] { "throbsprite.10.ico", "throb_10.png" }, new string[] { "throbsprite.11.ico", "throb_11.png" } };
      foreach (string[] ico in icoNames)
      {
        if (String.Compare(System.IO.Path.GetExtension(ico[0]), ".ico", StringComparison.OrdinalIgnoreCase) == 0)
          ResizeIcon(ico[0]).Save(System.IO.Path.Combine(IconFolder, ico[1]), System.Drawing.Imaging.ImageFormat.Png);
        else if (String.Compare(System.IO.Path.GetExtension(ico[0]), ".png", StringComparison.OrdinalIgnoreCase) == 0)
          ResizePng(ico[0]).Save(System.IO.Path.Combine(IconFolder, ico[1]), System.Drawing.Imaging.ImageFormat.Png);
      }
      long iStart = RestrictionLibrary.srlFunctions.TickCount();
      do
      {
        Gtk.Main.Iteration();
        Gtk.Main.IterationDo(false);
        System.Threading.Thread.Sleep(0);
        Gtk.Main.Iteration();
        bool stillMissing = false;
        foreach (string[] ico in icoNames)
        {
          if (!System.IO.File.Exists(System.IO.Path.Combine(IconFolder, ico[1])))
            stillMissing = true;
        }
        if (!stillMissing)
          break;
      } while (iStart + 1000 > RestrictionLibrary.srlFunctions.TickCount());
      MakeCustomIconListing();
    }
    private System.Drawing.Bitmap ResizeIcon(string resource)
    {
      if (trayRes < 8)
        trayRes = 8;
      Bitmap imgTray = new Bitmap(trayRes, trayRes);
      using (Graphics g = Graphics.FromImage(imgTray))
      {
        g.Clear(Color.Transparent);
        string res = "Resources.tray_16." + resource;
        if (trayRes > 16)
        {
          res = "Resources.tray_32." + resource;
        }
        g.DrawIcon(new System.Drawing.Icon(GetType(), res), new Rectangle(0, 0, trayRes, trayRes));
      }
      return imgTray;
    }
    private System.Drawing.Bitmap ResizePng(string resource)
    {
      if (trayRes < 8)
        trayRes = 8;
      Bitmap imgTray = new Bitmap(trayRes, trayRes);
      using (Graphics g = Graphics.FromImage(imgTray))
      {
        g.Clear(Color.Transparent);
        string res = "Resources." + resource;
        g.DrawImage(new System.Drawing.Bitmap(GetType(), res), new Rectangle(0, 0, trayRes, trayRes));
      }
      return imgTray;
    }
    private void MakeCustomIconListing()
    {
      if (trayRes < 8)
        trayRes = 8;
      List<string> icoNames = new List<string>();
      string[] sFiles = Directory.GetFiles(IconFolder);
      foreach (string sFile in sFiles)
      {
        if (String.Compare(System.IO.Path.GetExtension(sFile), ".png", StringComparison.OrdinalIgnoreCase) == 0 && String.Compare(System.IO.Path.GetFileName(sFile).Substring(0, 6), "graph_", StringComparison.OrdinalIgnoreCase) == 0)
          File.Delete(sFile);
      }
      for (long used = 0; used <= trayRes; used++)
      {
        using (Gdk.Pixbuf pIco = CreateUsageTrayIcon(used, trayRes, false, false))
        {
          pIco.Save(System.IO.Path.Combine(IconFolder, "graph_" + used + ".png"), "png");
          icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_" + used + ".png"));
        }
      }
      using (Gdk.Pixbuf pIco = CreateUsageTrayIcon(0, trayRes, false, true))
      {
        pIco.Save(System.IO.Path.Combine(IconFolder, "graph_free.png"), "png");
        icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_free.png"));
      }
      using (Gdk.Pixbuf pIco = CreateUsageTrayIcon(0, trayRes, true, false))
      {
        pIco.Save(System.IO.Path.Combine(IconFolder, "graph_slow.png"), "png");
        icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_slow.png"));
      }
      long iStart = RestrictionLibrary.srlFunctions.TickCount();
      do
      {
        Gtk.Main.Iteration();
        Gtk.Main.IterationDo(false);
        System.Threading.Thread.Sleep(0);
        Gtk.Main.Iteration();
        bool stillMissing = false;
        foreach (string ico in icoNames)
        {
          if (!System.IO.File.Exists(System.IO.Path.Combine(IconFolder, ico)))
            stillMissing = true;
        }
        if (!stillMissing)
          break;
      } while (iStart + 1000 > RestrictionLibrary.srlFunctions.TickCount());
      ResizePanels();
    }
    private void StartupCleanup()
    {
      if (TraySupported)
      {
        if (mySettings.TrayIconStyle == TrayStyles.Always)
          mTrayState = true;
        else if (mySettings.TrayIconStyle == TrayStyles.Minimized)
          mTrayState = false;
        else
          mTrayState = false;
      }
      else
        mTrayState = false;
      if (!TrayState)
        firstRestore = true;
      this.Opacity = 1;
      tmrStatus = GLib.Timeout.Add(500, tmrStatus_Tick);
      tmrUpdate = GLib.Timeout.Add(1000, tmrUpdate_Tick);
      if (myState != LoadStates.Loaded)
      {
        InitAccount();
        if (!string.IsNullOrEmpty(sAccount))
        {
          if (mySettings.AutoHide)
            HideToTray();
          SetTrayIcon("norm");
          EnableProgressIcon();
          SetStatusText("Initializing", "Beginning application initialization process...", false);
          MethodInvoker initDBInvoker = initDB;
          initDBInvoker.BeginInvoke(null, initDBInvoker);
        }
        else
        {
          firstRestore = true;
          ((Gtk.Label)mnuRestore.Child).TextWithMnemonic = "_Hide";
          this.SkipTaskbarHint = false;
          if (CurrentOS.IsMac)
            ActivationPolicy.setPolicy(ApplicationActivationPolicy.Regular);
          if (tmrIcon != 0)
          {
            GLib.Source.Remove(tmrIcon);
            tmrIcon = 0;
          }
          SetTrayIcon("norm");
          SetTag(LoadStates.Loaded);
          this.Resize(mySettings.MainSize.Width, mySettings.MainSize.Height);
          Main_SizeChanged(null, null);
          //cmdConfig.Click();
          tmrShowConfig = GLib.Timeout.Add(1000, tmrShowConfig_Tick);
        }
      }
    }
    private uint tmrResizeCheck;
    private Gdk.Rectangle lastRect;
    private Gdk.Rectangle thisRect;
    private void Form_SizeAllocated(object sender, SizeAllocatedArgs e)
    {
      if (e.Allocation.Equals(lastRect))
      {
        return;
      }
      lastRect = e.Allocation;
      if (tmrResizeCheck != 0)
      {
        GLib.Source.Remove(tmrResizeCheck);
        tmrResizeCheck = 0;
      }

      Main_SizeChanged(sender, e);

      thisRect = e.Allocation;
      tmrResizeCheck = GLib.Timeout.Add(400, frmMain_ResizeEnd);
    }
    private void Form_SizeConfigured(object sender, ConfigureEventArgs e)
    {
      Main_SizeChanged(sender, e);
    }
    private bool frmMain_ResizeEnd()
    {
      if (tmrResizeCheck != 0)
      {
        GLib.Source.Remove(tmrResizeCheck);
        tmrResizeCheck = 0;
      }
      if (!thisRect.IsEmpty)
      {
        this.Resize(thisRect.Width, thisRect.Height);
        Main_SizeChanged(null, null);
      }
      return false;
    }
    private float fRatio;
    private float fMin;
    private Gdk.Rectangle lastSize;
    private int iFontSize;
    protected void Main_SizeChanged(object o, EventArgs ea)
    {
      VBox baseBox = pnlDetails;
      if (firstRestore)
      {
        Gdk.Rectangle alloc;
        if (ea != null)
        {
          Gtk.SizeAllocatedArgs e = (Gtk.SizeAllocatedArgs)ea;
          if (e.Allocation.Equals(lastSize))
          {
            return;
          }
          lastSize = e.Allocation;
          alloc = e.Allocation;
        }
        else
        {
          alloc = this.Allocation;
        }
        if (mySettings == null)
        {
          ReLoadSettings();
        }
        if (fRatio == 0f | double.IsInfinity(fRatio) | double.IsNaN(fRatio))
        {
          fRatio = (GetMainFontSize() / 1024f) / 250f;
        } // 0.04125
        if (fMin == 0f | double.IsInfinity(fMin) | double.IsNaN(fMin))
        {
          fMin = GetMainFontSize() / 1024f;
        }

        if (mySettings.ScaleScreen)
        {
          float fontSize = fMin;
          if ((alloc.Width / 2) < alloc.Height)
          {
            if ((alloc.Width / 2) * fRatio > fMin)
            {
              fontSize = (int)Math.Ceiling((alloc.Width / 2) * fRatio);
            }
            else
            {
              fontSize = fMin;
            }
          }
          else
          {
            if (alloc.Height * fRatio > fMin)
            {
              fontSize = (int)Math.Ceiling(alloc.Height * fRatio);
            }
            else
            {
              fontSize = fMin;
            }
          }
          SetFontSize(ref baseBox, (int)Math.Ceiling(fontSize * 1024f));
          sbMainStatus.HeightRequest = (int)Math.Ceiling((GetFontSize() / 1024d) * 2.5d);
        }
        else
        {
          SetFontSize(ref baseBox, GetMainFontSize());
        }
        ResizePanels();
        if (myState == LoadStates.Loaded)
        {
          if (alloc.Width > 1 & alloc.Height > 1)
          {
            if (this.GdkWindow.State != Gdk.WindowState.Maximized)
            {
              mySettings.MainSize = alloc.Size;
            }
          }
        }
        else 
        {
          SetFontSize(ref lblRRS, GetFontSize());
          SetFontSize(ref lblNothing, (int)Math.Ceiling(GetFontSize() * 2.5f));
        }
      }
    }
    private int GetFontSize()
    {
      if (iFontSize == 0)
      {
        if (fRatio == 0f | double.IsInfinity(fRatio) | double.IsNaN(fRatio))
        {
          fRatio = (GetMainFontSize() / 1024f) / 250f;
        }
        if (fMin == 0f | double.IsInfinity(fMin) | double.IsNaN(fMin))
        {
          fMin = GetMainFontSize() / 1024f;
        }
        if (mySettings.ScaleScreen)
        {
          float fontSize = fMin;
          if ((this.Allocation.Width / 2) < this.Allocation.Height)
          {
            if ((this.Allocation.Width / 2) / fRatio > fMin)
            {
              fontSize = (int)Math.Ceiling((this.Allocation.Width / 2) * fRatio);
            }
            else
            {
              fontSize = fMin;
            }
          }
          else
          {
            if (this.Allocation.Height * fRatio > fMin)
            {
              fontSize = (int)Math.Ceiling(this.Allocation.Height * fRatio);
            }
            else
            {
              fontSize = fMin;
            }
          }
          iFontSize = (int)Math.Ceiling(fontSize * 1024f);
        }
        else
        {
          iFontSize = GetMainFontSize();
        }
      }
      return iFontSize;
    }
    private void SetFontSize(ref Label wLabel, int iSize)
    {
      string ttX = wLabel.TooltipText;
      if (markupList.ContainsKey(wLabel.Name + wLabel.Handle.ToString("x")))
      {
        string markup = markupList[wLabel.Name + wLabel.Handle.ToString("x")];
        string markupColor = "";
        string markupAnchor = "";
        if (markup.Contains("foreground="))
        {
          markupColor = markup.Substring(markup.IndexOf("foreground="));
          markupColor = " " + markupColor.Substring(0, markupColor.IndexOf("\">") + 1);
        }
        if (markup.Contains("<a"))
        {
          markupAnchor = markup.Substring(markup.IndexOf("<a"));
          markupAnchor = markupAnchor.Substring(0, markupAnchor.IndexOf(">") + 1);
          markupList[wLabel.Name + wLabel.Handle.ToString("x")] = markupAnchor + "<span size=\"" + iSize + "\"" + markupColor + ">" + wLabel.Text + "</span></a>";
          wLabel.Markup = markupList[wLabel.Name + wLabel.Handle.ToString("x")];
        }
        else
        {
          markupList[wLabel.Name + wLabel.Handle.ToString("x")] = "<span size=\"" + iSize + "\"" + markupColor + ">" + wLabel.Text + "</span>";
          wLabel.Markup = markupList[wLabel.Name + wLabel.Handle.ToString("x")];
        }
      }
      else
      {
        markupList.Add(wLabel.Name + wLabel.Handle.ToString("x"), "<span size=\"" + iSize + "\">" + wLabel.Text + "</span>");
        wLabel.Markup = markupList[wLabel.Name + wLabel.Handle.ToString("x")];
      }
      if (!string.IsNullOrEmpty(ttX))
      {
        wLabel.TooltipText = ttX;
      }
    }
    private void SetFontSize(ref VBox wPanel, int iSize)
    {
      iFontSize = iSize;
      Container bItem = (Container)wPanel;
      if (bItem.Children.Length > 0)
      {
        for (int i = 0; i < bItem.Children.Length; i++)
        {
          SetFontSize(ref bItem.Children[i], iSize);
        }
      }
    }
    private void SetFontSize(ref Widget wItem, int iSize)
    {
      if (wItem is Container)
      {
        Container bItem = (Container)wItem;
        if (bItem.Children.Length > 0)
        {
          for (int i = 0; i < bItem.Children.Length; i++)
          {
            SetFontSize(ref bItem.Children[i], iSize);
          }
        }
      }
      else
      {
        if (wItem.GetType().ToString() == "Gtk.Label")
        {
          Label wLabel = (Label)wItem;
          SetFontSize(ref wLabel, iSize);
        }
      }
    }
    private int GetMainFontSize()
    {
      return this.Style.FontDesc.Size;
    }
    private void ResizePanels()
    {
      if (trayRes < 8)
        trayRes = 8;
      string trayIcoVal = "";
        if (lblUsageUsedVal.Text == " -- ")
        {
          pctUsage.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayRProgress(pctUsage.Allocation.Size, 0, 1, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          trayIcoVal = "norm";
        }
        else
        {
          pctUsage.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayRProgress(pctUsage.Allocation.Size, uCache_used, uCache_lim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          if (imFree)
          {
            trayIcoVal = "graph_free";
          }
          else if (imSlowed)
          {
            trayIcoVal = "graph_slow";
          }
          else
          {
            int u = (int)Math.Round(((double)uCache_used / uCache_lim) * trayRes);
            if (u > trayRes)
              u = trayRes;
            trayIcoVal = "graph_" + u;
          }
        }
      
      if (trayIcoVal != "")
      {
        if (tmrIcon == 0)
        {
          SetTrayIcon(trayIcoVal);
        }
        else
        {
          sIconBefore = trayIcoVal;
        }
      }
    }
    private uint tmrRestored;
    private Gdk.Size szRestored;
    protected void Form_WindowStateChanged(object o, Gtk.WindowStateEventArgs e)
    {
      if (tmrRestored != 0)
      {
        GLib.Source.Remove(tmrRestored);
        tmrRestored = 0;
      }
      if (e.Event.ChangedMask == Gdk.WindowState.Iconified)
      {
        if (TraySupported & mySettings.TrayIconStyle != TrayStyles.Never)
        {
          bool isMinimized = ((e.Event.NewWindowState & Gdk.WindowState.Iconified) == Gdk.WindowState.Iconified);
          if (isMinimized)
          {
            if (mySettings.TrayIconStyle == TrayStyles.Minimized)
              TrayState = true;
            ((Gtk.Label)mnuRestore.Child).TextWithMnemonic = "_Restore";
            this.SkipTaskbarHint = true;
            if (CurrentOS.IsMac)
              ActivationPolicy.setPolicy(ApplicationActivationPolicy.Accessory);
          }
          else
          {
            if (mySettings.TrayIconStyle == TrayStyles.Minimized)
              TrayState = false;
            ((Gtk.Label)mnuRestore.Child).TextWithMnemonic = "_Hide";
            this.SkipTaskbarHint = false;
            if (CurrentOS.IsMac)
              ActivationPolicy.setPolicy(ApplicationActivationPolicy.Regular);
            if (!firstRestore)
            {
              firstRestore = true;
              szRestored = mySettings.MainSize;
              tmrRestored = GLib.Timeout.Add(600, tmrRestored_Tick);
            }
          }
        }
      }
      else if (e.Event.ChangedMask == Gdk.WindowState.Withdrawn)
      {
        if (TraySupported & mySettings.TrayIconStyle != TrayStyles.Never)
        {
          bool isWithdrawn = ((e.Event.NewWindowState & Gdk.WindowState.Withdrawn) == Gdk.WindowState.Withdrawn);
          if (isWithdrawn)
          {
            if (mySettings.TrayIconStyle == TrayStyles.Minimized)
              TrayState = true;
            ((Gtk.Label)mnuRestore.Child).TextWithMnemonic = "_Restore";
            this.SkipTaskbarHint = true;
            if (CurrentOS.IsMac)
              ActivationPolicy.setPolicy(ApplicationActivationPolicy.Accessory);
          }
        }
      }
      else if (e.Event.ChangedMask == Gdk.WindowState.Maximized)
      {
        bool isMaximized = ((e.Event.NewWindowState & Gdk.WindowState.Maximized) == Gdk.WindowState.Maximized);
        if (isMaximized)
        {
          this.SizeAllocate(this.Allocation);
        }
        else
        {
          szRestored = mySettings.MainSize;
          tmrRestored = GLib.Timeout.Add(100, tmrRestored_Tick);
        }
      }
      else if ((int)e.Event.ChangedMask == 32767)
      {
        //this.SizeAllocate(new Gdk.Rectangle(Gdk.Point.Zero, mySettings.MainSize));
      }
    }
    protected bool tmrRestored_Tick()
    {
      if (tmrRestored != 0)
      {
        GLib.Source.Remove(tmrRestored);
        tmrRestored = 0;
        this.Resize(szRestored.Width, szRestored.Height + 1);
        this.Resize(szRestored.Width, szRestored.Height);
        Gtk.Application.Invoke(null, null, Main_SizeChanged);
      }
      return false;
    }
    protected void Form_Closed(object sender, DeleteEventArgs a)
    {
      if (mySettings.TrayIconOnClose)
      {
        if ((this.GdkWindow.State & Gdk.WindowState.Iconified) != Gdk.WindowState.Iconified)
        {
          this.Iconify();
          a.RetVal = true;
          return;
        }
      }
      ClosingTime = true;
      if (tmrUpdate != 0)
      {
        GLib.Source.Remove(tmrUpdate);
        tmrUpdate = 0;
      }
      if (localData != null)
      {
        localDataEvent(false);
        localData.Dispose();
        localData = null;
      }
      if (mySettings != null)
      {
        mySettings.Save();
      }
      modDB.LOG_Terminate(false);
      HideTrayIcon();
      if (tmrStatus != 0)
      {
        GLib.Source.Remove(tmrStatus);
        tmrStatus = 0;
      }
      Application.Quit();
      a.RetVal = true;
    }
    #endregion
    #region "Initialization Functions"
    public void ReLoadSettings()
    {
      mySettings = new AppSettings();
      if (!mySettings.TLSProxy)
      {
        SecurityProtocolTypeEx useProtocol = SecurityProtocolTypeEx.None;
        foreach (SecurityProtocolTypeEx protocolTest in Enum.GetValues(typeof(SecurityProtocolTypeEx)))
        {
          if (((SecurityProtocolTypeEx)mySettings.SecurityProtocol & protocolTest) == protocolTest)
          {
            try
            {
              System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)protocolTest;
              useProtocol |= protocolTest;
            }
            catch (Exception)
            {
            }
          }
        }
        if (useProtocol == SecurityProtocolTypeEx.None)
        {
          foreach (SecurityProtocolTypeEx protocolTest in Enum.GetValues(typeof(SecurityProtocolTypeEx)))
          {
            try
            {
              System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)protocolTest;
              useProtocol |= protocolTest;
            }
            catch (Exception)
            {
            }
          }
          try
          {
            System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)useProtocol;
            mySettings.SecurityProtocol = (System.Net.SecurityProtocolType)useProtocol;
            mySettings.Save();
          }
          catch (Exception)
          {
          }
        }
        else
        {
          try
          {
            System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)useProtocol;
          }
          catch (Exception)
          {
          }
        }
      }
      modFunctions.ScreenDefaultColors(ref mySettings.Colors);
      modFunctions.NOTIFIER_STYLE = modFunctions.LoadAlertStyle(mySettings.AlertStyle);
      if (TraySupported)
      {
        if (mySettings.TrayIconStyle == TrayStyles.Always)
        {
          TrayState = true;
        }
        else if (mySettings.TrayIconStyle == TrayStyles.Minimized)
        {
          if (this.Visible)
            TrayState = false;
          else
            TrayState = true;
        }
        else
        {
          TrayState = false;
        }
      }
      else
        TrayState = false;
      if (string.IsNullOrEmpty(mySettings.NetTestURL))
      {
        cmdNetTest.Visible = false;
      }
      else
      {
        cmdNetTest.Visible = true;
        cmdNetTest.WidthRequest = 20;
        cmdNetTest.HeightRequest = 20;
        string sNetTestIco = System.IO.Path.Combine(modFunctions.AppDataPath, "netTest.png");
        if (File.Exists(sNetTestIco))
        {
          ((Gtk.Image)((Gtk.HBox)((Gtk.Alignment)cmdNetTest.Child).Child).Children[0]).Pixbuf = new Gdk.Pixbuf(sNetTestIco);
        }
        else
        {
          ((Gtk.Image)((Gtk.HBox)((Gtk.Alignment)cmdNetTest.Child).Child).Children[0]).PixbufAnimation = new Gdk.PixbufAnimation(null, "RestrictionTrackerGTK.Resources.throbber.gif");
          clsFavicon wsFavicon = new clsFavicon(wsFavicon_DownloadIconCompleted);
          wsFavicon.Start(mySettings.NetTestURL, mySettings.NetTestURL);
        }
        string sNetTestTitle = mySettings.NetTestURL;
        if (sNetTestTitle.Contains(Uri.SchemeDelimiter))
          sNetTestTitle = sNetTestTitle.Substring(sNetTestTitle.IndexOf(Uri.SchemeDelimiter) + 3);
        if (sNetTestTitle.StartsWith("www."))
          sNetTestTitle = sNetTestTitle.Substring(4);
        if (sNetTestTitle.Contains("/"))
          sNetTestTitle = sNetTestTitle.Substring(0, sNetTestTitle.IndexOf("/"));
        if (sNetTestTitle == "192.168.100.1" | ComparePixbufs(((Gtk.Image)((Gtk.HBox)((Gtk.Alignment)cmdNetTest.Child).Child).Children[0]).Pixbuf, Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.modem16.png")))
          sNetTestTitle = "ViaSat Modem";
        cmdNetTest.TooltipMarkup = "Visit " + sNetTestTitle + ".";
      }
    }
    bool ComparePixbufs(Gdk.Pixbuf Image1, Gdk.Pixbuf Image2)
    {
      if (Image1 == null)
        return false;
      if (Image2 == null)
        return false;
      try
      {
        if (Image1.Width != Image2.Width | Image1.Height != Image2.Height)
          return false;
      }
      catch (Exception)
      {
        return false;
      }
      for (int y = 0; y < Image1.Height; y++)
      {
        for (int x = 0; x < Image1.Width; x++)
        {
          IntPtr p1 = new IntPtr(Image1.Pixels.ToInt64() + (y * Image1.Rowstride) + (x * Image1.NChannels));
          byte p1R = System.Runtime.InteropServices.Marshal.ReadByte(p1);
          byte p1G = System.Runtime.InteropServices.Marshal.ReadByte(p1, 1);
          byte p1B = System.Runtime.InteropServices.Marshal.ReadByte(p1, 2);
          byte p1A = System.Runtime.InteropServices.Marshal.ReadByte(p1, 3);

          IntPtr p2 = new IntPtr(Image2.Pixels.ToInt64() + (y * Image2.Rowstride) + (x * Image2.NChannels));
          byte p2R = System.Runtime.InteropServices.Marshal.ReadByte(p2);
          byte p2G = System.Runtime.InteropServices.Marshal.ReadByte(p2, 1);
          byte p2B = System.Runtime.InteropServices.Marshal.ReadByte(p2, 2);
          byte p2A = System.Runtime.InteropServices.Marshal.ReadByte(p2, 3);

          if (p1R != p2R | p1G != p2G | p1B != p2B | p1A != p2A)
            return false;
        }
      }
      return true;
    }
    private class DownloadIconCompletedEventArgs
      : EventArgs
    {
      public Gdk.Pixbuf Icon16;
      public Gdk.Pixbuf Icon32;
      public object token;
      public Exception Error;
      public DownloadIconCompletedEventArgs(Gdk.Pixbuf ico16, Gdk.Pixbuf ico32, object oToken, Exception ex)
      {
        Icon16 = ico16;
        Icon32 = ico32;
        token = oToken;
        Error = ex;
      }
    }
    void wsFavicon_DownloadIconCompleted(Gdk.Pixbuf icon16, Gdk.Pixbuf icon32, object token, Exception Error)
    {
      Gtk.Application.Invoke(this, new DownloadIconCompletedEventArgs(icon16, icon32, token, Error), wsFavicon_DownloadIconCompletedAsync);
    }
    void wsFavicon_DownloadIconCompletedAsync(object sender, EventArgs ea)
    {
      DownloadIconCompletedEventArgs e = (DownloadIconCompletedEventArgs)ea;
      try
      {
        cmdNetTest.Visible = true;
        cmdNetTest.WidthRequest = 20;
        cmdNetTest.HeightRequest = 20;
        if (e.Error != null)
        {
          ((Gtk.Image)((Gtk.HBox)((Gtk.Alignment)cmdNetTest.Child).Child).Children[0]).Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.error.png");
        }
        else
        {
          e.Icon16.Save(System.IO.Path.Combine(modFunctions.AppDataPath, "netTest.png"), "png");
          ((Gtk.Image)((Gtk.HBox)((Gtk.Alignment)cmdNetTest.Child).Child).Children[0]).Pixbuf = e.Icon16;
        }
      }
      catch (Exception)
      {
        ((Gtk.Image)((Gtk.HBox)((Gtk.Alignment)cmdNetTest.Child).Child).Children[0]).Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.error.png");
      }
    }
    private void ReInit()
    {
      Gtk.Application.Invoke(Main_ReInit);
    }
    private void Main_ReInit(object o, EventArgs e)
    {
      if (localData != null)
      {
        localDataEvent(false);
        localData = null;
      }
      InitAccount();
      if (!string.IsNullOrEmpty(sAccount))
      {
        EnableProgressIcon();
        SetStatusText("Reloading", "Reloading History...", false);
        modDB.LOG_Initialize(sAccount, false);
        if (ClosingTime)
        {
          return;
        }
        DisplayUsage(false, false);
        SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Preparing Connection...", false);
        GetUsage();
      }
      if (ClosingTime)
        return;
      Main_SizeChanged(null, null);
    }
    private void EnableProgressIcon()
    {
      if (tmrIcon != 0)
      {
        GLib.Source.Remove(tmrIcon);
        tmrIcon = 0;
      }
      if (ClosingTime)
      {
        return;
      }
      sIconBefore = trayResource;
      bIconStop = false;
      iIconItem = 0;
      tmrIcon = GLib.Timeout.Add(200, tmrIcon_Tick);
    }
    private void StartTimer()
    {
      Gtk.Application.Invoke(null, null, Main_StartTimer);
    }
    private void Main_StartTimer(object o, EventArgs e)
    {
      NextGrabTick = long.MinValue;
      SetTag(LoadStates.Loaded);
    }
    private void SetTag(LoadStates Tag)
    {
      Gtk.Application.Invoke((object)Tag, null, Main_SetTag);
    }
    private void Main_SetTag(object o, EventArgs e)
    {
      myState = (LoadStates)o;
      if (myState == LoadStates.Loaded)
      {
        if (TraySupported & mySettings.TrayIconStyle == TrayStyles.Never)
          TrayState = false;
      }
    }
    private void initDB()
    {
      SetTag(LoadStates.DB);
      SetStatusText("Loading History", "Reading usage history into memory...", false);
      modDB.LOG_Initialize(sAccount, false);
      if (ClosingTime)
      {
        return;
      }
      if (tmrIcon != 0)
      {
        GLib.Source.Remove(tmrIcon);
        tmrIcon = 0;
      }
      SetStatusText("No History", "", false);
      DisplayUsage(true, false);
      MethodInvoker TimerInvoker = StartTimer;
      TimerInvoker.BeginInvoke(null, TimerInvoker);
    }
    private void InitAccount()
    {
      sAccount = mySettings.Account;
      if (!string.IsNullOrEmpty(mySettings.PassCrypt))
      {
        if (string.IsNullOrEmpty(mySettings.PassKey) | string.IsNullOrEmpty(mySettings.PassSalt))
          sPassword = StoredPasswordLegacy.DecryptApp(mySettings.PassCrypt);
        else
          sPassword = StoredPassword.Decrypt(mySettings.PassCrypt, mySettings.PassKey, mySettings.PassSalt);
      }
    }
    #endregion
    #region "Login Functions"
    private bool tmrShowConfig_Tick()
    {
      Gtk.Application.Invoke(null, null, Main_ShowConfig);
      return false;
    }
    private void Main_ShowConfig(object o, EventArgs e)
    {
      if (tmrShowConfig != 0)
      {
        GLib.Source.Remove(tmrShowConfig);
        tmrShowConfig = 0;
      }
      if (((Gtk.Label)mnuRestore.Child).Text == "Restore")
      {
        ShowFromTray();
      }
      cmdConfig.Click();
    }
    private bool tmrUpdate_Tick()
    {
      if (NextGrabTick != long.MaxValue)
      {
        long msInterval = mySettings.Interval * 60 * 1000;
        if (NextGrabTick == long.MinValue)
        {
          long minutesSinceLast = modFunctions.DateDiff(DateInterval.Minute, modDB.LOG_GetLast(), DateTime.Now);
          if (minutesSinceLast < mySettings.Interval)
          {
            long msSinceLast = minutesSinceLast * 60 * 1000;
            NextGrabTick = srlFunctions.TickCount() + (msInterval - msSinceLast) + (2 * 60 * 1000);
          }
          else
          {
            NextGrabTick = srlFunctions.TickCount();
          }
          if (NextGrabTick - srlFunctions.TickCount() < mySettings.StartWait * 60 * 1000)
          {
            NextGrabTick = srlFunctions.TickCount() + (mySettings.StartWait * 60 * 1000);
          }
        }
        if (srlFunctions.TickCount() >= NextGrabTick)
        {
          if ((mySettings.UpdateType != UpdateTypes.None) & (Math.Abs(DateTime.Now.Subtract(mySettings.LastUpdate).TotalDays) > mySettings.UpdateTime))
          {
            CheckForUpdates();
            NextGrabTick = srlFunctions.TickCount() + 10 * 60 * 1000;
          }
          else if (tmrSpeed != 0)
          {
            NextGrabTick = srlFunctions.TickCount() + 10 * 60 * 1000;
          }
          else
          {
            if (!string.IsNullOrEmpty(sAccount))
            {
              if (Math.Abs(modFunctions.DateDiff(DateInterval.Minute, modDB.LOG_GetLast(), DateTime.Now)) > 10)
              {
                if (!string.IsNullOrEmpty(sPassword))
                {
                  NextGrabTick = long.MaxValue;
                  PauseActivity = "Preparing Connection";
                  EnableProgressIcon();
                  SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Preparing Connection...", false);
                  MethodInvoker UsageInvoker = GetUsage;
                  UsageInvoker.BeginInvoke(null, null); 
                  return true;
                }
              }
            }
            NextGrabTick = srlFunctions.TickCount() + msInterval;
          }
        }
      }
      return true;
    }
    private void GetUsage()
    {
      Gtk.Application.Invoke(null, null, Main_GetUsage);
    }
    private void Main_GetUsage(object o, EventArgs e)
    {
      if (string.IsNullOrEmpty(sAccount) | string.IsNullOrEmpty(sPassword))
      {
        if (((Gtk.Label)mnuRestore.Child).Text == "Restore")
        {
          ShowFromTray();
        }
        cmdConfig.GrabFocus();
        modFunctions.ShowMessageBox(null, "You haven't entered your account details.\nPlease enter your account details in the Config window by clicking Configuration.", "Account Details Required", Gtk.DialogFlags.Modal, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok);
      }
      else
      {
        if (cmdRefresh.Sensitive)
        {
          cmdRefresh.Sensitive = false;
          NextGrabTick = srlFunctions.TickCount() + (mySettings.Timeout * 1000);
          if (localData != null)
          {
            localDataEvent(false);
            localData.Dispose();
            localData = null;
          }
          GrabAttempt = 0;
          localData = new RestrictionLibrary.Local.SiteConnection(modFunctions.AppData);
          localDataEvent(true);
        }
        else
        {
          if (localData != null)
          {
            localDataEvent(false);
            localData.Dispose();
            localData = null;
          }
          SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "", false);
          DisplayUsage(false, false);
          NextGrabTick = srlFunctions.TickCount() + 5000;
        }
      }
    }
    private class DisplayUsageEventArgs
      : EventArgs
    {
      public bool StatusText;
      public bool HardTime;
      public DisplayUsageEventArgs(bool bStatText, bool bHardTime)
      {
        StatusText = bStatText;
        HardTime = bHardTime;
      }
    }
    private void DisplayUsage(bool setStatusText, bool setHardTime)
    {
      Gtk.Application.Invoke(new object(), new DisplayUsageEventArgs(setStatusText, setHardTime), Main_DisplayUsage);
    }
    private void Main_DisplayUsage(object o, EventArgs ea)
    {
      DisplayUsageEventArgs e = (DisplayUsageEventArgs)ea;
      if (!cmdRefresh.Sensitive)
      {
        cmdRefresh.Sensitive = true;
      }
      if (tmrIcon != 0)
      {
        GLib.Source.Remove(tmrIcon);
        tmrIcon = 0;
      }
      if (e.HardTime)
      {
        NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      }
      else
      {
        NextGrabTick = long.MinValue;
      }
      if (modDB.LOG_GetCount() > 0)
      {
        DateTime dtDate;
        long lUsed;
        long lLimit;
        modDB.LOG_Get(modDB.LOG_GetCount() - 1, out dtDate, out lUsed, out lLimit);
        if (e.StatusText)
        {
          SetStatusText(srlFunctions.TimeToString(dtDate), "", false);
        }
        DisplayResults(lUsed, lLimit);
      }
      if ((this.SkipTaskbarHint == true) && CurrentOS.IsMac)
        ActivationPolicy.setPolicy(ApplicationActivationPolicy.Accessory);
    }
    private void SetNextLoginTime()
    {
      SetNextLoginTime(-1);
    }
    private void SetNextLoginTime(int MinutesAhead)
    {
      if (localData != null)
      {
        localDataEvent(false);
        localData.Dispose();
        localData = null;
      }
      if (MinutesAhead > -1)
      {
        NextGrabTick = srlFunctions.TickCount() + (MinutesAhead * 60 * 1000);
      }
      else
      {
        NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      }
    }
    #endregion
    #region "Local Usage Events"
    private void localData_ConnectionStatus(object sender, RestrictionLibrary.Local.SiteConnectionStatusEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_LocalDataConnectionStatus);
    }
    private void Main_LocalDataConnectionStatus(object o, EventArgs ea)
    {
      RestrictionLibrary.Local.SiteConnectionStatusEventArgs e = (RestrictionLibrary.Local.SiteConnectionStatusEventArgs)ea;
      NextGrabTick = srlFunctions.TickCount() + ((mySettings.Timeout + 15) * 1000);
      string sAppend = "";
      if (e.Attempt > 0)
      {
        if (e.Stage > 0)
        {
          sAppend = " (Stage " + (e.Stage + 1) + ", Redirect #" + e.Attempt + ")";
        }
        else
        {
          sAppend = " (Redirect #" + e.Attempt + ")";
        }
      }
      else if (e.Stage > 0)
      {
        sAppend = " (Stage " + (e.Stage + 1) + ")";
      }
      switch (e.Status)
      {
        case RestrictionLibrary.Local.SiteConnectionStates.Initialize:
          SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Initializing Connection" + sAppend + "...", false);
          break;
        case RestrictionLibrary.Local.SiteConnectionStates.Prepare:
          SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Preparing to Log In" + sAppend + "...", false);
          break;
        case RestrictionLibrary.Local.SiteConnectionStates.Login:
          switch (e.SubState)
          {
            case RestrictionLibrary.Local.SiteConnectionSubStates.ReadLogin:
              SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Reading Login Page" + sAppend + "...", false);
              break;
            case RestrictionLibrary.Local.SiteConnectionSubStates.Authenticate:
              SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Authenticating" + sAppend + "...", false);
              break;
            default:
              SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Logging In" + sAppend + "...", false);
              break;
          }
          break;
        case RestrictionLibrary.Local.SiteConnectionStates.TableDownload:
          switch (e.SubState)
          {
            case RestrictionLibrary.Local.SiteConnectionSubStates.LoadHome:
              SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Downloading Home Page" + sAppend + "...", false);
              break;
            case RestrictionLibrary.Local.SiteConnectionSubStates.LoadTable:
              SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Downloading Usage Table" + sAppend + "...", false);
              break;
            default:
              SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Downloading Usage Table" + sAppend + "...", false);
              break;
          }
          break;
        case RestrictionLibrary.Local.SiteConnectionStates.TableRead:
          SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Reading Usage Table" + sAppend + "...", false);
          break;
      }
    }
    private void localData_ConnectionFailure(object sender, RestrictionLibrary.Local.SiteConnectionFailureEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_LocalDataConnectionFailure);
    }
    private void Main_LocalDataConnectionFailure(object o, EventArgs ea)
    {
      RestrictionLibrary.Local.SiteConnectionFailureEventArgs e = (RestrictionLibrary.Local.SiteConnectionFailureEventArgs)ea;
      switch (e.Type)
      {
        case RestrictionLibrary.Local.SiteConnectionFailureType.LoginIssue:
          GrabAttempt = 0;
          SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), e.Message, true);
          DisplayUsage(false, false);
          return;
        case RestrictionLibrary.Local.SiteConnectionFailureType.ConnectionTimeout:
          if (GrabAttempt < mySettings.Retries)
          {
            GrabAttempt++;
            string sMessage = "Connection Timed Out! Retry " + GrabAttempt + " of " + mySettings.Retries + "...";
            SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), sMessage, true);
            if (localData != null)
            {
              localDataEvent(false);
              localData.Dispose();
              localData = null;
            }
            localData = new RestrictionLibrary.Local.SiteConnection(modFunctions.AppData);
            localDataEvent(true);
            return;
          }
          SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Connection Timed Out!", true);
          DisplayUsage(false, false);
          break;
        case RestrictionLibrary.Local.SiteConnectionFailureType.TLSTooOld:
          GrabAttempt = 0;
          if (mySettings.TLSProxy)
          {
            SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Please enable TLS 1.1 or 1.2 under Security Protocol in the Network tab of the Config window to connect.", true);
            DisplayUsage(false, false);
          }
          else
          {
            string clrVer = RestrictionLibrary.srlFunctions.CLRCleanVersion;
            Version clr = new Version(clrVer.Substring(5));
            if (clr.Major < 4 || (clr.Major == 4 & clr.Minor < 8))
            {
              SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Security Protocol requires MONO 4.8 or newer. If you can't install MONO 4.8, please use the TLS Proxy feature for now.", true);
              DisplayUsage(false, false);
            }
            else
            {
              if (e.Message == "VER")
              {
                SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Please enable TLS 1.1 or 1.2 under Security Protocol in the Network tab of the Config window to connect.", true);
                DisplayUsage(false, false);
              }
              else if (e.Message == "PROXY")
              {
                SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Even though TLS 1.1 or 1.2 was enabled, the server still didn't like the request. Please use the TLS Proxy feature under Security Protocol in the Network tab of the Config window to bypass this problem.", true);
                DisplayUsage(false, false);
              }
            }
          }
          break;
        case RestrictionLibrary.Local.SiteConnectionFailureType.LoginFailure:
          if (e.Message.EndsWith("Please try again.") && GrabAttempt < mySettings.Retries)
          {
            GrabAttempt++;
            string sMessage = e.Message.Substring(0, e.Message.IndexOf("Please try again.")) + "Retry " + GrabAttempt + " of " + mySettings.Retries + "...";
            SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), sMessage, true);
            if (localData != null)
            {
              localDataEvent(false);
              localData.Dispose();
              localData = null;
            }
            localData = new RestrictionLibrary.Local.SiteConnection(modFunctions.AppData);
            localDataEvent(true);
            return;
          }
          SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), e.Message, true);
          DisplayUsage(false, true);
          break;
        case RestrictionLibrary.Local.SiteConnectionFailureType.UnknownAccountDetails:
          GrabAttempt = 0;
          SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Please enter your account details in the Config window.", true);
          DisplayUsage(false, false);
          if ((this.GdkWindow.State & Gdk.WindowState.Iconified) == Gdk.WindowState.Iconified)
          {
            ShowFromTray();
          }
          cmdConfig.GrabFocus();
          modFunctions.ShowMessageBox(null, "You haven't entered your account details.\nPlease enter your account details in the Config window by clicking Configuration.", "Account Details Required", Gtk.DialogFlags.Modal, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok);
          break;
      }
      if (localData != null)
      {
        localDataEvent(false);
        localData.Dispose();
        localData = null;
      }
    }
    private void localData_ConnectionResult(object sender, RestrictionLibrary.Local.SiteResultEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_LocalDataConnectionResult);
    }
    private void Main_LocalDataConnectionResult(object o, EventArgs ea)
    {
      RestrictionLibrary.Local.SiteResultEventArgs e = (RestrictionLibrary.Local.SiteResultEventArgs)ea;
      GrabAttempt = 0;
      SetStatusText(srlFunctions.TimeToString(e.Update), "Saving History...", false);
      NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      modDB.LOG_Add(e.Update, e.Used, e.Limit, true);
      modFunctions.ScreenDefaultColors(ref mySettings.Colors );
      if (e.SlowedDetected)
        imSlowed = true;
      imFree = e.FreeDetected;
      DisplayUsage(true, true);
      if (localData != null)
      {
        localDataEvent(false);
        localData.Dispose();
        localData = null;
      }
    }
    #endregion
    #region "Graphs"
    private System.Threading.Timer tmrChanges;
    private void DisplayChangeInterval(object state)
    {
      Gtk.Application.Invoke(state, null, Main_DisplayChangeInterval);
    }
    private void Main_DisplayChangeInterval(object o, EventArgs a)
    {
      string state = (string)o;
      if (tmrChanges != null)
      {
        tmrChanges.Dispose();
        tmrChanges = null;
      }
      else
      {
        return;
      }
      long lUsed = uCache_used;
      long lLim = uCache_lim;
      long lFree = lLim - lUsed;
      if (lUsed != 0 | lLim > 0 | lFree != 0)
      {
        DoChange(ref lblUsageUsedVal, ref lUsed, false);
        DoChange(ref lblUsageFreeVal, ref lFree, (lFree <= 0));
        DoChange(ref lblUsageLimitVal, ref lLim, imSlowed);
      }
      ResizePanels();
      if (lUsed == 0 & lLim == 0 & lFree == 0)
      {
        return;
      }
      if (tmrChanges != null)
      {
        tmrChanges.Dispose();
        tmrChanges = null;
      }
      tmrChanges = new System.Threading.Timer(new System.Threading.TimerCallback(DisplayChangeInterval), state, 25, System.Threading.Timeout.Infinite);
    }
    private void DoChange(ref Label lblTemp, ref long toVal, bool red)
    {
      long tmpVal = 0;
      string tmpStr = " -- ";
      if (lblTemp.Text.Length > 3 & lblTemp.Text.Contains(" "))
      {
        string tmpS = lblTemp.Text.Substring(0, lblTemp.Text.LastIndexOf(' '));
        if (tmpS.Contains(","))
          tmpS = tmpS.Replace(",", "");
        if (modFunctions.IsNumeric(tmpS))
        {
          tmpVal = long.Parse(tmpS, System.Globalization.CultureInfo.InvariantCulture);
        }
        else
        {
          tmpVal = 0;
        }
        long majorDif = Math.Abs(tmpVal - toVal);
        if (majorDif < 10)
        {
          majorDif = 1;
        }
        else if (majorDif < 50)
        {
          majorDif = 3;
        }
        else if (majorDif < 100)
        {
          majorDif = 7;
        }
        else if (majorDif < 500)
        {
          majorDif = 73;
        }
        else if (majorDif < 1000)
        {
          majorDif = 271;
        }
        else if (majorDif < 5000)
        {
          majorDif = 977;
        }
        else if (majorDif < 10000)
        {
          majorDif = 3347;
        }
        else if (majorDif < 50000)
        {
          majorDif = 8237;
        }
        else
        {
          majorDif = 38671;
        }
        if (tmpVal > toVal)
        {
          tmpVal -= majorDif;
          tmpStr = tmpVal.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " MB";
        }
        else if (tmpVal < toVal)
        {
          tmpVal += majorDif;
          tmpStr = tmpVal.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " MB";
        }
        else
        {
          tmpStr = toVal.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " MB";
          toVal = 0;
        }
      }
      else
      {
        tmpStr = toVal.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " MB";
        toVal = 0;
      }
      if (red)
      {
        if (markupList.ContainsKey(lblTemp.Name + lblTemp.Handle.ToString("x")))
        {
          string markup = markupList[lblTemp.Name + lblTemp.Handle.ToString("x")];
          string markupAnchor = "";
          if (markup.Contains("<a"))
          {
            markupAnchor = markup.Substring(markup.IndexOf("<a"));
            markupAnchor = markupAnchor.Substring(0, markupAnchor.IndexOf(">") + 1);
            markupList[lblTemp.Name + lblTemp.Handle.ToString("x")] = markupAnchor + "<span size=\"" + GetFontSize() + "\" foreground=\"red\">" + tmpStr + "</span></a>";
            lblTemp.Markup = markupList[lblTemp.Name + lblTemp.Handle.ToString("x")];
          }
          else
          {
            markupList[lblTemp.Name + lblTemp.Handle.ToString("x")] = "<span size=\"" + GetFontSize() + "\" foreground=\"red\">" + tmpStr + "</span>";
            lblTemp.Markup = markupList[lblTemp.Name + lblTemp.Handle.ToString("x")];
          }
        }
        else
        {
          markupList.Add(lblTemp.Name + lblTemp.Handle.ToString("x"), "<span size=\"" + GetFontSize() + "\" foreground=\"red\">" + tmpStr + "</span>");
          lblTemp.Markup = markupList[lblTemp.Name + lblTemp.Handle.ToString("x")];
        }
      }
      else
      {
        if (markupList.ContainsKey(lblTemp.Name + lblTemp.Handle.ToString("x")))
        {
          string markup = markupList[lblTemp.Name + lblTemp.Handle.ToString("x")];
          string markupColor = "";
          string markupAnchor = "";
          if (markup.Contains("foreground="))
          {
            markupColor = markup.Substring(markup.IndexOf("foreground="));
            markupColor = " " + markupColor.Substring(0, markupColor.IndexOf("\">") + 1);
            if (markupColor == " foreground=\"red\"")
            {
              markupColor = "";
            }
          }
          if (markup.Contains("<a"))
          {
            markupAnchor = markup.Substring(markup.IndexOf("<a"));
            markupAnchor = markupAnchor.Substring(0, markupAnchor.IndexOf(">") + 1);
            markupList[lblTemp.Name + lblTemp.Handle.ToString("x")] = markupAnchor + "<span size=\"" + GetFontSize() + "\"" + markupColor + ">" + tmpStr + "</span></a>";
            lblTemp.Markup = markupList[lblTemp.Name + lblTemp.Handle.ToString("x")];
          }
          else
          {
            markupList[lblTemp.Name + lblTemp.Handle.ToString("x")] = "<span size=\"" + GetFontSize() + "\"" + markupColor + ">" + tmpStr + "</span>";
            lblTemp.Markup = markupList[lblTemp.Name + lblTemp.Handle.ToString("x")];
          }
        }
        else
        {
          markupList.Add(lblTemp.Name + lblTemp.Handle.ToString("x"), "<span size=\"" + GetFontSize() + "\">" + tmpStr + "</span>");
          lblTemp.Markup = markupList[lblTemp.Name + lblTemp.Handle.ToString("x")];
        }
      }
    }
    #region "Results"
    private string MBorGB(long value)
    {
      if (value > 999)
      {
        return Math.Round(((double)value / MBPerGB), mySettings.Accuracy).ToString().Trim() + " GB";
      }
      else
      {
        return value.ToString().Trim() + " MB";
      }
    }
    private string AccuratePercent(double value)
    {
      return modFunctions.FormatPercent(value, mySettings.Accuracy);
    }
    private void DisplayUsageResults(long lUsed, long lLimit, string sLastUpdate)
    {
      string sTTT = this.Title;
      imSlowed = (lUsed >= lLimit);
      if (!pnlUsage.Visible)
      {
        pnlUsage.Visible = true;
        pnlDisplays.Add(pnlUsage);
      }
      if (pnlNothing.Visible)
      {
        pnlNothing.Visible = false;
        pnlDisplays.Remove(pnlNothing);
      }
      uCache_used = lUsed;
      uCache_lim = lLimit;
      if (tmrChanges != null)
      {
        tmrChanges.Dispose();
        tmrChanges = null;
      }
      tmrChanges = new System.Threading.Timer(DisplayChangeInterval, null, 75, System.Threading.Timeout.Infinite);
      if (imSlowed)
      {
        lblUsageFreeVal.TooltipText = "You are over your usage limit!";
        lblUsageLimitVal.TooltipText = "Your connection has been restricted!";
      }
      else
      {
        lblUsageFreeVal.TooltipText = null;
        lblUsageLimitVal.TooltipText = null;
      }
      pctUsage.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayRProgress(pctUsage.Allocation.Size, lUsed, lLimit, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
      sTTT = "Satellite Usage" + (imSlowed ? " (Slowed) " : "") + "\n" +
        "Updated " + sLastUpdate + "\n" +
        MBorGB(lUsed) + " of " + MBorGB(lLimit) + " (" + AccuratePercent((double)lUsed / lLimit) + ")";
      if (lLimit > lUsed)
      {
        sTTT += "\n" + MBorGB(lLimit - lUsed) + " Free";
      }
      else if (lLimit < lUsed)
      {
        sTTT += "\n" + MBorGB(lUsed - lLimit) + " Over";
      }
      if (imFree)
      {
        sIconBefore = "graph_free";
      }
      else if (imSlowed)
      {
        sIconBefore = "graph_slow";
      }
      else
      {
        if (trayRes < 8)
          trayRes = 8;
        int u = (int)Math.Round(((double)lUsed / lLimit) * trayRes);
        if (u > trayRes)
          u = trayRes;
        sIconBefore = "graph_" + u;
      }
      bIconStop = true;
      SetTrayText(sTTT);
      if (mySettings.Overuse > 0)
      {
        if (lastBalloon > 0 && srlFunctions.TickCount() - lastBalloon < mySettings.Overtime * 60 * 1000)
        {
          return;
        }
        int timeCheck = -mySettings.Overtime;
        if (timeCheck <= -15)
        {
          DataRow[] lItems = Array.FindAll(modDB.usageDB.ToArray(), (DataRow satRow) => satRow.DATETIME.CompareTo(DateTime.Now.AddMinutes(timeCheck)) >= 0 & satRow.DATETIME.CompareTo(DateTime.Now) <= 0);
          for (int I = lItems.Length - 2; I >= 0; I += -1)
          {
            if (lUsed - lItems[I].USED >= mySettings.Overuse)
            {
              long ChangeSize = Math.Abs(lUsed - lItems[I].USED);
              ulong ChangeTime = (ulong)Math.Abs(modFunctions.DateDiff(DateInterval.Minute, lItems[I].DATETIME, DateTime.Now) * 60 * 1000);
              modFunctions.MakeNotifier(ref taskNotifier, false);
              if (taskNotifier != null)
              {
                taskNotifier.Show("Excessive Usage Detected", modFunctions.ProductName + " has logged a usage change of " + MBorGB(ChangeSize) + " in " + modFunctions.ConvertTime(ChangeTime, false, true) + "!", 200, 0, 100);
              }
              lastBalloon = srlFunctions.TickCount();
              break;
            }
          }
        }
      }
    }
    private void DisplayResults(long lUsed, long lLimit)
    {
      if (lLimit > 0)
      {
        DateTime lastUpdate = modDB.LOG_GetLast();
        string sLastUpdate = lastUpdate.ToString("M/d h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
        DisplayUsageResults(lUsed, lLimit, sLastUpdate);
      }
      else
      {
        if (pnlUsage.Visible)
        {
          pnlUsage.Visible = false;
          pnlDisplays.Remove(pnlUsage);
        }
        if (!pnlNothing.Visible)
        {
          pnlNothing.Visible = true;
          pnlDisplays.Add(pnlNothing);
        }

        SetTrayText(this.Title);
        sIconBefore = "norm";
        bIconStop = true;
      }
    }
    #endregion
    #endregion
    #region "Buttons"
    [GLib.ConnectBefore]
    protected void cmdRefresh_Click(object sender, ButtonReleaseEventArgs e)
    {
      if (e != null)
      {
        if (e.Event.Button != 1)
          return;
        if (e.Event.X < 0)
          return;
        if (e.Event.X > cmdRefresh.Allocation.Size.Width)
          return;
        if (e.Event.Y < 0)
          return;
        if (e.Event.Y > cmdRefresh.Allocation.Size.Height)
          return;
      }
      InitAccount();
      if ((!string.IsNullOrEmpty(sAccount)) & (!string.IsNullOrEmpty(sPassword)))
      {
        EnableProgressIcon();
        SetNextLoginTime();
        if (e != null)
        {
          if ((e.Event.State & Gtk.Accelerator.DefaultModMask) == Gdk.ModifierType.ControlMask)
          {
            cmdRefresh.Sensitive = false;
            SetStatusText("Reloading", "Reloading History...", false);
            ShowProgress("Reloading History...", "Reading DataBase...", true);
            modDB.LOG_Initialize(sAccount, true);
            HideProgress();
            if (ClosingTime)
            {
              return;
            }
            cmdRefresh.Sensitive = true;
          }
        }
        SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Reading Usage...", false);
        MethodInvoker UsageInvoker = GetUsage;
        UsageInvoker.BeginInvoke(null, null);
      }
      else
      {
        if (((Gtk.Label)mnuRestore.Child).Text == "Restore")
        {
          ShowFromTray();
        }
        cmdConfig.GrabFocus();
        modFunctions.ShowMessageBox(null, "You haven't entered your account details.\nPlease enter your account details in the Config window by clicking Configuration.", "Account Details Required", Gtk.DialogFlags.Modal, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok);
      }
    }
    public void cmdHistory_Click(object sender, EventArgs e)
    {
      mySettings.Save();
      if (MainClass.fHistory != null)
      {
        if (MainClass.fHistory.Visible)
        {
          return;
        }
        MainClass.fHistory.Dispose();
      }
      MainClass.fHistory = new frmHistory();
      MainClass.fHistory.TransientFor = this;
      MainClass.fHistory.Show();
      MainClass.fHistory.Present();
    }
    public void cmdConfig_Click(object sender, EventArgs e)
    {
      bool bReRun = false;
      if (localData != null)
      {
        localDataEvent(false);
        localData.Dispose();
        localData = null;
        bReRun = false;
      }
      mySettings.Save();
      NextGrabTick = long.MaxValue;
      PauseActivity = "Configuration Open";
      Gtk.ResponseType dRet;
      if (MainClass.fConfig != null)
      {
        if (MainClass.fConfig.Visible)
        {
          return;
        }
        MainClass.fConfig.Destroy();
      }
      MainClass.fConfig = new dlgConfig();
      MainClass.fConfig.TransientFor = this;
      MainClass.fConfig.Modal = true;
      do
      {
        dRet = (Gtk.ResponseType)MainClass.fConfig.Run();
      } while (dRet == ResponseType.None);
      MainClass.fConfig.Hide();
      MainClass.fConfig.Destroy();
      MainClass.fConfig = null;
      long waitTime = srlFunctions.TickCount() + 2000;
      if (myState != LoadStates.Loaded)
      {
        if (myState != LoadStates.DB)
        {
          MethodInvoker initDBInvoker = initDB;
          initDBInvoker.BeginInvoke(null, null);
        }
        while (myState != LoadStates.Loaded)
        {
          System.Threading.Thread.Sleep(0);
          System.Threading.Thread.Sleep(1);
          if (srlFunctions.TickCount() > waitTime)
            break;
        }
      }
      switch (dRet)
      {
        case ResponseType.Yes:
          mySettings = null;
          ReLoadSettings();
          SetNextLoginTime();
          MakeCustomIconListing();
          MethodInvoker ReInitInvoker = ReInit;
          ReInitInvoker.BeginInvoke(null, null);
          break;
        case ResponseType.Ok:
          mySettings = null;
          ReLoadSettings();
          MakeCustomIconListing();
          if (bReRun)
          {
            SetNextLoginTime();
            EnableProgressIcon();
            SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Preparing Connection...", false);
            MethodInvoker UsageInvoker = GetUsage;
            UsageInvoker.BeginInvoke(null, null);
          }
          else
          {
            DisplayUsage(true, false);
          }
          Main_SizeChanged(null, null);
          if (MainClass.fHistory != null)
          {
            MainClass.fHistory.mySettings = new AppSettings();
            modFunctions.ScreenDefaultColors(ref MainClass.fHistory.mySettings.Colors);
            MainClass.fHistory.DoResize(true);
          }
          break;
        case ResponseType.Reject:
          break;
        default:
          if (bReRun)
          {
            SetNextLoginTime();
            EnableProgressIcon();
            SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Preparing Connection...", false);
            MethodInvoker UsageInvoker = GetUsage;
            UsageInvoker.BeginInvoke(null, null);
          }
          else
          {
            DisplayUsage(false, false);
          }
          break;
      }
    }
    public void cmdAbout_Click(object sender, EventArgs e)
    {
      if (MainClass.fAbout != null)
      {
        if (MainClass.fAbout.Visible)
        {
          return;
        }
        MainClass.fAbout.Dispose();
      }
      MainClass.fAbout = new frmAbout();
      MainClass.fAbout.Show();
      MainClass.fAbout.Present();
    }
    #endregion
    #region "Menus"
    #region "Tray"
    protected void mnuRestore_Click(object sender, EventArgs e)
    {
      if (((Gtk.Label)mnuRestore.Child).Text == "Hide")
      {
        HideToTray();
      }
      else
      {
        ShowFromTray();
      }
    }
    protected void mnuAbout_Click(object sender, EventArgs e)
    {
      if (MainClass.fAbout != null)
      {
        if (MainClass.fAbout.Visible)
        {
          return;
        }
        MainClass.fAbout.Dispose();
      }
      MainClass.fAbout = new frmAbout();
      MainClass.fAbout.Show();
      MainClass.fAbout.Present();
    }
    protected void mnuExit_Click(object sender, EventArgs e)
    {
      Gtk.Main.Quit();
    }
    #endregion
    #region "Graph"
    protected void mnuGraphRefresh_Click(object sender, EventArgs e)
    {
      cmdRefresh_Click(sender, null);
    }
    protected void mnuGraphColors_Click(object sender, EventArgs e)
    {
      Gtk.ResponseType dRet;
      if (MainClass.fCustomColors != null)
      {
        if (MainClass.fCustomColors.Visible)
        {
          return;
        }
        MainClass.fCustomColors.Dispose();
      }
      MainClass.fCustomColors = new dlgCustomColors(mySettings);
      MainClass.fCustomColors.TransientFor = this;
      MainClass.fCustomColors.Modal = true;
      do
      {
        dRet = (Gtk.ResponseType)MainClass.fCustomColors.Run();
      } while (dRet == ResponseType.None);
      if (dRet == ResponseType.Yes)
      {
        mySettings = MainClass.fCustomColors.mySettings;
        mySettings.Save();
        Main_SizeChanged(null, null);
        if (MainClass.fHistory != null)
        {
          MainClass.fHistory.mySettings = new AppSettings();
          modFunctions.ScreenDefaultColors(ref MainClass.fHistory.mySettings.Colors);
          MainClass.fHistory.DoResize(true);
        }
        MakeCustomIconListing();
        DisplayUsage(false, false);
      }
      MainClass.fCustomColors.Destroy();
      MainClass.fCustomColors = null;
    }
    #endregion
    #endregion
    #region "StatusBar"
    private System.Threading.Timer tmrPulse;
    public void ShowProgress(string sTitle, string sSubtitle, bool withProgress)
    {
      if (!string.IsNullOrEmpty(sTitle))
      {
        if (sTitle.Contains("\n"))
        {
          sTitle = sTitle.Substring(0, sTitle.IndexOf("\n"));
        }
        Frame fStatus = (Frame)sbMainStatus.Children[0];
        fStatus.BorderWidth = 0;
        Box bStatus = (Box)fStatus.Children[0];
        bStatus.BorderWidth = 0;
        Label lStatus = (Label)bStatus.Children[0];
        lStatus.Markup = "<span size=\"" + GetFontSize() + "\">" + sTitle + "</span>";
        lStatus.Yalign = 0.25f;
        sbMainStatus.HeightRequest = (int)Math.Ceiling((GetFontSize() / 1024d) * 2.5d);
      }
      if (!string.IsNullOrEmpty(sSubtitle))
      {
        if (sSubtitle.Contains("\n"))
        {
          sSubtitle = sSubtitle.Substring(0, sSubtitle.IndexOf("\n"));
        }
        lblMainStatus.Text = sSubtitle;
      }
      lblMainStatus.Visible = true;
      if (withProgress)
      {
        pbMainStatus.Visible = true;
        if (tmrPulse == null)
        {
          tmrPulse = new System.Threading.Timer(tmrPulse_Tick, null, 0, 150);
        }
      }
    }
    private void tmrPulse_Tick(object o)
    {
      pbMainStatus.Pulse();
      Gtk.Main.IterationDo(false);
    }
    public void SetProgress(int value, int max, string Subtitle)
    {
      if (max > 0)
      {
        if (tmrPulse != null)
        {
          tmrPulse.Dispose();
          tmrPulse = null;
        }
        pbMainStatus.Fraction = ((double)value / max);
      }
      else
      {
        if (tmrPulse != null)
        {
          tmrPulse.Dispose();
          tmrPulse = null;
        }
        tmrPulse = new System.Threading.Timer(tmrPulse_Tick, null, 0, 150);
      }
      if (!string.IsNullOrEmpty(Subtitle))
      {
        lblMainStatus.Text = Subtitle;
      }
    }
    public void HideProgress()
    {
      if (tmrPulse != null)
      {
        tmrPulse.Dispose();
        tmrPulse = null;
      }
      Frame fStatus = (Frame)sbMainStatus.Children[0];
      Box bStatus = (Box)fStatus.Children[0];
      Label lStatus = (Label)bStatus.Children[0];
      lStatus.Markup = "<span size=\"" + GetFontSize() + "\">Idle</span>";
      pbMainStatus.Fraction = 0d;
      lblMainStatus.Text = "";

      pbMainStatus.Visible = false;
      lblMainStatus.Visible = false;
    }
    #endregion
    #region "Tray Icon"
    bool bIconStop;
    int iIconItem;
    string sIconBefore;
    private bool tmrIcon_Tick()
    {
      if (tmrIcon == 0)
        return false;
      if (iIconItem == 0 & bIconStop)
      {
        if (tmrIcon != 0)
        {
          GLib.Source.Remove(tmrIcon);
          tmrIcon = 0;
        }
        SetTrayIcon(sIconBefore);
        bIconStop = false;
        return false;
      }
      switch (iIconItem)
      {
        case 0:
          SetTrayIcon("throb_0");
          break;
        case 1:
          SetTrayIcon("throb_1");
          break;
        case 2:
          SetTrayIcon("throb_2");
          break;
        case 3:
          SetTrayIcon("throb_3");
          break;
        case 4:
          SetTrayIcon("throb_4");
          break;
        case 5:
          SetTrayIcon("throb_5");
          break;
        case 6:
          SetTrayIcon("throb_6");
          break;
        case 7:
          SetTrayIcon("throb_7");
          break;
        case 8:
          SetTrayIcon("throb_8");
          break;
        case 9:
          SetTrayIcon("throb_9");
          break;
        case 10:
          SetTrayIcon("throb_10");
          break;
        case 11:
          SetTrayIcon("throb_11");
          break;
      }
      iIconItem += 1;
      if (iIconItem >= 12)
      {
        iIconItem = 0;
      }
      return true;
    }
    protected void OnTrayIconPopup(object o, EventArgs e)
    {
      mnuTray.Popup();
    }
    protected void OnTrayIconActivate(object o, EventArgs e)
    {
      if (((Gtk.Label)mnuRestore.Child).Text == "Hide")
      {
        HideToTray();
      }
      else
      {
        ShowFromTray();
      }
    }
    public void ShowFromTray()
    {
      this.Present();
      this.Move((Screen.Width - this.Allocation.Width) / 2, (Screen.Height - this.Allocation.Height) / 2);
      szRestored = mySettings.MainSize;
      tmrRestored = GLib.Timeout.Add(1000, tmrRestored_Tick);
      if (mySettings.TrayIconStyle == TrayStyles.Minimized)
        TrayState = false;
    }
    public void HideToTray()
    {
      this.Iconify();
    }
    private void ShowTrayIcon()
    {
      if (TrayState & TraySupported)
      {
        if (mTraySupport == TraySupport.AppIndicator)
        {
          try
          {
            appIcon.Status = AppIndicator.Status.Active;
          }
          catch (Exception)
          {
            mTraySupport = TraySupport.Standard;
          }
        }
        if (mTraySupport == TraySupport.Standard)
          trayIcon.Visible = true;
        SetTrayIcon(trayResource);
      }
    }
    private void HideTrayIcon()
    {
      if (TraySupported)
      {
        if (mTraySupport == TraySupport.AppIndicator)
        {
          try
          {
            appIcon.Status = AppIndicator.Status.Passive;
          }
          catch (Exception)
          {
            mTraySupport = TraySupport.Standard;
          }
        }
        if (mTraySupport == TraySupport.Standard)
          trayIcon.Visible = false;
      }
    }
    private void SetTrayIcon(string resource)
    {
      trayResource = resource;
      if (TrayState & TraySupported)
      {
        if (mTraySupport == TraySupport.AppIndicator)
        {
          try
          {
            appIcon.IconName = resource;
          }
          catch (Exception)
          {
            mTraySupport = TraySupport.Standard;
          }
        }
        if (mTraySupport == TraySupport.Standard)
          trayIcon.File = System.IO.Path.Combine(IconFolder, resource + ".png");
      }
    }
    private void SetTrayText(string tooltip)
    {
      if (TraySupported)
      {
        if (mTraySupport == TraySupport.AppIndicator)
        {
          try
          {
            appIcon.Title = tooltip;
          }
          catch (Exception)
          {
            mTraySupport = TraySupport.Standard;
          }
        }
        if (mTraySupport == TraySupport.Standard)
        {
          trayIcon.Tooltip = tooltip;
          trayIcon.Blinking = false;
        }
      }
    }
    #region "Graphs"
    private Gdk.Pixbuf CreateUsageTrayIcon(long lUsed, long lLim, bool bSlow, bool bFree)
    {
      if (trayRes < 8)
        trayRes = 8;
      Bitmap imgTray = new Bitmap(trayRes, trayRes);
      Graphics g = Graphics.FromImage(imgTray);
      g.Clear(Color.Transparent);
      if (bSlow)
      {
        string restricted = "Resources.tray_16.restricted.ico";
        if (trayRes > 16)
        {
          restricted = "Resources.tray_32.restricted.ico";
        }
        g.DrawIcon(new System.Drawing.Icon(GetType(), restricted), new Rectangle(0, 0, trayRes, trayRes));
        if (lUsed < lLim)
          modFunctions.CreateTrayIcon_Left(ref g, lUsed, lLim, mySettings.Colors.TrayDownA, mySettings.Colors.TrayDownB, mySettings.Colors.TrayDownC, trayRes);
        if (lUsed < lLim)
          modFunctions.CreateTrayIcon_Right(ref g, lUsed, lLim, mySettings.Colors.TrayDownA, mySettings.Colors.TrayDownB, mySettings.Colors.TrayDownC, trayRes);
      }
      else if (bFree)
      {
        string free = "Resources.tray_16.free.ico";
        if (trayRes > 16)
        {
          free = "Resources.tray_32.free.ico";
        }
        g.DrawIcon(new System.Drawing.Icon(GetType(), free), new Rectangle(0, 0, trayRes, trayRes));
      }
      else
      {
        string norm = "Resources.tray_16.norm.ico";
        if (trayRes > 16)
        {
          norm = "Resources.tray_32.norm.ico";
        }
        g.DrawIcon(new System.Drawing.Icon(GetType(), norm), new Rectangle(0, 0, trayRes, trayRes));
        modFunctions.CreateTrayIcon_Left(ref g, lUsed, lLim, mySettings.Colors.TrayDownA, mySettings.Colors.TrayDownB, mySettings.Colors.TrayDownC, trayRes);
        modFunctions.CreateTrayIcon_Right(ref g, lUsed, lLim, mySettings.Colors.TrayDownA, mySettings.Colors.TrayDownB, mySettings.Colors.TrayDownC, trayRes);
      }
      g.Dispose();
      return modFunctions.ImageToPixbuf(imgTray);
    }
    private void evnGraph_Click(object o, ButtonReleaseEventArgs e)
    {
      if (e.Event.Button == 3)
      {
        mnuGraph.Popup();
      }
    }
    #endregion
    #endregion
    #region "Update Events"
    private void CheckForUpdates()
    {
      if (MainClass.fAbout != null)
      {
        if (MainClass.fAbout.Visible)
        {
          return;
        }
      }
      SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Checking for Software Update...", false);
      NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      if (updateChecker != null)
      {
        updateCheckerEvent(false);
        updateChecker.Dispose();
        updateChecker = null;
      }
      updateChecker = new clsUpdate();
      updateCheckerEvent(true);
      updateChecker.CheckVersion();
    }
    private void updateChecker_CheckResult(object sender, clsUpdate.CheckEventArgs e)
    {
      EventArgs ea = (EventArgs)e;
      Gtk.Application.Invoke(sender, ea, Main_UpdateCheckerCheckResult);
    }
    private void Main_UpdateCheckerCheckResult(object sender, EventArgs ea)
    {
      clsUpdate.CheckEventArgs e = (clsUpdate.CheckEventArgs)ea;
      mySettings.LastUpdate = DateTime.Now;
      mySettings.Save();
      if (e.Error == null & !e.Cancelled)
      {
        if (mySettings.UpdateType == UpdateTypes.Ask)
        {
          dlgUpdate fUpdate;
          switch (e.Result)
          {
            case ResultType.NewUpdate:
              fUpdate = new dlgUpdate();
              fUpdate.NewUpdate(e.Version, false);
              switch ((Gtk.ResponseType)fUpdate.Run())
              {
                case ResponseType.Yes:
                  if (localData != null)
                  {
                    localDataEvent(false);
                    localData.Dispose();
                    localData = null;
                  }
                  updateChecker.DownloadUpdate(sUpdatePath);
                  break;
                case ResponseType.No:
                  if (updateChecker != null)
                  {
                    updateCheckerEvent(false);
                    updateChecker.Dispose();
                    updateChecker = null;
                  }
                  NextGrabTick = long.MinValue;
                  break;
                case ResponseType.Ok:
                  if (localData != null)
                  {
                    localDataEvent(false);
                    localData.Dispose();
                    localData = null;
                  }
                  updateChecker.DownloadUpdate(sUpdatePath);
                  mySettings.UpdateBETA = false;
                  mySettings.Save();
                  break;
                case ResponseType.Cancel:
                  mySettings.UpdateBETA = false;
                  mySettings.Save();
                  if (updateChecker != null)
                  {
                    updateCheckerEvent(false);
                    updateChecker.Dispose();
                    updateChecker = null;
                  }
                  NextGrabTick = long.MinValue;
                  break;
                default:
                  if (updateChecker != null)
                  {
                    updateCheckerEvent(false);
                    updateChecker.Dispose();
                    updateChecker = null;
                  }
                  NextGrabTick = long.MinValue;
                  break;
              }
              fUpdate.Destroy();
              fUpdate.Dispose();
              fUpdate = null;
              break;
            case ResultType.NewBeta:
              if (mySettings.UpdateBETA)
              {
                fUpdate = new dlgUpdate();
                fUpdate.NewUpdate(e.Version, true);
                switch ((Gtk.ResponseType)fUpdate.Run())
                {
                  case ResponseType.Yes:
                    if (localData != null)
                    {
                      localDataEvent(false);
                      localData.Dispose();
                      localData = null;
                    }
                    updateChecker.DownloadUpdate(sUpdatePath);
                    break;
                  case ResponseType.No:
                    if (updateChecker != null)
                    {
                      updateCheckerEvent(false);
                      updateChecker.Dispose();
                      updateChecker = null;
                    }
                    NextGrabTick = long.MinValue;
                    break;
                  case ResponseType.Ok:
                    if (localData != null)
                    {
                      localDataEvent(false);
                      localData.Dispose();
                      localData = null;
                    }
                    updateChecker.DownloadUpdate(sUpdatePath);
                    mySettings.UpdateBETA = false;
                    mySettings.Save();
                    break;
                  case ResponseType.Cancel:
                    mySettings.UpdateBETA = false;
                    mySettings.Save();
                    if (updateChecker != null)
                    {
                      updateCheckerEvent(false);
                      updateChecker.Dispose();
                      updateChecker = null;
                    }
                    NextGrabTick = long.MinValue;
                    break;
                  default:
                    if (updateChecker != null)
                    {
                      updateCheckerEvent(false);
                      updateChecker.Dispose();
                      updateChecker = null;
                    }
                    NextGrabTick = long.MinValue;
                    break;
                }
                fUpdate.Destroy();
                fUpdate.Dispose();
                fUpdate = null;
              }
              break;
            default:
              SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), string.Empty, false);
              if (updateChecker != null)
              {
                updateCheckerEvent(false);
                updateChecker.Dispose();
                updateChecker = null;
              }
              NextGrabTick = long.MinValue;
              break;
          }
        }
        else
        {
          switch (e.Result)
          {
            case ResultType.NewUpdate:
              if (localData != null)
              {
                localDataEvent(false);
                localData.Dispose();
                localData = null;
              }
              updateChecker.DownloadUpdate(sUpdatePath);
              break;
            case ResultType.NewBeta:
              if (mySettings.UpdateBETA)
              {
                if (localData != null)
                {
                  localDataEvent(false);
                  localData.Dispose();
                  localData = null;
                }
                updateChecker.DownloadUpdate(sUpdatePath);
              }
              break;
            default:
              SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), string.Empty, false);
              if (updateChecker != null)
              {
                updateCheckerEvent(false);
                updateChecker.Dispose();
                updateChecker = null;
              }
              NextGrabTick = long.MinValue;
              break;
          }
        }
      }
    }
    private void updateChecker_DownloadingUpdate(object sender, EventArgs e)
    {
      Gtk.Application.Invoke(sender, e, Main_UpdateCheckerDownloadingUpdate);
    }
    private void Main_UpdateCheckerDownloadingUpdate(object sender, EventArgs e)
    {
      tmrSpeed = GLib.Timeout.Add(1000, tmrSpeed_Tick);
      SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Downloading Software Update...", false);
    }
    private void updateChecker_DownloadResult(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      EventArgs ea = (EventArgs)e;
      Gtk.Application.Invoke(sender, ea, Main_UpdateCheckerDownloadResult);
    }
    private void Main_UpdateCheckerDownloadResult(object sender, EventArgs ea)
    {
      System.ComponentModel.AsyncCompletedEventArgs e = (System.ComponentModel.AsyncCompletedEventArgs)ea;
      if (tmrSpeed != 0)
      {
        GLib.Source.Remove(tmrSpeed);
        tmrSpeed = 0;
      }
      if (e.Error != null)
      {
        SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Software Update Error: " + e.Error.Message, true);
        NextGrabTick = long.MinValue;
      }
      else if (e.Cancelled)
      {
        if (updateChecker != null)
        {
          updateCheckerEvent(false);
          updateChecker.Dispose();
          updateChecker = null;
        }
        SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Software Update Cancelled!", true);
        NextGrabTick = long.MinValue;
      }
      else
      {
        if (updateChecker != null)
        {
          updateCheckerEvent(false);
          updateChecker.Dispose();
          updateChecker = null;
        }
        SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Software Update Download Complete", false);
        if (File.Exists(sUpdatePath))
        {
          if (CurrentOS.IsLinux)
            System.Diagnostics.Process.Start("chmod", "+x \"" + sUpdatePath + "\"");
          if (modFunctions.RunTerminal(sUpdatePath))
          {
            Gtk.Application.Quit();
          }
          else
          {
            System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(sUpdatePath));
            modFunctions.ShowMessageBox(this, "The Update failed to start.\n\nPlease run " + System.IO.Path.GetFileName(sUpdatePath) + " manually to update.", "Error Running Update", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
          }
        }
        else
        {
          SetStatusText(srlFunctions.TimeToString(modDB.LOG_GetLast()), "Software Update Failure!", true);
          NextGrabTick = long.MinValue;
        }
      }
    }
    private long upCurSize;
    private long upTotalSize;
    private ulong upDownSpeed;
    private uint upCurPercent;
    private void updateChecker_UpdateProgressChanged(object sender, clsUpdate.ProgressEventArgs e)
    {
      EventArgs ea = (EventArgs)e;
      Gtk.Application.Invoke(sender, ea, Main_UpdateCheckerUpdateProgressChanged);
    }
    private void Main_UpdateCheckerUpdateProgressChanged(object sender, EventArgs ea)
    {
      clsUpdate.ProgressEventArgs e = (clsUpdate.ProgressEventArgs)ea;
      upCurSize = e.BytesReceived;
      upTotalSize = e.TotalBytesToReceive;
      upCurPercent = (uint)e.ProgressPercentage;
    }
    long upLastSize;
    private bool tmrSpeed_Tick()
    {
      if (upCurSize > upLastSize)
        upDownSpeed = (ulong)(upCurSize - upLastSize);
      else
        upDownSpeed = 0;
      upLastSize = upCurSize;
      string sProgress = "(" + upCurPercent + "%)";
      string sStatus = modFunctions.ByteSize((ulong)upCurSize) + " of " + modFunctions.ByteSize((ulong)upTotalSize) + " at " + modFunctions.ByteSize(upDownSpeed) + "/s...";
      if (upTotalSize == 0)
      {
        sStatus = "Downloading Update (Waiting for Response)...";
        sProgress = "(Waiting for Response)";
      }
      SetStatusText("Downloading Update " + sProgress, sStatus, false);
      if (upTotalSize == 0)
        return true;
      return upCurSize < upTotalSize;
    }
    #endregion
    #region "Useful Functions"
    private long StrToVal(string str)
    {
      return StrToVal(str, 1);
    }
    private long StrToVal(string str, int vMult)
    {
      if (string.IsNullOrEmpty(str))
      {
        return (0);
      }
      if (!str.Contains(" "))
      {
        return (long.Parse(str.Replace(",", "")) * vMult);
      }
      return (long.Parse(str.Substring(0, str.IndexOf(' ')).Replace(",", "")) * vMult);
    }
    private void CleanupResult(ref string result)
    {
      result = result.Replace("&nbsp;", " ");
      result = result.Replace("\t", "");
      result = result.Replace("\n", "");
      result = result.Replace("\r", "");
      result = result.Trim();
    }
    private class SetStatusTextEventArgs: EventArgs
    {
      public string Status;
      public string Details;
      public bool Alert;
      public SetStatusTextEventArgs(string state, string details, bool alert)
      {
        Status = state;
        Details = details;
        Alert = alert;
      }
    }
    private void SetStatusText(string Status, string Details, bool Alert)
    {
      SetStatusTextEventArgs se = new SetStatusTextEventArgs(Status, Details, Alert);
      Gtk.Application.Invoke(null, se, Main_SetStatusText);
    }
    private void Main_SetStatusText(object o, EventArgs ea)
    {
      SetStatusTextEventArgs e = (SetStatusTextEventArgs)ea;
      if (e.Status == "1/1/1970 12:00 AM")
      {
        if (e.Alert | e.Details.StartsWith("Next update in "))
        {
          sDisp_LT = sDISPLAY_LT_NONE;
        }
        else
        {
          sDisp_LT = sDISPLAY_LT_BUSY;
        }
      }
      else
      {
        sDisp_LT = e.Status;
      }
      if (string.IsNullOrEmpty(e.Details))
      {
        bAlert = 2;
        sDisp_TT_E = null;
      }
      else if (e.Alert)
      {
        bAlert = 1;
        sDisp_TT_E = e.Details;
      }
      else
      {
        bAlert = 0;
        sDisp_TT_E = e.Details;
      }
      System.Threading.Thread.Sleep(0);
    }
    private bool tmrStatus_Tick()
    {
      long lNext = NextGrabTick;
      long lNow = srlFunctions.TickCount();
      if (lNext == long.MaxValue)
      {
        lblStatus.TooltipText = "Update Temporarily Paused - " + PauseActivity;
        ShowProgress(lblStatus.TooltipText, "", false);
      }
      else if (lNext == long.MinValue)
      {
        lblStatus.TooltipText = "Next Update is Being Calculated";
        ShowProgress(lblStatus.TooltipText, "", false);
      }
      else
      {
        sDisp = sDISPLAY.Replace("%lt", sDisp_LT);
        if (lNext - lNow >= 1000)
        {
          sDisp_TT_T = modFunctions.ConvertTime((ulong)(lNext - lNow), false, false);
          sDisp_TT_M = sDISPLAY_TT_NEXT.Replace("%t", sDisp_TT_T);
        }
        else if (lNow - lNext >= 1000)
        {
          sDisp_TT_T = modFunctions.ConvertTime((ulong)(lNow - lNext), false, false);
          sDisp_TT_M = sDISPLAY_TT_LATE.Replace("%t", sDisp_TT_T);
        }
        else
        {
          sDisp_TT_T = sDISPLAY_TT_T_SOON;
          sDisp_TT_M = sDISPLAY_TT_NEXT.Replace("%t", sDisp_TT_T);
        }
        if (bAlert == 1)
        {
          if (!string.IsNullOrEmpty(lblStatus.Text))
          {
            if (lblStatus.Text.EndsWith(" !"))
            {
              lblStatus.Text = sDisp + "  ";
            }
            else
            {
              lblStatus.Text = sDisp + " !";
            }
            SetFontSize(ref lblStatus, GetFontSize());
          }
          sDispTT = sDISPLAY_TT_ERR.Replace("%m", sDisp_TT_M).Replace("%e", sDisp_TT_E);
        }
        else if (bAlert == 0)
        {
          if (!string.IsNullOrEmpty(lblStatus.TooltipText))
          {
            if (lblStatus.TooltipText.EndsWith("..."))
            {
              if (lblStatus.Text.EndsWith("..."))
              {
                lblStatus.Text = sDisp;
              }
              else if (lblStatus.Text.EndsWith("."))
              {
                lblStatus.Text += ".";
              }
              else
              {
                lblStatus.Text = sDisp + ".";
              }
              SetFontSize(ref lblStatus, GetFontSize());
            }
          }
          sDispTT = sDisp_TT_E;
        }
        else
        {
          lblStatus.Text = sDisp;
          sDispTT = sDisp_TT_M;
          SetFontSize(ref lblStatus, GetFontSize());
        }
        lblStatus.TooltipText = sDispTT;
        ShowProgress(lblStatus.TooltipText, "", false);
      }
      return true;
    }
    protected void cmdNetTest_Clicked(object sender, EventArgs e)
    {
      modFunctions.OpenURL(mySettings.NetTestURL, ref taskNotifier);
    }
    #endregion
  }
}
