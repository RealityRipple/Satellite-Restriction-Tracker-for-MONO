using System;

namespace RestrictionTrackerGTK
{
	public partial class frmAbout : Gtk.Window
	{
    private clsUpdate updateChecker;
    private uint tReset;
    private Gtk.Button cmdUpdate;
    private delegate void MethodInvoker();

#region "Form Events"
		public frmAbout() : base(Gtk.WindowType.Toplevel)
    {
      this.Build();
      this.WindowStateEvent += HandleWindowStateEvent;
      this.Title = "About " + modFunctions.ProductName();
      lblProduct.Markup = "<a href=\"http://srt.realityripple.com/For_MONO/\">" + modFunctions.ProductName() + "</a>";
      lblVersion.Markup = "<a href=\"http://srt.realityripple.com/changes.php\">Version " + modFunctions.DisplayVersion(modFunctions.ProductVersion()) + "</a>";
      lblCompany.Markup = "<a href=\"http://realityripple.com/\">" + modFunctions.CompanyName() + "</a>";
      txtDescription.Buffer.Text = "The RestrictionTracker utility monitors and logs ViaSat network usage and limits. It includes graphing software to let you monitor your usage history and predict future usage levels. All application coding by Andrew Sachen. This application is not endorsed by ViaSat, WildBlue, Exede, or any affiliate companies.";
      pctUpdate.PixbufAnimation = new Gdk.PixbufAnimation(null, "RestrictionTrackerGTK.Resources.throbber.gif");
      pctUpdate.Visible = false;
      pnlUpdate.Remove(lblUpdate);
      cmdUpdate = new Gtk.Button();
      cmdUpdate.Name = "cmdUpdate";
      cmdUpdate.UseUnderline = true;
      Gtk.Alignment cuAlign = new Gtk.Alignment(0.5f, 0.5f, 0f, 0f);
      Gtk.HBox cuBox = new Gtk.HBox();
      cuBox.Spacing = 2;
      Gtk.Image cuImage = new Gtk.Image();
      cuImage.Pixbuf = new Gdk.Pixbuf(null, "RestrictionTrackerGTK.Resources.web.png");
      cuBox.Add(cuImage);
      Gtk.Label cuLabel = new Gtk.Label();
      cuLabel.LabelProp = "Check for Updates";
      cuLabel.UseUnderline = true;
      cuBox.Add(cuLabel);
      cuAlign.Add(cuBox);
      cmdUpdate.Add(cuAlign);
      pnlUpdate.Add(cmdUpdate);
      ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).Position = 2;
      cmdUpdate.ShowAll();
      cmdUpdate.Visible = true;

