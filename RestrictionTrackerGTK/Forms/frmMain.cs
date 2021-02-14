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
    private localRestrictionTracker.SatHostTypes myPanel;
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
          _IconFolder = System.IO.Path.Combine(modFunctions.AppData, "trayIcons") + System.IO.Path.DirectorySeparatorChar.ToString();
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
    private void taskNotifierEvent(bool Add)
    {
      if (Add)
      {
        taskNotifier.ContentClick += taskNotifier_ContentClick;
        taskNotifier.CloseClick += taskNotifier_CloseClick;
      }
      else
      {
        taskNotifier.ContentClick -= taskNotifier_ContentClick;
        taskNotifier.CloseClick -= taskNotifier_CloseClick;
      }
    }
    private remoteRestrictionTracker remoteData;
    private void remoteDataEvent(bool Add)
    {
      if (Add)
      {
        remoteData.Failure += remoteData_Failure;
        remoteData.OKKey += remoteData_OKKey;
        remoteData.Success += remoteData_Success;
      }
      else
      {
        remoteData.Failure -= remoteData_Failure;
        remoteData.OKKey -= remoteData_OKKey;
        remoteData.Success -= remoteData_Success;
      }
    }
    private localRestrictionTracker localData;
    private void localDataEvent(bool Add)
    {
      if (Add)
      {
        localData.ConnectionStatus += localData_ConnectionStatus;
        localData.ConnectionFailure += localData_ConnectionFailure;
        localData.ConnectionDNXResult += localData_ConnectionDNXResult;
        localData.ConnectionWBXResult += localData_ConnectionWBXResult;
        localData.ConnectionRPXResult += localData_ConnectionRPXResult;
        localData.ConnectionRPLResult += localData_ConnectionRPLResult;
        localData.ConnectionWBLResult += localData_ConnectionWBLResult;
      }
      else
      {
        localData.ConnectionStatus -= localData_ConnectionStatus;
        localData.ConnectionFailure -= localData_ConnectionFailure;
        localData.ConnectionDNXResult -= localData_ConnectionDNXResult;
        localData.ConnectionWBXResult -= localData_ConnectionWBXResult;
        localData.ConnectionRPXResult -= localData_ConnectionRPXResult;
        localData.ConnectionRPLResult -= localData_ConnectionRPLResult;
        localData.ConnectionWBLResult -= localData_ConnectionWBLResult;
      }
    }
    private const string sWB = "https://myaccount.{0}/wbisp/{2}/{1}.jsp";
    private const string sRP = "https://{0}.ruralportal.net/us/{1}.do";
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
    private string sProvider;
    private bool imSlowed;
    private bool imFree = false;
    private bool FullCheck = true;
    private long NextGrabTick;
    private int GrabAttempt;
    private bool ClosingTime;
    private string sFailTray;
    private byte bAlert;
    private long typeA_down, typeA_up, typeA_dlim, typeA_ulim;
    private long typeB_used, typeB_lim;
    private bool updateFull;
    private long lastBalloon;
    private bool firstRestore;
    private bool checkedAJAX;
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
    #region "Server Type Determination"
    private class DetermineTypeOffline
    {
      public delegate void TypeDeterminedOfflineCallback(localRestrictionTracker.SatHostTypes HostType);
      private TypeDeterminedOfflineCallback c_callback;
      public DetermineTypeOffline(string Provider, TypeDeterminedOfflineCallback callback)
      {
        c_callback = callback;
        BeginTestInvoker beginInvoker = new BeginTestInvoker(BeginTest);
        beginInvoker.BeginInvoke(Provider, null, null);
      }
      private delegate void BeginTestInvoker(string Provider);
      private void BeginTest(string Provider)
      {
        if (Provider.ToLower() == "mydish.com" | Provider.ToLower() == "dish.com" | Provider.ToLower() == "dish.net")
        {
          if (c_callback == null)
          {
            System.Threading.Thread.Sleep(0);
            System.Threading.Thread.Sleep(100);
            System.Threading.Thread.Sleep(0);
          }
          if (c_callback != null)
          {
            c_callback.Invoke(localRestrictionTracker.SatHostTypes.Dish_EXEDE);
          }
        }
        else if (Provider.ToLower() == "exede.com" | Provider.ToLower() == "exede.net")
        {
          if (c_callback == null)
          {
            System.Threading.Thread.Sleep(0);
            System.Threading.Thread.Sleep(100);
            System.Threading.Thread.Sleep(0);
          }
          if (c_callback != null)
          {
            c_callback.Invoke(localRestrictionTracker.SatHostTypes.WildBlue_EXEDE);
          }
        }
        else if (Provider.ToLower() == "satelliteinternetco.com")
        {
          if (c_callback == null)
          {
            System.Threading.Thread.Sleep(0);
            System.Threading.Thread.Sleep(100);
            System.Threading.Thread.Sleep(0);
          }
          if (c_callback != null)
          {
            c_callback.Invoke(localRestrictionTracker.SatHostTypes.WildBlue_EXEDE_RESELLER);
          }
        }
        else
        {
          OfflineCheck();
        }
      }
      private void OfflineStats(ref float rpP, ref float exP, ref float wbP)
      {
        if (modDB.LOG_GetCount() > 0)
        {
          int TotalCount = 0;
          int RPGuess = 0;
          int ExGuess = 0;
          int WBGuess = 0;
          int logStep = 1;
          if (modDB.LOG_GetCount() > 50)
          {
            logStep = 10;
          }
          else if (modDB.LOG_GetCount() > 10)
          {
            logStep = 5;
          }
          else
          {
            logStep = 1;
          }
          for (int I = 0; I <= modDB.LOG_GetCount() - 1; I += logStep)
          {
            System.DateTime dtDate = default(System.DateTime);
            long lDown = 0;
            long lDLim = 0;
            long lUp = 0;
            long lULim = 0;
            modDB.LOG_Get(I, out dtDate, out lDown, out lDLim, out lUp, out lULim);
            if (lDLim == lULim)
            {
              if (lDown == lUp)
              {
                RPGuess += 1;
              }
              else
              {
                ExGuess += 1;
              }
            }
            else if (lULim == 0)
            {
              ExGuess += 1;
            }
            else
            {
              WBGuess += 1;
            }
            TotalCount += 1;
          }
          rpP = (float)RPGuess / TotalCount;
          exP = (float)ExGuess / TotalCount;
          wbP = (float)WBGuess / TotalCount;
        }
      }
      private void OfflineCheck()
      {
        float rpP = 0;
        float exP = 0;
        float wbP = 0;
        OfflineStats(ref rpP, ref exP, ref wbP);
        if (rpP == 0 & exP == 0 & wbP == 0)
        {
          if (c_callback != null)
          {
            c_callback.Invoke(localRestrictionTracker.SatHostTypes.Other);
          }
        }
        else
        {
          if (rpP > exP & rpP > wbP)
          {
            if (c_callback != null)
            {
              c_callback.Invoke(localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE);
            }
          }
          else if (exP > rpP & exP > wbP)
          {
            if (c_callback != null)
            {
              c_callback.Invoke(localRestrictionTracker.SatHostTypes.WildBlue_EXEDE);
            }
          }
          else if (wbP > rpP & wbP > exP)
          {
            if (c_callback != null)
            {
              c_callback.Invoke(localRestrictionTracker.SatHostTypes.WildBlue_LEGACY);
            }
          }
          else
          {
            if (rpP > wbP & exP > wbP & rpP == exP)
            {
              if (c_callback != null)
              {
                c_callback.Invoke(localRestrictionTracker.SatHostTypes.WildBlue_EXEDE);
              }
            }
            else
            {
              if (c_callback != null)
              {
                c_callback.Invoke(localRestrictionTracker.SatHostTypes.Other);
              }
              System.Diagnostics.Debugger.Break();
            }
          }
        }
      }
    }
    private class TypeDeterminedEventArgs
      : EventArgs
    {
      public DetermineType.SatHostGroup HostGroup;
      public TypeDeterminedEventArgs(DetermineType.SatHostGroup Type)
      {
        HostGroup = Type;
      }
    }
    private void TypeDetermination_TypeDetermined(DetermineType.SatHostGroup hostGroup)
    {
      Gtk.Application.Invoke(this, new TypeDeterminedEventArgs(hostGroup), Main_TypeDetermined);
    }
    private void Main_TypeDetermined(object sender, EventArgs ea)
    {
      TypeDeterminedEventArgs e = (TypeDeterminedEventArgs)ea;
      NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      if (e.HostGroup == DetermineType.SatHostGroup.Other)
      {
        if (tmrIcon != 0)
        {
          GLib.Source.Remove(tmrIcon);
          tmrIcon = 0;
        }
        DetermineTypeOffline TypeDeterminationOffline = new DetermineTypeOffline(sProvider, TypeDeterminationOffline_TypeDetermined);
        TypeDeterminationOffline.GetType();
      }
      else
      {
        if (e.HostGroup == DetermineType.SatHostGroup.Dish)
          mySettings.AccountType = localRestrictionTracker.SatHostTypes.Dish_EXEDE;
        else if (e.HostGroup == DetermineType.SatHostGroup.WildBlue)
          mySettings.AccountType = localRestrictionTracker.SatHostTypes.WildBlue_LEGACY;
        else if (e.HostGroup == DetermineType.SatHostGroup.RuralPortal)
          mySettings.AccountType = localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE;
        else if (e.HostGroup == DetermineType.SatHostGroup.Exede)
          mySettings.AccountType = localRestrictionTracker.SatHostTypes.WildBlue_EXEDE;
        else if (e.HostGroup == DetermineType.SatHostGroup.Exede)
          mySettings.AccountType = localRestrictionTracker.SatHostTypes.WildBlue_EXEDE_RESELLER;
        modFunctions.ScreenDefaultColors(ref mySettings.Colors, mySettings.AccountType);
        mySettings.Save();
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing Connection...", false);
        if (localData != null)
        {
          localDataEvent(false);
          localData.Dispose();
          localData = null;
        }
        GrabAttempt = 0;
        localData = new localRestrictionTracker(modFunctions.AppData);
        localDataEvent(true);
      }
    }
    private class TypeDeterminedOfflineEventArgs
      : EventArgs
    {
      public localRestrictionTracker.SatHostTypes HostType;
      public TypeDeterminedOfflineEventArgs(localRestrictionTracker.SatHostTypes Type)
      {
        HostType = Type;
      }
    }
    private void TypeDeterminationOffline_TypeDetermined(localRestrictionTracker.SatHostTypes hostType)
    {
      Gtk.Application.Invoke(this, new TypeDeterminedOfflineEventArgs(hostType), Main_TypeDeterminedOffline);
    }
    private void Main_TypeDeterminedOffline(object sender, EventArgs ea)
    {
      TypeDeterminedOfflineEventArgs e = (TypeDeterminedOfflineEventArgs)ea;
      NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      if (e.HostType == localRestrictionTracker.SatHostTypes.Other)
      {
        if (tmrIcon != 0)
        {
          GLib.Source.Remove(tmrIcon);
          tmrIcon = 0;
        }
        DisplayUsage(false, true);
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "Please connect to the Internet.", true);
      }
      else
      {
        mySettings.AccountType = e.HostType;
        modFunctions.ScreenDefaultColors(ref mySettings.Colors, mySettings.AccountType);
        mySettings.Save();
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing Connection...", false);
        if (localData != null)
        {
          localDataEvent(false);
          localData.Dispose();
          localData = null;
        }
        GrabAttempt = 0;
        localData = new localRestrictionTracker(modFunctions.AppData);
        localDataEvent(true);
      }
    }
    #endregion
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

      checkedAJAX = false;
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
      evnTypeADld.ButtonReleaseEvent += evnGraph_Click;
      evnTypeAUld.ButtonReleaseEvent += evnGraph_Click;
      evnTypeB.ButtonReleaseEvent += evnGraph_Click;

      pbMainStatus.Visible = false;
      lblMainStatus.Visible = false;

      DisplayResults(0, 0, 0, 0);

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
            appIcon = new AppIndicator.ApplicationIndicator("restriction-tracker", "norm", AppIndicator.Category.Communications, IconFolder);
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
        if (System.IO.Path.GetExtension(sFile).ToLower() == ".png")
          File.Delete(sFile);
      }
      string[][] icoNames = new string[][] { new string[] { "norm.ico", "norm.png" }, new string[] { "free.ico", "free.png" }, new string[] { "restricted.ico", "restricted.png" }, new string[] { "error.png", "error.png" }, new string[] { "throbsprite.0.ico", "throb_0.png" }, new string[] { "throbsprite.1.ico", "throb_1.png" }, new string[] { "throbsprite.2.ico", "throb_2.png" }, new string[] { "throbsprite.3.ico", "throb_3.png" }, new string[] { "throbsprite.4.ico", "throb_4.png" }, new string[] { "throbsprite.5.ico", "throb_5.png" }, new string[] { "throbsprite.6.ico", "throb_6.png" }, new string[] { "throbsprite.7.ico", "throb_7.png" }, new string[] { "throbsprite.8.ico", "throb_8.png" }, new string[] { "throbsprite.9.ico", "throb_9.png" }, new string[] { "throbsprite.10.ico", "throb_10.png" }, new string[] { "throbsprite.11.ico", "throb_11.png" } };
      foreach (string[] ico in icoNames)
      {
        if (System.IO.Path.GetExtension(ico[0]).ToLower() == ".ico")
          ResizeIcon(ico[0]).Save(System.IO.Path.Combine(IconFolder, ico[1]), System.Drawing.Imaging.ImageFormat.Png);
        else if (System.IO.Path.GetExtension(ico[0]).ToLower() == ".png")
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
      bool doBoth = true;
      bool doTypeA = false;
      List<string> icoNames = new List<string>();
      if (mySettings != null)
      {
        if (mySettings.AccountType != localRestrictionTracker.SatHostTypes.Other)
        {
          doBoth = false;
          if (mySettings.AccountType == localRestrictionTracker.SatHostTypes.WildBlue_LEGACY | mySettings.AccountType == localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY)
            doTypeA = true;
        }
      }
      string[] sFiles = Directory.GetFiles(IconFolder);
      foreach (string sFile in sFiles)
      {
        if ((System.IO.Path.GetExtension(sFile).ToLower() == ".png") && (System.IO.Path.GetFileName(sFile).ToLower().Substring(0, 6) == "graph_"))
          File.Delete(sFile);
      }
      if (doBoth)
      {
        for (long up = 0; up <= trayRes; up++)
        {
          for (long down = 0; down <= trayRes; down++)
          {
            using (Gdk.Pixbuf pIco = CreateTypeATrayIcon(down, trayRes, up, trayRes, false, false))
            {
              pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typea_" + down + "x" + up + ".png"), "png");
              icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typea_" + down + "x" + up + ".png"));
            }
          }
          using (Gdk.Pixbuf pIco = CreateTypeBTrayIcon(up, trayRes, false, false))
          {
            pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typeb_" + up + ".png"), "png");
            icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typeb_" + up + ".png"));
          }
        }
        using (Gdk.Pixbuf pIco = CreateTypeATrayIcon(0, trayRes, 0, trayRes, false, true))
        {
          pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typea_free.png"), "png");
          icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typea_free.png"));
        }
        using (Gdk.Pixbuf pIco = CreateTypeBTrayIcon(0, trayRes, false, true))
        {
          pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typeb_free.png"), "png");
          icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typeb_free.png"));
        }
        for (long down = 0; down <= trayRes; down++)
        {
          using (Gdk.Pixbuf pIco = CreateTypeATrayIcon(down, trayRes, 0, trayRes, true, false))
          {
            pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typea_" + down + "xslow.png"), "png");
            icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typea_" + down + "xslow.png"));
          }
        }
        for (long up = 0; up <= trayRes; up++)
        {
          using (Gdk.Pixbuf pIco = CreateTypeATrayIcon(0, trayRes, up, trayRes, true, false))
          {
            pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typea_slowx" + up + ".png"), "png");
            icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typea_slowx" + up + ".png"));
          }
        }
        using (Gdk.Pixbuf pIco = CreateTypeATrayIcon(0, trayRes, 0, trayRes, true, false))
        {
          pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typea_slowxslow.png"), "png");
          icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typea_slowxslow.png"));
        }
        using (Gdk.Pixbuf pIco = CreateTypeBTrayIcon(0, trayRes, true, false))
        {
          pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typeb_slow.png"), "png");
          icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typeb_slow.png"));
        }
      }
      else if (doTypeA)
      {
        for (long up = 0; up <= trayRes; up++)
        {
          for (long down = 0; down <= trayRes; down++)
          {
            using (Gdk.Pixbuf pIco = CreateTypeATrayIcon(down, trayRes, up, trayRes, false, false))
            {
              pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typea_" + down + "x" + up + ".png"), "png");
              icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typea_" + down + "x" + up + ".png"));
            }
          }
        }
        using (Gdk.Pixbuf pIco = CreateTypeATrayIcon(0, trayRes, 0, trayRes, false, true))
        {
          pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typea_free.png"), "png");
          icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typea_free.png"));
        }
        for (long down = 0; down <= trayRes; down++)
        {
          using (Gdk.Pixbuf pIco = CreateTypeATrayIcon(down, trayRes, 0, trayRes, true, false))
          {
            pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typea_" + down + "xslow.png"), "png");
            icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typea_" + down + "xslow.png"));
          }
        }
        for (long up = 0; up <= trayRes; up++)
        {
          using (Gdk.Pixbuf pIco = CreateTypeATrayIcon(0, trayRes, up, trayRes, true, false))
          {
            pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typea_slowx" + up + ".png"), "png");
            icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typea_slowx" + up + ".png"));
          }
        }
        using (Gdk.Pixbuf pIco = CreateTypeATrayIcon(0, trayRes, 0, trayRes, true, false))
        {
          pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typea_slowxslow.png"), "png");
          icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typea_slowxslow.png"));
        }
      }
      else
      {
        for (long up = 0; up <= trayRes; up++)
        {
          using (Gdk.Pixbuf pIco = CreateTypeBTrayIcon(up, trayRes, false, false))
          {
            pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typeb_" + up + ".png"), "png");
            icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typeb_" + up + ".png"));
          }
        }
        using (Gdk.Pixbuf pIco = CreateTypeBTrayIcon(0, trayRes, false, true))
        {
          pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typeb_free.png"), "png");
          icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typeb_free.png"));
        }
        using (Gdk.Pixbuf pIco = CreateTypeBTrayIcon(0, trayRes, true, false))
        {
          pIco.Save(System.IO.Path.Combine(IconFolder, "graph_typeb_slow.png"), "png");
          icoNames.Add(System.IO.Path.Combine(IconFolder, "graph_typeb_slow.png"));
        }
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
          MethodInvoker lookupInvoker = LookupProvider;
          lookupInvoker.BeginInvoke(null, lookupInvoker);
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
        if (myPanel == localRestrictionTracker.SatHostTypes.Other)
        {
          SetFontSize(ref lblRRS, GetFontSize());
          SetFontSize(ref lblNothing, (int)Math.Ceiling(GetFontSize() * 2.5f));
        }
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
        else if (myPanel != localRestrictionTracker.SatHostTypes.Other)
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
      if (myPanel == localRestrictionTracker.SatHostTypes.WildBlue_LEGACY | myPanel == localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY | myPanel == localRestrictionTracker.SatHostTypes.Dish_EXEDE)
      {
        if (lblTypeADldUsedVal.Text == " -- ")
        {
          pctTypeADld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(evnTypeADld.Allocation.Size, 0, 0, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          pctTypeAUld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(evnTypeAUld.Allocation.Size, 0, 0, mySettings.Accuracy, mySettings.Colors.MainUpA, mySettings.Colors.MainUpB, mySettings.Colors.MainUpC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          trayIcoVal = "norm";
        }
        else
        {
          pctTypeADld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(evnTypeADld.Allocation.Size, typeA_down, typeA_dlim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          pctTypeAUld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(evnTypeADld.Allocation.Size, typeA_up, typeA_ulim, mySettings.Accuracy, mySettings.Colors.MainUpA, mySettings.Colors.MainUpB, mySettings.Colors.MainUpC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          if (imFree)
          {
            trayIcoVal = "graph_typea_free";
          }
          else
          {
            int d = (int)Math.Round(((double)typeA_down / typeA_dlim) * trayRes);
            int u = (int)Math.Round(((double)typeA_up / typeA_ulim) * trayRes);
            if (imSlowed)
            {
              if (d == trayRes && u == trayRes)
              {
                trayIcoVal = "graph_typea_slowxslow";
              }
              else if (u == trayRes)
              {
                trayIcoVal = "graph_typea_" + d + "xslow";
              }
              else if (d == trayRes)
              {
                trayIcoVal = "graph_typea_slowx" + u;
              }
              else
              {
                trayIcoVal = "graph_typea_" + d + "x" + u;
              }
            }
            else
            {
              trayIcoVal = "graph_typea_" + d + "x" + u;
            }
          }
        }
      }
      else if (myPanel == localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE | myPanel == localRestrictionTracker.SatHostTypes.WildBlue_EXEDE | myPanel == localRestrictionTracker.SatHostTypes.WildBlue_EXEDE_RESELLER)
      {
        if (lblTypeBUsedVal.Text == " -- ")
        {
          pctTypeB.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayRProgress(pctTypeB.Allocation.Size, 0, 1, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          trayIcoVal = "norm";
        }
        else
        {
          pctTypeB.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayRProgress(pctTypeB.Allocation.Size, typeB_used, typeB_lim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          if (imFree)
          {
            trayIcoVal = "graph_typeb_free";
          }
          else if (imSlowed)
          {
            trayIcoVal = "graph_typeb_slow";
          }
          else
          {
            int u = (int)Math.Round(((double)typeB_used / typeB_lim) * trayRes);
            if (u > trayRes)
              u = trayRes;
            trayIcoVal = "graph_typeb_" + u;
          }
        }
      }
      else if (myPanel == localRestrictionTracker.SatHostTypes.Other)
      {
        lblNothing.Text = modFunctions.ProductName;
        modFunctions.PrepareLink(lblRRS);
        if (markupList.ContainsKey(lblRRS.Name + lblRRS.Handle.ToString("x")))
        {
          string markup = markupList[lblRRS.Name + lblRRS.Handle.ToString("x")];
          string markupColor = "";
          if (markup.Contains("foreground="))
          {
            markupColor = markup.Substring(markup.IndexOf("foreground="));
            markupColor = " " + markupColor.Substring(0, markupColor.IndexOf("\">") + 1);
          }
          markupList[lblRRS.Name + lblRRS.Handle.ToString("x")] = "<a href=\"http://realityripple.com/\"><span size=\"" + GetFontSize() + "\"" + markupColor + ">by " + modFunctions.CompanyName + "</span></a>";
          lblRRS.Markup = markupList[lblRRS.Name + lblRRS.Handle.ToString("x")];
        }
        else
        {
          markupList.Add(lblRRS.Name + lblRRS.Handle.ToString("x"), "<a href=\"http://realityripple.com/\">by " + modFunctions.CompanyName + "</a>");
          lblRRS.Markup = markupList[lblRRS.Name + lblRRS.Handle.ToString("x")];
        }
        lblRRS.TooltipText = "Visit RealityRipple.com.";
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
      if (remoteData != null)
      {
        remoteDataEvent(false);
        remoteData.Dispose();
        remoteData = null;
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
          if (string.IsNullOrEmpty(mySettings.RemoteKey))
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
            System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.None;
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
      modFunctions.ScreenDefaultColors(ref mySettings.Colors, mySettings.AccountType);
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
          clsFavicon wsFavicon = new clsFavicon(mySettings.NetTestURL, wsFavicon_DownloadIconCompleted, mySettings.NetTestURL);
          wsFavicon.GetType();
        }
        string sNetTestTitle = mySettings.NetTestURL;
        if (sNetTestTitle.Contains("://"))
          sNetTestTitle = sNetTestTitle.Substring(sNetTestTitle.IndexOf("://") + 3);
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
      if (remoteData != null)
      {
        remoteDataEvent(false);
        remoteData.Dispose();
        remoteData = null;
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
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing Connection...", false);
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
    private void LookupProvider()
    {
      SetTag(LoadStates.Lookup);
      SetStatusText("Loading History", "Reading usage history into memory...", false);
      modDB.LOG_Initialize(sAccount, false);
      if (ClosingTime)
      {
        return;
      }
      if (mySettings.AccountType == localRestrictionTracker.SatHostTypes.Other)
      {
        if (mySettings.AccountTypeForced)
        {
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Unknown Account Type.", true);
        }
        else
        {
          SetStatusText("Analyzing Account", "Determining your account type...", false);
          DetermineType TypeDetermination = new DetermineType(sProvider, mySettings.Timeout, mySettings.Proxy, TypeDetermination_TypeDetermined);
          TypeDetermination.GetType();
        }
      }
      else
      {
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
      if (!string.IsNullOrEmpty(sAccount))
      {
        if (sAccount.Contains("@") & sAccount.Contains("."))
        {
          sProvider = sAccount.Substring(sAccount.LastIndexOf("@") + 1).ToLower();
        }
        else
        {
          sAccount = "";
          sProvider = "";
        }
      }
      else
      {
        sAccount = "";
        sProvider = "";
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
              if (string.IsNullOrEmpty(sProvider))
              {
                sProvider = sAccount.Substring(sAccount.LastIndexOf("@") + 1).ToLower();
                SetStatusText("Reloading", "Reloading History...", false);
                modDB.LOG_Initialize(sAccount, false);
                if (ClosingTime)
                {
                  return true;
                }
              }
              if (Math.Abs(modFunctions.DateDiff(DateInterval.Minute, modDB.LOG_GetLast(), DateTime.Now)) > 10)
              {
                if (!string.IsNullOrEmpty(sProvider) & !string.IsNullOrEmpty(sPassword))
                {
                  updateFull = false;
                  NextGrabTick = long.MaxValue;
                  PauseActivity = "Preparing Connection";
                  EnableProgressIcon();
                  if (!checkedAJAX && mySettings.AccountType == localRestrictionTracker.SatHostTypes.WildBlue_EXEDE_RESELLER)
                  {
                    SetStatusText(modDB.LOG_GetLast().ToString("g"), "Checking for AJAX List Update...", false);
                    UpdateAJAXLists AJAXUpdate = new UpdateAJAXLists(sProvider, mySettings.Timeout, mySettings.Proxy, (object)"GetUsage", UpdateAJAXLists_UpdateChecked);
                    AJAXUpdate.GetType();
                  }
                  else
                  {
                    SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing Connection...", false);
                    MethodInvoker UsageInvoker = GetUsage;
                    UsageInvoker.BeginInvoke(null, null);
                  }
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
      if (string.IsNullOrEmpty(sAccount) | string.IsNullOrEmpty(sPassword) | !sAccount.Contains("@"))
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
          if (KeyCheck(mySettings.RemoteKey))
          {
            MethodInvoker remoteInvoker = GetRemoteUsage;
            remoteInvoker.BeginInvoke(null, null);
          }
          else
          {
            if (localData != null)
            {
              localDataEvent(false);
              localData.Dispose();
              localData = null;
            }
            GrabAttempt = 0;
            localData = new localRestrictionTracker(modFunctions.AppData);
            localDataEvent(true);
          }
        }
        else
        {
          if (remoteData != null)
          {
            remoteDataEvent(false);
            remoteData.Dispose();
            remoteData = null;
          }
          if (localData != null)
          {
            localDataEvent(false);
            localData.Dispose();
            localData = null;
          }
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "", false);
          DisplayUsage(false, false);
          NextGrabTick = srlFunctions.TickCount() + 5000;
        }
      }
    }
    private bool KeyCheck(string TestKey)
    {
      if (string.IsNullOrEmpty(TestKey.Trim()))
      {
        return false;
      }
      if (TestKey.Contains("-"))
      {
        string[] sKeys = TestKey.Split('-');
        if (sKeys.Length == 5)
        {
          if (sKeys[0].Trim().Length == 6 & sKeys[1].Trim().Length == 4 & sKeys[2].Trim().Length == 4 & sKeys[3].Trim().Length == 4 & sKeys[4].Trim().Length == 6)
          {
            return true;
          }
        }
      }
      return false;
    }
    private void GetRemoteUsage()
    {
      DateTime syncTime = mySettings.LastSyncTime;
      if (updateFull)
      {
        syncTime = new DateTime(2001, 1, 1);
      }
      remoteData = new remoteRestrictionTracker(sAccount, sPassword, mySettings.RemoteKey, mySettings.Proxy, mySettings.Timeout, syncTime, modFunctions.AppData);
      remoteDataEvent(true);
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
        long lDown;
        long lDLim;
        long lUp;
        long lULim;
        modDB.LOG_Get(modDB.LOG_GetCount() - 1, out dtDate, out lDown, out lDLim, out lUp, out lULim);
        if (e.StatusText)
        {
          SetStatusText(dtDate.ToString("g"), "", false);
        }
        DisplayResults(lDown, lDLim, lUp, lULim);
      }
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
      if (remoteData != null)
      {
        remoteDataEvent(false);
        remoteData.Dispose();
        remoteData = null;
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
    private void localData_ConnectionStatus(object sender, localRestrictionTracker.ConnectionStatusEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_LocalDataConnectionStatus);
    }
    private void Main_LocalDataConnectionStatus(object o, EventArgs ea)
    {
      localRestrictionTracker.ConnectionStatusEventArgs e = (localRestrictionTracker.ConnectionStatusEventArgs)ea;
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
        case localRestrictionTracker.ConnectionStates.Initialize:
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Initializing Connection" + sAppend + "...", false);
          break;
        case localRestrictionTracker.ConnectionStates.Prepare:
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing to Log In" + sAppend + "...", false);
          break;
        case localRestrictionTracker.ConnectionStates.Login:
          switch (e.SubState)
          {
            case localRestrictionTracker.ConnectionSubStates.ReadLogin:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Reading Login Page" + sAppend + "...", false);
              break;
            case localRestrictionTracker.ConnectionSubStates.Authenticate:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Authenticating" + sAppend + "...", false);
              break;
            case localRestrictionTracker.ConnectionSubStates.Verify:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Verifying Authentication" + sAppend + "...", false);
              break;
            default:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Logging In" + sAppend + "...", false);
              break;
          }
          break;
        case localRestrictionTracker.ConnectionStates.TableDownload:
          switch (e.SubState)
          {
            case localRestrictionTracker.ConnectionSubStates.LoadHome:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Downloading Home Page" + sAppend + "...", false);
              break;
            case localRestrictionTracker.ConnectionSubStates.LoadAJAX:
              if (e.Attempt == 0)
                SetStatusText(modDB.LOG_GetLast().ToString("g"), "Downloading AJAX Data (" + e.Stage + " of " + localData.ExedeResellerAJAXFirstTryRequests + ")...", false);
              else
                SetStatusText(modDB.LOG_GetLast().ToString("g"), "Downloading AJAX Data (" + e.Stage + " of " + localData.ExedeResellerAJAXSecondTryRequests + ")...", false);
              break;
            case localRestrictionTracker.ConnectionSubStates.LoadTable:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Downloading Usage Table" + sAppend + "...", false);
              break;
            default:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Downloading Usage Table" + sAppend + "...", false);
              break;
          }
          break;
        case localRestrictionTracker.ConnectionStates.TableRead:
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Reading Usage Table" + sAppend + "...", false);
          break;
      }
    }
    private void localData_ConnectionFailure(object sender, localRestrictionTracker.ConnectionFailureEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_LocalDataConnectionFailure);
    }
    private void Main_LocalDataConnectionFailure(object o, EventArgs ea)
    {
      localRestrictionTracker.ConnectionFailureEventArgs e = (localRestrictionTracker.ConnectionFailureEventArgs)ea;
      switch (e.Type)
      {
        case localRestrictionTracker.ConnectionFailureEventArgs.FailureType.LoginIssue:
          GrabAttempt = 0;
          SetStatusText(modDB.LOG_GetLast().ToString("g"), e.Message, true);
          if (!string.IsNullOrEmpty(e.Fail))
          {
            FailFile(e.Fail, true);
          }
          DisplayUsage(false, false);
          return;
        case localRestrictionTracker.ConnectionFailureEventArgs.FailureType.ConnectionTimeout:
          if (GrabAttempt < mySettings.Retries)
          {
            GrabAttempt++;
            string sMessage = "Connection Timed Out! Retry " + GrabAttempt + " of " + mySettings.Retries + "...";
            SetStatusText(modDB.LOG_GetLast().ToString("g"), sMessage, true);
            if (localData != null)
            {
              localDataEvent(false);
              localData.Dispose();
              localData = null;
            }
            localData = new localRestrictionTracker(modFunctions.AppData);
            localDataEvent(true);
            return;
          }
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Connection Timed Out!", true);
          DisplayUsage(false, false);
          break;
        case localRestrictionTracker.ConnectionFailureEventArgs.FailureType.TLSTooOld:
          GrabAttempt = 0;
          if (mySettings.TLSProxy)
          {
            SetStatusText(modDB.LOG_GetLast().ToString("g"), "Please enable TLS 1.1 or 1.2 under Security Protocol in the Network tab of the Config window to connect.", true);
            DisplayUsage(false, false);
          }
          else
          {
            string clrVer = RestrictionLibrary.srlFunctions.GetCLRCleanVersion();
            Version clr = new Version(clrVer.Substring(5));
            if (clr.Major < 4 || (clr.Major == 4 & clr.Minor < 8))
            {
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Security Protocol requires MONO 4.8 or newer. If you can't install MONO 4.8, please use the TLS Proxy feature for now.", true);
              DisplayUsage(false, false);
            }
            else
            {
              if (e.Message == "VER")
              {
                SetStatusText(modDB.LOG_GetLast().ToString("g"), "Please enable TLS 1.1 or 1.2 under Security Protocol in the Network tab of the Config window to connect.", true);
                DisplayUsage(false, false);
              }
              else if (e.Message == "PROXY")
              {
                SetStatusText(modDB.LOG_GetLast().ToString("g"), "Even though TLS 1.1 or 1.2 was enabled, the server still didn't like the request. Please let me know you got this message. You can use the TLS Proxy feature under Security Protocol in the Network tab of the Config window to bypass this problem for now.", true);
                DisplayUsage(false, false);
              }
            }
          }
          break;
        case localRestrictionTracker.ConnectionFailureEventArgs.FailureType.LoginFailure:
          if (!string.IsNullOrEmpty(e.Fail))
          {
            FailFile(e.Fail);
          }
          if (e.Message.EndsWith("Please try again.") && GrabAttempt < mySettings.Retries)
          {
            GrabAttempt++;
            string sMessage = e.Message.Substring(0, e.Message.IndexOf("Please try again.")) + "Retry " + GrabAttempt + " of " + mySettings.Retries + "...";
            SetStatusText(modDB.LOG_GetLast().ToString("g"), sMessage, true);
            if (localData != null)
            {
              localDataEvent(false);
              localData.Dispose();
              localData = null;
            }
            localData = new localRestrictionTracker(modFunctions.AppData);
            localDataEvent(true);
            return;
          }
          if ((e.Message == "AJAX failed to yield data table." || e.Message == "Can't determine AJAX order.") && GrabAttempt < 1)
          {
            GrabAttempt++;
            if (localData != null)
            {
              localDataEvent(false);
              localData.Dispose();
              localData = null;
            }
            string sMessage = e.Message + " Attempting to Update AJAX Lists...";
            SetStatusText(modDB.LOG_GetLast().ToString("g"), sMessage, false);
            UpdateAJAXLists AJAXUpdate = new UpdateAJAXLists(sProvider, mySettings.Timeout, mySettings.Proxy, GrabAttempt, UpdateAJAXLists_ListUpdated);
            AJAXUpdate.GetType();
            return;
          }
          SetStatusText(modDB.LOG_GetLast().ToString("g"), e.Message, true);
          DisplayUsage(false, true);
          break;
        case localRestrictionTracker.ConnectionFailureEventArgs.FailureType.FatalLoginFailure:
          GrabAttempt = 0;
          mySettings.AccountType = localRestrictionTracker.SatHostTypes.Other;
          SetStatusText(modDB.LOG_GetLast().ToString("g"), e.Message, true);
          if (!string.IsNullOrEmpty(e.Fail))
          {
            FailFile(e.Fail);
          }
          DisplayUsage(false, false);
          break;
        case localRestrictionTracker.ConnectionFailureEventArgs.FailureType.UnknownAccountDetails:
          GrabAttempt = 0;
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Please enter your account details in the Config window.", true);
          DisplayUsage(false, false);
          if ((this.GdkWindow.State & Gdk.WindowState.Iconified) == Gdk.WindowState.Iconified)
          {
            ShowFromTray();
          }
          cmdConfig.GrabFocus();
          modFunctions.ShowMessageBox(null, "You haven't entered your account details.\nPlease enter your account details in the Config window by clicking Configuration.", "Account Details Required", Gtk.DialogFlags.Modal, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok);
          break;
        case localRestrictionTracker.ConnectionFailureEventArgs.FailureType.UnknownAccountType:
          GrabAttempt = 0;
          if (mySettings.AccountTypeForced)
          {
            SetStatusText(modDB.LOG_GetLast().ToString("g"), "Unknown Account Type.", true);
          }
          else
          {
            SetStatusText("Analyzing Account", "Determining your account type...", false);
            DetermineType TypeDetermination = new DetermineType(sProvider, mySettings.Timeout, mySettings.Proxy, TypeDetermination_TypeDetermined);
            TypeDetermination.GetType();
          }
          break;
      }
      if (localData != null)
      {
        localDataEvent(false);
        localData.Dispose();
        localData = null;
      }
    }
    private void localData_ConnectionDNXResult(object sender, localRestrictionTracker.TYPEA2ResultEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_LocalDataConnectionDNXResult);
    }
    private void Main_LocalDataConnectionDNXResult(object o, EventArgs ea)
    {
      localRestrictionTracker.TYPEA2ResultEventArgs e = (localRestrictionTracker.TYPEA2ResultEventArgs)ea;
      GrabAttempt = 0;
      SetStatusText(e.Update.ToString("g"), "Saving History...", false);
      NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      modDB.LOG_Add(e.Update, e.AnyTime, e.AnyTimeLimit, e.OffPeak, e.OffPeakLimit, true);
      myPanel = localRestrictionTracker.SatHostTypes.Dish_EXEDE;
      mySettings.AccountType = localRestrictionTracker.SatHostTypes.Dish_EXEDE;
      mySettings.Save();
      modFunctions.ScreenDefaultColors(ref mySettings.Colors, mySettings.AccountType);
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
      SaveToHostList();
    }
    private void localData_ConnectionRPXResult(object sender, localRestrictionTracker.TYPEBResultEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_LocalDataConnectionRPXResult);
    }
    private void Main_LocalDataConnectionRPXResult(object o, EventArgs ea)
    {
      localRestrictionTracker.TYPEBResultEventArgs e = (localRestrictionTracker.TYPEBResultEventArgs)ea;
      GrabAttempt = 0;
      SetStatusText(e.Update.ToString("g"), "Saving History...", false);
      NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      modDB.LOG_Add(e.Update, e.Used, e.Limit, e.Used, e.Limit, true);
      myPanel = localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE;
      mySettings.AccountType = localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE;
      mySettings.Save();
      modFunctions.ScreenDefaultColors(ref mySettings.Colors, mySettings.AccountType);
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
      SaveToHostList();
    }
    private void localData_ConnectionRPLResult(object sender, localRestrictionTracker.TYPEAResultEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_LocalDataConnectionRPLResult);
    }
    private void Main_LocalDataConnectionRPLResult(object o, EventArgs ea)
    {
      localRestrictionTracker.TYPEAResultEventArgs e = (localRestrictionTracker.TYPEAResultEventArgs)ea;
      GrabAttempt = 0;
      SetStatusText(e.Update.ToString("g"), "Saving History...", false);
      NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      modDB.LOG_Add(e.Update, e.Download, e.DownloadLimit, e.Upload, e.UploadLimit, true);
      myPanel = localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY;
      mySettings.AccountType = localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY;
      mySettings.Save();
      modFunctions.ScreenDefaultColors(ref mySettings.Colors, mySettings.AccountType);
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
      SaveToHostList();
    }
    private void localData_ConnectionWBLResult(object sender, localRestrictionTracker.TYPEAResultEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_LocalDataConnectionWBLResult);
    }
    private void Main_LocalDataConnectionWBLResult(object o, EventArgs ea)
    {
      localRestrictionTracker.TYPEAResultEventArgs e = (localRestrictionTracker.TYPEAResultEventArgs)ea;
      GrabAttempt = 0;
      SetStatusText(e.Update.ToString("g"), "Saving History...", false);
      NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      modDB.LOG_Add(e.Update, e.Download, e.DownloadLimit, e.Upload, e.UploadLimit, true);
      myPanel = localRestrictionTracker.SatHostTypes.WildBlue_LEGACY;
      mySettings.AccountType = localRestrictionTracker.SatHostTypes.WildBlue_LEGACY;
      mySettings.Save();
      modFunctions.ScreenDefaultColors(ref mySettings.Colors, mySettings.AccountType);
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
      SaveToHostList();
    }
    private void localData_ConnectionWBXResult(object sender, localRestrictionTracker.TYPEBResultEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_LocalDataConnectionWBXResult);
    }
    private void Main_LocalDataConnectionWBXResult(object o, EventArgs ea)
    {
      localRestrictionTracker.TYPEBResultEventArgs e = (localRestrictionTracker.TYPEBResultEventArgs)ea;
      GrabAttempt = 0;
      SetStatusText(e.Update.ToString("g"), "Saving History...", false);
      NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      modDB.LOG_Add(e.Update, e.Used, e.Limit, e.Used, e.Limit, true);
      myPanel = localRestrictionTracker.SatHostTypes.WildBlue_EXEDE;
      mySettings.AccountType = localRestrictionTracker.SatHostTypes.WildBlue_EXEDE;
      mySettings.Save();
      modFunctions.ScreenDefaultColors(ref mySettings.Colors, mySettings.AccountType);
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
    private void localData_ConnectionWXRResult(object sender, localRestrictionTracker.TYPEBResultEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_LocalDataConnectionWXRResult);
    }
    private void Main_LocalDataConnectionWXRResult(object o, EventArgs ea)
    {
      localRestrictionTracker.TYPEBResultEventArgs e = (localRestrictionTracker.TYPEBResultEventArgs)ea;
      GrabAttempt = 0;
      SetStatusText(e.Update.ToString("g"), "Saving History...", false);
      NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      modDB.LOG_Add(e.Update, e.Used, e.Limit, e.Used, e.Limit, true);
      myPanel = localRestrictionTracker.SatHostTypes.WildBlue_EXEDE_RESELLER;
      mySettings.AccountType = localRestrictionTracker.SatHostTypes.WildBlue_EXEDE_RESELLER;
      mySettings.Save();
      modFunctions.ScreenDefaultColors(ref mySettings.Colors, mySettings.AccountType);
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
    #region "HostList"
    private bool didHostListSave = false;
    private void SaveToHostList()
    {
      Gtk.Application.Invoke(new object(), new EventArgs(), Main_SaveToHostList);
    }
    private void Main_SaveToHostList(object o, EventArgs ea)
    {
      if (didHostListSave)
        return;
      try
      {
        string myProvider = mySettings.Account.Substring(mySettings.Account.LastIndexOf("@") + 1).ToLower();
        System.Net.WebRequest sckHostList = System.Net.HttpWebRequest.Create("http://wb.realityripple.com/hosts/?add=" + myProvider);
        sckHostList.BeginGetResponse(null, null);
        didHostListSave = true;
      }
      catch (Exception)
      {
        didHostListSave = false;
      }
    }
    #endregion
    #endregion
    #region "Remote Usage Events"
    private void remoteData_Failure(object sender, remoteRestrictionTracker.FailureEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_RemoteDataFailure);
    }
    private void Main_RemoteDataFailure(object o, EventArgs ea)
    {
      remoteRestrictionTracker.FailureEventArgs e = (remoteRestrictionTracker.FailureEventArgs)ea;
      string sErr = "There was an error verifying your Product Key.";
      switch (e.Type)
      {
        case remoteRestrictionTracker.FailureEventArgs.FailType.BadLogin:
          sErr = "There was a server error. Please try again later.";
          break;
        case remoteRestrictionTracker.FailureEventArgs.FailType.BadPassword:
          sErr = "Your Password is incorrect.";
          break;
        case remoteRestrictionTracker.FailureEventArgs.FailType.BadProduct:
          sErr = "Your Product Key has been disabled.";
          mySettings.RemoteKey = string.Empty;
          MethodInvoker UsageInvoker = GetUsage;
          UsageInvoker.BeginInvoke(null, null);
          break;
        case remoteRestrictionTracker.FailureEventArgs.FailType.BadServer:
          sErr = "There was a fault double-checking the server. You may have a security issue.";
          break;
        case remoteRestrictionTracker.FailureEventArgs.FailType.NoData:
          sErr = "There is no usage data." + (string.IsNullOrEmpty(e.Details) ? "Please wait 15 minutes." : " " + e.Details);
          break;
        case remoteRestrictionTracker.FailureEventArgs.FailType.NoPassword:
          sErr = "Your Password has not been Registered on the Remote Service.";
          break;
        case remoteRestrictionTracker.FailureEventArgs.FailType.NoUsername:
          sErr = "Your Account is not Registered for the Remote Service.";
          mySettings.RemoteKey = string.Empty;
          MethodInvoker GetUsageInvoker = GetUsage;
          GetUsageInvoker.BeginInvoke(null, null);
          break;
        case remoteRestrictionTracker.FailureEventArgs.FailType.Network:
          sErr = "Network Connection Error" + (string.IsNullOrEmpty(e.Details) ? "." : ": " + e.Details);
          break;
        case remoteRestrictionTracker.FailureEventArgs.FailType.NotBase64:
          sErr = "The server did not respond in the right manner. Please check your Internet connection" + (string.IsNullOrEmpty(e.Details) ? "." : ": " + e.Details);
          break;
      }
      if (remoteData != null)
      {
        remoteDataEvent(false);
        remoteData.Dispose();
        remoteData = null;
      }
      DisplayUsage(false, true);
      SetStatusText(modDB.LOG_GetLast().ToString("g"), "Service Failure: " + sErr, true);
    }
    private void remoteData_OKKey(object sender, System.EventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_RemoteDataOKKey);
    }
    private void Main_RemoteDataOKKey(object o, EventArgs e)
    {
      NextGrabTick = srlFunctions.TickCount() + (mySettings.Timeout * 1000);
      SetStatusText(modDB.LOG_GetLast().ToString("g"), "Account Accessed! Getting Usage...", false);
    }
    private void remoteData_Success(object sender, remoteRestrictionTracker.SuccessEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_RemoteDataSuccess);
    }
    private void Main_RemoteDataSuccess(object o, EventArgs ea)
    {
      remoteRestrictionTracker.SuccessEventArgs e = (remoteRestrictionTracker.SuccessEventArgs)ea;
      string LastTime = modDB.LOG_GetLast().ToString("g");
      NextGrabTick = srlFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      if (FullCheck)
      {
        SetStatusText(LastTime, "Synchronizing History...", false);
      }
      else
      {
        SetStatusText(LastTime, "Saving History...", false);
      }
      if (e != null)
      {
        mySettings.AccountType = (localRestrictionTracker.SatHostTypes)e.Provider;
        modFunctions.ScreenDefaultColors(ref mySettings.Colors, mySettings.AccountType);
        mySettings.Save();
        int iPercent = 0;
        int iInterval = 1;
        long iStart = srlFunctions.TickCount();
        for (int I = 0; I <= e.Results.Length - 1; I++)
        {
          remoteRestrictionTracker.SuccessEventArgs.Result Row = e.Results[I];
          if (FullCheck)
          {
            if (Math.Abs(iPercent - Math.Floor(((double)I / (e.Results.Length - 1)) * 100d)) >= iInterval)
            {
              iPercent = (int)Math.Floor(((double)I / (e.Results.Length - 1)) * 100d);
              Gtk.Main.Iteration();
              SetStatusText(LastTime, "Synchronizing History [" + iPercent + "%]...", false);
              Gtk.Main.Iteration();
              Gtk.Main.IterationDo(false);
              System.Threading.Thread.Sleep(0);
              Gtk.Main.Iteration();
              if ((iPercent == 4))
              {
                long iDur = srlFunctions.TickCount() - iStart;
                if (iDur <= 700)
                {
                  iInterval = 2;
                }
              }
            }
            modDB.LOG_Add(Row.Time, Row.Down, Row.DownMax, Row.Up, Row.UpMax, (I == e.Results.Length - 1));
          }
          else
          {
            if (modFunctions.DateDiff(DateInterval.Minute, modDB.LOG_GetLast(), Row.Time) > 1)
            {
              modDB.LOG_Add(Row.Time, Row.Down, Row.DownMax, Row.Up, Row.UpMax, (I == e.Results.Length - 1));
            }
          }
        }
        FullCheck = false;
        mySettings.LastSyncTime = modDB.LOG_GetLast();
        mySettings.Save();
        DisplayUsage(true, true);
      }
      else
      {
        if (modDB.LOG_GetCount() == 0)
          SetStatusText("No History", "No data received from the server!", true);
        DisplayUsage(true, true);
      }
      if (remoteData != null)
      {
        remoteDataEvent(false);
        remoteData.Dispose();
        remoteData = null;
      }
      SaveToHostList();
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
      switch (state)
      {
        case "TYPEA":
          long wDown = typeA_down;
          long wDLim = typeA_dlim;
          long wUp = typeA_up;
          long wULim = typeA_ulim;
          long wDFree = typeA_dlim - typeA_down;
          long wUFree = typeA_ulim - typeA_up;
          if (wDown > 0 | wDFree != 0 | wDLim > 0 | wUp > 0 | wUFree != 0 | wULim > 0)
          {
            DoChange(ref lblTypeADldUsedVal, ref wDown, false);
            DoChange(ref lblTypeADldFreeVal, ref wDFree, (wDFree <= 0));
            DoChange(ref lblTypeADldLimitVal, ref wDLim, imSlowed);
            DoChange(ref lblTypeAUldUsedVal, ref wUp, false);
            DoChange(ref lblTypeAUldFreeVal, ref wUFree, (wUFree <= 0));
            DoChange(ref lblTypeAUldLimitVal, ref wULim, imSlowed);
          }
          ResizePanels();
          if (wDown == 0 & wDFree == 0 & wDLim == 0 & wUp == 0 & wUFree == 0 & wULim == 0)
          {
            this.SizeAllocate(new Gdk.Rectangle(Gdk.Point.Zero, mySettings.MainSize));
            return;
          }
          break;
        case "TYPEB":
          long lUsed = typeB_used;
          long lLim = typeB_lim;
          long lFree = lLim - lUsed;
          if (lUsed != 0 | lLim > 0 | lFree != 0)
          {
            DoChange(ref lblTypeBUsedVal, ref lUsed, false);
            DoChange(ref lblTypeBFreeVal, ref lFree, (lFree <= 0));
            DoChange(ref lblTypeBLimitVal, ref lLim, imSlowed);
          }
          ResizePanels();
          if (lUsed == 0 & lLim == 0 & lFree == 0)
          {
            return;
          }
          break;
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
          tmpVal = long.Parse(tmpS);
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
          tmpStr = tmpVal.ToString("N0").Trim() + " MB";
        }
        else if (tmpVal < toVal)
        {
          tmpVal += majorDif;
          tmpStr = tmpVal.ToString("N0").Trim() + " MB";
        }
        else
        {
          tmpStr = toVal.ToString("N0").Trim() + " MB";
          toVal = 0;
        }
      }
      else
      {
        tmpStr = toVal.ToString("N0").Trim() + " MB";
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
    private void DisplayTypeAResults(long lDown, long lDownLim, long lUp, long lUpLim, string sLastUpdate)
    {
      string sTTT = this.Title;
      bool overDown, overUp;
      overDown = (lDown >= lDownLim);
      overUp = (lUp >= lUpLim);
      if (overDown | overUp)
      {
        imSlowed = true;
      }
      else if ((lDown < (lDownLim * 0.7)) & (lUp < (lUpLim * 0.7)))
      {
        imSlowed = false;
      }
      if (!pnlTypeA.Visible)
      {
        pnlTypeA.Visible = true;
        pnlDisplays.Add(pnlTypeA);
      }
      if (pnlTypeB.Visible)
      {
        pnlTypeB.Visible = false;
        pnlDisplays.Remove(pnlTypeB);
      }
      if (pnlNothing.Visible)
      {
        pnlNothing.Visible = false;
        pnlDisplays.Remove(pnlNothing);
      }
      typeA_down = lDown;
      typeA_dlim = lDownLim;
      typeA_up = lUp;
      typeA_ulim = lUpLim;
      if (tmrChanges != null)
      {
        tmrChanges.Dispose();
        tmrChanges = null;
      }
      tmrChanges = new System.Threading.Timer(DisplayChangeInterval, (object)"TYPEA", 75, System.Threading.Timeout.Infinite);
      if (overDown)
        lblTypeADldFreeVal.TooltipText = "You are over your Download limit!";
      else
        lblTypeADldFreeVal.TooltipText = null;
      if (overUp)
        lblTypeAUldFreeVal.TooltipText = "You are over your Upload limit!";
      else
        lblTypeAUldFreeVal.TooltipText = null;
      if (imSlowed)
      {
        lblTypeADldLimitVal.TooltipText = "Your connection has been restricted!";
        lblTypeAUldLimitVal.TooltipText = "Your connection has been restricted!";
      }
      else
      {
        lblTypeADldLimitVal.TooltipText = null;
        lblTypeAUldLimitVal.TooltipText = null;
      }
      pctTypeADld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(pctTypeADld.Allocation.Size, lDown, lDownLim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
      pctTypeAUld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(pctTypeAUld.Allocation.Size, lUp, lUpLim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
      lblTypeADld.Text = "Download (" + AccuratePercent((double)lDown / lDownLim) + ")";
      lblTypeAUld.Text = "Upload (" + AccuratePercent((double)lUp / lUpLim) + ")";
      SetFontSize(ref lblTypeADld, GetFontSize());
      SetFontSize(ref lblTypeAUld, GetFontSize());
      pctTypeADld.TooltipText = "Graph representing your download usage.";
      pctTypeAUld.TooltipText = "Graph representing your upload usage.";
      string dFree, uFree;
      if (lDownLim > lDown)
      {
        dFree = ", " + MBorGB(lDownLim - lDown) + " Free";
      }
      else if (lDownLim < lDown)
      {
        dFree = ", " + MBorGB(lDown - lDownLim) + " Over";
      }
      else
      {
        dFree = "";
      }
      if (lUpLim > lUp)
      {
        uFree = ", " + MBorGB(lUpLim - lUp) + " Free";
      }
      else if (lUpLim < lUp)
      {
        uFree = ", " + MBorGB(lUp - lUpLim) + " Over";
      }
      else
      {
        uFree = "";
      }
      sTTT = "Satellite Usage" + (imSlowed ? " (Slowed) " : "") + "\n" +
        "Last Updated " + sLastUpdate + "\n" +
        "Download: " + MBorGB(lDown) + " (" + AccuratePercent((double)lDown / lDownLim) + ")" + dFree + "\n" +
        "Upload: " + MBorGB(lUp) + " (" + AccuratePercent((double)lUp / lUpLim) + ")" + uFree;
      if (imFree)
      {
        sIconBefore = "graph_typea_free";
      }
      else
      {
        if (trayRes < 8)
          trayRes = 8;
        int d = (int)Math.Round(((double)lDown / lDownLim) * trayRes);
        int u = (int)Math.Round(((double)lUp / lUpLim) * trayRes);
        if (imSlowed)
        {
          if (d == trayRes && u == trayRes)
          {
            sIconBefore = "graph_typea_slowxslow";
          }
          else if (u == trayRes)
          {
            sIconBefore = "graph_typea_" + d + "xslow";
          }
          else if (d == trayRes)
          {
            sIconBefore = "graph_typea_slowx" + u;
          }
          else
          {
            sIconBefore = "graph_typea_" + d + "x" + u;
          }
        }
        else
        {
          sIconBefore = "graph_typea_" + d + "x" + u;
        }
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
          DataBase.DataRow[] lItems = Array.FindAll(modDB.usageDB.ToArray(), (DataBase.DataRow satRow) => satRow.DATETIME.CompareTo(DateTime.Now.AddMinutes(timeCheck)) >= 0 & satRow.DATETIME.CompareTo(DateTime.Now) <= 0);
          for (int I = lItems.Length - 2; I >= 0; I += -1)
          {
            if (lDown - lItems[I].DOWNLOAD >= mySettings.Overuse)
            {
              long ChangeSize = Math.Abs(lDown - lItems[I].DOWNLOAD);
              ulong ChangeTime = (ulong)Math.Abs(modFunctions.DateDiff(DateInterval.Minute, lItems[I].DATETIME, DateTime.Now) * 60 * 1000);
              modFunctions.MakeNotifier(ref taskNotifier, false);
              if (taskNotifier != null)
              {
                taskNotifierEvent(true);
                taskNotifier.Show("Excessive Download Detected", modFunctions.ProductName + " has logged a download of " + MBorGB(ChangeSize) + " in " + modFunctions.ConvertTime(ChangeTime, false, true) + "!", 200, 0, 100);
              }
              lastBalloon = srlFunctions.TickCount();
              break;
            }
            else if (lUp - lItems[I].UPLOAD >= mySettings.Overuse)
            {
              long ChangeSize = Math.Abs(lDown - lItems[I].DOWNLOAD);
              ulong ChangeTime = (ulong)Math.Abs(modFunctions.DateDiff(DateInterval.Minute, lItems[I].DATETIME, DateTime.Now) * 60 * 1000);
              modFunctions.MakeNotifier(ref taskNotifier, false);
              if (taskNotifier != null)
              {
                taskNotifierEvent(true);
                taskNotifier.Show("Excessive Upload Detected", modFunctions.ProductName + " has logged an upload usage change of " + MBorGB(ChangeSize) + " in " + modFunctions.ConvertTime(ChangeTime, false, true) + "!", 200, 0, 100);
              }
              lastBalloon = srlFunctions.TickCount();
              break;
            }
          }
        }
      }
    }
    private void DisplayTypeA2Results(long lDown, long lDownLim, long lUp, long lUpLim, string sLastUpdate)
    {
      string sTTT = this.Title;
      if (lDown >= lDownLim)
      {
        imSlowed = true;
      }
      if ((mySettings.AccountType == localRestrictionTracker.SatHostTypes.WildBlue_EXEDE) | (mySettings.AccountType == localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE) | (mySettings.AccountType == localRestrictionTracker.SatHostTypes.Dish_EXEDE))
      {
        if (lDown < lDownLim)
        {
          imSlowed = false;
        }
      }
      else
      {
        if (lDown < lDownLim * 0.7)
        {
          imSlowed = false;
        }
      }
      if (!pnlTypeA.Visible)
      {
        pnlTypeA.Visible = true;
        pnlDisplays.Add(pnlTypeA);
      }
      if (pnlTypeB.Visible)
      {
        pnlTypeB.Visible = false;
        pnlDisplays.Remove(pnlTypeB);
      }
      if (pnlNothing.Visible)
      {
        pnlNothing.Visible = false;
        pnlDisplays.Remove(pnlNothing);
      }

      typeA_down = lDown;
      typeA_dlim = lDownLim;
      typeA_up = lUp;
      typeA_ulim = lUpLim;
      if (tmrChanges != null)
      {
        tmrChanges.Dispose();
        tmrChanges = null;
      }
      tmrChanges = new System.Threading.Timer(DisplayChangeInterval, (object)"TYPEA", 75, System.Threading.Timeout.Infinite);
      if (imSlowed)
      {
        lblTypeADldFreeVal.TooltipText = "You are over your Anytime limit!";
        lblTypeADldLimitVal.TooltipText = "Your Anytime connection has been restricted!";
      }
      else
      {
        lblTypeADldFreeVal.TooltipText = null;
        lblTypeADldLimitVal.TooltipText = null;
      }
      if (lUp >= lUpLim)
      {
        lblTypeAUldFreeVal.TooltipText = "You are over your Off-Peak limit!";
        lblTypeAUldLimitVal.TooltipText = "Your Off-Peak connection has been restricted!";
      }
      else
      {
        lblTypeAUldFreeVal.TooltipText = null;
        lblTypeAUldLimitVal.TooltipText = null;
      }
      pctTypeADld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(pctTypeADld.Allocation.Size, lDown, lDownLim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
      pctTypeAUld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(pctTypeAUld.Allocation.Size, lUp, lUpLim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
      lblTypeADld.Text = "Anytime (" + AccuratePercent((double)lDown / lDownLim) + ")";
      lblTypeAUld.Text = "Off-Peak (" + AccuratePercent((double)lUp / lUpLim) + ")";
      SetFontSize(ref lblTypeADld, GetFontSize());
      SetFontSize(ref lblTypeAUld, GetFontSize());
      pctTypeADld.TooltipText = "Graph representing your Anytime usage.";
      pctTypeAUld.TooltipText = "Graph representing your Off-Peak usage (used between 2 AM and 8 AM).";
      string atFree, opFree;
      if (lDownLim > lDown)
      {
        atFree = ", " + MBorGB(lDownLim - lDown) + " Free";
      }
      else if (lDownLim < lDown)
      {
        atFree = ", " + MBorGB(lDown - lDownLim) + " Over";
      }
      else
      {
        atFree = "";
      }
      if (lUpLim > lUp)
      {
        opFree = ", " + MBorGB(lUpLim - lUp) + " Free";
      }
      else if (lUpLim < lUp)
      {
        opFree = ", " + MBorGB(lUp - lUpLim) + " Over";
      }
      else
      {
        opFree = "";
      }
      sTTT = "Satellite Usage" + (imSlowed ? " (Slowed) " : "") + "\n" +
        "Last Updated " + sLastUpdate + "\n" +
        "Anytime: " + MBorGB(lDown) + " (" + AccuratePercent((double)lDown / lDownLim) + ")" + atFree + "\n" +
        "Off-Peak: " + MBorGB(lUp) + " (" + AccuratePercent((double)lUp / lUpLim) + ")" + opFree;
      if (imFree)
      {
        sIconBefore = "graph_typea_free";
      }
      else
      {
        if (trayRes < 8)
          trayRes = 8;
        int d = (int)Math.Round(((double)lDown / lDownLim) * trayRes);
        int u = (int)Math.Round(((double)lUp / lUpLim) * trayRes);
        if (imSlowed)
        {
          if (d == trayRes && u == trayRes)
          {
            sIconBefore = "graph_typea_slowxslow";
          }
          else if (u == trayRes)
          {
            sIconBefore = "graph_typea_" + d + "xslow";
          }
          else if (d == trayRes)
          {
            sIconBefore = "graph_typea_slowx" + u;
          }
          else
          {
            sIconBefore = "graph_typea_" + d + "x" + u;
          }
        }
        else
        {
          sIconBefore = "graph_typea_" + d + "x" + u;
        }
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
          DataBase.DataRow[] lItems = modDB.LOG_GetRange(DateTime.Now.AddMinutes(timeCheck), DateTime.Now);
          for (int I = lItems.Length - 2; I >= 0; I += -1)
          {
            if (lDown - lItems[I].DOWNLOAD >= mySettings.Overuse)
            {
              long ChangeSize = Math.Abs(lDown - lItems[I].DOWNLOAD);
              ulong ChangeTime = (ulong)Math.Abs(modFunctions.DateDiff(DateInterval.Minute, lItems[I].DATETIME, DateTime.Now) * 60 * 1000);
              modFunctions.MakeNotifier(ref taskNotifier, false);
              if (taskNotifier != null)
              {
                taskNotifierEvent(true);
                taskNotifier.Show("Excessive Usage Detected", modFunctions.ProductName + " has logged a usage change of " + MBorGB(ChangeSize) + " in " + modFunctions.ConvertTime(ChangeTime, false, true) + "!", 200, 0, 100);
              }
              lastBalloon = srlFunctions.TickCount();
              break;
            }
            else if (lUp - lItems[I].UPLOAD >= mySettings.Overuse)
            {
              long ChangeSize = Math.Abs(lDown - lItems[I].DOWNLOAD);
              ulong ChangeTime = (ulong)Math.Abs(modFunctions.DateDiff(DateInterval.Minute, lItems[I].DATETIME, DateTime.Now) * 60 * 1000);
              modFunctions.MakeNotifier(ref taskNotifier, false);
              if (taskNotifier != null)
              {
                taskNotifierEvent(true);
                taskNotifier.Show("Excessive Off-Peak Usage Detected", modFunctions.ProductName + " has logged an Off-Peak usage change of " + MBorGB(ChangeSize) + " in " + modFunctions.ConvertTime(ChangeTime, false, true) + "!", 200, 0, 100);
              }
              lastBalloon = srlFunctions.TickCount();
              break;
            }
          }
        }
      }
    }
    private void DisplayTypeBResults(long lDown, long lDownLim, long lUp, long lUpLim, string sLastUpdate)
    {
      if (lDown != lUp & lDownLim != lUpLim)
      {
        DisplayTypeAResults(lDown, lDownLim, lUp, lUpLim, sLastUpdate);
        return;
      }
      string sTTT = this.Title;
      imSlowed = (lDown >= lDownLim);
      if (pnlTypeA.Visible)
      {
        pnlTypeA.Visible = false;
        pnlDisplays.Remove(pnlTypeA);
      }
      if (!pnlTypeB.Visible)
      {
        pnlTypeB.Visible = true;
        pnlDisplays.Add(pnlTypeB);
      }
      if (pnlNothing.Visible)
      {
        pnlNothing.Visible = false;
        pnlDisplays.Remove(pnlNothing);
      }
      typeB_used = lDown;
      typeB_lim = lDownLim;
      if (tmrChanges != null)
      {
        tmrChanges.Dispose();
        tmrChanges = null;
      }
      tmrChanges = new System.Threading.Timer(DisplayChangeInterval, (object)"TYPEB", 75, System.Threading.Timeout.Infinite);
      if (imSlowed)
      {
        lblTypeBFreeVal.TooltipText = "You are over your usage limit!";
        lblTypeBLimitVal.TooltipText = "Your connection has been restricted!";
      }
      else
      {
        lblTypeBFreeVal.TooltipText = null;
        lblTypeBLimitVal.TooltipText = null;
      }
      pctTypeB.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayRProgress(pctTypeB.Allocation.Size, lDown, lDownLim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
      sTTT = "Satellite Usage" + (imSlowed ? " (Slowed) " : "") + "\n" +
        "Last Updated " + sLastUpdate + "\n" +
        "Using " + MBorGB(lDown) + " of " + MBorGB(lDownLim) + " (" + AccuratePercent((double)lDown / lDownLim) + ")";
      if (lDownLim > lDown)
      {
        sTTT += "\n" + MBorGB(lDownLim - lDown) + " Free";
      }
      else if (lDownLim < lDown)
      {
        sTTT += "\n" + MBorGB(lDown - lDownLim) + " Over";
      }
      if (imFree)
      {
        sIconBefore = "graph_typeb_free";
      }
      else if (imSlowed)
      {
        sIconBefore = "graph_typeb_slow";
      }
      else
      {
        if (trayRes < 8)
          trayRes = 8;
        int u = (int)Math.Round(((double)lDown / lDownLim) * trayRes);
        if (u > trayRes)
          u = trayRes;
        sIconBefore = "graph_typeb_" + u;
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
          DataBase.DataRow[] lItems = Array.FindAll(modDB.usageDB.ToArray(), (DataBase.DataRow satRow) => satRow.DATETIME.CompareTo(DateTime.Now.AddMinutes(timeCheck)) >= 0 & satRow.DATETIME.CompareTo(DateTime.Now) <= 0);
          for (int I = lItems.Length - 2; I >= 0; I += -1)
          {
            if (lDown - lItems[I].DOWNLOAD >= mySettings.Overuse)
            {
              long ChangeSize = Math.Abs(lDown - lItems[I].DOWNLOAD);
              ulong ChangeTime = (ulong)Math.Abs(modFunctions.DateDiff(DateInterval.Minute, lItems[I].DATETIME, DateTime.Now) * 60 * 1000);
              modFunctions.MakeNotifier(ref taskNotifier, false);
              if (taskNotifier != null)
              {
                taskNotifierEvent(true);
                taskNotifier.Show("Excessive Usage Detected", modFunctions.ProductName + " has logged a usage change of " + MBorGB(ChangeSize) + " in " + modFunctions.ConvertTime(ChangeTime, false, true) + "!", 200, 0, 100);
              }
              lastBalloon = srlFunctions.TickCount();
              break;
            }
          }
        }
      }
    }
    private void DisplayResults(long lDown, long lDownLim, long lUp, long lUpLim)
    {
      if ((lDownLim > 0) | (lUpLim > 0))
      {
        DateTime lastUpdate = modDB.LOG_GetLast();
        string sLastUpdate = lastUpdate.ToString("M/d h:mm tt");
        myPanel = mySettings.AccountType;
        switch (mySettings.AccountType)
        {
          case localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE:
          case localRestrictionTracker.SatHostTypes.WildBlue_EXEDE:
          case localRestrictionTracker.SatHostTypes.WildBlue_EXEDE_RESELLER:
            DisplayTypeBResults(lDown, lDownLim, lUp, lUpLim, sLastUpdate);
            break;
          case localRestrictionTracker.SatHostTypes.Dish_EXEDE:
            DisplayTypeA2Results(lDown, lDownLim, lUp, lUpLim, sLastUpdate);
            break;
          default:
            DisplayTypeAResults(lDown, lDownLim, lUp, lUpLim, sLastUpdate);
            break;
        }
      }
      else
      {
        if (pnlTypeA.Visible)
        {
          pnlTypeA.Visible = false;
          pnlDisplays.Remove(pnlTypeA);
        }
        if (pnlTypeB.Visible)
        {
          pnlTypeB.Visible = false;
          pnlDisplays.Remove(pnlTypeB);
        }
        if (!pnlNothing.Visible)
        {
          pnlNothing.Visible = true;
          pnlDisplays.Add(pnlNothing);
        }

        myPanel = localRestrictionTracker.SatHostTypes.Other;
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
      if ((!string.IsNullOrEmpty(sAccount)) & (!string.IsNullOrEmpty(sProvider)) & (!string.IsNullOrEmpty(sPassword)))
      {
        EnableProgressIcon();
        SetNextLoginTime();
        if (e != null)
        {
          if ((e.Event.State & Gtk.Accelerator.DefaultModMask) == Gdk.ModifierType.ControlMask)
          {
            updateFull = true;
            FullCheck = true;
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
          else
          {
            updateFull = false;
          }
        }
        else
        {
          updateFull = false;
        }
        if (!checkedAJAX && mySettings.AccountType == localRestrictionTracker.SatHostTypes.WildBlue_EXEDE_RESELLER)
        {
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Checking for AJAX List Update...", false);
          UpdateAJAXLists AJAXUpdate = new UpdateAJAXLists(sProvider, mySettings.Timeout, mySettings.Proxy, (object)"GetUsage", UpdateAJAXLists_UpdateChecked);
          AJAXUpdate.GetType();
        }
        else
        {
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Reading Usage...", false);
          MethodInvoker UsageInvoker = GetUsage;
          UsageInvoker.BeginInvoke(null, null);
        }
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
      if (remoteData != null)
      {
        bReRun = true;
        remoteDataEvent(false);
        remoteData.Dispose();
        remoteData = null;
        DisplayUsage(true, false);
      }
      else if (localData != null)
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
        if (myState != LoadStates.Lookup)
        {
          MethodInvoker lookupInvoker = LookupProvider;
          lookupInvoker.BeginInvoke(null, null);
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
          didHostListSave = false;
          mySettings = null;
          ReLoadSettings();
          SetNextLoginTime();
          MakeCustomIconListing();
          MethodInvoker ReInitInvoker = ReInit;
          ReInitInvoker.BeginInvoke(null, null);
          break;
        case ResponseType.Ok:
          didHostListSave = false;
          mySettings = null;
          ReLoadSettings();
          MakeCustomIconListing();
          if (bReRun)
          {
            SetNextLoginTime();
            EnableProgressIcon();
            SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing Connection...", false);
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
            modFunctions.ScreenDefaultColors(ref MainClass.fHistory.mySettings.Colors, MainClass.fHistory.mySettings.AccountType);
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
            SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing Connection...", false);
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
          modFunctions.ScreenDefaultColors(ref MainClass.fHistory.mySettings.Colors, MainClass.fHistory.mySettings.AccountType);
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
    private void taskNotifier_CloseClick(object sender, EventArgs e)
    {
      sFailTray = "";
    }
    private void taskNotifier_ContentClick(object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty(sFailTray))
      {
        ParamaterizedInvoker tFTP = modFunctions.SaveToFTP;
        tFTP.BeginInvoke((object)sFailTray, null, null);
      }
      sFailTray = "";
    }
    #region "Graphs"
    private Gdk.Pixbuf CreateTypeATrayIcon(long lDown, long lDownLim, long lUp, long lUpLim, bool bSlow, bool bFree)
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
        if (lDown < lDownLim)
          modFunctions.CreateTrayIcon_Left(ref g, lDown, lDownLim, mySettings.Colors.TrayDownA, mySettings.Colors.TrayDownB, mySettings.Colors.TrayDownC, trayRes);
        if (lUp < lUpLim)
          modFunctions.CreateTrayIcon_Right(ref g, lUp, lUpLim, mySettings.Colors.TrayUpA, mySettings.Colors.TrayUpB, mySettings.Colors.TrayUpC, trayRes);
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
        modFunctions.CreateTrayIcon_Left(ref g, lDown, lDownLim, mySettings.Colors.TrayDownA, mySettings.Colors.TrayDownB, mySettings.Colors.TrayDownC, trayRes);
        modFunctions.CreateTrayIcon_Right(ref g, lUp, lUpLim, mySettings.Colors.TrayUpA, mySettings.Colors.TrayUpB, mySettings.Colors.TrayUpC, trayRes);
      }
      g.Dispose();
      return modFunctions.ImageToPixbuf(imgTray);
    }
    private Gdk.Pixbuf CreateTypeBTrayIcon(long lUsed, long lLim, bool bSlow, bool bFree)
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
      SetStatusText(modDB.LOG_GetLast().ToString("g"), "Checking for Software Update...", false);
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
                  if (remoteData != null)
                  {
                    remoteDataEvent(false);
                    remoteData.Dispose();
                    remoteData = null;
                  }
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
                  if (remoteData != null)
                  {
                    remoteDataEvent(false);
                    remoteData.Dispose();
                    remoteData = null;
                  }
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
                    if (remoteData != null)
                    {
                      remoteDataEvent(false);
                      remoteData.Dispose();
                      remoteData = null;
                    }
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
                    if (remoteData != null)
                    {
                      remoteDataEvent(false);
                      remoteData.Dispose();
                      remoteData = null;
                    }
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
              SetStatusText(modDB.LOG_GetLast().ToString("g"), string.Empty, false);
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
              if (remoteData != null)
              {
                remoteDataEvent(false);
                remoteData.Dispose();
                remoteData = null;
              }
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
                if (remoteData != null)
                {
                  remoteDataEvent(false);
                  remoteData.Dispose();
                  remoteData = null;
                }
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
              SetStatusText(modDB.LOG_GetLast().ToString("g"), string.Empty, false);
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
      SetStatusText(modDB.LOG_GetLast().ToString("g"), "Downloading Software Update...", false);
    }
    private void updateChecker_DownloadResult(object sender, clsUpdate.DownloadEventArgs e)
    {
      EventArgs ea = (EventArgs)e;
      Gtk.Application.Invoke(sender, ea, Main_UpdateCheckerDownloadResult);
    }
    private void Main_UpdateCheckerDownloadResult(object sender, EventArgs ea)
    {
      clsUpdate.DownloadEventArgs e = (clsUpdate.DownloadEventArgs)ea;
      if (tmrSpeed != 0)
      {
        GLib.Source.Remove(tmrSpeed);
        tmrSpeed = 0;
      }
      if (e.Error != null)
      {
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "Software Update Error: " + e.Error.Message, true);
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
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "Software Update Cancelled!", true);
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
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "Software Update Download Complete", false);
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
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Software Update Failure!", true);
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
    private void UpdateAJAXLists_ListUpdated(object asyncState, string shortList, string fullList)
    {
      if (string.IsNullOrEmpty(shortList) || string.IsNullOrEmpty(fullList))
      {
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "Unable to Update AJAX Lists.", true);
        DisplayUsage(false, true);
        return;
      }
      if (mySettings.AJAXOrderShort == shortList && mySettings.AJAXOrderFull == fullList)
      {
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "AJAX failed to yield data table. A fix should be available soon.", true);
        DisplayUsage(false, true);
        return;
      }
      mySettings.AJAXOrderShort = shortList;
      mySettings.AJAXOrderFull = fullList;
      mySettings.Save();
      SetStatusText(modDB.LOG_GetLast().ToString("g"), "Updated AJAX Lists. Reconnecting...", false);
      if (localData != null)
      {
        localDataEvent(false);
        localData.Dispose();
        localData = null;
      }
      localData = new localRestrictionTracker(modFunctions.AppData);
    }
    private void UpdateAJAXLists_UpdateChecked(object asyncState, string shortList, string fullList)
    {
      checkedAJAX = true;
      if (string.IsNullOrEmpty(shortList) || string.IsNullOrEmpty(fullList))
      {
        if (!string.IsNullOrEmpty(shortList))
        {
          if (shortList.StartsWith("ERR_"))
            SetStatusText(modDB.LOG_GetLast().ToString("g"), "Error Updating AJAX Lists. Preparing Connection anyway...\nError: " + shortList.Substring(4), false);
          else if (shortList.StartsWith("URL_"))
            SetStatusText(modDB.LOG_GetLast().ToString("g"), "Unable to Update AJAX Lists. Preparing Connection anyway...\nRedirected to " + shortList.Substring(4) + ".", false);
          else if (shortList == "DATA_EMPTY")
            SetStatusText(modDB.LOG_GetLast().ToString("g"), "Unable to Update AJAX Lists. Preparing Connection anyway...\nNo Data from Server.", false);
          else if (shortList.StartsWith("DATA_REDIR_"))
            SetStatusText(modDB.LOG_GetLast().ToString("g"), "Unable to Update AJAX Lists. Preparing Connection anyway...\nRedirection to new URL.\n" + shortList.Substring(11), false);
          else if (shortList.StartsWith("DATA_SEP_"))
            SetStatusText(modDB.LOG_GetLast().ToString("g"), "Unable to Update AJAX Lists. Preparing Connection anyway...\nMalformed Data from Server.\n" + shortList.Substring(9), false);
          else
            SetStatusText(modDB.LOG_GetLast().ToString("g"), "Unable to Update AJAX Lists. Preparing Connection anyway...\nData: " + shortList, false);
        }
        else
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Unable to Update AJAX Lists. Preparing Connection anyway...", false);
        DisplayUsage(false, true);
        return;
      }
      else if (mySettings.AJAXOrderShort == shortList && mySettings.AJAXOrderFull == fullList)
      {
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing Connection...", false);
      }
      else
      {
        mySettings.AJAXOrderShort = shortList;
        mySettings.AJAXOrderFull = fullList;
        mySettings.Save();
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "AJAX Lists Updated. Preparing Connection...", false);
      }
      MethodInvoker UsageInvoker = GetUsage;
      UsageInvoker.BeginInvoke(null, null);
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
    #region "Failure Reports"
    private int previousFail = 0;
    private string previousFailS = null;
    public void FailResponse(string Ret)
    {
      ResolveEventArgs ea = new ResolveEventArgs(Ret);
      Gtk.Application.Invoke(null, (EventArgs)ea, Main_FailResponse);
    }
    private void Main_FailResponse(object sender, EventArgs ea)
    {
      ResolveEventArgs e = (ResolveEventArgs)ea;
      string Ret = e.Name;
      modFunctions.MakeNotifier(ref taskNotifier, false);
      if (taskNotifier != null)
      {
        taskNotifierEvent(true);
        if (Ret == "added")
          taskNotifier.Show("Error Report Sent", "Your report has been received by " + modFunctions.CompanyName + ".\nThank you for helping to improve " + modFunctions.ProductName + "!", 200, 15 * 1000, 100);
        else if (Ret == "exists")
          taskNotifier.Show("Error Already Reported", "This error has already been reported. It should be fixed in the next release.\nThank you anyway!", 200, 15 * 1000, 100);
        else
          taskNotifier.Show("Error Reporting Error", modFunctions.ProductName + " was unable to contact the " + modFunctions.CompanyName + " servers. Please check your internet connection.", 200, 30 * 1000, 100);
      }
    }
    private void FailFile(string sFail)
    {
      if (clsUpdate.QuickCheckVersion() == ResultType.NoUpdate)
      {
        if (previousFailS == sFail && previousFail == DateTime.Now.DayOfYear)
          return;
        if (previousFailS == sFail && previousFail > 0)
        {
          int nextDay = previousFail + 1;
          if (nextDay == 366)
            nextDay = 1;
          if (DateTime.Now.DayOfYear == nextDay && DateTime.Now.Hour < 9)
            return;
        }
        sFailTray = sFail;
        modFunctions.MakeNotifier(ref taskNotifier, true);
        if (taskNotifier != null)
        {
          taskNotifierEvent(true);
          taskNotifier.Show("Error Reading Page Data", modFunctions.ProductName + " encountered data it does not understand.\nClick this alert to report the problem to " + modFunctions.CompanyName + ".", 200, 3 * 60 * 1000, 100);
        }
        previousFail = DateTime.Now.DayOfYear;
        previousFailS = sFail;
      }
    }
    private void FailFile(string sFail, bool bJustFeedback)
    {
      if (clsUpdate.QuickCheckVersion() == ResultType.NoUpdate)
      {
        if (previousFailS == sFail && previousFail == DateTime.Now.DayOfYear)
          return;
        if (previousFailS == sFail && previousFail > 0)
        {
          int nextDay = previousFail + 1;
          if (nextDay == 366)
            nextDay = 1;
          if (DateTime.Now.DayOfYear == nextDay && DateTime.Now.Hour < 9)
            return;
        }
        sFailTray = sFail;
        modFunctions.MakeNotifier(ref taskNotifier, true);
        if (taskNotifier != null)
        {
          taskNotifierEvent(true);
          if (bJustFeedback)
            taskNotifier.Show("Page Data Feedback Request", modFunctions.CompanyName + " has requested that information from your connection be sent back to the servers for further analysis.\nClick this alert if you'd like to help out.", 200, 1 * 60 * 1000, 100);
          else
            taskNotifier.Show("Error Reading Page Data", modFunctions.ProductName + " encountered data it does not understand.\nClick this alert to report the problem to " + modFunctions.CompanyName + ".", 200, 3 * 60 * 1000, 100);
        }
        previousFail = DateTime.Now.DayOfYear;
        previousFailS = sFail;
      }
    }
    #endregion
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
      try
      {
        if (mySettings.NetTestURL.Contains("://"))
        {
          System.Diagnostics.Process.Start(mySettings.NetTestURL);
        }
        else
        {
          System.Diagnostics.Process.Start("http://" + mySettings.NetTestURL);
        }
      }
      catch (Exception ex)
      {
        modFunctions.MakeNotifier(ref taskNotifier, false);
        if (taskNotifier != null)
          taskNotifier.Show("Failed to run Web Browser", modFunctions.ProductName + " could not navigate to \"" + mySettings.NetTestURL + "\"!\n" + ex.Message, 200, 3000, 100);
      }
    }
    #endregion
  }
}
