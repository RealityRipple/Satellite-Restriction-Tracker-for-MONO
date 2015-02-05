using System;
using System.IO;

namespace RestrictionTrackerGTK
{
  public partial class frmAbout : Gtk.Window
  {
    private string sUpdatePath = System.IO.Path.Combine(modFunctions.AppData, "Setup");
    private clsUpdate updateChecker;
    private uint tReset;
    private uint tSpeed;
    private Gtk.Button cmdUpdate;
    private delegate void MethodInvoker();
    #region "Form Events"
    public frmAbout() : base(Gtk.WindowType.Toplevel)
    {
      sUpdatePath = System.IO.Path.Combine(modFunctions.AppData, "Setup");
      if (CurrentOS.IsMac)
        sUpdatePath += ".dmg";
      else if (CurrentOS.IsLinux)
        sUpdatePath += ".bz2.sh";
      this.Build();
      this.WindowStateEvent += HandleWindowStateEvent;
      this.Title = "About " + modFunctions.ProductName;
      modFunctions.PrepareLink(lblProduct);
      lblProduct.Markup = "<a href=\"http://srt.realityripple.com/For_MONO/\">" + modFunctions.ProductName + "</a>";
      modFunctions.PrepareLink(lblVersion);
      lblVersion.Markup = "<a href=\"http://srt.realityripple.com/changes.php\">Version " + modFunctions.DisplayVersion(modFunctions.ProductVersion) + "</a>";
      modFunctions.PrepareLink(lblCompany);
      lblCompany.Markup = "<a href=\"http://realityripple.com/\">" + modFunctions.CompanyName + "</a>";
      txtDescription.Buffer.Text = "The RestrictionTracker utility monitors and logs ViaSat network usage and limits. It includes graphing software to let you monitor your usage history and predict future usage levels. All application coding by Andrew Sachen. This application is not endorsed by ViaSat, WildBlue, Exede, or any affiliate companies.";
      pctUpdate.PixbufAnimation = new Gdk.PixbufAnimation(null, "RestrictionTrackerGTK.Resources.throbber.gif");
      pctUpdate.Visible = false;
      pnlUpdate.Remove(lblUpdate);
      cmdUpdate = new Gtk.Button();
      cmdUpdate.Name = "cmdUpdate";
      cmdUpdate.UseUnderline = true;
      Gtk.Label cuLabel = new Gtk.Label();
      cuLabel.LabelProp = "Check for _Updates";
      cuLabel.UseUnderline = true;
      cmdUpdate.Add(cuLabel);
      pnlUpdate.Add(cmdUpdate);
      ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).Position = 2;
      cmdUpdate.ShowAll();
      cmdUpdate.Visible = true;
      cmdUpdate.TooltipText = "Check for a new version of " + modFunctions.ProductName + ".";
      ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).PackType = Gtk.PackType.Start;
      ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).Fill = false;
      ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).Expand = false;
      cmdUpdate.Clicked += cmdUpdate_Click;
      int upHeight = 24;
      lblProduct.HeightRequest = upHeight;
      lblVersion.HeightRequest = upHeight;
      lblCompany.HeightRequest = upHeight;
    }
    void HandleWindowStateEvent(object o, Gtk.WindowStateEventArgs args)
    {
      if (args.Event.ChangedMask == Gdk.WindowState.Iconified)
      {
        if ((args.Event.NewWindowState & Gdk.WindowState.Iconified) == Gdk.WindowState.Iconified)
        {
          args.Event.Window.Deiconify();
        }
      }
    }
    protected void OnHidden(object sender, EventArgs e)
    {
      if (tReset != 0)
      {
        GLib.Source.Remove(tReset);
        tReset = 0;
      }
      if (updateChecker != null)
      {
        updateChecker.Dispose();
        updateChecker = null;
      }
    }
    #endregion
    #region "Buttons"
    protected void cmdOK_Click(object sender, EventArgs e)
    {
      this.Hide();
    }
    protected void cmdDonate_Click(object sender, EventArgs e)
    {
      System.Diagnostics.Process.Start("http://realityripple.com/donate.php?itm=Satellite+Restriction+Tracker");
    }
    #endregion
    #region "Updates"
    protected void cmdUpdate_Click(object sender, EventArgs e)
    {
      switch (GetUpdateValue())
      {
        case "Check for Updates":
          byte lState = modDB.LOG_State;
          switch (lState)
          {
            case 0:
              SetUpdateValue("Update Skipped: Log is being read", false);
              RestartReset();
              break;
            case 1:
              SetUpdateValue("Initializing Update Check", true);
              MethodInvoker checkInvoker = BeginCheck;
              checkInvoker.BeginInvoke(null, null);
              break;
            case 2:
              SetUpdateValue("Update Skipped: Log is being saved", false);
              RestartReset();
              break;
            default:
              SetUpdateValue("Update Skipped: Log is being edited", false);
              RestartReset();
              break;
          }
          break;
        case "New Update Available":
          updateChecker.DownloadUpdate(sUpdatePath);
          break;
        case "New BETA Available":
          updateChecker.DownloadUpdate(sUpdatePath);
          break;
        case "Apply Update":
          if (File.Exists(sUpdatePath))
          {
            if (modFunctions.RunTerminal(sUpdatePath))
              Gtk.Application.Quit();
            else
            {
              System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(sUpdatePath));
              modFunctions.ShowMessageBox(this, "The Update failed to start.\n\nPlease run " + System.IO.Path.GetFileName(sUpdatePath) + " manually to update.", "Error Running Update", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
            }
          }
          else
          {
            SetUpdateValue("Update Failed: File was not saved", false);
            RestartReset();
          }
          break;
      }
    }
    private void RestartReset()
    {
      if (tReset != 0)
      {
        GLib.Source.Remove(tReset);
        tReset = 0;
      }
      tReset = GLib.Timeout.Add(3500, ResetUpdate); 
    }
    private void UpdateReset()
    {
      if (tReset != 0)
      {
        GLib.Source.Remove(tReset);
        tReset = 0;
      }
      tReset = GLib.Timeout.Add(1000, NewUpdate); 
    }
    private bool ResetUpdate()
    {
      if (tReset == 0)
      {
        return false;
      }
      if (tReset != 0)
      {
        GLib.Source.Remove(tReset);
        tReset = 0;
      }
      SetButtonUpdate("Check for _Updates", "Check for a new version of Satellite Restriction Tracker.");
      return false;
    }
    private bool NewUpdate()
    {
      Gtk.Application.Invoke(null, null, Main_NewUpdate);
      return false;
    }
    private void Main_NewUpdate(object o, EventArgs e)
    {
      if (tReset == 0)
      {
        return;
      }
      if (tReset != 0)
      {
        GLib.Source.Remove(tReset);
        tReset = 0;
      }
      if (CurrentOS.IsLinux)
        System.Diagnostics.Process.Start("chmod", "+x \"" + sUpdatePath + "\"");
      SetButtonUpdate("Apply _Update", modFunctions.ProductName + " must be restarted before the update can be applied.");
    }
    private void BeginCheck()
    {
      updateChecker = new clsUpdate();
      updateChecker.CheckingVersion += updateChecker_CheckingVersion;
      updateChecker.CheckProgressChanged += updateChecker_CheckProgressChanged;
      updateChecker.CheckResult += updateChecker_CheckResult;
      updateChecker.DownloadingUpdate+= updateChecker_DownloadingUpdate;
      updateChecker.UpdateProgressChanged += updateChecker_UpdateProgressChanged;
      updateChecker.DownloadResult += updateChecker_DownloadResult;
      updateChecker.CheckVersion();
    }

    private class UpdateValueEventArgs : EventArgs
    {
      public string Message;
      public bool Throbber;
      public string ToolTip;
      public UpdateValueEventArgs(string msg, bool throb)
      {
        Message = msg;
        Throbber = throb;
        ToolTip = msg;
      }
      public UpdateValueEventArgs(string msg, bool throb, string tt)
      {
        Message = msg;
        Throbber = throb;
        if (String.IsNullOrEmpty(tt))
          ToolTip = msg;
        else
          ToolTip = tt;
      }
    }
    private string GetUpdateValue()
    {
      return ((Gtk.Label)cmdUpdate.Child).Text;
    }
    private void SetUpdateValue(string Message, bool Throbber)
    {
      UpdateValueEventArgs e = new UpdateValueEventArgs(Message, Throbber);
      Gtk.Application.Invoke(null, (EventArgs)e, Main_SetUpdateValue);
    }
    private void SetUpdateValue(string Message, bool Throbber, string ToolTip)
    {
      UpdateValueEventArgs e = new UpdateValueEventArgs(Message, Throbber, ToolTip);
      Gtk.Application.Invoke(null, (EventArgs)e, Main_SetUpdateValue);
    }
    private void Main_SetUpdateValue(object o, EventArgs ea)
    {
      UpdateValueEventArgs e = (UpdateValueEventArgs)ea;
      if (cmdUpdate.Visible)
      {
        int upHeight = cmdUpdate.Allocation.Height;
        cmdUpdate.Visible = false;
        pnlUpdate.Remove(cmdUpdate);
        pnlUpdate.Add(lblUpdate);
        lblUpdate.Visible = true;
        lblUpdate.HeightRequest = upHeight;
      }
      if (pctUpdate.Visible != e.Throbber)
      {
        pctUpdate.Visible = e.Throbber;
      }
      lblUpdate.Markup = e.Message;
      lblUpdate.TooltipText = e.ToolTip;
      pctUpdate.TooltipText = e.ToolTip;
    }
    private void SetButtonUpdate(string Message, string ToolTip)
    {
      UpdateValueEventArgs e = new UpdateValueEventArgs(Message, false, ToolTip);
      Gtk.Application.Invoke(null, (EventArgs)e, Main_SetButtonUpdate);
    }
    private void Main_SetButtonUpdate(object o, EventArgs ea)
    {
      UpdateValueEventArgs e = (UpdateValueEventArgs)ea;
      if (lblUpdate.Visible)
      {
        pctUpdate.Visible = false;
        lblUpdate.Visible = false;
        pnlUpdate.Remove(lblUpdate);
        pnlUpdate.Add(cmdUpdate);
        ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).Position = 2;
        ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).PackType = Gtk.PackType.Start;
        ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).Fill = false;
        ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).Expand = false;
        cmdUpdate.Visible = true;
      }
      ((Gtk.Label)cmdUpdate.Child).LabelProp = e.Message;
      cmdUpdate.TooltipText = e.ToolTip;
    }
    protected void updateChecker_CheckingVersion(object sender, EventArgs e)
    {
      SetUpdateValue("Checking for Updates", true);
    }
    protected void updateChecker_CheckProgressChanged(object sender, clsUpdate.ProgressEventArgs e)
    {
      string sProgress = "(" + e.ProgressPercentage.ToString() + "%)";
      SetUpdateValue("Checking for Updates " + sProgress, true);
    }
    protected void updateChecker_CheckResult(object sender, clsUpdate.CheckEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_UpdateCheckerCheckResult);
    }
    private void Main_UpdateCheckerCheckResult(object sender, EventArgs ea)
    {
      clsUpdate.CheckEventArgs e = (clsUpdate.CheckEventArgs)ea;
      if (e.Error != null)
      {
        if (e.Error.Message.Contains("The remote name could not be resolved"))
        {
          SetUpdateValue("Unable to Connect: Server not Found", false);
          if (tReset != 0)
          {
            GLib.Source.Remove(tReset);
            tReset = 0;
          }
          tReset = GLib.Timeout.Add(3500, ResetUpdate);
        }
        else
        {
          SetUpdateValue(e.Error.Message, false);
          RestartReset();
        }
      }
      else if (e.Cancelled)
      {
        SetUpdateValue("Update Check Cancelled", false);
        RestartReset();
      }
      else
      {
        AppSettings mySettings = new AppSettings();
        dlgUpdate fUpdate;
        switch (e.Result)
        {
          case clsUpdate.CheckEventArgs.ResultType.NewUpdate:
            SetButtonUpdate("_New Update Available", "Click to begin download.");
            System.Threading.Thread.Sleep(0);
            fUpdate = new dlgUpdate();
            fUpdate.NewUpdate(e.Version, false);
            switch ((Gtk.ResponseType) fUpdate.Run())
            {
              case Gtk.ResponseType.Yes:
                updateChecker.DownloadUpdate(sUpdatePath);
                break;
              case Gtk.ResponseType.No:
                break;
              case Gtk.ResponseType.Ok:
                updateChecker.DownloadUpdate(sUpdatePath);
                mySettings.UpdateBETA = false;
                mySettings.Save();
                break;
              case Gtk.ResponseType.Cancel:
              mySettings.UpdateBETA = false;
                mySettings.Save();
                break;
              default:
                break;
            }
            fUpdate.Destroy();
            fUpdate.Dispose();
            fUpdate = null;
            break;
          case clsUpdate.CheckEventArgs.ResultType.NewBeta:
          if (mySettings.UpdateBETA)
            {
              SetButtonUpdate("_New BETA Available", "Click to begin download.");
              System.Threading.Thread.Sleep(0);
              fUpdate = new dlgUpdate();
              fUpdate.NewUpdate(e.Version, true);
              switch ((Gtk.ResponseType)fUpdate.Run())
              {
                case Gtk.ResponseType.Yes:
                  updateChecker.DownloadUpdate(sUpdatePath);
                  break;
                case Gtk.ResponseType.No:
                  break;
                case Gtk.ResponseType.Ok:
                  updateChecker.DownloadUpdate(sUpdatePath);
                  mySettings.UpdateBETA = false;
                  mySettings.Save();
                  break;
                case Gtk.ResponseType.Cancel:
                  mySettings.UpdateBETA = false;
                  mySettings.Save();
                  SetUpdateValue("No New Updates", false);
                  break;
                default:
                  break;
              }
              fUpdate.Destroy();
              fUpdate.Dispose();
              fUpdate = null;
            }
            else
            {
              SetUpdateValue("No New Updates", false);
              RestartReset();
            }
            break;
          case clsUpdate.CheckEventArgs.ResultType.NoUpdate:
            SetUpdateValue("No New Updates", false);
            RestartReset();
            break;
        }
        mySettings = null;
      }
    }
    void updateChecker_DownloadingUpdate (object sender, EventArgs e)
    {
      Gtk.Application.Invoke(sender, e, Main_UpdateCheckerDownloadingUpdate);
    }
    void Main_UpdateCheckerDownloadingUpdate(object sender, EventArgs e)
    {
      StartSpeed();
      SetUpdateValue("Downloading Update", true);
    }
    void updateChecker_DownloadResult (object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_UpdateCheckerDownloadResult);
    }
    void Main_UpdateCheckerDownloadResult(object sender, EventArgs ea)
    {
      System.ComponentModel.AsyncCompletedEventArgs e = (System.ComponentModel.AsyncCompletedEventArgs)ea;
      StopSpeed();
      if (e.Error != null)
        SetUpdateValue(e.Error.Message,false);
      else if (e.Cancelled)
      {
        if (updateChecker != null)
          updateChecker.Dispose();
        SetUpdateValue("Download Cancelled",false);
      }
      else
      {
        if (updateChecker != null)
          updateChecker.Dispose();
        SetUpdateValue("Download Complete",false);
        System.Threading.Thread.Sleep(0);
        if (File.Exists(sUpdatePath))
          UpdateReset();
        else
          SetUpdateValue("Update Failure",false);
      }
    }
    private long CurSize;
    private long TotalSize;
    private ulong DownSpeed;
    private int CurPercent;
    void updateChecker_UpdateProgressChanged (object sender, clsUpdate.ProgressEventArgs e)
    {
      Gtk.Application.Invoke(sender, (EventArgs)e, Main_UpdateCheckerUpdateProgressChanged);
    }
    void Main_UpdateCheckerUpdateProgressChanged(object sender, EventArgs ea)
    {
      clsUpdate.ProgressEventArgs e = (clsUpdate.ProgressEventArgs)ea;
      CurSize = e.BytesReceived;
      TotalSize = e.TotalBytesToReceive;
      CurPercent = e.ProgressPercentage;
    }
    private long LastSize;
    private void StopSpeed()
    {
      if (tSpeed != 0)
      {
        GLib.Source.Remove(tSpeed);
        tSpeed = 0;
      }
    }
    private void StartSpeed()
    {
      StopSpeed();
      tSpeed = GLib.Timeout.Add(1000, speed_tick); 
    }
    bool speed_tick()
    {
      if (tSpeed == 0)
      if (CurSize > LastSize)
        DownSpeed = (ulong) (CurSize - LastSize);
      else
        DownSpeed = 0ul;
      LastSize = CurSize;
      string sProgress = "(" + CurPercent + "%)";
      string sStatus = modFunctions.ByteSize((ulong) CurSize) + " of " + modFunctions.ByteSize((ulong) TotalSize) + " at " + modFunctions.ByteSize(DownSpeed) + "/s";
      if (TotalSize == 0)
      {
        sStatus = "Downloading Update (Waiting for Response)";
        sProgress = "(Waiting for Response)";
      }
      SetUpdateValue("Downloading Update " + sProgress, true, sStatus);
      return true;
    }
    #endregion
  }
}