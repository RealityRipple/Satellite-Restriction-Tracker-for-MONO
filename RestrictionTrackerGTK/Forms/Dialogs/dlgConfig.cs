using System;
using System.IO;
using Mono.Unix.Native;
using Gdk;
using RestrictionLibrary;
using Gtk;
using System.Drawing;
using System.Security.Principal;
using System.Diagnostics;
using System.Security.Policy;
using System.Runtime.InteropServices;
namespace RestrictionTrackerGTK
{
  public partial class dlgConfig:
    Gtk.Dialog
  {
    private bool bSaved, bAccount, bLoaded, bHardChange;
    private AppSettings mySettings;
    private uint pIcoWait;
    private Gdk.Pixbuf icoNetTest;
    private delegate void MethodInvoker();
    #region "Form Events"
    public dlgConfig()
    {
      bLoaded = false;
      this.Build();
      if (CurrentOS.IsMac)
      {
        pctAccountViaSatIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.account_user.png");
        ((Gtk.Image)((Gtk.HBox)((Gtk.Alignment)cmdPassDisplay.Child).Child).Children[0]).Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.pass.png");
        pctPrefStartIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.prefs_power.png");
        pctPrefAccuracyIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.prefs_accuracy.png");
        pctPrefAlertIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.prefs_notify.png");
        pctPrefInterfaceIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.prefs_interface.png");
        pctPrefColorIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.prefs_colors.png");
        pctNetworkTimeoutIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.net_network.png");
        pctNetworkProxyIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.net_proxy.png");
        pctNetworkProtocolIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_security.png");
        pctNetworkUpdateIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.net_update.png");
        pctAdvancedDataIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_data.png");
        pctAdvancedNetTestIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_nettest_none.png");
        ((Gtk.Box.BoxChild)this.ActionArea[cmdSave]).Position = 1;
        ((Gtk.Box.BoxChild)this.ActionArea[cmdClose]).Position = 0;
      }
      else
      {
        ((Gtk.Box.BoxChild)this.ActionArea[cmdSave]).Position = 0;
        ((Gtk.Box.BoxChild)this.ActionArea[cmdClose]).Position = 1;
      }
      txtStartWait.Alignment = 1;
      txtInterval.Alignment = 1;
      txtAccuracy.Alignment = 1;
      txtOverSize.Alignment = 1;
      txtOverTime.Alignment = 1;
      txtTimeout.Alignment = 1;
      txtRetries.Alignment = 1;
      txtProxyPort.Alignment = 1;
      string sLocalPath = modFunctions.LocalAppData;
      if (sLocalPath.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Personal)))
        sLocalPath = "~" + sLocalPath.Substring(Environment.GetFolderPath(Environment.SpecialFolder.Personal).Length);
      if (sLocalPath.EndsWith("."))
        sLocalPath = sLocalPath.Substring(0, sLocalPath.Length - 1);
      optHistoryLocalConfig.TooltipMarkup = "Save History Data to the local <b>" + sLocalPath + "</b> directory.";
      lblAdvancedDataDescription.LabelProp = "Your usage data will be stored in this directory. By default, data is stored in the " + sLocalPath + " directory.";
      if (sLocalPath.Contains("n"))
        sLocalPath = sLocalPath.Insert(sLocalPath.IndexOf("n"), "_");
      optHistoryLocalConfig.Label = sLocalPath;
      AddEventHandlers();
      mySettings = new AppSettings();
      string Username = mySettings.Account;
      txtAccount.Text = Username;
      if (mySettings.PassCrypt != null)
      {
        if (string.IsNullOrEmpty(mySettings.PassKey) | string.IsNullOrEmpty(mySettings.PassSalt))
          txtPassword.Text = RestrictionLibrary.StoredPasswordLegacy.DecryptApp(mySettings.PassCrypt);
        else
          txtPassword.Text = RestrictionLibrary.StoredPassword.Decrypt(mySettings.PassCrypt, mySettings.PassKey, mySettings.PassSalt);
      }
      txtPassword.Visibility = false;
      if (mySettings.StartWait > (int)txtStartWait.Adjustment.Upper)
      {
        mySettings.StartWait = (int)txtStartWait.Adjustment.Upper;
      }
      if (mySettings.StartWait < (int)txtStartWait.Adjustment.Lower)
      {
        mySettings.StartWait = (int)txtStartWait.Adjustment.Lower;
      }
      txtStartWait.Value = (int)mySettings.StartWait;
      txtStartWait.Adjustment.PageIncrement = 1;
      chkStartup.Active = modFunctions.RunOnStartup;
      if (mySettings.Interval > (int)txtInterval.Adjustment.Upper)
      {
        mySettings.Interval = (int)txtInterval.Adjustment.Upper;
      }
      if (mySettings.Interval < (int)txtInterval.Adjustment.Lower)
      {
        mySettings.Interval = (int)txtInterval.Adjustment.Lower;
      }
      txtInterval.Value = (int)mySettings.Interval;
      txtInterval.Adjustment.PageIncrement = 5;
      if (mySettings.Accuracy > (int)txtAccuracy.Adjustment.Upper)
      {
        mySettings.Accuracy = (int)txtAccuracy.Adjustment.Upper;
      }
      if (mySettings.Accuracy < (int)txtAccuracy.Adjustment.Lower)
      {
        mySettings.Accuracy = (int)txtAccuracy.Adjustment.Lower;
      }
      txtAccuracy.Value = (int)mySettings.Accuracy;
      txtAccuracy.Adjustment.PageIncrement = 1;
      if (mySettings.Overuse == 0)
      {
        chkOverAlert.Active = false;
        txtOverSize.Value = 100;
      }
      else
      {
        chkOverAlert.Active = true;
        txtOverSize.Value = mySettings.Overuse;
      }
      chkOverAlert_Activated(null, null);
      txtOverTime.Value = mySettings.Overtime;
      if (mySettings.Timeout > (int)txtTimeout.Adjustment.Upper)
      {
        mySettings.Timeout = (int)txtTimeout.Adjustment.Upper;
      }
      if (mySettings.Timeout < (int)txtTimeout.Adjustment.Lower)
      {
        mySettings.Timeout = (int)txtTimeout.Adjustment.Lower;
      }
      txtTimeout.Value = (int)mySettings.Timeout;
      txtTimeout.Adjustment.PageIncrement = 15;
      if (mySettings.Retries > (int)txtRetries.Adjustment.Upper)
      {
        mySettings.Retries = (int)txtRetries.Adjustment.Upper;
      }
      if (mySettings.Retries < (int)txtRetries.Adjustment.Lower)
      {
        mySettings.Retries = (int)txtRetries.Adjustment.Lower;
      }
      txtRetries.Value = (int)mySettings.Retries;
      txtRetries.Adjustment.PageIncrement = 1;
      if (mySettings.Proxy == null)
      {
        cmbProxyType.Active = 0;
        txtProxyAddress.Text = "";
        txtProxyPort.Value = 8080d;
        txtProxyUser.Text = "";
        txtProxyPassword.Text = "";
        txtProxyDomain.Text = "";
      }
      else if (mySettings.Proxy.Equals(System.Net.WebRequest.DefaultWebProxy))
      {
        cmbProxyType.Active = 1;
        txtProxyAddress.Text = "";
        txtProxyPort.Value = 8080d;
        txtProxyUser.Text = "";
        txtProxyPassword.Text = "";
        txtProxyDomain.Text = "";
      }
      else
      {
        System.Net.WebProxy wProxy = (System.Net.WebProxy)mySettings.Proxy;
        if (modFunctions.IsNumeric(wProxy.Address.Host.Replace(".", "")))
        {
          cmbProxyType.Active = 2;
          txtProxyAddress.Text = wProxy.Address.Host;
          txtProxyPort.Value = (double)wProxy.Address.Port;
        }
        else
        {
          cmbProxyType.Active = 3;
          txtProxyAddress.Text = wProxy.Address.OriginalString;
          txtProxyPort.Value = 8080;
        }
        if (wProxy.Credentials != null)
        {
          System.Net.NetworkCredential mCreds = wProxy.Credentials.GetCredential(null, "");
          txtProxyUser.Text = mCreds.UserName;
          txtProxyPassword.Text = mCreds.Password;
          if (string.IsNullOrEmpty(mCreds.Domain))
          {
            txtProxyDomain.Text = "";
          }
          else
          {
            txtProxyDomain.Text = mCreds.Domain;
          }
        }
        else
        {
          txtProxyUser.Text = "";
          txtProxyPassword.Text = "";
          txtProxyDomain.Text = "";
        }
      }
      cmbProxyType_Changed(null, null);
      chkNetworkProtocolSSL3.Active = ((((SecurityProtocolTypeEx)mySettings.SecurityProtocol) & SecurityProtocolTypeEx.Ssl3) == SecurityProtocolTypeEx.Ssl3);
      chkNetworkProtocolTLS10.Active = ((((SecurityProtocolTypeEx)mySettings.SecurityProtocol) & SecurityProtocolTypeEx.Tls10) == SecurityProtocolTypeEx.Tls10);
      chkNetworkProtocolTLS11.Active = ((((SecurityProtocolTypeEx)mySettings.SecurityProtocol) & SecurityProtocolTypeEx.Tls11) == SecurityProtocolTypeEx.Tls11);
      chkNetworkProtocolTLS12.Active = ((((SecurityProtocolTypeEx)mySettings.SecurityProtocol) & SecurityProtocolTypeEx.Tls12) == SecurityProtocolTypeEx.Tls12);
      bool useTLSProxy = false;
      /*
      SecurityProtocolTypeEx myProtocol = (SecurityProtocolTypeEx) System.Net.ServicePointManager.SecurityProtocol;
      try
      {
        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType) SecurityProtocolTypeEx.Ssl3;
      }
      catch (Exception)
      {
        useTLSProxy = true;
      }
      try
      {
        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType) SecurityProtocolTypeEx.Tls10;
      }
      catch (Exception)
      {
        useTLSProxy = true;
      }
      try
      {
        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType) SecurityProtocolTypeEx.Tls11;
      }
      catch (Exception)
      {
        useTLSProxy = true;
      }
      try
      {
        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType) SecurityProtocolTypeEx.Tls12;
      }
      catch (Exception)
      {
        useTLSProxy = true;
      }
      System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType) myProtocol;
      */
      useTLSProxy = true;
      /*
      string clrVer = RestrictionLibrary.srlFunctions.CLRCleanVersion;
      Version clr = new Version(clrVer.Substring(5));
      if (clr.Major > 4 || (clr.Major == 4 & clr.Minor >= 8))
        useTLSProxy = false;
      */       
      if (useTLSProxy)
      {
        chkTLSProxy.Visible = true;
        chkTLSProxy.Active = mySettings.TLSProxy;
      }
      else
      {
        chkTLSProxy.Active = false;
        chkTLSProxy.Visible = false;
      }
      chkNetworkSecurityEnforce.Active = mySettings.SecurityEnforced;
      RunNetworkProtocolTest();
      if (string.IsNullOrEmpty(mySettings.NetTestURL))
      {
        optNetTestNone.Active = true;
      }
      else
      {
        switch (mySettings.NetTestURL)
        {
          case "http://testmy.net":
            optNetTestTestMyNet.Active = true;
            break;
          case "http://speedtest.net":
            optNetTestSpeedTest.Active = true;
            break;
          default:
            optNetTestCustom.Active = true;
            txtNetTestCustom.Text = mySettings.NetTestURL;
            break;
        }
      }
      switch (mySettings.UpdateType)
      {
        case UpdateTypes.Auto:
          cmbUpdateAutomation.Active = 0;
          break;
        case UpdateTypes.Ask:
          cmbUpdateAutomation.Active = 1;
          break;
        case UpdateTypes.None:
          cmbUpdateAutomation.Active = 2;
          break;
      }
      chkUpdateBETA.Active = mySettings.UpdateBETA;
      switch (mySettings.UpdateTime)
      {
        case 1:
          cmbUpdateInterval.Active = 0;
          break;
        case 3:
          cmbUpdateInterval.Active = 1;
          break;
        case 7:
          cmbUpdateInterval.Active = 2;
          break;
        case 15:
          cmbUpdateInterval.Active = 3;
          break;
        case 30:
          cmbUpdateInterval.Active = 4;
          break;
        default:
          cmbUpdateInterval.Active = 3;
          break;
      }

