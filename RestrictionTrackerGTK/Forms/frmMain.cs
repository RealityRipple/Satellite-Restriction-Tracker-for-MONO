using System;
using System.IO;
using Gtk;
using RestrictionLibrary;
using System.Drawing;
using System.Collections.Generic;
namespace RestrictionTrackerGTK
{
  public partial class frmMain: Gtk.Window
  {
    private localRestrictionTracker.SatHostTypes myPanel;
    private enum LoadStates
    {
      Loading,
      Loaded,
      Lookup
    }
    private static object trayIcon;
    private byte TrayStyle;
    //0=off, 1==standard, 2=appind
    private int trayRes = 16;
    private String _IconFolder = "";
    private String IconFolder
    {
      get
      {
        if (String.IsNullOrEmpty(_IconFolder))
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
    private uint tmrUpdate;
    private uint tmrIcon;
    private uint tmrSpeed;
    private uint tmrStatus;
    private uint tmrShow;
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
    private RestrictionLibrary.CookieAwareWebClient wsHostList;
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
    private bool ClosingTime;
    private string sFailTray;
    private byte bAlert;
    private long wb_down, wb_up, wb_dlim, wb_ulim;
    private long r_used, r_lim;
    private bool updateFull;
    private long lastBalloon;
    private bool firstRestore;
    private Dictionary<string,string> markupList = new Dictionary<string, string>();
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
      public event TypeDeterminedEventHandler TypeDetermined;
      public delegate void TypeDeterminedEventHandler(object Sender,TypeDeterminedEventArgs e);
      public DetermineTypeOffline(string Provider, object Sender)
      {
        System.Threading.Thread.Sleep(0);
        ParamaterizedInvoker testCallback = new ParamaterizedInvoker(BeginTest);
        object[] oparams = { Provider, Sender };
        object param = (object)oparams;
        testCallback.BeginInvoke(param, null, null);
      }
      private void BeginTest(object state)
      {
        object[] stateArray = (object[])state;
        string Provider = (string)stateArray[0];
        object Sender = stateArray[1];
        if (Provider.ToLower() == "dish.com" | Provider.ToLower() == "dish.net")
        {
          if (TypeDetermined == null)
          {
            System.Threading.Thread.Sleep(0);
            System.Threading.Thread.Sleep(100);
            System.Threading.Thread.Sleep(0);
          }
          if (TypeDetermined != null)
          {
            TypeDetermined(Sender, new TypeDeterminedEventArgs(localRestrictionTracker.SatHostTypes.DishNet_EXEDE));
          }
        }
        else if (Provider.ToLower() == "exede.com" | Provider.ToLower() == "exede.net")
        {
          if (TypeDetermined == null)
          {
            System.Threading.Thread.Sleep(0);
            System.Threading.Thread.Sleep(100);
            System.Threading.Thread.Sleep(0);
          }
          if (TypeDetermined != null)
          {
            TypeDetermined(Sender, new TypeDeterminedEventArgs(localRestrictionTracker.SatHostTypes.WildBlue_EXEDE));
          }
        }
        else
        {
          OfflineCheck(state);
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
      private void OfflineCheck(object Sender)
      {
        float rpP = 0;
        float exP = 0;
        float wbP = 0;
        OfflineStats(ref rpP, ref exP, ref wbP);
        if (rpP == 0 & exP == 0 & wbP == 0)
        {
          if (TypeDetermined != null)
          {
            TypeDetermined(Sender, new TypeDeterminedEventArgs(localRestrictionTracker.SatHostTypes.Other));
          }
        }
        else
        {
          if (rpP > exP & rpP > wbP)
          {
            if (TypeDetermined != null)
            {
              TypeDetermined(Sender, new TypeDeterminedEventArgs(localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE));
            }
          }
          else if (exP > rpP & exP > wbP)
          {
            if (TypeDetermined != null)
            {
              TypeDetermined(Sender, new TypeDeterminedEventArgs(localRestrictionTracker.SatHostTypes.WildBlue_EXEDE));
            }
          }
          else if (wbP > rpP & wbP > exP)
          {
            if (TypeDetermined != null)
            {
              TypeDetermined(Sender, new TypeDeterminedEventArgs(localRestrictionTracker.SatHostTypes.WildBlue_LEGACY));
            }
          }
          else
          {
            if (rpP > wbP & exP > wbP & rpP == exP)
            {
              if (TypeDetermined != null)
              {
                TypeDetermined(Sender, new TypeDeterminedEventArgs(localRestrictionTracker.SatHostTypes.WildBlue_EXEDE));
              }
            }
            else
            {
              if (TypeDetermined != null)
              {
                TypeDetermined(Sender, new TypeDeterminedEventArgs(localRestrictionTracker.SatHostTypes.Other));
              }
              System.Diagnostics.Debugger.Break();
            }
          }
        }
      }
    }
    private DetermineType withEventsField_TypeDetermination;
    private DetermineType TypeDetermination
    {
      get
      {
        return withEventsField_TypeDetermination;
      }
      set
      {
        if (withEventsField_TypeDetermination != null)
        {
          withEventsField_TypeDetermination.TypeDetermined -= TypeDetermination_TypeDetermined;
        }
        withEventsField_TypeDetermination = value;
        if (withEventsField_TypeDetermination != null)
        {
          withEventsField_TypeDetermination.TypeDetermined += TypeDetermination_TypeDetermined;
        }
      }
    }
    private DetermineTypeOffline withEventsField_TypeDeterminationOffline;
    private DetermineTypeOffline TypeDeterminationOffline
    {
      get
      {
        return withEventsField_TypeDeterminationOffline;
      }
      set
      {
        if (withEventsField_TypeDeterminationOffline != null)
        {
          withEventsField_TypeDeterminationOffline.TypeDetermined -= TypeDeterminationOffline_TypeDetermined;
        }
        withEventsField_TypeDeterminationOffline = value;
        if (withEventsField_TypeDeterminationOffline != null)
        {
          withEventsField_TypeDeterminationOffline.TypeDetermined += TypeDeterminationOffline_TypeDetermined;
        }
      }
    }
    private void TypeDetermination_TypeDetermined(object Sender, DetermineType.TypeDeterminedEventArgs e)
    {
      Gtk.Application.Invoke(Sender, e, Main_TypeDetermined);
    }
    private void Main_TypeDetermined(object sender, EventArgs ea)
    {
      DetermineType.TypeDeterminedEventArgs e = (DetermineType.TypeDeterminedEventArgs)ea;
      NextGrabTick = modFunctions.TickCount() + (mySettings.Timeout * 1000);
      if (e.HostGroup == DetermineType.TypeDeterminedEventArgs.SatHostGroup.Other)
      {
        if (tmrIcon != 0)
        {
          GLib.Source.Remove(tmrIcon);
          tmrIcon = 0;
        }
        TypeDeterminationOffline = new DetermineTypeOffline(sProvider, sender);
      }
      else
      {
        if (e.HostGroup == DetermineType.TypeDeterminedEventArgs.SatHostGroup.DishNet)
          mySettings.AccountType = localRestrictionTracker.SatHostTypes.DishNet_EXEDE;
        else if (e.HostGroup == DetermineType.TypeDeterminedEventArgs.SatHostGroup.WildBlue)
          mySettings.AccountType = localRestrictionTracker.SatHostTypes.WildBlue_LEGACY;
        else if (e.HostGroup == DetermineType.TypeDeterminedEventArgs.SatHostGroup.RuralPortal)
          mySettings.AccountType = localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE;
        else if (e.HostGroup == DetermineType.TypeDeterminedEventArgs.SatHostGroup.Exede)
          mySettings.AccountType = localRestrictionTracker.SatHostTypes.WildBlue_EXEDE;
        if (mySettings.Colors.MainDownA == Color.Transparent)
        {
          SetDefaultColors();
        }
        mySettings.Save();
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing Connection...", false);
        if (localData != null)
        {
          localDataEvent(false);
          localData.Dispose();
          localData = null;
        }
        localData = new localRestrictionTracker(modFunctions.AppData);
        localDataEvent(true);
        MethodInvoker connectInvoker = new MethodInvoker(localData.Connect);
        connectInvoker.BeginInvoke(null, null);
        
      }
    }
    private class TypeDeterminedEventArgs : EventArgs
    {
      public localRestrictionTracker.SatHostTypes HostType;
      public TypeDeterminedEventArgs(localRestrictionTracker.SatHostTypes Type)
      {
        HostType = Type;
      }
    }
    private void TypeDeterminationOffline_TypeDetermined(object Sender, TypeDeterminedEventArgs e)
    {
      Gtk.Application.Invoke(Sender, e, Main_TypeDeterminedOffline);
    }
    private void Main_TypeDeterminedOffline(object sender, EventArgs ea)
    {
      TypeDeterminedEventArgs e = (TypeDeterminedEventArgs)ea;
      NextGrabTick = modFunctions.TickCount() + (mySettings.Timeout * 1000);
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
        if (mySettings.Colors.MainDownA == Color.Transparent)
        {
          SetDefaultColors();
        }
        mySettings.Save();
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing Connection...", false);
        if (localData != null)
        {
          localDataEvent(false);
          localData.Dispose();
          localData = null;
        }
        localData = new localRestrictionTracker(modFunctions.AppData);
        localDataEvent(true);
        MethodInvoker connectInvoker = new MethodInvoker(localData.Connect);
        connectInvoker.BeginInvoke(null, null);
      }
    }
    #endregion
    #region "Form Events"
    public frmMain(): base (Gtk.WindowType.Toplevel)
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
      TrayStyle = 0;

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
        mySettings = new AppSettings();
        if (mySettings.Colors.MainDownA == Color.Transparent)
        {
          SetDefaultColors();
        }
        modFunctions.NOTIFIER_STYLE = modFunctions.LoadAlertStyle(mySettings.AlertStyle);
      }
      System.Net.ServicePointManager.SecurityProtocol = mySettings.Protocol;
      System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;

      NextGrabTick = long.MinValue;


      cmdRefresh.ButtonReleaseEvent += cmdRefresh_Click;
      cmdHistory.Clicked += cmdHistory_Click;
      cmdConfig.Clicked += cmdConfig_Click;
      cmdAbout.Clicked += cmdAbout_Click;
      evnDld.ButtonReleaseEvent += evnGraph_Click;
      evnUld.ButtonReleaseEvent += evnGraph_Click;
      evnExede.ButtonReleaseEvent += evnGraph_Click;
      evnRural.ButtonReleaseEvent += evnGraph_Click;

      pbMainStatus.Visible = false;
      lblMainStatus.Visible = false;

      DisplayResults(0, 0, 0, 0);

      this.Resize(mySettings.MainSize.Width, mySettings.MainSize.Height);
      Main_SizeChanged(null, null);

      trayIcon = new StatusIcon();
      ((StatusIcon)trayIcon).SizeChanged += trayIcon_SizeChanged;
      tmrShow = GLib.Timeout.Add(2000, trayIcon_NoGo);
    }
    private void trayIcon_SizeChanged(object sender, SizeChangedArgs e)
    {
      if (((StatusIcon)trayIcon).Embedded)
      {
        if (e.Size > 7)
        {
          if (TrayStyle == 0)
          {
            TrayStyle = 1;
            trayRes = e.Size;
            MakeIconListing();
            if (tmrShow != 0)
            {
              GLib.Source.Remove(tmrShow);
              tmrShow = 0;
              ((StatusIcon)trayIcon).Activate += OnTrayIconActivate;
              ((StatusIcon)trayIcon).PopupMenu += OnTrayIconPopup;
              SetTrayText(modFunctions.ProductName);
              firstRestore = false;
              StartupCleanup();
            }
          }
          else
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
        if (((StatusIcon)trayIcon).Embedded)
        {
          TrayStyle = 1;
          trayRes = ((StatusIcon)trayIcon).Size;
          if (trayRes < 8)
            trayRes = 8;
          MakeIconListing();
          ((StatusIcon)trayIcon).Activate += OnTrayIconActivate;
          ((StatusIcon)trayIcon).PopupMenu += OnTrayIconPopup;
          SetTrayText(modFunctions.ProductName);
          firstRestore = false;
        }
        else
        {
          trayRes = 16;
          MakeIconListing();
          try
          {
            trayIcon = new AppIndicator.ApplicationIndicator("restriction-tracker", "norm", AppIndicator.Category.Communications, IconFolder);
            TrayStyle = 2;
            ((AppIndicator.ApplicationIndicator)trayIcon).Menu = mnuTray;
            ((AppIndicator.ApplicationIndicator)trayIcon).Status = AppIndicator.Status.Active;
            SetTrayText(modFunctions.ProductName);
            firstRestore = false;
          }
          catch (Exception)
          {
            Directory.Delete(IconFolder, true);
            TrayStyle = 0;
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
      ResizeIcon("norm.ico").Save(System.IO.Path.Combine(IconFolder, "norm.png"), System.Drawing.Imaging.ImageFormat.Png);
      ResizeIcon("free.ico").Save(System.IO.Path.Combine(IconFolder, "free.png"), System.Drawing.Imaging.ImageFormat.Png);
      ResizeIcon("restricted.ico").Save(System.IO.Path.Combine(IconFolder, "restricted.png"), System.Drawing.Imaging.ImageFormat.Png);
      ResizePng("error.png").Save(System.IO.Path.Combine(IconFolder, "error.png"), System.Drawing.Imaging.ImageFormat.Png);
      ResizeIcon("throbsprite.1.ico").Save(System.IO.Path.Combine(IconFolder, "throb_1.png"), System.Drawing.Imaging.ImageFormat.Png);
      ResizeIcon("throbsprite.2.ico").Save(System.IO.Path.Combine(IconFolder, "throb_2.png"), System.Drawing.Imaging.ImageFormat.Png);
      ResizeIcon("throbsprite.3.ico").Save(System.IO.Path.Combine(IconFolder, "throb_3.png"), System.Drawing.Imaging.ImageFormat.Png);
      ResizeIcon("throbsprite.4.ico").Save(System.IO.Path.Combine(IconFolder, "throb_4.png"), System.Drawing.Imaging.ImageFormat.Png);
      ResizeIcon("throbsprite.5.ico").Save(System.IO.Path.Combine(IconFolder, "throb_5.png"), System.Drawing.Imaging.ImageFormat.Png);
      ResizeIcon("throbsprite.7.ico").Save(System.IO.Path.Combine(IconFolder, "throb_7.png"), System.Drawing.Imaging.ImageFormat.Png);
      ResizeIcon("throbsprite.8.ico").Save(System.IO.Path.Combine(IconFolder, "throb_8.png"), System.Drawing.Imaging.ImageFormat.Png);
      ResizeIcon("throbsprite.9.ico").Save(System.IO.Path.Combine(IconFolder, "throb_9.png"), System.Drawing.Imaging.ImageFormat.Png);
      ResizeIcon("throbsprite.10.ico").Save(System.IO.Path.Combine(IconFolder, "throb_10.png"), System.Drawing.Imaging.ImageFormat.Png);
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
      bool doWB = false;
      if (mySettings != null)
      {
        if (mySettings.AccountType != localRestrictionTracker.SatHostTypes.Other)
        {
          doBoth = false;
          if (mySettings.AccountType == localRestrictionTracker.SatHostTypes.WildBlue_LEGACY || mySettings.AccountType == localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY)
            doWB = true;
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
            using (Gdk.Pixbuf pIco = CreateTrayIcon(down, trayRes, up, trayRes))
            {
              pIco.Save(System.IO.Path.Combine(IconFolder, "graph_wb_" + down + "x" + up + ".png"), "png");
            }
          }
          using (Gdk.Pixbuf pIco = CreateRTrayIcon(up, trayRes))
          {
            pIco.Save(System.IO.Path.Combine(IconFolder, "graph_r_" + up + ".png"), "png");
          }
        }
      }
      else if (doWB)
      {
        for (long up = 0; up <= trayRes; up++)
        {
          for (long down = 0; down <= trayRes; down++)
          {
            using (Gdk.Pixbuf pIco = CreateTrayIcon(down, trayRes, up, trayRes))
            {
              pIco.Save(System.IO.Path.Combine(IconFolder, "graph_wb_" + down + "x" + up + ".png"), "png");
            }
          }
        }
      }
      else
      {
        for (long up = 0; up <= trayRes; up++)
        {
          using (Gdk.Pixbuf pIco = CreateRTrayIcon(up, trayRes))
          {
            pIco.Save(System.IO.Path.Combine(IconFolder, "graph_r_" + up + ".png"), "png");
          }
        }
      }
    }
    private void StartupCleanup()
    {
      this.Opacity = 1;
      tmrStatus = GLib.Timeout.Add(500, tmrStatus_Tick);
      tmrUpdate = GLib.Timeout.Add(1000, tmrUpdate_Tick);
      if (myState != LoadStates.Loaded)
      {
        InitAccount();
        if (!String.IsNullOrEmpty(sAccount))
        {
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
          if (tmrIcon != 0)
          {
            GLib.Source.Remove(tmrIcon);
            tmrIcon = 0;
          }
          SetTrayIcon("norm");
          SetTag(LoadStates.Loaded);
          this.Resize(mySettings.MainSize.Width, mySettings.MainSize.Height);
          Main_SizeChanged(null, null);

          cmdConfig.Click();
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
          mySettings = new AppSettings();
          if (mySettings.Colors.MainDownA == Color.Transparent)
          {
            SetDefaultColors();
          }
          modFunctions.NOTIFIER_STYLE = modFunctions.LoadAlertStyle(mySettings.AlertStyle);
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
      if (myPanel == localRestrictionTracker.SatHostTypes.WildBlue_LEGACY || myPanel == localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY || myPanel == localRestrictionTracker.SatHostTypes.DishNet_EXEDE)
      {
        if (lblDldUsed.Text == " -- ")
        {
          pctDld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(evnDld.Allocation.Size, 0, 0, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          pctUld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(evnUld.Allocation.Size, 0, 0, mySettings.Accuracy, mySettings.Colors.MainUpA, mySettings.Colors.MainUpB, mySettings.Colors.MainUpC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          SetTrayIcon("norm");
        }
        else
        {
          pctDld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(evnDld.Allocation.Size, wb_down, wb_dlim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          pctUld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(evnDld.Allocation.Size, wb_up, wb_ulim, mySettings.Accuracy, mySettings.Colors.MainUpA, mySettings.Colors.MainUpB, mySettings.Colors.MainUpC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          int d = (int)Math.Round(((double)wb_down / wb_dlim) * trayRes);
          int u = (int)Math.Round(((double)wb_up / wb_ulim) * trayRes);
          SetTrayIcon("graph_wb_" + d + "x" + u); 
        }
      }
      else if (myPanel == localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE || myPanel == localRestrictionTracker.SatHostTypes.WildBlue_EXEDE)
      {
        if (lblRuralUsedVal.Text == " -- ")
        {
          pctRural.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayRProgress(pctRural.Allocation.Size, 0, 1, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          SetTrayIcon("norm");
        }
        else
        {
          pctRural.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayRProgress(pctRural.Allocation.Size, r_used, r_lim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
          int u = (int)Math.Round(((double)r_used / r_lim) * trayRes);
          if (u > trayRes)
            u = trayRes;
          SetTrayIcon("graph_r_" + u);
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
        if (TrayStyle > 0)
        {
          bool isMinimized = ((e.Event.NewWindowState & Gdk.WindowState.Iconified) == Gdk.WindowState.Iconified);
          if (isMinimized)
          {
            ((Gtk.Label)mnuRestore.Child).TextWithMnemonic = "_Restore";
            this.SkipTaskbarHint = true;
          }
          else
          {
            ((Gtk.Label)mnuRestore.Child).TextWithMnemonic = "_Hide";
            this.SkipTaskbarHint = false;
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
        if (TrayStyle > 0)
        {
          bool isWithdrawn = ((e.Event.NewWindowState & Gdk.WindowState.Withdrawn) == Gdk.WindowState.Withdrawn);
          if (isWithdrawn)
          {
            ((Gtk.Label)mnuRestore.Child).TextWithMnemonic = "_Restore";
            this.SkipTaskbarHint = true;
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
      ClosingTime = true;
      if (tmrUpdate != 0)
      {
        GLib.Source.Remove(tmrUpdate);
        tmrUpdate = 0;
      }
      if (mySettings != null)
      { 
        mySettings.Save();
      }
      modDB.LOG_Terminate(false);
      if (TrayStyle == 2)
      {
        try
        {
          ((AppIndicator.ApplicationIndicator)trayIcon).Status = AppIndicator.Status.Passive;
        }
        catch (Exception)
        {
          TrayStyle = 1;
        }
      }
      if (TrayStyle == 1)
        ((StatusIcon)trayIcon).Visible = false;
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
      System.Net.ServicePointManager.SecurityProtocol = mySettings.Protocol;
      if (mySettings.Colors.MainDownA == Color.Transparent)
      {
        SetDefaultColors();
      }
      modFunctions.NOTIFIER_STYLE = modFunctions.LoadAlertStyle(mySettings.AlertStyle);
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
      if (!String.IsNullOrEmpty(sAccount))
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
        SetStatusText("Analyzing Account", "Determining your account type...", false);
        TypeDetermination = new DetermineType(sProvider, mySettings.Timeout, mySettings.Proxy);
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
      if (!String.IsNullOrEmpty(mySettings.PassCrypt))
      {
        sPassword = StoredPassword.DecryptApp(mySettings.PassCrypt);
      }
      if (!String.IsNullOrEmpty(sAccount))
      {
        if (sAccount.Contains("@") && sAccount.Contains("."))
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
    private bool tmrUpdate_Tick()
    {
      if (NextGrabTick != long.MaxValue)
      {
        long msInterval = mySettings.Interval * 60 * 1000;
        if (NextGrabTick == long.MinValue)
        {
          long minutesSinceLast = modFunctions.DateDiff(modFunctions.DateInterval.Minute, modDB.LOG_GetLast(), DateTime.Now);
          if (minutesSinceLast < mySettings.Interval)
          {
            long msSinceLast = minutesSinceLast * 60 * 1000;
            NextGrabTick = modFunctions.TickCount() + (msInterval - msSinceLast) + (2 * 60 * 1000);
          }
          else
          {
            NextGrabTick = modFunctions.TickCount();
          }
          if (NextGrabTick - modFunctions.TickCount() < mySettings.StartWait * 60 * 1000)
          {
            NextGrabTick = modFunctions.TickCount() + (mySettings.StartWait * 60 * 1000);
          }
        }
        if (modFunctions.TickCount() >= NextGrabTick)
        {
          if ((mySettings.UpdateType != AppSettings.UpdateTypes.None) && (Math.Abs(DateTime.Now.Subtract(mySettings.LastUpdate).TotalDays) > mySettings.UpdateTime))
            CheckForUpdates();
          else
          {
            if (!String.IsNullOrEmpty(sAccount))
            {
              if (String.IsNullOrEmpty(sProvider))
              {
                sProvider = sAccount.Substring(sAccount.LastIndexOf("@") + 1).ToLower();
                SetStatusText("Reloading", "Reloading History...", false);
                modDB.LOG_Initialize(sAccount, false);
                if (ClosingTime)
                {
                  return true;
                }
              }
              if (Math.Abs(modFunctions.DateDiff(modFunctions.DateInterval.Minute, modDB.LOG_GetLast(), DateTime.Now)) > 10)
              {
                if (!string.IsNullOrEmpty(sProvider) && !string.IsNullOrEmpty(sPassword))
                {
                  updateFull = false;
                  NextGrabTick = long.MaxValue;
                  EnableProgressIcon();
                  SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing Connection...", false);
                  MethodInvoker UsageInvoker = GetUsage;
                  UsageInvoker.BeginInvoke(null, null);
                  return true;
                }
              }
            }
            NextGrabTick = modFunctions.TickCount() + msInterval;
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
      if (String.IsNullOrEmpty(sAccount) | String.IsNullOrEmpty(sPassword) | !sAccount.Contains("@"))
      {
        if (((Gtk.Label)mnuRestore.Child).Text == "Restore")
        {
          ShowFromTray();
        }
        cmdConfig.GrabFocus();
        modFunctions.ShowMessageBox(null, "Please enter your account details in the configuration window.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok);
      }
      else
      {
        if (cmdRefresh.Sensitive)
        {
          cmdRefresh.Sensitive = false;
          NextGrabTick = modFunctions.TickCount() + (mySettings.Timeout * 1000);
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
            localData = new localRestrictionTracker(modFunctions.AppData);
            localDataEvent(true);
            MethodInvoker localInvoker = localData.Connect;
            localInvoker.BeginInvoke(null, null);
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
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing Connection...", false);
          DisplayUsage(false, false);
          NextGrabTick = modFunctions.TickCount() + 5000;
        }
      }
    }
    private bool KeyCheck(string TestKey)
    {
      if (String.IsNullOrEmpty(TestKey.Trim()))
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
      :EventArgs
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
        NextGrabTick = modFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
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
        NextGrabTick = modFunctions.TickCount() + (MinutesAhead * 60 * 1000);
      }
      else
      {
        NextGrabTick = modFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
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
      NextGrabTick = modFunctions.TickCount() + (mySettings.Timeout * 1000);
      switch (e.Status)
      {
        case localRestrictionTracker.ConnectionStates.Initialize:
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Initializing Connection...", false);
          break;
        case localRestrictionTracker.ConnectionStates.Prepare:
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparing to Log In...", false);
          break;
        case localRestrictionTracker.ConnectionStates.Login:
          switch (e.SubState)
          {
            case localRestrictionTracker.ConnectionSubStates.ReadLogin:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Reading Login Page...", false);
              break;
            case localRestrictionTracker.ConnectionSubStates.AuthPrepare:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Preparinng Authentication...", false);
              break;
            case localRestrictionTracker.ConnectionSubStates.Authenticate:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Authenticating...", false);
              break;
            case localRestrictionTracker.ConnectionSubStates.AuthenticateRetry:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Re-Authenticating...", false);
              break;
            case localRestrictionTracker.ConnectionSubStates.Verify:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Verifying Authentication...", false);
              break;
            default:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Logging In...", false);
              break;
          }
          break;
        case localRestrictionTracker.ConnectionStates.TableDownload:
          switch (e.SubState)
          {
            case localRestrictionTracker.ConnectionSubStates.LoadHome:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Downloading Home Page...", false);
              break;
            case localRestrictionTracker.ConnectionSubStates.LoadAJAX:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Downloading AJAX Data (" + modFunctions.FormatPercent((double)e.SubPercentage, 0) + ")...", false);
              break;
            case localRestrictionTracker.ConnectionSubStates.LoadTable:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Downloading Usage Table...", false);
              break;
            case localRestrictionTracker.ConnectionSubStates.LoadTableRetry:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Re-Downloading Usage Table...", false);
              break;
            default:
              SetStatusText(modDB.LOG_GetLast().ToString("g"), "Downloading Usage Table...", false);
              break;
          }
          break;
        case localRestrictionTracker.ConnectionStates.TableRead:
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Reading Usage Table...", false);
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
          SetStatusText(modDB.LOG_GetLast().ToString("g"), e.Message, true);
          return;
        case localRestrictionTracker.ConnectionFailureEventArgs.FailureType.ConnectionTimeout:
          SetStatusText(modDB.LOG_GetLast().ToString("g"), "Connection Timed Out!", true);
          DisplayUsage(false, false);
          break;
        case localRestrictionTracker.ConnectionFailureEventArgs.FailureType.LoginFailure:
          SetStatusText(modDB.LOG_GetLast().ToString("g"), e.Message, true);
          if (!string.IsNullOrEmpty(e.Fail))
          {
            FailFile(e.Fail);
          }
          DisplayUsage(false, true);
          break;
        case localRestrictionTracker.ConnectionFailureEventArgs.FailureType.FatalLoginFailure:
          mySettings.AccountType = localRestrictionTracker.SatHostTypes.Other;
          SetStatusText(modDB.LOG_GetLast().ToString("g"), e.Message, true);
          if (!string.IsNullOrEmpty(e.Fail))
          {
            FailFile(e.Fail);
          }
          DisplayUsage(false, false);
          break;
        case localRestrictionTracker.ConnectionFailureEventArgs.FailureType.UnknownAccountDetails:
          if ((this.GdkWindow.State & Gdk.WindowState.Iconified) != 0)
          {
            ShowFromTray();
          }
          cmdConfig.GrabFocus();
          modFunctions.ShowMessageBox(null, "Please enter your account details in the configuration window.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok);
          break;
        case localRestrictionTracker.ConnectionFailureEventArgs.FailureType.UnknownAccountType:
          SetStatusText("Analyzing Account", "Determining your account type...", false);
          TypeDetermination = new DetermineType(sProvider, mySettings.Timeout, mySettings.Proxy);
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
      SetStatusText(e.Update.ToString("g"), "Saving History...", false);
      NextGrabTick = modFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      modDB.LOG_Add(e.Update, e.AnyTime, e.AnyTimeLimit, e.OffPeak, e.OffPeakLimit, true);
      myPanel = localRestrictionTracker.SatHostTypes.DishNet_EXEDE;
      mySettings.AccountType = localRestrictionTracker.SatHostTypes.DishNet_EXEDE;
      mySettings.Save();
      if (mySettings.Colors.MainDownA == Color.Transparent)
      {
        SetDefaultColors();
      }
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
      SetStatusText(e.Update.ToString("g"), "Saving History...", false);
      NextGrabTick = modFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      modDB.LOG_Add(e.Update, e.Used, e.Limit, e.Used, e.Limit, true);
      myPanel = localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE;
      mySettings.AccountType = localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE;
      mySettings.Save();
      if (mySettings.Colors.MainDownA == Color.Transparent)
      {
        SetDefaultColors();
      }
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
      SetStatusText(e.Update.ToString("g"), "Saving History...", false);
      NextGrabTick = modFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      modDB.LOG_Add(e.Update, e.Download, e.DownloadLimit, e.Upload, e.UploadLimit, true);
      myPanel = localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY;
      mySettings.AccountType = localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY;
      mySettings.Save();
      if (mySettings.Colors.MainDownA == Color.Transparent)
      {
        SetDefaultColors();
      }
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
      SetStatusText(e.Update.ToString("g"), "Saving History...", false);
      NextGrabTick = modFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      modDB.LOG_Add(e.Update, e.Download, e.DownloadLimit, e.Upload, e.UploadLimit, true);
      myPanel = localRestrictionTracker.SatHostTypes.WildBlue_LEGACY;
      mySettings.AccountType = localRestrictionTracker.SatHostTypes.WildBlue_LEGACY;
      mySettings.Save();
      if (mySettings.Colors.MainDownA == Color.Transparent)
      {
        SetDefaultColors();
      }
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
      SetStatusText(e.Update.ToString("g"), "Saving History...", false);
      NextGrabTick = modFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
      modDB.LOG_Add(e.Update, e.Used, e.Limit, e.Used, e.Limit, true);
      myPanel = localRestrictionTracker.SatHostTypes.WildBlue_EXEDE;
      mySettings.AccountType = localRestrictionTracker.SatHostTypes.WildBlue_EXEDE;
      mySettings.Save();
      if (mySettings.Colors.MainDownA == Color.Transparent)
      {
        SetDefaultColors();
      }
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
        if (wsHostList != null)
        {
          wsHostList.Dispose();
          wsHostList = null;
        }
        string myProvider = mySettings.Account.Substring(mySettings.Account.LastIndexOf("@") + 1).ToLower();
        wsHostList = new CookieAwareWebClient();
        wsHostList.DownloadDataAsync(new Uri("http://wb.realityripple.com/hosts/?add=" + myProvider), "UPDATE");
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
          sErr = "Network Connection Error" + (string.IsNullOrEmpty(e.Details) ? "." : ": (" + e.Details + ")");
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
      NextGrabTick = modFunctions.TickCount() + (mySettings.Timeout * 1000);
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
      NextGrabTick = modFunctions.TickCount() + (mySettings.Interval * 60 * 1000);
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
        if (mySettings.Colors.MainDownA == Color.Transparent)
        {
          SetDefaultColors();
        }
        mySettings.Save();
        int iPercent = 0;
        int iInterval = 1;
        long iStart = modFunctions.TickCount();
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
                long iDur = modFunctions.TickCount() - iStart;
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
            if (modFunctions.DateDiff(modFunctions.DateInterval.Minute, modDB.LOG_GetLast(), Row.Time) > 1)
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
      string state = (String)o;
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
        case "RURAL":
          long rUsed = r_used;
          long rLim = r_lim;
          long rRemain = rLim - rUsed;
          if (rUsed != 0 | rLim > 0 | rRemain != 0)
          {
            DoChange(ref lblRuralUsedVal, ref rUsed, !(rRemain > 0));
            DoChange(ref lblRuralRemainVal, ref rRemain, false);
            DoChange(ref lblRuralAllowedVal, ref rLim, false);
          }
          ResizePanels();
          if (rUsed == 0 & rLim == 0 & rRemain == 0)
          {
            return;
          }
          break;
        case "WB":
          long wDown = wb_down;
          long wDLim = wb_dlim;
          long wUp = wb_up;
          long wULim = wb_ulim;
          long wDFree = wb_dlim - wb_down;
          long wUFree = wb_ulim - wb_up;
          if (wDown > 0 | wDFree != 0 | wDLim > 0 | wUp > 0 | wUFree != 0 | wULim > 0)
          {
            DoChange(ref lblDldUsed, ref wDown, (wDown >= wDLim));
            DoChange(ref lblDldFree, ref wDFree, (wDFree <= 0));
            DoChange(ref lblDldLimit, ref  wDLim, false);
            DoChange(ref lblUldUsed, ref wUp, (wUp >= wULim));
            DoChange(ref lblUldFree, ref wUFree, (wUFree <= 0));
            DoChange(ref lblUldLimit, ref wULim, false);
          }
          ResizePanels();
          if (wDown == 0 & wDFree == 0 & wDLim == 0 & wUp == 0 & wUFree == 0 & wULim == 0)
          {
            this.SizeAllocate(new Gdk.Rectangle(Gdk.Point.Zero, mySettings.MainSize));
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
    private void DisplayRResults(long lDown, long lDownLim, long lUp, long lUpLim, string sLastUpdate)
    {
      if (lDown != lUp && lDownLim != lUpLim)
      {
        DisplayWResults(lDown, lDownLim, lUp, lUpLim, sLastUpdate);
        return;
      }
      string sTTT = this.Title;
      if (lDown >= lDownLim)
      {
        imSlowed = true;
      }
      if (lDown < lDownLim * 0.7)
      {
        imSlowed = false;
      }
      if (pnlWildBlue.Visible)
      {
        pnlWildBlue.Visible = false;
        pnlDisplays.Remove(pnlWildBlue);
      }
      if (pnlExede.Visible)
      {
        pnlExede.Visible = false;
        pnlDisplays.Remove(pnlExede);
      }
      if (!pnlRural.Visible)
      {
        pnlRural.Visible = true;
        pnlDisplays.Add(pnlRural);
      }
      if (pnlNothing.Visible)
      {
        pnlNothing.Visible = false;
        pnlDisplays.Remove(pnlNothing);
      }
      r_used = lDown;
      r_lim = lDownLim;
      if (tmrChanges != null)
      {
        tmrChanges.Dispose();
        tmrChanges = null;
      }
      tmrChanges = new System.Threading.Timer(DisplayChangeInterval, (object)"RURAL", 75, System.Threading.Timeout.Infinite);
      pctRural.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayRProgress(pctRural.Allocation.Size, lDown, lDownLim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
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
      if (tmrIcon != 0)
      {
        GLib.Source.Remove(tmrIcon);
        tmrIcon = 0;
      }
      if (trayRes < 8)
        trayRes = 8;
      int u = (int)Math.Round(((double)lDown / lDownLim) * trayRes);
      if (u > trayRes)
        u = trayRes;
      SetTrayIcon("graph_r_" + u);
      SetTrayText(sTTT);
      if (mySettings.Overuse > 0)
      {
        if (lastBalloon > 0 && modFunctions.TickCount() - lastBalloon < mySettings.Overtime * 60 * 1000)
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
              ulong ChangeTime = (ulong)Math.Abs(modFunctions.DateDiff(modFunctions.DateInterval.Minute, lItems[I].DATETIME, DateTime.Now) * 60 * 1000);
              modFunctions.MakeNotifier(ref taskNotifier, false);
              if (taskNotifier != null)
              {
                taskNotifierEvent(true);
                taskNotifier.Show("Excessive Usage Detected", modFunctions.ProductName + " has logged a usage change of " + MBorGB(ChangeSize) + " in " + modFunctions.ConvertTime(ChangeTime, false, true) + "!", 200, 0, 100);
              }
              lastBalloon = modFunctions.TickCount();
              break;
            }
          }
        }
      }
    }
    private void DisplayDResults(long lDown, long lDownLim, long lUp, long lUpLim, string sLastUpdate)
    {
      string sTTT = this.Title;
      if (lDown >= lDownLim)
      {
        imSlowed = true;
      }
      if (lDown < lDownLim * 0.7)
      {
        imSlowed = false;
      }
      if (!pnlWildBlue.Visible)
      {
        pnlWildBlue.Visible = true;
        pnlDisplays.Add(pnlWildBlue);
      }
      if (pnlExede.Visible)
      {
        pnlExede.Visible = false;
        pnlDisplays.Remove(pnlExede);
      }
      if (pnlRural.Visible)
      {
        pnlRural.Visible = false;
        pnlDisplays.Remove(pnlRural);
      }
      if (pnlNothing.Visible)
      {
        pnlNothing.Visible = false;
        pnlDisplays.Remove(pnlNothing);
      }

      wb_down = lDown;
      wb_dlim = lDownLim;
      wb_up = lUp;
      wb_ulim = lUpLim;
      if (tmrChanges != null)
      {
        tmrChanges.Dispose();
        tmrChanges = null;
      }
      tmrChanges = new System.Threading.Timer(DisplayChangeInterval, (object)"WB", 75, System.Threading.Timeout.Infinite);
      pctDld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(pctDld.Allocation.Size, lDown, lDownLim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
      pctUld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(pctUld.Allocation.Size, lUp, lUpLim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
      lblDld.Text = "Anytime (" + AccuratePercent((double)lDown / lDownLim) + ")";
      lblUld.Text = "Off-Peak (" + AccuratePercent((double)lUp / lUpLim) + ")";
      SetFontSize(ref lblDld, GetFontSize());
      SetFontSize(ref lblUld, GetFontSize());
      pctDld.TooltipText = "Graph representing your Anytime usage.";
      pctUld.TooltipText = "Graph representing your Off-Peak usage (used between 2 AM and 8 AM).";
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
      if (tmrIcon != 0)
      {
        GLib.Source.Remove(tmrIcon);
        tmrIcon = 0;
      }
      if (trayRes < 8)
        trayRes = 8;
      int d = (int)Math.Round(((double)lDown / lDownLim) * trayRes);
      int u = (int)Math.Round(((double)lUp / lUpLim) * trayRes);
      SetTrayIcon("graph_wb_" + d + "x" + u);
      SetTrayText(sTTT);
      if (mySettings.Overuse > 0)
      {
        if (lastBalloon > 0 && modFunctions.TickCount() - lastBalloon < mySettings.Overtime * 60 * 1000)
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
              ulong ChangeTime = (ulong)Math.Abs(modFunctions.DateDiff(modFunctions.DateInterval.Minute, lItems[I].DATETIME, DateTime.Now) * 60 * 1000);
              modFunctions.MakeNotifier(ref taskNotifier, false);
              if (taskNotifier != null)
              {
                taskNotifierEvent(true);
                taskNotifier.Show("Excessive Usage Detected", modFunctions.ProductName + " has logged a usage change of " + MBorGB(ChangeSize) + " in " + modFunctions.ConvertTime(ChangeTime, false, true) + "!", 200, 0, 100);
              }
              lastBalloon = modFunctions.TickCount();
              break;
            }
            else if (lUp - lItems[I].UPLOAD >= mySettings.Overuse)
            {
              long ChangeSize = Math.Abs(lDown - lItems[I].DOWNLOAD);
              ulong ChangeTime = (ulong)Math.Abs(modFunctions.DateDiff(modFunctions.DateInterval.Minute, lItems[I].DATETIME, DateTime.Now) * 60 * 1000);
              modFunctions.MakeNotifier(ref taskNotifier, false);
              if (taskNotifier != null)
              {
                taskNotifierEvent(true);
                taskNotifier.Show("Excessive Off-Peak Usage Detected", modFunctions.ProductName + " has logged an Off-Peak usage change of " + MBorGB(ChangeSize) + " in " + modFunctions.ConvertTime(ChangeTime, false, true) + "!", 200, 0, 100);
              }
              lastBalloon = modFunctions.TickCount();
              break;
            }
          }
        }
      }
    }
    private void DisplayWResults(long lDown, long lDownLim, long lUp, long lUpLim, string sLastUpdate)
    {
      string sTTT = this.Title;
      if ((lDown >= lDownLim) | (lUp >= lUpLim))
      {
        imSlowed = true;
      }
      if ((lDown < lDownLim * 0.7) & (lUp < lUpLim * 0.7))
      {
        imSlowed = false;
      }
      if (!pnlWildBlue.Visible)
      {
        pnlWildBlue.Visible = true;
        pnlDisplays.Add(pnlWildBlue);
      }
      if (pnlExede.Visible)
      {
        pnlExede.Visible = false;
        pnlDisplays.Remove(pnlExede);
      }
      if (pnlRural.Visible)
      {
        pnlRural.Visible = false;
        pnlDisplays.Remove(pnlRural);
      }
      if (pnlNothing.Visible)
      {
        pnlNothing.Visible = false;
        pnlDisplays.Remove(pnlNothing);
      }

      wb_down = lDown;
      wb_dlim = lDownLim;
      wb_up = lUp;
      wb_ulim = lUpLim;
      if (tmrChanges != null)
      {
        tmrChanges.Dispose();
        tmrChanges = null;
      }
      tmrChanges = new System.Threading.Timer(DisplayChangeInterval, (object)"WB", 75, System.Threading.Timeout.Infinite);
      pctDld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(pctDld.Allocation.Size, lDown, lDownLim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
      pctUld.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayProgress(pctUld.Allocation.Size, lUp, lUpLim, mySettings.Accuracy, mySettings.Colors.MainDownA, mySettings.Colors.MainDownB, mySettings.Colors.MainDownC, mySettings.Colors.MainText, mySettings.Colors.MainBackground));
      lblDld.Text = "Download (" + AccuratePercent((double)lDown / lDownLim) + ")";
      lblUld.Text = "Upload (" + AccuratePercent((double)lUp / lUpLim) + ")";
      SetFontSize(ref lblDld, GetFontSize());
      SetFontSize(ref lblUld, GetFontSize());
      pctDld.TooltipText = "Graph representing your download usage.";
      pctUld.TooltipText = "Graph representing your upload usage.";
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
      if (tmrIcon != 0)
      {
        GLib.Source.Remove(tmrIcon);
        tmrIcon = 0;
      }
      if (trayRes < 8)
        trayRes = 8;
      int d = (int)Math.Round(((double)lDown / lDownLim) * trayRes);
      int u = (int)Math.Round(((double)lUp / lUpLim) * trayRes);
      SetTrayIcon("graph_wb_" + d + "x" + u);
      SetTrayText(sTTT);
      if (mySettings.Overuse > 0)
      {
        if (lastBalloon > 0 && modFunctions.TickCount() - lastBalloon < mySettings.Overtime * 60 * 1000)
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
              ulong ChangeTime = (ulong)Math.Abs(modFunctions.DateDiff(modFunctions.DateInterval.Minute, lItems[I].DATETIME, DateTime.Now) * 60 * 1000);
              modFunctions.MakeNotifier(ref taskNotifier, false);
              if (taskNotifier != null)
              {
                taskNotifierEvent(true);
                taskNotifier.Show("Excessive Download Detected", modFunctions.ProductName + " has logged a download of " + MBorGB(ChangeSize) + " in " + modFunctions.ConvertTime(ChangeTime, false, true) + "!", 200, 0, 100);
              }
              lastBalloon = modFunctions.TickCount();
              break;
            }
            else if (lUp - lItems[I].UPLOAD >= mySettings.Overuse)
            {
              long ChangeSize = Math.Abs(lDown - lItems[I].DOWNLOAD);
              ulong ChangeTime = (ulong)Math.Abs(modFunctions.DateDiff(modFunctions.DateInterval.Minute, lItems[I].DATETIME, DateTime.Now) * 60 * 1000);
              modFunctions.MakeNotifier(ref taskNotifier, false);
              if (taskNotifier != null)
              {
                taskNotifierEvent(true);
                taskNotifier.Show("Excessive Upload Detected", modFunctions.ProductName + " has logged an upload usage change of " + MBorGB(ChangeSize) + " in " + modFunctions.ConvertTime(ChangeTime, false, true) + "!", 200, 0, 100);
              }
              lastBalloon = modFunctions.TickCount();
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
            DisplayRResults(lDown, lDownLim, lUp, lUpLim, sLastUpdate);
            break;
          case localRestrictionTracker.SatHostTypes.DishNet_EXEDE:
            DisplayDResults(lDown, lDownLim, lUp, lUpLim, sLastUpdate);
            break;
          case localRestrictionTracker.SatHostTypes.WildBlue_LEGACY:
          case localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY:
            DisplayWResults(lDown, lDownLim, lUp, lUpLim, sLastUpdate);
            break;
          default:
            DisplayWResults(lDown, lDownLim, lUp, lUpLim, sLastUpdate);
            break;
        }
      }
      else
      {
        if (pnlWildBlue.Visible)
        {
          pnlWildBlue.Visible = false;
          pnlDisplays.Remove(pnlWildBlue);
        }
        if (pnlExede.Visible)
        {
          pnlExede.Visible = false;
          pnlDisplays.Remove(pnlExede);
        }
        if (pnlRural.Visible)
        {
          pnlRural.Visible = false;
          pnlDisplays.Remove(pnlRural);
        }
        if (!pnlNothing.Visible)
        {
          pnlNothing.Visible = true;
          pnlDisplays.Add(pnlNothing);
        }

        myPanel = localRestrictionTracker.SatHostTypes.Other;
        SetTrayText(this.Title);
        if (tmrIcon != 0)
        {
          GLib.Source.Remove(tmrIcon);
          tmrIcon = 0;
        }
        SetTrayIcon("norm");
      }
    }
    #endregion
    #endregion
    #region "Buttons"
    [GLib.ConnectBefore]
    protected void cmdRefresh_Click(object sender, ButtonReleaseEventArgs e)
    {
      InitAccount();
      if ((!String.IsNullOrEmpty(sAccount)) & (!String.IsNullOrEmpty(sProvider)) & (!String.IsNullOrEmpty(sPassword)))
      {
        EnableProgressIcon();
        SetNextLoginTime();
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
        SetStatusText(modDB.LOG_GetLast().ToString("g"), "Reading Usage...", false);
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
        modFunctions.ShowMessageBox(null, "Please enter your account details in the configuration window.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok);
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
      long waitTime = modFunctions.TickCount() + 2000;
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
          if (modFunctions.TickCount() > waitTime)
            break;
        }
      }
      switch (dRet)
      {
        case ResponseType.Yes:
          didHostListSave = false;
          mySettings = null;
          mySettings = new AppSettings();
          System.Net.ServicePointManager.SecurityProtocol = mySettings.Protocol;
          if (mySettings.Colors.MainDownA == Color.Transparent)
          {
            SetDefaultColors();
          }
          modFunctions.NOTIFIER_STYLE = modFunctions.LoadAlertStyle(mySettings.AlertStyle);
          SetNextLoginTime();
          MakeCustomIconListing();
          MethodInvoker ReInitInvoker = ReInit;
          ReInitInvoker.BeginInvoke(null, null);
          break;
        case ResponseType.Ok:
          didHostListSave = false;
          mySettings = null;
          mySettings = new AppSettings();
          System.Net.ServicePointManager.SecurityProtocol = mySettings.Protocol;
          if (mySettings.Colors.MainDownA == Color.Transparent)
          {
            SetDefaultColors();
          }
          modFunctions.NOTIFIER_STYLE = modFunctions.LoadAlertStyle(mySettings.AlertStyle);
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
      cmdRefresh.Click();
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
      if (!String.IsNullOrEmpty(sTitle))
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
      if (!String.IsNullOrEmpty(sSubtitle))
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
      if (!String.IsNullOrEmpty(Subtitle))
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
    int iIconItem;
    private bool tmrIcon_Tick()
    {
      switch (iIconItem)
      {
        case 0:
        case 6:
        case 11:
          SetTrayIcon("norm");
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
    }
    public void HideToTray()
    {
      this.Iconify();
    }
    private void SetTrayIcon(string resource)
    {
      if (TrayStyle == 2)
      {
        try
        {
          ((AppIndicator.ApplicationIndicator)trayIcon).IconName = resource;
        }
        catch (Exception)
        {
          TrayStyle = 1;
        }
      }
      if (TrayStyle == 1)
        ((StatusIcon)trayIcon).File = System.IO.Path.Combine(IconFolder, resource + ".png");
    }
    private void SetTrayText(string tooltip)
    {
      if (TrayStyle == 2)
      {
        try
        {
          ((AppIndicator.ApplicationIndicator)trayIcon).Title = tooltip;
        }
        catch (Exception)
        {
          TrayStyle = 1;
        }
      }
      if (TrayStyle == 1)
      {
        ((StatusIcon)trayIcon).Tooltip = tooltip;
        ((StatusIcon)trayIcon).Blinking = false;
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
    private Gdk.Pixbuf CreateTrayIcon(long lDown, long lDownLim, long lUp, long lUpLim)
    {
      if (trayRes < 8)
        trayRes = 8;
      Bitmap imgTray = new Bitmap(trayRes, trayRes);
      Graphics g = Graphics.FromImage(imgTray);

      g.Clear(Color.Transparent);
      if (imSlowed)
      {
        string restricted = "Resources.tray_16.restricted.ico";
        if (trayRes > 16)
        {
          restricted = "Resources.tray_32.restricted.ico";
        }
        g.DrawIcon(new System.Drawing.Icon(GetType(), restricted), new Rectangle(0, 0, trayRes, trayRes));
        modFunctions.CreateTrayIcon_Left(ref g, lDown, lDownLim, mySettings.Colors.TrayDownA, mySettings.Colors.TrayDownB, mySettings.Colors.TrayDownC, trayRes);
        modFunctions.CreateTrayIcon_Right(ref g, lUp, lUpLim, mySettings.Colors.TrayUpA, mySettings.Colors.TrayUpB, mySettings.Colors.TrayUpC, trayRes);
      }
      else if (imFree)
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
    private Gdk.Pixbuf CreateRTrayIcon(long lUsed, long lLim)
    {
      if (trayRes < 8)
        trayRes = 8;
      Bitmap imgTray = new Bitmap(trayRes, trayRes);
      Graphics g = Graphics.FromImage(imgTray);
      g.Clear(Color.Transparent);
      if (imSlowed)
      {
        string restricted = "Resources.tray_16.restricted.ico";
        if (trayRes > 16)
        {
          restricted = "Resources.tray_32.restricted.ico";
        }
        g.DrawIcon(new System.Drawing.Icon(GetType(), restricted), new Rectangle(0, 0, trayRes, trayRes));
        modFunctions.CreateTrayIcon_Left(ref g, lUsed, lLim, mySettings.Colors.TrayDownA, mySettings.Colors.TrayDownB, mySettings.Colors.TrayDownC, trayRes);
        modFunctions.CreateTrayIcon_Right(ref g, lUsed, lLim, mySettings.Colors.TrayDownA, mySettings.Colors.TrayDownB, mySettings.Colors.TrayDownC, trayRes);
      }
      else if (imFree)
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
      NextGrabTick = modFunctions.TickCount() + (mySettings.Timeout * 1000);
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
      clsUpdate.CheckEventArgs e = (clsUpdate.CheckEventArgs) ea;
      mySettings.LastUpdate = DateTime.Now;
      mySettings.Save();
      if (e.Error == null && !e.Cancelled)
      {
        if (mySettings.UpdateType == AppSettings.UpdateTypes.Ask)
        {
          dlgUpdate fUpdate;
          switch (e.Result)
          {
            case clsUpdate.CheckEventArgs.ResultType.NewUpdate:
              fUpdate = new dlgUpdate();
              fUpdate.NewUpdate(e.Version, false);
              switch ((Gtk.ResponseType)fUpdate.Run())
              {
                case ResponseType.Yes:
                  if (remoteData != null)
                  {
                    remoteData.Dispose();
                    remoteData = null;
                  }
                  if (localData != null)
                  {
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
                    remoteData.Dispose();
                    remoteData = null;
                  }
                  if (localData != null)
                  {
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
            case clsUpdate.CheckEventArgs.ResultType.NewBeta:
              if (mySettings.UpdateBETA)
              {
                fUpdate = new dlgUpdate();
                fUpdate.NewUpdate(e.Version, true);
                switch ((Gtk.ResponseType)fUpdate.Run())
                {
                  case ResponseType.Yes:
                    if (remoteData != null)
                    {
                      remoteData.Dispose();
                      remoteData = null;
                    }
                    if (localData != null)
                    {
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
                      remoteData.Dispose();
                      remoteData = null;
                    }
                    if (localData != null)
                    {
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
            case clsUpdate.CheckEventArgs.ResultType.NewUpdate:
              if (remoteData != null)
              {
                remoteData.Dispose();
                remoteData = null;
              }
              if (localData != null)
              {
                localData.Dispose();
                localData = null;
              }
              updateChecker.DownloadUpdate(sUpdatePath);
              break;
            case clsUpdate.CheckEventArgs.ResultType.NewBeta:
              if (mySettings.UpdateBETA)
              {
                if (remoteData != null)
                {
                  remoteData.Dispose();
                  remoteData = null;
                }
                if (localData != null)
                {
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
      tmrSpeed = GLib.Timeout.Add(1000, tmrUpdate_Tick);
      SetStatusText(modDB.LOG_GetLast().ToString("g"), "Downloading Software Update...", false);
    }
    private void updateChecker_DownloadResult(object sender, clsUpdate.DownloadEventArgs e)
    {
      EventArgs ea = (EventArgs)e;
      Gtk.Application.Invoke(sender, ea, Main_UpdateCheckerDownloadResult);
    }
    private void Main_UpdateCheckerDownloadResult(object sender, EventArgs ea)
    {
      clsUpdate.DownloadEventArgs e = (clsUpdate.DownloadEventArgs) ea;
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
      clsUpdate.ProgressEventArgs e = (clsUpdate.ProgressEventArgs) ea;
      upCurSize = e.BytesReceived;
      upTotalSize = e.TotalBytesToReceive;
      upCurPercent = (uint) e.ProgressPercentage;
    }
    long upLastSize;
    private bool tmrSpeed_Tick()
    {
      if (upCurSize > upLastSize)
        upDownSpeed = (ulong) (upCurSize - upLastSize);
      else
        upDownSpeed = 0;
      upLastSize = upCurSize;
      string sProgress = "(" + upCurPercent + "%)";
      string sStatus = modFunctions.ByteSize((ulong) upCurSize) + " of " + modFunctions.ByteSize((ulong) upTotalSize) + " at " + modFunctions.ByteSize(upDownSpeed) + "/s...";
      if (upTotalSize == 0)
      {
        sStatus = "Downloading Update (Waiting for Response)...";
        sProgress = "(Waiting for Response)";
      }
      SetStatusText("Downloading Update " + sProgress, sStatus, false);
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
      if (String.IsNullOrEmpty(str))
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
    public void FailResponse(bool Ret)
    {
      string sRet = "F";
      if (Ret)
        sRet = "T";
      ResolveEventArgs ea = new ResolveEventArgs(sRet);
      Gtk.Application.Invoke(null, (EventArgs)ea, Main_FailResponse);
    }
    private void Main_FailResponse(object sender, EventArgs ea)
    {
      ResolveEventArgs e = (ResolveEventArgs)ea;
      bool ret = (e.Name == "T");
      modFunctions.MakeNotifier(ref taskNotifier, false);
      if (taskNotifier != null)
      {
        taskNotifierEvent(true);
        if (ret)
          taskNotifier.Show("Error Report Sent", "Your report has been received by " + modFunctions.CompanyName + ".\nThank you for helping to improve " + modFunctions.ProductName + "!", 200, 15 * 1000, 100);
        else
          taskNotifier.Show("Error Reporting Error", modFunctions.ProductName + " was unable to contact the " + modFunctions.CompanyName + " servers. Please check your internet connection.", 200, 30 * 1000, 100);
      }
    }
    private void FailFile(string sFail)
    {
      if (clsUpdate.QuickCheckVersion() == clsUpdate.CheckEventArgs.ResultType.NoUpdate)
      {
        sFailTray = sFail;
        modFunctions.MakeNotifier(ref taskNotifier, true);
        if (taskNotifier != null)
        {
          taskNotifierEvent(true);
          taskNotifier.Show("Error Reading Page Data", modFunctions.ProductName + " encountered data it does not understand.\nClick this alert to report the problem to " + modFunctions.CompanyName + ".", 200, 3 * 60 * 1000, 100);
        }
      }
    }
    #endregion
    private class SetStatusTextEventArgs : EventArgs
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
        if (e.Alert || e.Details.StartsWith("Next update in "))
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
      if (String.IsNullOrEmpty(e.Details))
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
      long lNow = modFunctions.TickCount();
      if (lNext == long.MaxValue)
      {
        lblStatus.TooltipText = "Update Temporarily Paused";
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
    private void SetDefaultColors()
    {
      Gtk.Application.Invoke(null, null, Main_SetDefaultColors);
    }
    private void Main_SetDefaultColors(object o, EventArgs ea)
    {
      localRestrictionTracker.SatHostTypes useStyle = myPanel;
      if (useStyle == localRestrictionTracker.SatHostTypes.Other)
      {
        useStyle = mySettings.AccountType;
      }
      switch (useStyle)
      {
        case localRestrictionTracker.SatHostTypes.WildBlue_LEGACY:
        case localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY:
          mySettings.Colors.MainDownA = Color.DarkBlue;
          mySettings.Colors.MainDownB = Color.Blue;
          mySettings.Colors.MainDownC = Color.Aqua;
          mySettings.Colors.MainUpA = Color.DarkBlue;
          mySettings.Colors.MainUpB = Color.Blue;
          mySettings.Colors.MainUpC = Color.Aqua;
          mySettings.Colors.MainText = Color.White;
          mySettings.Colors.MainBackground = Color.Black;

          mySettings.Colors.TrayDownA = Color.DarkBlue;
          mySettings.Colors.TrayDownB = Color.Blue;
          mySettings.Colors.TrayDownC = Color.Aqua;
          mySettings.Colors.TrayUpA = Color.DarkBlue;
          mySettings.Colors.TrayUpB = Color.Blue;
          mySettings.Colors.TrayUpC = Color.Aqua;

          mySettings.Colors.HistoryDownA = Color.DarkBlue;
          mySettings.Colors.HistoryDownB = Color.Blue;
          mySettings.Colors.HistoryDownC = Color.Aqua;
          mySettings.Colors.HistoryDownMax = Color.Yellow;
          mySettings.Colors.HistoryUpA = Color.DarkBlue;
          mySettings.Colors.HistoryUpB = Color.Blue;
          mySettings.Colors.HistoryUpC = Color.Aqua;
          mySettings.Colors.HistoryUpMax = Color.Yellow;
          mySettings.Colors.HistoryText = Color.Black;
          mySettings.Colors.HistoryBackground = Color.White;
          break;
        case localRestrictionTracker.SatHostTypes.WildBlue_EXEDE:
        case localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE:
          mySettings.Colors.MainDownA = Color.DarkBlue;
          mySettings.Colors.MainDownB = Color.Blue;
          mySettings.Colors.MainDownC = Color.Aqua;
          mySettings.Colors.MainUpA = Color.Transparent;
          mySettings.Colors.MainUpB = Color.Transparent;
          mySettings.Colors.MainUpC = Color.Transparent;
          mySettings.Colors.MainText = Color.White;
          mySettings.Colors.MainBackground = Color.Black;

          mySettings.Colors.TrayDownA = Color.DarkBlue;
          mySettings.Colors.TrayDownB = Color.Blue;
          mySettings.Colors.TrayDownC = Color.Aqua;
          mySettings.Colors.TrayUpA = Color.Transparent;
          mySettings.Colors.TrayUpB = Color.Transparent;
          mySettings.Colors.TrayUpC = Color.Transparent;

          mySettings.Colors.HistoryDownA = Color.DarkBlue;
          mySettings.Colors.HistoryDownB = Color.Blue;
          mySettings.Colors.HistoryDownC = Color.Aqua;
          mySettings.Colors.HistoryDownMax = Color.Yellow;
          mySettings.Colors.HistoryUpA = Color.Transparent;
          mySettings.Colors.HistoryUpB = Color.Transparent;
          mySettings.Colors.HistoryUpC = Color.Transparent;
          mySettings.Colors.HistoryUpMax = Color.Transparent;
          mySettings.Colors.HistoryText = Color.Black;
          mySettings.Colors.HistoryBackground = Color.White;
          break;
        case localRestrictionTracker.SatHostTypes.DishNet_EXEDE:
          mySettings.Colors.MainDownA = Color.DarkBlue;
          mySettings.Colors.MainDownB = Color.Blue;
          mySettings.Colors.MainDownC = Color.Aqua;
          mySettings.Colors.MainUpA = Color.DarkBlue;
          mySettings.Colors.MainUpB = Color.Blue;
          mySettings.Colors.MainUpC = Color.Aqua;
          mySettings.Colors.MainText = Color.White;
          mySettings.Colors.MainBackground = Color.Black;

          mySettings.Colors.TrayDownA = Color.DarkBlue;
          mySettings.Colors.TrayDownB = Color.Blue;
          mySettings.Colors.TrayDownC = Color.Aqua;
          mySettings.Colors.TrayUpA = Color.DarkBlue;
          mySettings.Colors.TrayUpB = Color.Blue;
          mySettings.Colors.TrayUpC = Color.Aqua;

          mySettings.Colors.HistoryDownA = Color.DarkBlue;
          mySettings.Colors.HistoryDownB = Color.Blue;
          mySettings.Colors.HistoryDownC = Color.Aqua;
          mySettings.Colors.HistoryDownMax = Color.Yellow;
          mySettings.Colors.HistoryUpA = Color.DarkBlue;
          mySettings.Colors.HistoryUpB = Color.Blue;
          mySettings.Colors.HistoryUpC = Color.Aqua;
          mySettings.Colors.HistoryUpMax = Color.Yellow;
          mySettings.Colors.HistoryText = Color.Black;
          mySettings.Colors.HistoryBackground = Color.White;
          break;
        default:
          break;

      }
    }
    #endregion
  }
}