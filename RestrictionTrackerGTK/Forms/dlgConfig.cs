using System;

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
    private uint tmrAnim;
    private const string LINK_PURCHASE = "<a href=\"http://srt.realityripple.com/c_signup.php\">Purchase a Key</a>";
    private const string LINK_PURCHASE_TT = "If you do not have a Product Key for the Remote Usage Service, you can purchase one online for as little as $15.00 a year.";
    private const string LINK_PANEL = "User Panel";
    private const string LINK_PANEL_TT = "Manage your Remote Usage Service account online.";
#region "Form Events"
    public dlgConfig()
    {
      bLoaded = false;
      this.Build();
      if (CurrentOS.IsMac)
      {
        ((Gtk.Box.BoxChild)this.ActionArea[cmdSave]).Position = 1;
        ((Gtk.Box.BoxChild)this.ActionArea[cmdClose]).Position = 0;
      }
      else
      {
        ((Gtk.Box.BoxChild)this.ActionArea[cmdSave]).Position = 0;
        ((Gtk.Box.BoxChild)this.ActionArea[cmdClose]).Position = 1;
      }
      DrawTitle();
      this.WindowStateEvent += HandleWindowStateEvent;
      txtAccount.Changed += txtAccount_Changed;
      txtPassword.ClipboardPasted += txtPassword_ClipboardPasted;

      cmbProvider.Changed += ValuesChanged;
      txtPassword.Changed += ValuesChanged;
      txtInterval.Changed += ValuesChanged;
      cmdPassDisplay.Toggled += cmdPassDisplay_Toggled;
      txtAccuracy.Changed += ValuesChanged;
      txtTimeout.Changed += ValuesChanged;
      chkScaleScreen.Clicked += ValuesChanged;
      txtHistoryDir.CurrentFolderChanged += txtHistoryDir_CurrentFolderChanged;
      txtOverSize.Changed += ValuesChanged;
      txtOverTime.Changed += ValuesChanged;
      chkBeta.Clicked += ValuesChanged;
      txtProxyAddress.Changed += ValuesChanged;
      txtProxyPort.Changed += ValuesChanged;
      txtProxyUser.Changed += ValuesChanged;
      txtProxyPassword.Changed += ValuesChanged;
      txtProxyDomain.Changed += ValuesChanged;
      chkProtocolSSL.Toggled += ValuesChanged;

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

      chkOverAlert.Clicked += chkOverAlert_Activated;
      cmbProxyType.Changed += cmbProxyType_Changed;

      evnKeyState.ButtonPressEvent += evnKeyState_ButtonPressEvent;



      evnSRT.EnterNotifyEvent += evnSRT_EnterNotifyEvent;
      cmdAlertStyle.Clicked += cmdAlertStyle_Click;
      cmdColors.Clicked += cmdColors_Click;
      cmdSave.Clicked += cmdSave_Click;
      cmdClose.Clicked += cmdClose_Click;

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
        pctKeyState.TooltipText = "Thank you for purchasing the Remote Usage Service for " + modFunctions.ProductName() + "!";
        lblPurchaseKey.Markup = "<a href=\"http://wb.realityripple.com?wbEMail=" + mySettings.Account + "&amp;wbKey=" + mySettings.RemoteKey + "&amp;wbSubmit=\">" + LINK_PANEL + "</a>";
        lblPurchaseKey.TooltipText = LINK_PANEL_TT;
      }
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
      chkScaleScreen.Active = mySettings.ScaleScreen;
      DoCheck();
      if (String.IsNullOrEmpty(mySettings.HistoryDir))
      {
        mySettings.HistoryDir = modFunctions.MySaveDir;
      }
      txtHistoryDir.SetCurrentFolder(mySettings.HistoryDir);
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
      chkBeta.Active = mySettings.BetaCheck;
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
      chkProtocolSSL.Active = (mySettings.Protocol == System.Net.SecurityProtocolType.Ssl3);
      bSaved = false;
      bAccount = false;
      cmdSave.Sensitive = false;
      this.Show();
      this.GdkWindow.SetDecorations(Gdk.WMDecoration.All | Gdk.WMDecoration.Maximize | Gdk.WMDecoration.Minimize | Gdk.WMDecoration.Resizeh | Gdk.WMDecoration.Menu);
      this.GdkWindow.Functions = Gdk.WMFunction.All | Gdk.WMFunction.Maximize | Gdk.WMFunction.Minimize | Gdk.WMFunction.Resize;
      DrawTitle();
      if (!CurrentOS.IsMac)
      {
        bLoaded = true;
      }
      this.Response += dlgConfig_Response;
      PopulateHostList();
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
            this.Respond(Gtk.ResponseType.No);
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

    void txtHistoryDir_CurrentFolderChanged(object sender, EventArgs e)
    {
      if (bLoaded)
      {
        cmdSave.Sensitive = true;
      }
      else if (CurrentOS.IsMac)
      {
        bLoaded = true;
      }
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
        cmdSave.Sensitive = true;
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
        Console.WriteLine((e.Event.State & Gdk.ModifierType.ControlMask));
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
        cmdSave.Sensitive = true;
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

    protected void chkOverAlert_Activated(object sender, EventArgs e)
    {
      txtOverSize.Sensitive = chkOverAlert.Active;
      lblOverSize.Sensitive = chkOverAlert.Active;
      txtOverTime.Sensitive = chkOverAlert.Active;
      lblOverTime.Sensitive = chkOverAlert.Active;
      if (bLoaded)
      {
        cmdSave.Sensitive = true;
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
        cmdSave.Sensitive = true;
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
        cmdSave.Sensitive = true;
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
        cmdSave.Sensitive = true;
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
        txtHistoryDir.SetCurrentFolder(modFunctions.MySaveDir);
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
      mySettings.Interval = txtInterval.ValueAsInt;
      mySettings.Accuracy = txtAccuracy.ValueAsInt;
      mySettings.Timeout = txtTimeout.ValueAsInt;
      mySettings.ScaleScreen = chkScaleScreen.Active;
      if (String.IsNullOrEmpty(mySettings.HistoryDir))
      {
        mySettings.HistoryDir = modFunctions.MySaveDir;
      }
      if (mySettings.HistoryDir != txtHistoryDir.CurrentFolder)
      {
        string[] sOldFiles = System.IO.Directory.GetFiles(mySettings.HistoryDir);
        string[] sNewFiles = {};
        if (System.IO.Directory.Exists(txtHistoryDir.CurrentFolder))
        {
          sNewFiles = System.IO.Directory.GetFiles(txtHistoryDir.CurrentFolder);
        }
        else
        {
          System.IO.Directory.CreateDirectory(txtHistoryDir.CurrentFolder);
        }
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
                  System.IO.File.Move(sFile, txtHistoryDir.CurrentFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(sFile));
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
                  if (System.IO.File.Exists(txtHistoryDir.CurrentFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(sFile)))
                  {
                    continue;
                  }
                  System.IO.File.Move(sFile, txtHistoryDir.CurrentFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(sFile));
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
                System.IO.File.Move(sFile, txtHistoryDir.CurrentFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(sFile));
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
              System.IO.File.Move(sFile, txtHistoryDir.CurrentFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(sFile));
            }
          }
        }
        mySettings.HistoryDir = txtHistoryDir.CurrentFolder;
        bAccount = true;
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
      mySettings.BetaCheck = chkBeta.Active;
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
      if (chkProtocolSSL.Active)
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
      if (bHardChange)
        return true;
      if (String.Compare(mySettings.Account, txtAccount.Text + "@" + cmbProvider.Entry.Text, true) != 0)
        return true;
      if (mySettings.PassCrypt != RestrictionLibrary.StoredPassword.EncryptApp(txtPassword.Text))
        return true;
      string sKey = txtKey1.Text + "-" + txtKey2.Text + "-" + txtKey3.Text + "-" + txtKey4.Text + "-" + txtKey5.Text;
      if (string.Compare(mySettings.RemoteKey, sKey, true) != 0)
        return true;
      if ((int) mySettings.Interval - txtInterval.Value != 0)
        return true;
      if ((int) mySettings.Accuracy - txtAccuracy.Value != 0)
        return true;
      if ((int) mySettings.Timeout - txtTimeout.Value != 0)
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
      if (mySettings.BetaCheck != chkBeta.Active)
        return true;

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
      if (mySettings.Protocol == System.Net.SecurityProtocolType.Ssl3 && !chkProtocolSSL.Active)
      {
        return true;
      }
      else if (mySettings.Protocol == System.Net.SecurityProtocolType.Tls && chkProtocolSSL.Active)
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

    private int drawPos;

    private bool tmrAnim_Tick()
    {
      Gdk.Size imgSize = new Gdk.Size(evnSRT.Allocation.Width, 45);
      int iPos = drawPos;
      if (iPos >= imgSize.Width)
      {
        DrawTitle();
        return true;
      }
      else
      {
        iPos += 2;
      }
      using (System.Drawing.Bitmap bmpAnim = new System.Drawing.Bitmap(imgSize.Width, imgSize.Height))
      {
        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmpAnim))
        {
          g.Clear(System.Drawing.Color.Black);
          g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
          g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
          g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
          g.FillRectangle(new System.Drawing.Drawing2D.LinearGradientBrush(new System.Drawing.Rectangle(0, 0, imgSize.Width, imgSize.Height), System.Drawing.Color.Black, System.Drawing.Color.DeepSkyBlue, System.Drawing.Drawing2D.LinearGradientMode.Vertical), 0, 0, imgSize.Width, imgSize.Height);
          try
          {
            System.Drawing.Font a12 = new System.Drawing.Font("Arial", 12);
            System.Drawing.Font a8 = new System.Drawing.Font("Arial", 8);
            System.Drawing.SizeF appSize = g.MeasureString("@" + modFunctions.ProductName() + "@", a12);
            System.Drawing.SizeF cmpSize = g.MeasureString("@" + modFunctions.CompanyName() + "@", a8);
            g.DrawString("                Restriction", a12, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(5, 7, (float)appSize.Width, (float)appSize.Height));
            g.DrawString("                Restriction", a12, System.Drawing.Brushes.White, new System.Drawing.RectangleF(4, 6, (float)appSize.Width, (float)appSize.Height));
            g.DrawIconUnstretched(new System.Drawing.Icon(GetType(), "Resources.norm.ico"), new System.Drawing.Rectangle(iPos, (imgSize.Height / 2) - 16, 32, 32));
            g.DrawString("Satellite                      Tracker", a12, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(5, 7, appSize.Width, appSize.Height));
            g.DrawString("Satellite                      Tracker", a12, System.Drawing.Brushes.White, new System.Drawing.RectangleF(4, 6, appSize.Width, appSize.Height));
            g.DrawString("by " + modFunctions.CompanyName(), a8, System.Drawing.Brushes.RoyalBlue, new System.Drawing.RectangleF(imgSize.Width - cmpSize.Width - 3, imgSize.Height - cmpSize.Height - 3, cmpSize.Width + 4, cmpSize.Height + 4));
            g.DrawString("by " + modFunctions.CompanyName(), a8, System.Drawing.Brushes.White, new System.Drawing.RectangleF(imgSize.Width - cmpSize.Width - 4, imgSize.Height - cmpSize.Height - 4, cmpSize.Width + 4, cmpSize.Height + 4));
          }
          catch (Exception)
          {
            g.DrawIconUnstretched(new System.Drawing.Icon(GetType(), "Resources.norm.ico"), new System.Drawing.Rectangle(iPos, (imgSize.Height / 2) - 16, 32, 32));
          }
        }
        pctSRT.Pixbuf = modFunctions.ImageToPixbuf(bmpAnim);
      }
      drawPos = iPos;
      return true;
    }

    protected void evnSRT_EnterNotifyEvent(object sender, Gtk.EnterNotifyEventArgs e)
    {
      if (tmrAnim == 0)
      {
        tmrAnim = GLib.Timeout.Add(75, tmrAnim_Tick);
      }
    }

    private void DrawTitle()
    {
      drawPos = -32;
      if (tmrAnim != 0)
      {
        GLib.Source.Remove(tmrAnim);
        tmrAnim = 0;
      }
      Gdk.Size imgSize = new Gdk.Size(evnSRT.Allocation.Width, 45);
      using (System.Drawing.Bitmap bmpAnim = new System.Drawing.Bitmap(imgSize.Width, imgSize.Height))
      {
        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmpAnim))
        {
          g.Clear(System.Drawing.Color.Black);
          g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
          g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
          g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
          g.FillRectangle(new System.Drawing.Drawing2D.LinearGradientBrush(new System.Drawing.Rectangle(0, 0, imgSize.Width, imgSize.Height), System.Drawing.Color.Black, System.Drawing.Color.DeepSkyBlue, System.Drawing.Drawing2D.LinearGradientMode.Vertical), 0, 0, imgSize.Width, imgSize.Height);
          try
          {
            System.Drawing.Font a12 = new System.Drawing.Font("Arial", 12);
            System.Drawing.Font a8 = new System.Drawing.Font("Arial", 8);
            System.Drawing.SizeF appSize = g.MeasureString("@" + modFunctions.ProductName() + "@", a12);
            System.Drawing.SizeF cmpSize = g.MeasureString("@" + modFunctions.CompanyName() + "@", a8);
            g.DrawString("                Restriction", a12, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(5, 7, (float)appSize.Width, (float)appSize.Height));
            g.DrawString("                Restriction", a12, System.Drawing.Brushes.White, new System.Drawing.RectangleF(4, 6, (float)appSize.Width, (float)appSize.Height));
            g.DrawString("Satellite                      Tracker", a12, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(5, 7, appSize.Width, appSize.Height));
            g.DrawString("Satellite                      Tracker", a12, System.Drawing.Brushes.White, new System.Drawing.RectangleF(4, 6, appSize.Width, appSize.Height));
            g.DrawString("by " + modFunctions.CompanyName(), a8, System.Drawing.Brushes.RoyalBlue, new System.Drawing.RectangleF(imgSize.Width - cmpSize.Width - 3, imgSize.Height - cmpSize.Height - 3, cmpSize.Width + 4, cmpSize.Height + 4));
            g.DrawString("by " + modFunctions.CompanyName(), a8, System.Drawing.Brushes.White, new System.Drawing.RectangleF(imgSize.Width - cmpSize.Width - 4, imgSize.Height - cmpSize.Height - 4, cmpSize.Width + 4, cmpSize.Height + 4));
          }
          catch (Exception)
          {

          }
        }
        pctSRT.Pixbuf = modFunctions.ImageToPixbuf(bmpAnim);
      }
    }
  }
}

