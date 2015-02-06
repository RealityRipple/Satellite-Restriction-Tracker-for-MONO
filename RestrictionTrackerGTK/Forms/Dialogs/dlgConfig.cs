using System;
using System.IO;
namespace RestrictionTrackerGTK
{
  public partial class dlgConfig : Gtk.Dialog
  {
    private RestrictionLibrary.remoteRestrictionTracker remoteTest;
    private RestrictionLibrary.CookieAwareWebClient wsHostList;
    private bool bSaved, bAccount, bLoaded, bHardChange, bRemoteAcct;
    private AppSettings mySettings;
    private uint pChecker;
    private string checkKey;
    private const string LINK_PURCHASE = "<a href=\"http://srt.realityripple.com/c_signup.php\">Purchase a Remote Usage Service Subscription</a>";
    private const string LINK_PURCHASE_TT = "If you do not have a Product Key for the Remote Usage Service, you can purchase one online for as little as $15.00 a year.";
    private const string LINK_PANEL = "Visit the Remote Usage Service User Panel Page";
    private const string LINK_PANEL_TT = "Manage your Remote Usage Service account online.";
    #region "Form Events"
    public dlgConfig()
    {
      bLoaded = false;
      this.Build();
      if (CurrentOS.IsMac)
      {
        pctAccountViaSatIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.account_user.png");
        pctAccountProviderIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.account_provider.png");
        pctAccountKeyIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.account_key.png");
        pctPrefStartIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.prefs_power.png");
        pctPrefAccuracyIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.prefs_accuracy.png");
        pctPrefAlertIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.prefs_notify.png");
        pctPrefColorIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.prefs_colors.png");
        pctNetworkTimeoutIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.net_network.png");
        pctNetworkProxyIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.net_proxy.png");
        pctNetworkProtocolIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_security.png");
        pctNetworkUpdateIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.net_update.png");
        pctAdvancedDataIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_data.png");
        pctAdvancedInterfaceIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.os_x.advanced_interface.png");
        ((Gtk.Box.BoxChild)this.ActionArea[cmdSave]).Position = 1;
        ((Gtk.Box.BoxChild)this.ActionArea[cmdClose]).Position = 0;
      }
      else
      {
        ((Gtk.Box.BoxChild)this.ActionArea[cmdSave]).Position = 0;
        ((Gtk.Box.BoxChild)this.ActionArea[cmdClose]).Position = 1;
      }
      string sLocalPath = modFunctions.LocalAppData;
      if (sLocalPath.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Personal)))
        sLocalPath = "~" + sLocalPath.Substring(Environment.GetFolderPath(Environment.SpecialFolder.Personal).Length);
      if (sLocalPath.EndsWith("."))
        sLocalPath = sLocalPath.Substring(0, sLocalPath.Length - 1);
      optHistoryLocalConfig.Label = sLocalPath;
      optHistoryLocalConfig.TooltipMarkup = "Save History Data to the local <b>" + sLocalPath + "</b> directory.";
      lblAdvancedDataDescription.LabelProp = "Your usage data will be stored in this directory. By default, data is stored in the " + sLocalPath + " directory.";
      AddEventHandlers();
      mySettings = new AppSettings();
      string sAccount = mySettings.Account;
      string sUsername, sProvider;
      if (!String.IsNullOrEmpty(sAccount) && (sAccount.Contains("@") && sAccount.Contains(".")))
      {
        sUsername = sAccount.Substring(0, sAccount.LastIndexOf("@"));
        sProvider = sAccount.Substring(sAccount.LastIndexOf("@") + 1);
      }
      else
      {
        sUsername = sAccount;
        sProvider = "";
      }
      txtAccount.Text = sUsername;
      UseDefaultHostList();
      cmbProvider.Entry.Text = sProvider;
      if (mySettings.PassCrypt != null)
      {
        txtPassword.Text = RestrictionLibrary.StoredPassword.DecryptApp(mySettings.PassCrypt);
      }
      txtPassword.Visibility = false;
      switch (mySettings.AccountType)
      {
        case RestrictionLibrary.localRestrictionTracker.SatHostTypes.WildBlue_LEGACY:
          optAccountTypeWBL.Active = true;
          break;
        case RestrictionLibrary.localRestrictionTracker.SatHostTypes.WildBlue_EXEDE:
          optAccountTypeWBX.Active = true;
          break;
        case RestrictionLibrary.localRestrictionTracker.SatHostTypes.DishNet_EXEDE:
          optAccountTypeDNX.Active = true;
          break;
        case RestrictionLibrary.localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY:
          optAccountTypeRPL.Active = true;
          break;
        case RestrictionLibrary.localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE:
          optAccountTypeRPX.Active = true;
          break;
      }
      chkAccountTypeAuto.Active = !mySettings.AccountTypeForced;
      string sKey = mySettings.RemoteKey;
      if (sKey.Contains("-"))
      {
        string[] sKeys = sKey.Split('-');
        if (sKeys.Length == 5)
        {
          txtKey1.Text = sKeys[0].Trim();
          txtKey2.Text = sKeys[1].Trim();
          txtKey3.Text = sKeys[2].Trim();
          txtKey4.Text = sKeys[3].Trim();
          txtKey5.Text = sKeys[4].Trim();
        }
        else
        {
          txtKey1.Text = "";
          txtKey2.Text = "";
          txtKey3.Text = "";
          txtKey4.Text = "";
          txtKey5.Text = "";
        }
      }
      else
      {
        txtKey1.Text = "";
        txtKey2.Text = "";
        txtKey3.Text = "";
        txtKey4.Text = "";
        txtKey5.Text = "";
      }
      modFunctions.PrepareLink(lblPurchaseKey);
      if (txtKey1.Text.Length < 6 || txtKey2.Text.Length < 4 || txtKey3.Text.Length < 4 || txtKey4.Text.Length < 4 || txtKey5.Text.Length < 6)
      {
        bRemoteAcct = false;
        pctKeyState.Pixbuf = null;
        pctKeyState.PixbufAnimation = null;
        pctKeyState.TooltipText = "";
        lblPurchaseKey.Markup = LINK_PURCHASE;
        lblPurchaseKey.TooltipText = LINK_PURCHASE_TT;
      }
      else
      {
        bRemoteAcct = true;
        pctKeyState.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.ok.png");
        pctKeyState.TooltipText = "Thank you for purchasing the Remote Usage Service for " + modFunctions.ProductName + "!";
        lblPurchaseKey.Markup = "<a href=\"http://wb.realityripple.com?wbEMail=" + mySettings.Account + "&amp;wbKey=" + mySettings.RemoteKey + "&amp;wbSubmit=\">" + LINK_PANEL + "</a>";
        lblPurchaseKey.TooltipText = LINK_PANEL_TT;
      }
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
          if (String.IsNullOrEmpty(mCreds.Domain))
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
      chkNetworkProtocolSSL.Active = (mySettings.Protocol == System.Net.SecurityProtocolType.Ssl3);
      switch (mySettings.UpdateType)
      {
        case AppSettings.UpdateTypes.Auto:
          cmbUpdateAutomation.Active = 0;
          break;
        case AppSettings.UpdateTypes.Ask:
          cmbUpdateAutomation.Active = 1;
          break;
        case AppSettings.UpdateTypes.None:
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
      if (!hD.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
        hD += System.IO.Path.DirectorySeparatorChar;
      else if (string.Compare(hD, modFunctions.AppDataPath, true) == 0)
        optHistoryLocalConfig.  Active = true;
      else
        optHistoryCustom.Active = true;
      txtHistoryDir.SetCurrentFolder(mySettings.HistoryDir);


      chkScaleScreen.Active = mySettings.ScaleScreen;

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
      PopulateHostList();
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
      cmbProvider.Changed += ValuesChanged;
      chkAccountTypeAuto.Clicked += chkAccountTypeAuto_Clicked;
      optAccountTypeWBL.Clicked += ValuesChanged;
      optAccountTypeWBX.Clicked += ValuesChanged;
      optAccountTypeDNX.Clicked += ValuesChanged;
      optAccountTypeRPL.Clicked += ValuesChanged;
      optAccountTypeRPX.Clicked += ValuesChanged;
      //
      txtKey1.AddEvents((int)Gdk.EventType.KeyPress);
      txtKey1.Changed += txtProductKey_Changed;
      txtKey1.KeyPressEvent += txtProductKey_KeyPressEvent;
      txtKey1.TextInserted += txtProductKey_InsertText;
      txtKey2.AddEvents((int)Gdk.EventType.KeyPress);
      txtKey2.Changed += txtProductKey_Changed;
      txtKey2.KeyPressEvent += txtProductKey_KeyPressEvent;
      txtKey2.TextInserted += txtProductKey_InsertText;
      txtKey3.AddEvents((int)Gdk.EventType.KeyPress);
      txtKey3.Changed += txtProductKey_Changed;
      txtKey3.KeyPressEvent += txtProductKey_KeyPressEvent;
      txtKey3.TextInserted += txtProductKey_InsertText;
      txtKey4.AddEvents((int)Gdk.EventType.KeyPress);
      txtKey4.Changed += txtProductKey_Changed;
      txtKey4.KeyPressEvent += txtProductKey_KeyPressEvent;
      txtKey4.TextInserted += txtProductKey_InsertText;
      txtKey5.AddEvents((int)Gdk.EventType.KeyPress);
      txtKey5.Changed += txtProductKey_Changed;
      txtKey5.KeyPressEvent += txtProductKey_KeyPressEvent;
      txtKey5.TextInserted += txtProductKey_InsertText;
      evnKeyState.ButtonPressEvent += evnKeyState_ButtonPressEvent;
      //
      // Preferences
      //
      chkStartup.Clicked += ValuesChanged;
      txtStartWait.Changed += ValuesChanged;
      //
      txtInterval.Changed += ValuesChanged;
      txtAccuracy.Changed += ValuesChanged;
      //
      txtOverSize.Changed += ValuesChanged;
      txtOverTime.Changed += ValuesChanged;
      chkOverAlert.Clicked += chkOverAlert_Activated;
      cmdAlertStyle.Clicked += cmdAlertStyle_Click;
      //
      cmdColors.Clicked += cmdColors_Click;
      //
      // Network
      //
      txtTimeout.Changed += ValuesChanged;
      //
      cmbProxyType.Changed += cmbProxyType_Changed;
      txtProxyAddress.Changed += ValuesChanged;
      txtProxyPort.Changed += ValuesChanged;
      txtProxyUser.Changed += ValuesChanged;
      txtProxyPassword.Changed += ValuesChanged;
      txtProxyDomain.Changed += ValuesChanged;
      //
      chkNetworkProtocolSSL.Toggled += ValuesChanged;
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
      chkScaleScreen.Clicked += ValuesChanged;
      
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

    private bool RunAccountTest()
    {
      if (pChecker != 0)
      {
        GLib.Source.Remove(pChecker);
        pChecker = 0;
        remoteTest = new RestrictionLibrary.remoteRestrictionTracker(txtAccount.Text + "@" + cmbProvider.Entry.Text, "", checkKey, mySettings.Proxy, mySettings.Timeout, new DateTime(2001, 1, 1), modFunctions.AppData);
        remoteTest.Failure += remoteTest_Failure;
        remoteTest.OKKey += remoteTest_OKKey;
      }
      else
      {
        return false;
      }

      return false;
    }

    protected void dlgConfig_Response(object o, Gtk.ResponseArgs args)
    {
      this.Response -= dlgConfig_Response;
      if (bSaved)
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
      else if (cmdSave.Sensitive)
      {
        Gtk.ResponseType saveRet = modFunctions.ShowMessageBoxYNC(this, "Some settings have been changed but not saved.\n\nDo you want to save the changes to your configuration?", "Save Configuration?", Gtk.DialogFlags.Modal);
        if (saveRet == Gtk.ResponseType.Yes)
        {
          cmdSave.Click();
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
          System.Diagnostics.Process.Start(modFunctions.AppDataPath);
        else
          modFunctions.ShowMessageBox(this, "The directory \"" + modFunctions.AppDataPath + "\" does not exist.\nPlease save the configuration first.", "Missing Directory", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
      else
        if (Directory.Exists(txtHistoryDir.CurrentFolder))
          System.Diagnostics.Process.Start(txtHistoryDir.CurrentFolder);
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
      if (pChecker != 0)
      {
        bRemoteAcct = CheckState;
        GLib.Source.Remove(pChecker);
        pChecker = 0;
      }
      if (remoteTest != null)
      {
        bRemoteAcct = CheckState;
        remoteTest.Dispose();
        remoteTest = null;
      }
      lblPurchaseKey.Markup = LINK_PURCHASE;
      lblPurchaseKey.TooltipText = LINK_PURCHASE_TT;
      if (txtKey1.Text.Length < 6 || txtKey2.Text.Length < 4 || txtKey3.Text.Length < 4 || txtKey4.Text.Length < 4 || txtKey5.Text.Length < 6)
      {
        cmdSave.Sensitive = SettingsChanged();
      }
      else
      {
        KeyCheck();
      }
    }

    protected void txtProductKey_InsertText(object o, Gtk.TextInsertedArgs e)
    {
      e.RetVal = true;
      Gtk.Entry txtSender = (Gtk.Entry)o;
      txtSender.TextInserted -= txtProductKey_InsertText;
      txtSender.Text = txtSender.Text.ToUpper();
      txtSender.TextInserted += txtProductKey_InsertText; 
    }

    [GLib.ConnectBefore]
    protected void txtProductKey_KeyPressEvent(object o, Gtk.KeyPressEventArgs e)
    {
      Gtk.Entry txtSender = (Gtk.Entry)o;
      if ((e.Event.Key == Gdk.Key.v) && ((e.Event.State & Gdk.ModifierType.ControlMask) == Gdk.ModifierType.ControlMask))
      {
        Gtk.Clipboard cb = Gtk.Clipboard.Get(Gdk.Selection.Clipboard);

        if (!String.IsNullOrEmpty(cb.WaitForText()))
        {
          string sKey = cb.WaitForText();
          if (sKey.Contains("-"))
          {
            string[] sKeys = sKey.Split('-');
            if (sKeys.Length == 5)
            {
              txtKey1.Text = sKeys[0];
              txtKey2.Text = sKeys[1];
              txtKey3.Text = sKeys[2];
              txtKey4.Text = sKeys[3];
              txtKey5.Text = sKeys[4];
            }
            else
            {
              txtSender.Text = sKey;
            }
          }
          else
          {
            txtSender.Text = sKey;
          }
        }
        e.RetVal = true;
      }
      else if ((e.Event.Key == Gdk.Key.Delete) || ((e.Event.Key == Gdk.Key.x) & ((e.Event.State | Gdk.ModifierType.ControlMask) == Gdk.ModifierType.ControlMask)) || ((e.Event.Key == Gdk.Key.c) & ((e.Event.State | Gdk.ModifierType.ControlMask) == Gdk.ModifierType.ControlMask)) || (e.Event.Key == Gdk.Key.End) || (e.Event.Key == Gdk.Key.Home) || (e.Event.Key == Gdk.Key.leftarrow) || (e.Event.Key == Gdk.Key.rightarrow) || (e.Event.Key == Gdk.Key.Tab) || (e.Event.Key == Gdk.Key.Shift_L) || (e.Event.Key == Gdk.Key.Shift_R) || (e.Event.Key == Gdk.Key.Alt_L) || (e.Event.Key == Gdk.Key.Alt_R) || (e.Event.Key == Gdk.Key.Control_L) || (e.Event.Key == Gdk.Key.Control_R))
      {
        e.RetVal = false;
      }
      else if (e.Event.Key == Gdk.Key.BackSpace)
      {
        int start, end;
        txtSender.GetSelectionBounds(out start, out end);
        if ((String.IsNullOrEmpty(txtSender.Text)) && (Math.Abs(end - start) == 0))
        {
          if (txtSender == txtKey1)
          {
            e.RetVal = true;
          }
          else if (txtSender == txtKey2)
          {
            txtKey1.GrabFocus();
            txtKey1.DeleteText(txtKey1.Text.Length - 1, txtKey1.Text.Length);
            txtKey1.SelectRegion(txtKey1.Text.Length, txtKey1.Text.Length);
            e.RetVal = true;
          }
          else if (txtSender == txtKey3)
          {
            txtKey2.GrabFocus();
            txtKey2.DeleteText(txtKey2.Text.Length - 1, txtKey2.Text.Length);
            txtKey2.SelectRegion(txtKey2.Text.Length, txtKey2.Text.Length);
            e.RetVal = true;
          }
          else if (txtSender == txtKey4)
          {
            txtKey3.GrabFocus();
            txtKey3.DeleteText(txtKey3.Text.Length - 1, txtKey3.Text.Length);
            txtKey3.SelectRegion(txtKey3.Text.Length, txtKey3.Text.Length);
            e.RetVal = true;
          }
          else if (txtSender == txtKey5)
          {
            txtKey4.GrabFocus();
            txtKey4.DeleteText(txtKey4.Text.Length - 1, txtKey4.Text.Length);
            txtKey4.SelectRegion(txtKey4.Text.Length, txtKey4.Text.Length);
            e.RetVal = true;
          }

        }
      }
      else
      {
        int start, end;
        txtSender.GetSelectionBounds(out start, out end);
        if ((txtSender.Text.Length == txtSender.MaxLength) && (Math.Abs(end - start) == 0))
        {
          if (txtSender == txtKey1)
          {
            txtKey2.GrabFocus();
            txtKey2.Text = Convert.ToChar(e.Event.KeyValue).ToString();
            txtKey2.SelectRegion(txtKey2.Text.Length, txtKey2.Text.Length);
          }
          else if (txtSender == txtKey2)
          {
            txtKey3.GrabFocus();
            txtKey3.Text = Convert.ToChar(e.Event.KeyValue).ToString();
            txtKey3.SelectRegion(txtKey3.Text.Length, txtKey3.Text.Length);
          }
          else if (txtSender == txtKey3)
          {
            txtKey4.GrabFocus();
            txtKey4.Text = Convert.ToChar(e.Event.KeyValue).ToString();
            txtKey4.SelectRegion(txtKey4.Text.Length, txtKey4.Text.Length);
          }
          else if (txtSender == txtKey4)
          {
            txtKey5.GrabFocus();
            txtKey5.Text = Convert.ToChar(e.Event.KeyValue).ToString();
            txtKey5.SelectRegion(txtKey5.Text.Length, txtKey5.Text.Length);
          }
          e.RetVal = true;
        }
      }
    }

    protected void txtProductKey_Changed(object o, EventArgs e)
    {
      if (!bLoaded)
      {
        return;
      }
      if (pChecker != 0)
      {
        bRemoteAcct = CheckState;
        GLib.Source.Remove(pChecker);
        pChecker = 0;
      }
      if (remoteTest != null)
      {
        bRemoteAcct = CheckState;
        remoteTest.Dispose();
        remoteTest = null;
      }
      lblPurchaseKey.Markup = LINK_PURCHASE;
      lblPurchaseKey.TooltipText = LINK_PURCHASE_TT;
      if ((txtKey1.Text.Length < 6) || (txtKey2.Text.Length < 4) || (txtKey3.Text.Length < 4) || (txtKey4.Text.Length < 4) || (txtKey5.Text.Length < 6))
      {
        bRemoteAcct = false;
        pctKeyState.Pixbuf = null;
        pctKeyState.PixbufAnimation = null;
        pctKeyState.TooltipText = "";
        cmdSave.Sensitive = SettingsChanged();
        DoCheck();
      }
      else
      {
        KeyCheck();
      }
    }

    protected void cmdPassDisplay_Toggled(object sender, EventArgs e)
    {
      txtPassword.Visibility = !(txtPassword.Visibility);
    }

    protected void chkAccountTypeAuto_Clicked(object sender, EventArgs e)
    {
      optAccountTypeWBL.Sensitive = !chkAccountTypeAuto.Active;
      optAccountTypeDNX.Sensitive = !chkAccountTypeAuto.Active;
      optAccountTypeWBX.Sensitive = !chkAccountTypeAuto.Active;
      optAccountTypeRPL.Sensitive = !chkAccountTypeAuto.Active;
      optAccountTypeRPX.Sensitive = !chkAccountTypeAuto.Active;
      if (bLoaded)
      {
        cmdSave.Sensitive = SettingsChanged();
      }
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
        ((Gtk.Table.TableChild)pnlProxy[txtProxyAddress]).RightAttach = 2;
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

    protected void remoteTest_Failure(object sender, RestrictionLibrary.remoteRestrictionTracker.FailureEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_RemoteTestFailure);
    }

    private void Main_RemoteTestFailure(object sender, EventArgs ea)
    {
      RestrictionLibrary.remoteRestrictionTracker.FailureEventArgs e = (RestrictionLibrary.remoteRestrictionTracker.FailureEventArgs)ea;
      bool bToSave = true;
      if (!CheckState)
        bToSave = false;
      if (SettingsChanged())
        bToSave = false;
      bRemoteAcct = false;
      pctKeyState.PixbufAnimation = null;
      pctKeyState.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.error.png");
      string sErr = "There was an error verifying your key!";
      switch (e.Type)
      {
        case RestrictionLibrary.remoteRestrictionTracker.FailureEventArgs.FailType.BadLogin:
          sErr = "There was a server error. Please try again later.";
          break;
          case RestrictionLibrary.remoteRestrictionTracker.FailureEventArgs.FailType.BadPassword:
          sErr = "Your password is incorrect!";
          break;
          case RestrictionLibrary.remoteRestrictionTracker.FailureEventArgs.FailType.BadProduct:
          sErr = "Your Product Key is incorrect!";
          break;
          case RestrictionLibrary.remoteRestrictionTracker.FailureEventArgs.FailType.BadServer:
          sErr = "There was a fault double-checking the server. You may have a security issue.";
          break;
          case RestrictionLibrary.remoteRestrictionTracker.FailureEventArgs.FailType.NoData:
          sErr = "There is no data on your account yet!";
          break;
          case RestrictionLibrary.remoteRestrictionTracker.FailureEventArgs.FailType.NoPassword:
          sErr = "Your account has no password registered to it!";
          break;
          case RestrictionLibrary.remoteRestrictionTracker.FailureEventArgs.FailType.NoUsername:
          sErr = "Your account is not registered!";
          break;
          case RestrictionLibrary.remoteRestrictionTracker.FailureEventArgs.FailType.Network:
          sErr = "There was a connection related error. Please check your Internet connection. (" + e.Details + ")";
          break;
      }
      if (pChecker != 0)
      {
        GLib.Source.Remove(pChecker);
        pChecker = 0;
      }
      if (remoteTest != null)
      {
        remoteTest.Dispose();
        remoteTest = null;
      }
      pctKeyState.TooltipText = sErr;
      DoCheck();
      cmdSave.Sensitive = bToSave;
    }

    protected void remoteTest_OKKey(object sender, EventArgs e)
    {
      Gtk.Application.Invoke(sender, e, Main_RemoteTestOKKey);
    }

    private void Main_RemoteTestOKKey(object sender, EventArgs e)
    {
      bool bToSave = true;
      if (CheckState)
        bToSave = false;
      if (SettingsChanged())
        bToSave = true;
      bRemoteAcct = true;
      pctKeyState.PixbufAnimation = null;
      pctKeyState.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.ok.png");
      pctKeyState.TooltipText = "Your key has been verified!";
      lblPurchaseKey.Markup = "<a href=\"http://wb.realityripple.com?wbEMail=" + txtAccount.Text + "@" + cmbProvider.Entry.Text + "&amp;wbKey=" + txtKey1.Text + "-" + txtKey2.Text + "-" + txtKey3.Text + "-" + txtKey4.Text + "-" + txtKey5.Text + "&amp;wbSubmit=\">" + LINK_PANEL + "</a>";
      if (pChecker != 0)
      {
        GLib.Source.Remove(pChecker);
        pChecker = 0;
      }
      if (remoteTest != null)
      {
        remoteTest.Dispose();
        remoteTest = null;
      }
      lblPurchaseKey.TooltipText = LINK_PANEL_TT;
      DoCheck();
      cmdSave.Sensitive = bToSave;
    }

    private void PopulateHostList()
    {
      wsHostList = new RestrictionLibrary.CookieAwareWebClient();
      wsHostList.DownloadStringCompleted += wsHostList_DownloadStringCompleted;
      wsHostList.DownloadStringAsync(new Uri("http://wb.realityripple.com/hosts"), "GRAB");
    }

    void wsHostList_DownloadStringCompleted (object sender, System.Net.DownloadStringCompletedEventArgs e)
    {
      try
      {
        if ((string) e.UserState == "GRAB")
        {
          Gtk.Application.Invoke(sender, (EventArgs)e, Main_HostListDownloadStringCompleted);
        }
      }
      catch(Exception)
      {}
    }

    private void Main_HostListDownloadStringCompleted(object sender, EventArgs ea)
    {
      System.Net.DownloadStringCompletedEventArgs e = (System.Net.DownloadStringCompletedEventArgs)ea;
      if (e.Error == null && !e.Cancelled && !String.IsNullOrEmpty(e.Result))
      {
        try
        {
          if (e.Result.Contains("\n"))
          {
            String[] HostList = e.Result.Split('\n');
            bLoaded = false;
            ClearHostList();
            for (int i = 0; i < HostList.Length; i++)
            {
              cmbProvider.AppendText(HostList[i]);
            }
            if (mySettings.Account.Contains("@"))
            {
              string sProvider = mySettings.Account.Substring(mySettings.Account.LastIndexOf("@") + 1);
              cmbProvider.Entry.Text = sProvider;
            }
            bLoaded = true;
          }
        }
        catch
        {}
      }
    }

    private void ClearHostList()
    {
      Gtk.TreeIter ti= new Gtk.TreeIter();
      bool iter = cmbProvider.Model.GetIterFirst(out ti);
      while (iter)
      {
        cmbProvider.RemoveText(0);
        iter = cmbProvider.Model.GetIterFirst(out ti);
      }
    }

    private void UseDefaultHostList()
    {
      ClearHostList();
      cmbProvider.AppendText("wildblue.net");
      cmbProvider.AppendText("exede.net");
      cmbProvider.AppendText("dishmail.net");
      cmbProvider.AppendText("dish.net");
    }

    private void SaveToHostList(string Provider)
    {
      ResolveEventArgs er = new ResolveEventArgs(Provider);
      Gtk.Application.Invoke(null, (EventArgs) er, Main_SaveToHostList);
    }

    private void Main_SaveToHostList(object sender, EventArgs ea)
    {
      ResolveEventArgs e = (ResolveEventArgs)ea;
      string Provider = e.Name;
      if (wsHostList != null)
      {
        wsHostList.Dispose();
        wsHostList = null;
      }
      wsHostList = new RestrictionLibrary.CookieAwareWebClient();
      wsHostList.DownloadDataAsync(new Uri("http://wb.realityripple.com/hosts/?add=" + Provider), "UPDATE");
    }
    #endregion

    #region "Remote Service Results"
    protected void evnKeyState_ButtonPressEvent(object sender, Gtk.ButtonPressEventArgs e)
    {
      if(txtKey1.Text.Length == 6 && txtKey2.Text.Length == 4 && txtKey3.Text.Length == 4 && txtKey4.Text.Length == 4 && txtKey5.Text.Length == 6)
        KeyCheck();
    }
    bool CheckState;
    private void KeyCheck()
    {
      pctKeyState.PixbufAnimation = new Gdk.PixbufAnimation(null, "RestrictionTrackerGTK.Resources.throbber.gif");
      CheckState = bRemoteAcct;
      bRemoteAcct = false;
      pctKeyState.TooltipText = "Verifying your key...";
      string sKeyTest = txtKey1.Text + "-" + txtKey2.Text + "-" + txtKey3.Text + "-" + txtKey4.Text + "-" + txtKey5.Text;
      cmdSave.Sensitive = false;
      if (pChecker != 0)
      {
        GLib.Source.Remove(pChecker);
        pChecker = 0;
      }
      checkKey = sKeyTest;
      pChecker = GLib.Timeout.Add(500, RunAccountTest);
    }
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
      if (String.IsNullOrEmpty(txtAccount.Text))
      {
        modFunctions.ShowMessageBox(this, "Please enter your ViaSat account Username.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
        txtAccount.GrabFocus();
        return;
      }
      if (String.IsNullOrEmpty(cmbProvider.Entry.Text))
      {
        modFunctions.ShowMessageBox(this, "Please enter your ViaSat Provider address or select one from the list.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
        cmbProvider.GrabFocus();
        return;
      }
      if (String.IsNullOrEmpty(txtPassword.Text))
      {
        modFunctions.ShowMessageBox(this, "Please enter your ViaSat account Password.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
        txtPassword.GrabFocus();
        return;
      }
      if (String.IsNullOrEmpty(txtHistoryDir.CurrentFolder))
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
      if (cmbProvider.Entry.Text.ToLower().Contains("excede") ||
          cmbProvider.Entry.Text.ToLower().Contains("force") ||
          cmbProvider.Entry.Text.ToLower().Contains("mysso") ||
          cmbProvider.Entry.Text.ToLower().Contains("myexede") ||
          cmbProvider.Entry.Text.ToLower().Contains("my.exede"))
        cmbProvider.Entry.Text = "exede.net";
      if (string.Compare(mySettings.Account,txtAccount.Text + "@" + cmbProvider.Entry.Text, true) != 0)
      {
        mySettings.Account = txtAccount.Text + "@" + cmbProvider.Entry.Text;
        bAccount = true;
      }
      if (RestrictionLibrary.StoredPassword.DecryptApp(mySettings.PassCrypt) != txtPassword.Text)
      {
        mySettings.PassCrypt = RestrictionLibrary.StoredPassword.EncryptApp(txtPassword.Text);
        bAccount = true;
      }
      string sKey = txtKey1.Text + "-" + txtKey2.Text + "-" + txtKey3.Text + "-" + txtKey4.Text + "-" + txtKey5.Text;
      if (mySettings.RemoteKey != sKey)
      {
        if (bRemoteAcct)
        {
          mySettings.RemoteKey = sKey;
        }
        else
        {
          mySettings.RemoteKey = "";
        }
        bAccount = true;
      }
      modFunctions.RunOnStartup = chkStartup.Active;
      mySettings.StartWait = txtStartWait.ValueAsInt;
      mySettings.Interval = txtInterval.ValueAsInt;
      mySettings.Accuracy = txtAccuracy.ValueAsInt;
      mySettings.Timeout = txtTimeout.ValueAsInt;
      mySettings.ScaleScreen = chkScaleScreen.Active;
      if (String.IsNullOrEmpty(mySettings.HistoryDir))
      {
        mySettings.HistoryDir = modFunctions.MySaveDir(true);
      }
      if (mySettings.HistoryDir != txtHistoryDir.CurrentFolder)
      {
        bool continueChange = true;
        string[] sOldFiles = Directory.GetFiles(mySettings.HistoryDir);
        string[] sNewFiles = {};
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
                    if (System.IO.Path.GetFileName(sNew).ToLower() == "user.config")
                    {
                      continue;
                    }
                    if (System.IO.Path.GetFileName(sNew).ToLower() == "del.bat")
                    {
                      continue;
                    }
                    sOverWrites.Add(System.IO.Path.GetFileName(sNew));
                  }
                }
              }
              if (sOverWrites.Count > 0)
              {
                if (modFunctions.ShowMessageBox(this, "Files exist in the new Data Directory:\n" + String.Join("\n", sOverWrites.ToArray()) + "\n\nOverwrite them?", "Overwrite Files?", Gtk.DialogFlags.Modal, Gtk.MessageType.Question, Gtk.ButtonsType.YesNo) == Gtk.ResponseType.Yes)
                {
                  foreach (string sFile in sOldFiles)
                  {
                    if (System.IO.Path.GetFileName(sFile).ToLower() == "user.config")
                    {
                      continue;
                    }
                    if (System.IO.Path.GetFileName(sFile).ToLower() == "del.bat")
                    {
                      continue;
                    }
                    File.Move(sFile, System.IO.Path.Combine(txtHistoryDir.CurrentFolder, System.IO.Path.GetFileName(sFile)));
                  }
                }
                else
                {
                  foreach (string sFile in sOldFiles)
                  {
                    if (System.IO.Path.GetFileName(sFile).ToLower() == "user.config")
                    {
                      continue;
                    }
                    if (System.IO.Path.GetFileName(sFile).ToLower() == "del.bat")
                    {
                      continue;
                    }
                    if (File.Exists(System.IO.Path.Combine(txtHistoryDir.CurrentFolder, System.IO.Path.GetFileName(sFile))))
                    {
                      continue;
                    }
                    File.Move(sFile, System.IO.Path.Combine(txtHistoryDir.CurrentFolder, System.IO.Path.GetFileName(sFile)));
                  }
                }
              }
              else
              {
                foreach (string sFile in sOldFiles)
                {
                  if (System.IO.Path.GetFileName(sFile).ToLower() == "user.config")
                  {
                    continue;
                  }
                  if (System.IO.Path.GetFileName(sFile).ToLower() == "del.bat")
                  {
                    continue;
                  }
                  File.Move(sFile, System.IO.Path.Combine(txtHistoryDir.CurrentFolder, System.IO.Path.GetFileName(sFile)));
                }
              }
            }
            else
            {
              foreach (string sFile in sOldFiles)
              {
                if (System.IO.Path.GetFileName(sFile).ToLower() == "user.config")
                {
                  continue;
                }
                if (System.IO.Path.GetFileName(sFile).ToLower() == "del.bat")
                {
                  continue;
                }
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
          if (!hD.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            hD += System.IO.Path.DirectorySeparatorChar;
          if (string.Compare(hD, modFunctions.AppDataPath, true) == 0)
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
          mySettings.UpdateType = AppSettings.UpdateTypes.Auto;
          break;
        case 1:
          mySettings.UpdateType = AppSettings.UpdateTypes.Ask;
          break;
        case 2:
          mySettings.UpdateType = AppSettings.UpdateTypes.None;
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
        if (String.IsNullOrEmpty(txtProxyAddress.Text))
        {
          modFunctions.ShowMessageBox(this, "Please enter a Proxy address or choose a different option.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
          txtProxyAddress.GrabFocus();
          return;
        }
        if (String.IsNullOrEmpty(txtProxyUser.Text) && String.IsNullOrEmpty(txtProxyPassword.Text) && String.IsNullOrEmpty(txtProxyDomain.Text))
        {
          mySettings.Proxy = new System.Net.WebProxy(txtProxyAddress.Text, txtProxyPort.ValueAsInt);
        }
        else
        {
          if (String.IsNullOrEmpty(txtProxyDomain.Text))
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
        if (String.IsNullOrEmpty(txtProxyAddress.Text))
        {
          modFunctions.ShowMessageBox(this, "Please enter a Proxy address or choose a different option.", "", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
          txtProxyAddress.GrabFocus();
          return;
        }
        if (String.IsNullOrEmpty(txtProxyUser.Text) && String.IsNullOrEmpty(txtProxyPassword.Text) && String.IsNullOrEmpty(txtProxyDomain.Text))
        {
          mySettings.Proxy = new System.Net.WebProxy(txtProxyAddress.Text);
        }
        else
        {
          if (String.IsNullOrEmpty(txtProxyDomain.Text))
          {
            mySettings.Proxy = new System.Net.WebProxy(txtProxyAddress.Text, false, null, new System.Net.NetworkCredential(txtProxyUser.Text, txtProxyPassword.Text));
          }
          else
          {
            mySettings.Proxy = new System.Net.WebProxy(txtProxyAddress.Text, false, null, new System.Net.NetworkCredential(txtProxyUser.Text, txtProxyPassword.Text, txtProxyDomain.Text));
          }
        }
      }
      if (chkNetworkProtocolSSL.Active)
      {
        mySettings.Protocol = System.Net.SecurityProtocolType.Ssl3;
      }
      else
      {
        mySettings.Protocol = System.Net.SecurityProtocolType.Tls;
      }
      mySettings.Save();
      bHardChange = false;
      bSaved = true;
      cmdSave.Sensitive = false;
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
      if (String.Compare(mySettings.Account, txtAccount.Text + "@" + cmbProvider.Entry.Text, true) != 0)
        return true;
      if (mySettings.PassCrypt != RestrictionLibrary.StoredPassword.EncryptApp(txtPassword.Text))
        return true;
      if (mySettings.AccountTypeForced == chkAccountTypeAuto.Active)
        return true;
      if (!chkAccountTypeAuto.Active)
      {
        switch (mySettings.AccountType)
        {
          case RestrictionLibrary.localRestrictionTracker.SatHostTypes.WildBlue_LEGACY:
            if (!optAccountTypeWBL.Active)
              return true;
            break;
          case RestrictionLibrary.localRestrictionTracker.SatHostTypes.WildBlue_EXEDE:
            if (!optAccountTypeWBX.Active)
              return true;
            break;
          case RestrictionLibrary.localRestrictionTracker.SatHostTypes.DishNet_EXEDE:
            if (!optAccountTypeDNX.Active)
              return true;
            break;
          case RestrictionLibrary.localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY:
            if (!optAccountTypeRPL.Active)
              return true;
            break;
          case RestrictionLibrary.localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE:
            if (!optAccountTypeRPX.Active)
              return true;
            break;
        }
      }
      string sKey = txtKey1.Text + "-" + txtKey2.Text + "-" + txtKey3.Text + "-" + txtKey4.Text + "-" + txtKey5.Text;
      if (string.Compare(mySettings.RemoteKey, sKey, true) != 0)
        return true;
      if ((int)mySettings.StartWait - txtStartWait.Value != 0)
        return true;
      if ((int) mySettings.Interval - txtInterval.Value != 0)
        return true;
      if ((int) mySettings.Accuracy - txtAccuracy.Value != 0)
        return true;
      if ((int) mySettings.Timeout - txtTimeout.Value != 0)
        return true;
      if (chkStartup.Active ^ File.Exists(modFunctions.StartupPath))
        return true;
      if (mySettings.ScaleScreen != chkScaleScreen.Active)
        return true;
      if (string.Compare(mySettings.HistoryDir, txtHistoryDir.CurrentFolder, true) != 0)
        return true;
      if (chkOverAlert.Active ^ (mySettings.Overuse > 0))
        return true;
      if (chkOverAlert.Active)
      {
        if ((int)mySettings.Overuse - txtOverSize.Value != 0)
          return true;
      }
      if ((int) mySettings.Overtime - txtOverTime.Value != 0)
        return true;
      if (mySettings.UpdateBETA != chkUpdateBETA.Active)
        return true;
      switch (cmbUpdateAutomation.Active)
      {
        case 0:
          if (mySettings.UpdateType != AppSettings.UpdateTypes.Auto)
            return true;
          break;
        case 1:
          if (mySettings.UpdateType != AppSettings.UpdateTypes.Ask)
            return true;
          break;
        case 2:
          if (mySettings.UpdateType != AppSettings.UpdateTypes.None)
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
          if (String.Compare(txtProxyAddress.Text, addr.Host) != 0)
            return true;
          if ((int) txtProxyPort.Value - addr.Port != 0)
            return true;
        }
        if (cmbProxyType.Active == 3)
        {
          if (string.Compare(txtProxyAddress.Text, addr.OriginalString) != 0)
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
          if (string.IsNullOrEmpty(txtProxyUser.Text) && string.IsNullOrEmpty(txtProxyPassword.Text) && string.IsNullOrEmpty(txtProxyDomain.Text))
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
      if (mySettings.Protocol == System.Net.SecurityProtocolType.Ssl3 && !chkNetworkProtocolSSL.Active)
      {
        return true;
      }
      else if (mySettings.Protocol == System.Net.SecurityProtocolType.Tls && chkNetworkProtocolSSL.Active)
      {
        return true;
      }
      return false;
    }

    private void DoCheck()
    {
      if (bRemoteAcct)
      {
        txtInterval.Adjustment.Lower = 30;
      }
      else
      {
        txtInterval.Adjustment.Lower = 15;
      }
      if (txtInterval.Value < txtInterval.Adjustment.Lower)
      {
        txtInterval.Value = txtInterval.Adjustment.Lower;
      }
    }
   
  }
}