      DoCheck();
      string hD = mySettings.HistoryDir;
      if (string.IsNullOrEmpty(hD))
        hD = modFunctions.AppDataPath;
      if (hD.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
        hD = hD.Substring(0, hD.Length - 1);
      if (string.Compare(hD, modFunctions.AppDataPath, StringComparison.OrdinalIgnoreCase) == 0)
      {
        optHistoryLocalConfig.Active = true;
      }
      else
      {
        optHistoryCustom.Active = true;
      }
      txtHistoryDir.SetCurrentFolder(hD);
      chkScaleScreen.Active = mySettings.ScaleScreen;
      if (MainClass.fMain.TraySupported)
      {
        switch (mySettings.TrayIconStyle)
        {
          case TrayStyles.Always:
            chkTrayIcon.Active = true;
            chkTrayMin.Active = false;
            break;
          case TrayStyles.Minimized:
            chkTrayIcon.Active = true;
            chkTrayMin.Active = true;
            break;
          case TrayStyles.Never:
            chkTrayIcon.Active = false;
            chkTrayMin.Active = false;
            break;
        }
        chkTrayIcon_Clicked(null, null);
      }
      else
      {
        chkTrayIcon.Sensitive = false;
        chkTrayMin.Sensitive = false;
        chkTrayIcon.TooltipText += "\n(Requires AppIndicator Library)";
      }
      chkTrayClose.Active = mySettings.TrayIconOnClose;

      chkAutoHide.Active = mySettings.AutoHide;

      bSaved = false;
      bAccount = false;
      cmdSave.Sensitive = false;
      this.Show();
      this.GdkWindow.SetDecorations(Gdk.WMDecoration.All | Gdk.WMDecoration.Maximize | Gdk.WMDecoration.Minimize | Gdk.WMDecoration.Resizeh | Gdk.WMDecoration.Menu);
      this.GdkWindow.Functions = Gdk.WMFunction.All | Gdk.WMFunction.Maximize | Gdk.WMFunction.Minimize | Gdk.WMFunction.Resize;
      if (!CurrentOS.IsMac)
      {
        bLoaded = true;
      }
      this.Response += dlgConfig_Response;
    }
    private void AddEventHandlers()
    {
      this.WindowStateEvent += HandleWindowStateEvent;
      //
      // Account
      //
      txtAccount.Changed += txtAccount_Changed;
      txtPassword.ClipboardPasted += txtPassword_ClipboardPasted;
      txtPassword.Changed += ValuesChanged;
      cmdPassDisplay.Toggled += cmdPassDisplay_Toggled;
      //
      // Preferences
      //
      chkStartup.Clicked += ValuesChanged;
      txtStartWait.ValueChanged += ValuesChanged;
      //
      txtInterval.ValueChanged += ValuesChanged;
      txtAccuracy.ValueChanged += ValuesChanged;
      //
      txtOverSize.ValueChanged += ValuesChanged;
      txtOverTime.ValueChanged += ValuesChanged;
      chkOverAlert.Clicked += chkOverAlert_Activated;
      cmdAlertStyle.Clicked += cmdAlertStyle_Click;
      //
      chkScaleScreen.Clicked += ValuesChanged;
      chkTrayIcon.Clicked += chkTrayIcon_Clicked;
      chkTrayMin.Clicked += ValuesChanged;
      chkTrayClose.Clicked += ValuesChanged;
      chkAutoHide.Clicked += ValuesChanged;
      //
      cmdColors.Clicked += cmdColors_Click;
      //
      // Network
      //
      txtTimeout.Changed += ValuesChanged;
      txtRetries.Changed += ValuesChanged;
      //
      cmbProxyType.Changed += cmbProxyType_Changed;
      txtProxyAddress.Changed += ValuesChanged;
      txtProxyPort.ValueChanged += ValuesChanged;
      txtProxyUser.Changed += ValuesChanged;
      txtProxyPassword.Changed += ValuesChanged;
      txtProxyDomain.Changed += ValuesChanged;
      //
      chkTLSProxy.Clicked += chkTLSProxy_Clicked;
      chkNetworkSecurityEnforce.Clicked += chkNetworkSecurityEnforce_Clicked;
      chkNetworkProtocolSSL3.Clicked += ValuesChanged;
      chkNetworkProtocolTLS10.Clicked += ValuesChanged;
      chkNetworkProtocolTLS11.Clicked += ValuesChanged;
      chkNetworkProtocolTLS12.Clicked += ValuesChanged;
      //
      cmbUpdateAutomation.Changed += cmbUpdateAutomation_Changed;
      chkUpdateBETA.Clicked += ValuesChanged;
      cmbUpdateInterval.Changed += ValuesChanged;
      //
      // Adavnced
      //
      optHistoryLocalConfig.Clicked += optHistoryLocalConfig_Clicked;
      optHistoryCustom.Clicked += optHistoryCustom_Clicked;
      txtHistoryDir.CurrentFolderChanged += txtHistoryDir_CurrentFolderChanged;
      cmdHistoryDirOpen.Clicked += cmdHistoryDirOpen_Clicked;
      //
      optNetTestNone.Clicked += optNetTestNone_Clicked;
      optNetTestTestMyNet.Clicked += optNetTestTestMyNet_Clicked;
      optNetTestSpeedTest.Clicked += optNetTestSpeedTest_Clicked;
      optNetTestCustom.Clicked += optNetTestCustom_Clicked;

      txtNetTestCustom.FocusOutEvent += txtNetTestCustom_FocusOut;
      txtNetTestCustom.Changed += txtNetTestCustom_TextChanged;

      cmdSave.Clicked += cmdSave_Click;
      cmdClose.Clicked += cmdClose_Click;
    }
    void HandleWindowStateEvent(object o, Gtk.WindowStateEventArgs args)
    {
      if (args.Event.ChangedMask == Gdk.WindowState.Iconified)
      {
        if ((args.Event.NewWindowState & Gdk.WindowState.Iconified) == Gdk.WindowState.Iconified)
        {
          this.Deiconify();
        }
      }
    }
    private void RunNetworkProtocolTest()
    {
      lblRetries1.Sensitive = true;
      txtRetries.Sensitive = true;
      lblRetries2.Sensitive = true;
      txtRetries.TooltipText = "Number of times to retry a connection that times out or fails before giving up.\nIf you run into errrors that say \"Please try again\", you can increase this number to improve the chances of a good connection.";
      chkTLSProxy.Sensitive = true;
      chkTLSProxy.TooltipText = "If your version of the MONO Framework does not support the Security Protocol required for your provider, you can use this Proxy to connect through the RealityRipple.com server.";
      if (chkTLSProxy.Active)
      {
        chkNetworkSecurityEnforce.Sensitive = false;
        chkNetworkSecurityEnforce.TooltipText = "The server's certificate will be validated at the discretion of the TLS Proxy.";
        chkNetworkProtocolSSL3.Sensitive = true;
        chkNetworkProtocolSSL3.TooltipText = "Check this box to allow use of the older SSL 3.0 protocol, which is vulnerable to attacks.";
        chkNetworkProtocolTLS10.Sensitive = true;
        chkNetworkProtocolTLS10.TooltipText = "Check this box to allow use of the older TLS 1.0 protocol, which may be vulnerable to attacks.";
        chkNetworkProtocolTLS11.Sensitive = true;
        chkNetworkProtocolTLS11.TooltipText = "Check this box to allow use of the newer, safer TLS 1.1 protocol.";
        chkNetworkProtocolTLS12.Sensitive = true;
        chkNetworkProtocolTLS12.TooltipText = "Check this box to allow use of the latest TLS 1.2 protocol.";
        return;
      }
      chkNetworkSecurityEnforce.Sensitive = !chkTLSProxy.Active;
      chkNetworkSecurityEnforce.TooltipText = "Enforce network certificate validation. If the server's certificate is not valid, your connection may fail.\nTurning this feature off may potentially expose your computer or transmitted information to third parties.";
      bool canSSL3 = true;
      bool canTLS10 = true;
      bool canTLS11 = true;
      bool canTLS12 = true;
      SecurityProtocolTypeEx myProtocol = (SecurityProtocolTypeEx)System.Net.ServicePointManager.SecurityProtocol;
      try
      {
        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.Ssl3;
      }
      catch (Exception)
      {
        canSSL3 = false;
      }
      try
      {
        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.Tls10;
      }
      catch (Exception)
      {
        canTLS10 = false;
      }
      try
      {
        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.Tls11;
      }
      catch (Exception)
      {
        canTLS11 = false;
      }
      try
      {
        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.Tls12;
      }
      catch (Exception)
      {
        canTLS12 = false;
      }
      string clrVer = RestrictionLibrary.srlFunctions.CLRCleanVersion;
      Version clr = new Version(clrVer.Substring(5));
      if (clr.Major < 4 || (clr.Major == 4 & clr.Minor < 8))
      {
        canTLS11 = false;
        canTLS12 = false;
      }
      System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)myProtocol;
      if (canSSL3)
      {
        chkNetworkProtocolSSL3.Sensitive = true;
        chkNetworkProtocolSSL3.TooltipText = "Check this box to allow use of the older SSL 3.0 protocol, which is vulnerable to attacks.";
      }
      else
      {
        chkNetworkProtocolSSL3.Active = false;
        chkNetworkProtocolSSL3.Sensitive = false;
        chkNetworkProtocolSSL3.TooltipText = "Your version of the MONO Framework does not allow SSL 3.0 connections. Probably for the best, as this protocol is vulnerable to attacks.";
      }
      if (canTLS10)
      {
        chkNetworkProtocolTLS10.Sensitive = true;
        chkNetworkProtocolTLS10.TooltipText = "Check this box to allow use of the older TLS 1.0 protocol, which may be vulnerable to attacks.";
      }
      else
      {
        chkNetworkProtocolTLS10.Active = false;
        chkNetworkProtocolTLS10.Sensitive = false;
        chkNetworkProtocolTLS10.TooltipText = "Your version of the MONO Framework does not allow TLS 1.0 connections. Probably for the best, as this protocol is vulnerable to attacks.";
      }
      if (canTLS11)
      {
        chkNetworkProtocolTLS11.Sensitive = true;
        chkNetworkProtocolTLS11.TooltipText = "Check this box to allow use of the newer, safer TLS 1.1 protocol.";
      }
      else
      {
        chkNetworkProtocolTLS11.Active = false;
        chkNetworkProtocolTLS11.Sensitive = false;
        chkNetworkProtocolTLS11.TooltipText = "Your version of the MONO Framework does not allow TLS 1.1 connections. Please update to MONO 4.8 or newer.";
      }
      if (canTLS12)
      {
        chkNetworkProtocolTLS12.Sensitive = true;
        chkNetworkProtocolTLS12.TooltipText = "Check this box to allow use of the latest TLS 1.2 protocol.";
      }
      else
      {
        chkNetworkProtocolTLS12.Active = false;
        chkNetworkProtocolTLS12.Sensitive = false;
        chkNetworkProtocolTLS12.TooltipText = "Your version of the MONO Framework does not allow TLS 1.2 connections. Please update to MONO 4.8 or newer.";
      }
    }
    protected void dlgConfig_Response(object o, Gtk.ResponseArgs args)
    {
      this.Response -= dlgConfig_Response;
      if (cmdSave.Sensitive)
      {
        Gtk.ResponseType saveRet = modFunctions.ShowMessageBoxYNC(this, "Some settings have been changed but not saved.\n\nDo you want to save the changes to your configuration?", "Save Configuration?", Gtk.DialogFlags.Modal);
        if (saveRet == Gtk.ResponseType.Yes)
        {
          cmdSave.Click();
          if (cmdSave.Sensitive)
          {
            this.Respond(Gtk.ResponseType.None);
            this.Response += dlgConfig_Response;
            return;
          }
          if (bAccount)
            this.Respond(Gtk.ResponseType.Yes);
          else
            this.Respond(Gtk.ResponseType.Ok);
        }
        else if (saveRet == Gtk.ResponseType.No)
        {
          this.Respond(Gtk.ResponseType.No);
        }
        else if (saveRet == Gtk.ResponseType.Cancel)
        {
          this.Respond(Gtk.ResponseType.None);
          this.Response += dlgConfig_Response;
        }
      }
      else if (bSaved)
      {
        if (bAccount)
        {
          this.Respond(Gtk.ResponseType.Yes);
        }
        else
        {
          this.Respond(Gtk.ResponseType.Ok);
        }
      }
      else
      {
        this.Respond(Gtk.ResponseType.No);
      }
    }
    #endregion
    #region "Inputs"
    protected void txtPassword_ClipboardPasted(object sender, EventArgs e)
    {
      txtPassword.Text = txtPassword.Text.Trim();
    }
    protected void txtHistoryDir_CurrentFolderChanged(object sender, EventArgs e)
    {
      if (bLoaded)
      {
        cmdSave.Sensitive = SettingsChanged();
      }
      else if (CurrentOS.IsMac)
      {
        bLoaded = true;
      }
    }
    protected void cmdHistoryDirOpen_Clicked(object sender, EventArgs e)
    {
      if (optHistoryLocalConfig.Active)
        if (Directory.Exists(modFunctions.AppDataPath))
        {
          try
          {
            System.Diagnostics.Process.Start(modFunctions.AppDataPath);
          }
          catch (Exception ex)
          {
            TaskbarNotifier taskNotifier = null;
            modFunctions.MakeNotifier(ref taskNotifier, false);
            try
            {
              Gtk.Clipboard cb = Gtk.Clipboard.Get(Gdk.Selection.Clipboard);
              cb.Text = modFunctions.AppDataPath;
            }
            catch (Exception)
            {
              if (taskNotifier != null)
                taskNotifier.Show("Failed to run File Manager", modFunctions.ProductName + " could not navigate to \"" + modFunctions.AppDataPath + "\"!\n" + ex.Message, 200, 3000, 100);
              return;
            }
            if (taskNotifier != null)
              taskNotifier.Show("Path Copied to Clipboard", modFunctions.ProductName + " could not navigate to \"" + modFunctions.AppDataPath + "\", so the path was copied to your clipboard instead!", 200, 3000, 100);
          }
        }
        else
          modFunctions.ShowMessageBox(this, "The directory \"" + modFunctions.AppDataPath + "\" does not exist.\nPlease save the configuration first.", "Missing Directory", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
      else if (Directory.Exists(txtHistoryDir.CurrentFolder))
      {
        try
        {
          System.Diagnostics.Process.Start(txtHistoryDir.CurrentFolder);
        }
        catch (Exception ex)
        {
          TaskbarNotifier taskNotifier = null;
          modFunctions.MakeNotifier(ref taskNotifier, false);
          try
          {
            Gtk.Clipboard cb = Gtk.Clipboard.Get(Gdk.Selection.Clipboard);
            cb.Text = modFunctions.AppDataPath;
          }
          catch (Exception)
          {
            if (taskNotifier != null)
              taskNotifier.Show("Failed to run File Manager", modFunctions.ProductName + " could not navigate to \"" + txtHistoryDir.CurrentFolder + "\"!\n" + ex.Message, 200, 3000, 100);
            return;
          }
          if (taskNotifier != null)
            taskNotifier.Show("Path Copied to Clipboard", modFunctions.ProductName + " could not navigate to \"" + txtHistoryDir.CurrentFolder + "\", so the path was copied to your clipboard instead!", 200, 3000, 100);
        }
      }
      else
        modFunctions.ShowMessageBox(this, "The directory \"" + txtHistoryDir.CurrentFolder + "\" does not exist.\nPlease save the configuration first.", "Missing Directory", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
    }
    protected void ValuesChanged(object sender, EventArgs e)
    {
      if (bLoaded)
      {
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    protected void txtAccount_Changed(object sender, EventArgs e)
    {
      if (!bLoaded)
      {
        return;
      }
      cmdSave.Sensitive = SettingsChanged();
    }
    protected void cmdPassDisplay_Toggled(object sender, EventArgs e)
    {
      txtPassword.Visibility = !(txtPassword.Visibility);
    }
    protected void chkOverAlert_Activated(object sender, EventArgs e)
    {
      txtOverSize.Sensitive = chkOverAlert.Active;
      lblOverSize1.Sensitive = chkOverAlert.Active;
      lblOverSize2.Sensitive = chkOverAlert.Active;
      txtOverTime.Sensitive = chkOverAlert.Active;
      lblOverTime1.Sensitive = chkOverAlert.Active;
      lblOverTime2.Sensitive = chkOverAlert.Active;
      cmdAlertStyle.Sensitive = chkOverAlert.Active;
      if (bLoaded)
      {
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    protected void cmbProxyType_Changed(object sender, EventArgs e)
    {
      if (cmbProxyType.Active == 2)
      {
        lblProxyAddr.Sensitive = true;
        lblProxyAddr.Text = "IP Address:";
        txtProxyAddress.Sensitive = true;
        ((Gtk.Table.TableChild)pnlProxy[txtProxyAddress]).RightAttach = 1;
        lblProxyPort.Visible = true;
        txtProxyPort.Visible = true;
        lblProxyPort.Sensitive = true;
        txtProxyPort.Sensitive = true;
        lblProxyUser.Sensitive = true;
        txtProxyUser.Sensitive = true;
        lblProxyPassword.Sensitive = true;
        txtProxyPassword.Sensitive = true;
        lblProxyDomain.Sensitive = true;
        txtProxyDomain.Sensitive = true;
      }
      else if (cmbProxyType.Active == 3)
      {
        lblProxyAddr.Sensitive = true;
        lblProxyAddr.Text = "URL:";
        txtProxyAddress.Sensitive = true;
        lblProxyPort.Visible = false;
        txtProxyPort.Visible = false;
        lblProxyPort.Sensitive = false;
        txtProxyPort.Sensitive = false;
        ((Gtk.Table.TableChild)pnlProxy[txtProxyAddress]).RightAttach = 3;
        lblProxyUser.Sensitive = true;
        txtProxyUser.Sensitive = true;
        lblProxyPassword.Sensitive = true;
        txtProxyPassword.Sensitive = true;
        lblProxyDomain.Sensitive = true;
        txtProxyDomain.Sensitive = true;
      }
      else
      {
        lblProxyAddr.Sensitive = false;
        lblProxyAddr.Text = "IP Address:";
        txtProxyAddress.Sensitive = false;
        ((Gtk.Table.TableChild)pnlProxy[txtProxyAddress]).RightAttach = 1;
        lblProxyPort.Visible = true;
        txtProxyPort.Visible = true;
        lblProxyPort.Sensitive = false;
        txtProxyPort.Sensitive = false;
        lblProxyUser.Sensitive = false;
        txtProxyUser.Sensitive = false;
        lblProxyPassword.Sensitive = false;
        txtProxyPassword.Sensitive = false;
        lblProxyDomain.Sensitive = false;
        txtProxyDomain.Sensitive = false;
      }
      if (bLoaded)
      {
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    protected void chkNetworkSecurityEnforce_Clicked(object sender, EventArgs e)
    {
      if (bLoaded)
      {
        RunNetworkProtocolTest();
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    protected void chkTLSProxy_Clicked(object sender, EventArgs e)
    {
      if (bLoaded)
      {
        RunNetworkProtocolTest();
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    protected void cmbUpdateAutomation_Changed(object sender, EventArgs e)
    {
      switch (cmbUpdateAutomation.Active)
      {
        case 0:
          lblUpdateInterval.Sensitive = true;
          cmbUpdateInterval.Sensitive = true;
          chkUpdateBETA.Sensitive = true;
          break;
        case 1:
          lblUpdateInterval.Sensitive = true;
          cmbUpdateInterval.Sensitive = true;
          chkUpdateBETA.Sensitive = true;
          break;
        case 2:
          lblUpdateInterval.Sensitive = false;
          cmbUpdateInterval.Sensitive = false;
          chkUpdateBETA.Sensitive = false;
          break;
        default:
          lblUpdateInterval.Sensitive = false;
          cmbUpdateInterval.Sensitive = false;
          chkUpdateBETA.Sensitive = false;
          break;
      }
      if (bLoaded)
      {
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    protected void optHistoryLocalConfig_Clicked(object sender, EventArgs e)
    {
      if (optHistoryLocalConfig.Active)
      {
        txtHistoryDir.Sensitive = optHistoryCustom.Active;
        txtHistoryDir.SetCurrentFolder(modFunctions.AppDataPath);
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    protected void optHistoryCustom_Clicked(object sender, EventArgs e)
    {
      if (optHistoryCustom.Active)
      {
        txtHistoryDir.Sensitive = optHistoryCustom.Active;
        txtHistoryDir.SetCurrentFolder(modFunctions.MySaveDir());
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    protected void optNetTestNone_Clicked(object sender, EventArgs e)
    {
      if (optNetTestNone.Active)
      {
        txtNetTestCustom.Text = "";
        txtNetTestCustom.Sensitive = false;
        if (CurrentOS.IsMac)
          SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_nettest_none.png"));
        else
          SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_none.png"));
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    protected void optNetTestTestMyNet_Clicked(object sender, EventArgs e)
    {
      if (optNetTestTestMyNet.Active)
      {
        txtNetTestCustom.Text = "http://testmy.net";
        txtNetTestCustom.Sensitive = false;
        int token = MakeAToken(txtNetTestCustom.Text);
        if (CurrentOS.IsMac)
          SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_nettest_load.png"), false, token, null);
        else
          SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_load.png"), false, token, null);
        clsFavicon wsFavicon = new clsFavicon(wsFavicon_DownloadIconCompleted);
        wsFavicon.Start(txtNetTestCustom.Text, token);
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    protected void optNetTestSpeedTest_Clicked(object sender, EventArgs e)
    {
      if (optNetTestSpeedTest.Active)
      {
        txtNetTestCustom.Text = "http://speedtest.net";
        txtNetTestCustom.Sensitive = false;
        int token = MakeAToken(txtNetTestCustom.Text);
        if (CurrentOS.IsMac)
          SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_nettest_load.png"), false, token, null);
        else
          SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_load.png"), false, token, null);
        clsFavicon wsFavicon = new clsFavicon(wsFavicon_DownloadIconCompleted);
        wsFavicon.Start(txtNetTestCustom.Text, token);
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    protected void optNetTestCustom_Clicked(object sender, EventArgs e)
    {
      if (optNetTestCustom.Active)
      {
        txtNetTestCustom.Text = "";
        txtNetTestCustom.Sensitive = true;
        if (CurrentOS.IsMac)
          SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_nettest_edit.png"));
        else
          SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_edit.png"));
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    void txtNetTestCustom_FocusOut(object o, Gtk.FocusOutEventArgs args)
    {
      if (optNetTestCustom.Active)
      {
        if (icoNetTest == null)
        {
          if (pIcoWait != 0)
          {
            GLib.Source.Remove(pIcoWait);
            pIcoWait = 0;
          }
          if (!string.IsNullOrEmpty(txtNetTestCustom.Text))
          {
            int token = MakeAToken(txtNetTestCustom.Text);
            if (CurrentOS.IsMac)
              SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_nettest_load.png"), false, token, null);
            else
              SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_load.png"), false, token, null);
            clsFavicon wsFavicon = new clsFavicon(wsFavicon_DownloadIconCompleted);
            wsFavicon.Start(txtNetTestCustom.Text, token);
          }
        }
      }
    }
    protected void txtNetTestCustom_TextChanged(object sender, EventArgs e)
    {
      if (optNetTestCustom.Active)
      {
        if (CurrentOS.IsMac)
          SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_nettest_edit.png"));
        else
          SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_edit.png"));
        cmdSave.Sensitive = SettingsChanged();
        if (pIcoWait != 0)
        {
          GLib.Source.Remove(pIcoWait);
          pIcoWait = 0;
        }
        pIcoWait = GLib.Timeout.Add(4000, tmrIcoWait_Tick);
      }
    }
    protected void chkTrayIcon_Clicked(object sender, EventArgs e)
    {
      chkTrayMin.Sensitive = chkTrayIcon.Active;
      if (!chkTrayMin.Sensitive)
        chkTrayMin.Active = false;
      if (bLoaded)
      {
        cmdSave.Sensitive = SettingsChanged();
      }
    }
    #region "Net Test"
    private class FaviconDownloadIconCompletedEventArgs: EventArgs
    {
      public Gdk.Pixbuf Icon16;
      public Gdk.Pixbuf Icon32;
      public object token;
      public Exception Error;
      public FaviconDownloadIconCompletedEventArgs(Gdk.Pixbuf ico16, Gdk.Pixbuf ico32, object objToken, Exception exError)
      {
        Icon16 = ico16;
        Icon32 = ico32;
        token = objToken;
        Error = exError;
      }
    }
    protected void wsFavicon_DownloadIconCompleted(Gdk.Pixbuf icon16, Gdk.Pixbuf icon32, object token, Exception error)
    {
      FaviconDownloadIconCompletedEventArgs e = new FaviconDownloadIconCompletedEventArgs(icon16, icon32, token, error);
      Gtk.Application.Invoke(this, (EventArgs)e, wsFavicon_DownloadIconCompletedAsync);
    }
    private void wsFavicon_DownloadIconCompletedAsync(object sender, EventArgs ea)
    {
      FaviconDownloadIconCompletedEventArgs e = (FaviconDownloadIconCompletedEventArgs)ea;
      if (e.Error != null)
      {
        if (CurrentOS.IsMac)
          SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_nettest_error.png"), true, e.token, null);
        else
          SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_error.png"), true, e.token, null);
      }
      else
      {
        SetNetTestImage(e.Icon32, true, e.token, (object)e.Icon16);
      }
    }
    protected void SetNetTestImage(Gdk.Pixbuf Image)
    {
      waitingForEndOf = (object)0;
      pctAdvancedNetTestIcon.Pixbuf = Image;
      icoNetTest = null;
    }
    object waitingForEndOf;
    protected void SetNetTestImage(Gdk.Pixbuf Image, bool End, object Token, object tag)
    {
      if (waitingForEndOf == null)
        waitingForEndOf = (object)0;
      if ((End) & (((int)Token) != ((int)waitingForEndOf)))
        return;
      pctAdvancedNetTestIcon.Pixbuf = Image;
      icoNetTest = (Gdk.Pixbuf)tag;
      if (End)
        waitingForEndOf = (object)0;
      else
        waitingForEndOf = Token;
    }
    private bool tmrIcoWait_Tick()
    {
      if (optNetTestCustom.Active)
      {
        if (!string.IsNullOrEmpty(txtNetTestCustom.Text))
        {
          int token = MakeAToken(txtNetTestCustom.Text);
          if (CurrentOS.IsMac)
            SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_nettest_load.png"), false, token, null);
          else
            SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_load.png"), false, token, null);
          clsFavicon wsFavicon = new clsFavicon(wsFavicon_DownloadIconCompleted);
          wsFavicon.Start(txtNetTestCustom.Text, token);
        }
        else
        {
          if (CurrentOS.IsMac)
            SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_nettest_error.png"));
          else
            SetNetTestImage(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_error.png"));
        }
      }
      return false;
    }
    private static int MakeAToken(string fromString)
    {
      Random rand = new Random();
      uint iToken = (uint)(fromString.Length * rand.Next(0, 31) + rand.Next(0, int.MaxValue - 1));
      iToken = iToken % int.MaxValue;
      for (int i = 0; i < fromString.Length; i++)
      {
        iToken *= fromString[i];
        iToken %= int.MaxValue;
      }
      return (int)iToken;
    }
    #endregion
    #endregion
    #region "Save/Close Buttons"
    protected void cmdAlertStyle_Click(object sender, EventArgs e)
    {
      if (MainClass.fAlertSelection != null)
      {
        if (MainClass.fAlertSelection.Visible)
        {
          return;
        }
        MainClass.fAlertSelection.Dispose();
      }
      MainClass.fAlertSelection = new dlgAlertSelection(mySettings.AlertStyle);
      MainClass.fAlertSelection.TransientFor = this;
      MainClass.fAlertSelection.Modal = true;
      MainClass.fAlertSelection.WindowPosition = Gtk.WindowPosition.CenterOnParent;
      Gtk.ResponseType ret = (Gtk.ResponseType)MainClass.fAlertSelection.Run();

      if (ret == Gtk.ResponseType.Yes)
      {
        mySettings.AlertStyle = MainClass.fAlertSelection.AlertStyle;
        bHardChange = true;
        cmdSave.Sensitive = SettingsChanged();
      }
      MainClass.fAlertSelection.Destroy();
    }
    protected void cmdColors_Click(object sender, EventArgs e)
    {
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
      MainClass.fCustomColors.WindowPosition = Gtk.WindowPosition.CenterOnParent;
      Gtk.ResponseType dRet;
      do
      {
        dRet = (Gtk.ResponseType)MainClass.fCustomColors.Run();
      } while (dRet == Gtk.ResponseType.None);
      if (dRet == Gtk.ResponseType.Yes)
      {
        mySettings = MainClass.fCustomColors.mySettings;
        bHardChange = true;
        cmdSave.Sensitive = SettingsChanged();
      }
      MainClass.fCustomColors.Destroy();
    }
    protected void cmdSave_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrEmpty(txtAccount.Text))
      {
        modFunctions.ShowMessageBox(this, "Please enter your ViaSat account Username before saving the configuration.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
        txtAccount.GrabFocus();
        return;
      }
      if (string.IsNullOrEmpty(txtPassword.Text))
      {
        modFunctions.ShowMessageBox(this, "Please enter your ViaSat account Password before saving the configuration.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
        txtPassword.GrabFocus();
        return;
      }
      if (!(chkNetworkProtocolSSL3.Active | chkNetworkProtocolTLS10.Active | chkNetworkProtocolTLS11.Active | chkNetworkProtocolTLS12.Active))
      {
        modFunctions.ShowMessageBox(this, "Please select at least one Security Protocol type to connect with before saving the configuration.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
        if (chkNetworkProtocolTLS12.CanFocus)
        {
          chkNetworkProtocolTLS12.GrabFocus();
        }
        else if (chkNetworkProtocolTLS11.CanFocus)
        {
          chkNetworkProtocolTLS11.GrabFocus();
        }
        else if (chkNetworkProtocolTLS10.CanFocus)
        {
          chkNetworkProtocolTLS10.GrabFocus();
        }
        else if (chkNetworkProtocolSSL3.CanFocus)
        {
          chkNetworkProtocolSSL3.GrabFocus();
        }
        else
        {
          lblNetworkProtocolDescription.GrabFocus();
        }
        return;
      }
      if (string.IsNullOrEmpty(txtHistoryDir.CurrentFolder))
      {
        txtHistoryDir.SetCurrentFolder(modFunctions.MySaveDir(true));
      }
      foreach (char c in System.IO.Path.GetInvalidPathChars())
      {
        if (txtHistoryDir.CurrentFolder.Contains(c.ToString()))
        {
          modFunctions.ShowMessageBox(this, "The directory you have entered contains invalid characters. Please choose a different directory.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
          txtHistoryDir.GrabFocus();
          return;
        }
      }
      if (string.Compare(mySettings.Account, txtAccount.Text , StringComparison.OrdinalIgnoreCase) != 0)
      {
        mySettings.Account = txtAccount.Text;
        bAccount = true;
      }
      bool newPass = false;
      if (string.IsNullOrEmpty(mySettings.PassKey))
        newPass = true;
      else
        newPass = (string.Compare(StoredPassword.Decrypt(mySettings.PassCrypt, mySettings.PassKey, mySettings.PassSalt), txtPassword.Text, StringComparison.Ordinal) != 0);
      if (newPass)
      {
        byte[] newKey = StoredPassword.GenerateKey();
        byte[] newSalt = StoredPassword.GenerateSalt();
        mySettings.PassCrypt = StoredPassword.Encrypt(txtPassword.Text, newKey, newSalt);
        mySettings.PassKey = Convert.ToBase64String(newKey);
        mySettings.PassSalt = Convert.ToBase64String(newSalt);
        bAccount = true;
      }
      modFunctions.RunOnStartup = chkStartup.Active;
      mySettings.StartWait = txtStartWait.ValueAsInt;
      mySettings.Interval = txtInterval.ValueAsInt;
      mySettings.Accuracy = txtAccuracy.ValueAsInt;
      mySettings.Timeout = txtTimeout.ValueAsInt;
      mySettings.Retries = txtRetries.ValueAsInt;
      mySettings.ScaleScreen = chkScaleScreen.Active;
      if (chkTrayIcon.Active)
      {
        if (chkTrayMin.Active)
          mySettings.TrayIconStyle = TrayStyles.Minimized;
        else
          mySettings.TrayIconStyle = TrayStyles.Always;
      }
      else
        mySettings.TrayIconStyle = TrayStyles.Never;
      mySettings.TrayIconOnClose = chkTrayClose.Active;
      mySettings.AutoHide = chkAutoHide.Active;
      if (string.IsNullOrEmpty(mySettings.HistoryDir))
      {
        mySettings.HistoryDir = modFunctions.MySaveDir(true);
      }
      if (!modFunctions.DirectoryEqualityCheck(mySettings.HistoryDir, txtHistoryDir.CurrentFolder))
      {
        bool continueChange = true;
        string[] sOldFiles = Directory.GetFiles(mySettings.HistoryDir);
        string[] sNewFiles = { };
        if (Directory.Exists(txtHistoryDir.CurrentFolder))
        {
          sNewFiles = Directory.GetFiles(txtHistoryDir.CurrentFolder);
        }
        else
        {
          if (continueChange)
          {
            try
            {
              Directory.CreateDirectory(txtHistoryDir.CurrentFolder);
            }
            catch (Exception)
            {
              modFunctions.ShowMessageBox(this, modFunctions.ProductName + " was unable to create the directory \"" + txtHistoryDir.CurrentFolder + "\"!", modFunctions.ProductName, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
              continueChange = false;
            }
          }
        }
        if (continueChange)
        {
          modDB.LOG_Terminate(true);
          string[] sSkipFiles = { "USER.CONFIG", "RESTRICTIONTRACKER-32.PNG", "RESTRICTIONTRACKER-256.PNG", "SAT.ICNS", "RESTRICTIONTRACKER.EXE", "RESTRICTIONTRACKER.EXE.MDB", "RESTRICTIONTRACKERLIB.DLL", "APPINDICATOR-SHARP.DLL", "MICROSOFT.VISUALBASIC.DLL", "CONFIG" };
          if (sOldFiles.Length > 0)
          {
            if (sNewFiles.Length > 0)
            {
              System.Collections.Generic.List<string> sOverWrites = new System.Collections.Generic.List<string>();
              foreach (string sOld in sOldFiles)
              {
                foreach (string sNew in sNewFiles)
                {
                  if (System.IO.Path.GetFileName(sNew) == System.IO.Path.GetFileName(sOld))
                  {
                    bool doSkip = false;
                    foreach (string sSkip in sSkipFiles)
                    {
                      if (System.IO.Path.GetFileName(sNew).ToUpperInvariant() == sSkip)
                      {
                        doSkip = true;
                        break;
                      }
                    }
                    if (doSkip)
                      continue;
                    sOverWrites.Add(System.IO.Path.GetFileName(sNew));
                  }
                }
              }
              if (sOverWrites.Count > 0)
              {
                if (modFunctions.ShowMessageBox(this, "Files exist in the new Data Directory:\n" + string.Join("\n", sOverWrites.ToArray()) + "\n\nOverwrite them?", "Overwrite Files?", Gtk.DialogFlags.Modal, Gtk.MessageType.Question, Gtk.ButtonsType.YesNo) == Gtk.ResponseType.Yes)
                {
                  foreach (string sFile in sOldFiles)
                  {
                    bool doSkip = false;
                    foreach (string sSkip in sSkipFiles)
                    {
                      if (System.IO.Path.GetFileName(sFile).ToUpperInvariant() == sSkip)
                      {
                        doSkip = true;
                        break;
                      }
                    }
                    if (doSkip)
                      continue;
                    File.Move(sFile, System.IO.Path.Combine(txtHistoryDir.CurrentFolder, System.IO.Path.GetFileName(sFile)));
                  }
                }
                else
                {
                  foreach (string sFile in sOldFiles)
                  {
                    bool doSkip = false;
                    foreach (string sSkip in sSkipFiles)
                    {
                      if (System.IO.Path.GetFileName(sFile).ToUpperInvariant() == sSkip)
                      {
                        doSkip = true;
                        break;
                      }
                    }
                    if (doSkip)
                      continue;
                    File.Move(sFile, System.IO.Path.Combine(txtHistoryDir.CurrentFolder, System.IO.Path.GetFileName(sFile)));
                  }
                }
              }
              else
              {
                foreach (string sFile in sOldFiles)
                {
                  bool doSkip = false;
                  foreach (string sSkip in sSkipFiles)
                  {
                    if (System.IO.Path.GetFileName(sFile).ToUpperInvariant() == sSkip)
                    {
                      doSkip = true;
                      break;
                    }
                  }
                  if (doSkip)
                    continue;
                  File.Move(sFile, System.IO.Path.Combine(txtHistoryDir.CurrentFolder, System.IO.Path.GetFileName(sFile)));
                }
              }
            }
            else
            {
              foreach (string sFile in sOldFiles)
              {
                bool doSkip = false;
                foreach (string sSkip in sSkipFiles)
                {
                  if (System.IO.Path.GetFileName(sFile).ToUpperInvariant() == sSkip)
                  {
                    doSkip = true;
                    break;
                  }
                }
                if (doSkip)
                  continue;
                File.Move(sFile, System.IO.Path.Combine(txtHistoryDir.CurrentFolder, System.IO.Path.GetFileName(sFile)));
              }
            }
          }
          mySettings.HistoryDir = txtHistoryDir.CurrentFolder;
          bAccount = true;
        }
        else
        {
          string hD = mySettings.HistoryDir;
          if (hD.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            hD = hD.Substring(0, hD.Length - 1);
          if (string.Compare(hD, modFunctions.AppDataPath, StringComparison.OrdinalIgnoreCase) == 0)
            optHistoryLocalConfig.Active = true;
          else
            optHistoryCustom.Active = true;
          txtHistoryDir.SetCurrentFolder(mySettings.HistoryDir);
        }
      }

      if (chkOverAlert.Active)
      {
        mySettings.Overuse = txtOverSize.ValueAsInt;
      }
      else
      {
        mySettings.Overuse = 0;
      }
      mySettings.Overtime = txtOverTime.ValueAsInt;
      switch (cmbUpdateAutomation.Active)
      {
        case 0:
          mySettings.UpdateType = UpdateTypes.Auto;
          break;
        case 1:
          mySettings.UpdateType = UpdateTypes.Ask;
          break;
        case 2:
          mySettings.UpdateType = UpdateTypes.None;
          break;
      }
      mySettings.UpdateBETA = chkUpdateBETA.Active;
      switch (cmbUpdateInterval.Active)
      {
        case 0:
          mySettings.UpdateTime = 1;
          break;
        case 1:
          mySettings.UpdateTime = 3;
          break;
        case 2:
          mySettings.UpdateTime = 7;
          break;
        case 3:
          mySettings.UpdateTime = 15;
          break;
        case 4:
          mySettings.UpdateTime = 30;
          break;
      }

      if (cmbProxyType.Active == 0)
      {
        mySettings.Proxy = null;
      }
      else if (cmbProxyType.Active == 1)
      {
        mySettings.Proxy = System.Net.WebRequest.DefaultWebProxy;
      }
      else if (cmbProxyType.Active == 2)
      {
        if (string.IsNullOrEmpty(txtProxyAddress.Text))
        {
          modFunctions.ShowMessageBox(this, "Please enter a Proxy address or choose a different option.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
          txtProxyAddress.GrabFocus();
          return;
        }
        if (string.IsNullOrEmpty(txtProxyUser.Text) & string.IsNullOrEmpty(txtProxyPassword.Text) & string.IsNullOrEmpty(txtProxyDomain.Text))
        {
          mySettings.Proxy = new System.Net.WebProxy(txtProxyAddress.Text, txtProxyPort.ValueAsInt);
        }
        else
        {
          if (string.IsNullOrEmpty(txtProxyDomain.Text))
          {
            mySettings.Proxy = new System.Net.WebProxy(txtProxyAddress.Text, txtProxyPort.ValueAsInt);
            mySettings.Proxy.Credentials = new System.Net.NetworkCredential(txtProxyUser.Text, txtProxyPassword.Text);
          }
          else
          {
            mySettings.Proxy = new System.Net.WebProxy(txtProxyAddress.Text, txtProxyPort.ValueAsInt);
            mySettings.Proxy.Credentials = new System.Net.NetworkCredential(txtProxyUser.Text, txtProxyPassword.Text, txtProxyDomain.Text);
          }
        }
      }
      else if (cmbProxyType.Active == 3)
      {
        if (string.IsNullOrEmpty(txtProxyAddress.Text))
        {
          modFunctions.ShowMessageBox(this, "Please enter a Proxy address or choose a different option.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
          txtProxyAddress.GrabFocus();
          return;
        }
        if (string.IsNullOrEmpty(txtProxyUser.Text) & string.IsNullOrEmpty(txtProxyPassword.Text) & string.IsNullOrEmpty(txtProxyDomain.Text))
        {
          mySettings.Proxy = new System.Net.WebProxy(txtProxyAddress.Text);
        }
        else
        {
          if (string.IsNullOrEmpty(txtProxyDomain.Text))
          {
            mySettings.Proxy = new System.Net.WebProxy(txtProxyAddress.Text, false, null, new System.Net.NetworkCredential(txtProxyUser.Text, txtProxyPassword.Text));
          }
          else
          {
            mySettings.Proxy = new System.Net.WebProxy(txtProxyAddress.Text, false, null, new System.Net.NetworkCredential(txtProxyUser.Text, txtProxyPassword.Text, txtProxyDomain.Text));
          }
        }
      }
      mySettings.TLSProxy = chkTLSProxy.Active;
      mySettings.SecurityProtocol = (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.None;
      if (chkNetworkProtocolSSL3.Active)
      {
        mySettings.SecurityProtocol |= (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.Ssl3;
      }
      if (chkNetworkProtocolTLS10.Active)
      {
        mySettings.SecurityProtocol |= (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.Tls10;
      }
      if (chkNetworkProtocolTLS11.Active)
      {
        mySettings.SecurityProtocol |= (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.Tls11;
      }
      if (chkNetworkProtocolTLS12.Active)
      {
        mySettings.SecurityProtocol |= (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.Tls12;
      }
      mySettings.SecurityEnforced = chkNetworkSecurityEnforce.Active;
      string sNetTestIco = System.IO.Path.Combine(modFunctions.AppDataPath, "netTest.png");
      try
      {
        if (System.IO.File.Exists(sNetTestIco))
          System.IO.File.Delete(sNetTestIco);
      }
      catch (Exception)
      {
      }
      if (icoNetTest != null)
      {
        icoNetTest.Save(sNetTestIco, "png");
      }
      if (optNetTestTestMyNet.Active)
      {
        mySettings.NetTestURL = "http://testmy.net";
      }
      else if (optNetTestSpeedTest.Active)
      {
        mySettings.NetTestURL = "http://speedtest.net";
      }
      else if (optNetTestCustom.Active)
      {
        mySettings.NetTestURL = txtNetTestCustom.Text;
      }
      else
      {
        mySettings.NetTestURL = null;
        try
        {
          if (System.IO.File.Exists(sNetTestIco))
            System.IO.File.Delete(sNetTestIco);
        }
        catch (Exception)
        {
        }
      }
      bool saveFail = false;
      if (!mySettings.Save())
        saveFail = true;
      if (!saveFail)
      {
        bHardChange = false;
        bSaved = true;
        cmdSave.Sensitive = false;
      }
    }
    protected void cmdClose_Click(object sender, EventArgs e)
    {
      this.Respond(Gtk.ResponseType.Close);
    }
    #endregion
    private bool SettingsChanged()
    {
      if (mySettings == null)
        return false;
      if (bHardChange)
        return true;
      if (string.Compare(mySettings.Account, txtAccount.Text, StringComparison.OrdinalIgnoreCase) != 0)
        return true;
      if (string.IsNullOrEmpty(mySettings.PassKey) | string.IsNullOrEmpty(mySettings.PassSalt))
      {
        return true;
      }
      else
      {
        if (RestrictionLibrary.StoredPassword.Decrypt(mySettings.PassCrypt, mySettings.PassKey, mySettings.PassSalt) != txtPassword.Text)
          return true;
      }
      if ((int)mySettings.StartWait - txtStartWait.Value != 0)
        return true;
      if ((int)mySettings.Interval - txtInterval.Value != 0)
        return true;
      if ((int)mySettings.Accuracy - txtAccuracy.Value != 0)
        return true;
      if ((int)mySettings.Timeout - txtTimeout.Value != 0)
        return true;
      if ((int)mySettings.Retries - txtRetries.Value != 0)
        return true;
      if (chkStartup.Active ^ File.Exists(modFunctions.StartupPath))
        return true;
      if (mySettings.ScaleScreen != chkScaleScreen.Active)
        return true;
      switch (mySettings.TrayIconStyle)
      {
        case TrayStyles.Always:
          if (!chkTrayIcon.Active | chkTrayMin.Active)
            return true;
          break;
        case TrayStyles.Minimized:
          if (!chkTrayIcon.Active | !chkTrayMin.Active)
            return true;
          break;
        case TrayStyles.Never:
          if (chkTrayIcon.Active)
            return true;
          break;
      }
      if (mySettings.TrayIconOnClose != chkTrayClose.Active)
        return true;
      if (mySettings.AutoHide != chkAutoHide.Active)
        return true;
      if (!modFunctions.DirectoryEqualityCheck(mySettings.HistoryDir, txtHistoryDir.CurrentFolder))
        return true;
      if (chkOverAlert.Active ^ (mySettings.Overuse > 0))
        return true;
      if (chkOverAlert.Active)
      {
        if ((int)mySettings.Overuse - txtOverSize.Value != 0)
          return true;
      }
      if ((int)mySettings.Overtime - txtOverTime.Value != 0)
        return true;
      if (mySettings.UpdateBETA != chkUpdateBETA.Active)
        return true;
      switch (cmbUpdateAutomation.Active)
      {
        case 0:
          if (mySettings.UpdateType != UpdateTypes.Auto)
            return true;
          break;
        case 1:
          if (mySettings.UpdateType != UpdateTypes.Ask)
            return true;
          break;
        case 2:
          if (mySettings.UpdateType != UpdateTypes.None)
            return true;
          break;
      }
      switch (cmbUpdateInterval.Active)
      {
        case 0:
          if (mySettings.UpdateTime != 1)
            return true;
          break;
        case 1:
          if (mySettings.UpdateTime != 3)
            return true;
          break;
        case 2:
          if (mySettings.UpdateTime != 7)
            return true;
          break;
        case 3:
          if (mySettings.UpdateTime != 15)
            return true;
          break;
        case 4:
          if (mySettings.UpdateTime != 30)
            return true;
          break;
      }
      if (mySettings.Proxy == null)
      {
        if (cmbProxyType.Active != 0)
          return true;
      }
      else if (mySettings.Proxy == System.Net.WebRequest.DefaultWebProxy)
      {
        if (cmbProxyType.Active != 1)
          return true;
      }
      else
      {
        if (cmbProxyType.Active == 0)
          return true;
        if (cmbProxyType.Active == 1)
          return true;
        Uri addr = ((System.Net.WebProxy)mySettings.Proxy).Address;
        if (cmbProxyType.Active == 2)
        {
          if (string.Compare(txtProxyAddress.Text, addr.Host, StringComparison.OrdinalIgnoreCase) != 0)
            return true;
          if ((int)txtProxyPort.Value - addr.Port != 0)
            return true;
        }
        if (cmbProxyType.Active == 3)
        {
          if (string.Compare(txtProxyAddress.Text, addr.OriginalString, StringComparison.OrdinalIgnoreCase) != 0)
            return true;
        }
        if (mySettings.Proxy.Credentials == null)
        {
          if (!string.IsNullOrEmpty(txtProxyUser.Text))
            return true;
          if (!string.IsNullOrEmpty(txtProxyPassword.Text))
            return true;
          if (!string.IsNullOrEmpty(txtProxyDomain.Text))
            return true;
        }
        else
        {
          if (string.IsNullOrEmpty(txtProxyUser.Text) & string.IsNullOrEmpty(txtProxyPassword.Text) & string.IsNullOrEmpty(txtProxyDomain.Text))
            return true;
          else
          {
            if (string.IsNullOrEmpty(txtProxyDomain.Text))
            {
              if (mySettings.Proxy.Credentials != new System.Net.NetworkCredential(txtProxyUser.Text, txtProxyPassword.Text))
                return true;
            }
            else
            {
              if (mySettings.Proxy.Credentials != new System.Net.NetworkCredential(txtProxyUser.Text, txtProxyPassword.Text, txtProxyDomain.Text))
                return true;
            }
          }
        }
      }
      if (mySettings.TLSProxy != chkTLSProxy.Active)
      {
        return true;
      }
      if ((((SecurityProtocolTypeEx)mySettings.SecurityProtocol & SecurityProtocolTypeEx.Ssl3) == SecurityProtocolTypeEx.Ssl3) == !chkNetworkProtocolSSL3.Active)
      {
        return true;
      }
      if ((((SecurityProtocolTypeEx)mySettings.SecurityProtocol & SecurityProtocolTypeEx.Tls10) == SecurityProtocolTypeEx.Tls10) == !chkNetworkProtocolTLS10.Active)
      {
        return true;
      }
      if ((((SecurityProtocolTypeEx)mySettings.SecurityProtocol & SecurityProtocolTypeEx.Tls11) == SecurityProtocolTypeEx.Tls11) == !chkNetworkProtocolTLS11.Active)
      {
        return true;
      }
      if ((((SecurityProtocolTypeEx)mySettings.SecurityProtocol & SecurityProtocolTypeEx.Tls12) == SecurityProtocolTypeEx.Tls12) == !chkNetworkProtocolTLS12.Active)
      {
        return true;
      }
      if (mySettings.SecurityEnforced == !chkNetworkSecurityEnforce.Active)
      {
        return true;
      }
      if (optNetTestNone.Active)
      {
        if (!string.IsNullOrEmpty(mySettings.NetTestURL))
          return true;
      }
      else if (optNetTestTestMyNet.Active)
      {
        if (mySettings.NetTestURL != "http://testmy.net")
          return true;
      }
      else if (optNetTestSpeedTest.Active)
      {
        if (mySettings.NetTestURL != "http://speedtest.net")
          return true;
      }
      else if (optNetTestCustom.Active)
      {
        if (mySettings.NetTestURL != txtNetTestCustom.Text)
          return true;
      }
      return false;
    }
    private void DoCheck()
    {
      /*
      if (bRemoteAcct)
      {
        txtInterval.Adjustment.Lower = 30;
      }
      else
      {
        txtInterval.Adjustment.Lower = 15;
      }
      */
      if (txtInterval.Value < txtInterval.Adjustment.Lower)
      {
        txtInterval.Value = txtInterval.Adjustment.Lower;
      }
      RunNetworkProtocolTest();
    }
  }
}