      cmdUpdate.TooltipText = "Check for a new version of Satellite Restriction Tracker";

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
      if (((Gtk.Label)((Gtk.HBox)((Gtk.Alignment)cmdUpdate.Child).Child).Children[1]).Text == "Check for Updates")
      {
        int upHeight = cmdUpdate.Allocation.Height;
        cmdUpdate.Visible = false;
        pnlUpdate.Remove(cmdUpdate);
        pnlUpdate.Add(lblUpdate);
        lblUpdate.Visible = true;
        lblUpdate.HeightRequest = upHeight;
        byte lState = modDB.LOG_State;
        switch (lState)
        {
          case 0:
            SetUpdateValue("Update Skipped: Log is being read", false);
            if (tReset != 0)
            {
              GLib.Source.Remove(tReset);
              tReset = 0;
            }
            tReset = GLib.Timeout.Add(3500, ResetUpdate); 
            break;
          case 1:
            SetUpdateValue("Initializing Update Check", true);
            MethodInvoker checkInvoker = BeginCheck;
            checkInvoker.BeginInvoke(null, null);
            break;
          case 2:
            SetUpdateValue("Update Skipped: Log is being saved", false);
            if (tReset != 0)
            {
              GLib.Source.Remove(tReset);
              tReset = 0;
            }
            tReset = GLib.Timeout.Add(3500, ResetUpdate); 
            break;
          default:
            SetUpdateValue("Update Skipped: Log is being edited", false);
            if (tReset != 0)
            {
              GLib.Source.Remove(tReset);
              tReset = 0;
            }
            tReset = GLib.Timeout.Add(3500, ResetUpdate); 
            break;
        }
      }
      else if (((Gtk.Label)((Gtk.HBox)((Gtk.Alignment)cmdUpdate.Child).Child).Children[1]).Text == "Visit Website")
      {
        System.Diagnostics.Process.Start("http://srt.realityripple.com/For_MONO/");
      }
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
      lblUpdate.Visible = false;
      pnlUpdate.Remove(lblUpdate);
      pnlUpdate.Add(cmdUpdate);
      ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).Position = 2;
      ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).PackType = Gtk.PackType.Start;
      ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).Fill = false;
      ((Gtk.Box.BoxChild)pnlUpdate[cmdUpdate]).Expand = false;
      cmdUpdate.Visible = true;
      ((Gtk.Label)((Gtk.HBox)((Gtk.Alignment)cmdUpdate.Child).Child).Children[1]).Text = "Check for Updates";
      cmdUpdate.TooltipText = "Check for a new version of Satellite Restriction Tracker.";
      return false;
    }

    private void NewUpdate()
    {
      Gtk.Application.Invoke(null, null, Main_NewUpdate);
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
      lblUpdate.Visible = false;
      pnlUpdate.Remove(lblUpdate);
      pnlUpdate.Add(cmdUpdate);
      cmdUpdate.Visible = true;
      ((Gtk.Label)((Gtk.HBox)((Gtk.Alignment)cmdUpdate.Child).Child).Children[1]).Text = "Visit Website";
      cmdUpdate.TooltipText = "Download the latest version from RealityRipple.com.";
    }

    private void BeginCheck()
    {
      updateChecker = new clsUpdate();
      updateChecker.CheckingVersion += updateChecker_CheckingVersion;
      updateChecker.CheckProgressChanged += updateChecker_CheckProgressChanged;
      updateChecker.CheckResult += updateChecker_CheckResult;
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
      if (pctUpdate.Visible != e.Throbber)
      {
        pctUpdate.Visible = e.Throbber;
      }
      lblUpdate.Markup = e.Message;
      lblUpdate.TooltipText = e.ToolTip;
      pctUpdate.TooltipText = e.ToolTip;
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
          if (tReset != 0)
          {
            GLib.Source.Remove(tReset);
            tReset = 0;
          }
          tReset = GLib.Timeout.Add(3500, ResetUpdate); 
        }
      }
      else if (e.Cancelled)
      {
        SetUpdateValue("Update Check Cancelled", false);
        if (tReset != 0)
        {
          GLib.Source.Remove(tReset);
          tReset = 0;
        }
        tReset = GLib.Timeout.Add(3500, ResetUpdate); 
      }
      else
      {
        AppSettings mySettings = new AppSettings();
        switch (e.Result)
        {
          case clsUpdate.CheckEventArgs.ResultType.NewUpdate:
            SetUpdateValue("<a href=\"http://srt.realityripple.com/For_MONO/\">New Update Available</a>", false, "New Update Available");
            System.Threading.Thread.Sleep(0);
            dlgUpdate fUpdate = new dlgUpdate();
            fUpdate.NewUpdate(e.Version, false);
            switch ((Gtk.ResponseType)fUpdate.Run())
            {
              case Gtk.ResponseType.Yes:
                System.Diagnostics.Process.Start("http://srt.realityripple.com/For_MONO/");
                if (tReset != 0)
                {
                  GLib.Source.Remove(tReset);
                  tReset = 0;
                }
                tReset = GLib.Timeout.Add(3500, ResetUpdate); 
                break;
              case Gtk.ResponseType.No:
                break;
              case Gtk.ResponseType.Ok:
                System.Diagnostics.Process.Start("http://srt.realityripple.com/For_MONO/");
                if (tReset != 0)
                {
                  GLib.Source.Remove(tReset);
                  tReset = 0;
                }
                tReset = GLib.Timeout.Add(3500, ResetUpdate); 
                mySettings.BetaCheck = false;
                mySettings.Save();
                break;
              case Gtk.ResponseType.Cancel:
                mySettings.BetaCheck = false;
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
            if (mySettings.BetaCheck)
            {
              SetUpdateValue("<a href=\"http://srt.realityripple.com/For_MONO/\">New BETA Available</a>", false);
              System.Threading.Thread.Sleep(0);
              fUpdate.NewUpdate(e.Version, false);
              switch ((Gtk.ResponseType)fUpdate.Run())
              {
                case Gtk.ResponseType.Yes:
                  System.Diagnostics.Process.Start("http://srt.realityripple.com/For_MONO/");
                  if (tReset != 0)
                  {
                    GLib.Source.Remove(tReset);
                    tReset = 0;
                  }
                  tReset = GLib.Timeout.Add(3500, ResetUpdate); 
                  break;
                case Gtk.ResponseType.No:
                  break;
                case Gtk.ResponseType.Ok:
                  System.Diagnostics.Process.Start("http://srt.realityripple.com/For_MONO/");
                  if (tReset != 0)
                  {
                    GLib.Source.Remove(tReset);
                    tReset = 0;
                  }
                  tReset = GLib.Timeout.Add(3500, ResetUpdate); 
                  mySettings.BetaCheck = false;
                  mySettings.Save();
                  break;
                case Gtk.ResponseType.Cancel:
                  mySettings.BetaCheck = false;
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
            }
            break;
          case clsUpdate.CheckEventArgs.ResultType.NoUpdate:
            SetUpdateValue("No New Updates", false);
            break;
        }
      }
    }
#endregion 
	}
}