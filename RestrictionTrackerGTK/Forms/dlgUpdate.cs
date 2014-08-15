using System;

namespace RestrictionTrackerGTK
{
  public partial class dlgUpdate : Gtk.Dialog
  {
    private RestrictionLibrary.CookieAwareWebClient sckVerInfo;
    private bool Ret;

    public dlgUpdate()
    {
      this.Build();
      if (CurrentOS.IsMac)
      {
        ((Gtk.Box.BoxChild)this.ActionArea[cmdDownload]).Position = 1;
        ((Gtk.Box.BoxChild)this.ActionArea[cmdCancel]).Position = 0;
      }
      else
      {
        ((Gtk.Box.BoxChild)this.ActionArea[cmdDownload]).Position = 0;
        ((Gtk.Box.BoxChild)this.ActionArea[cmdCancel]).Position = 1;
      }
      this.Show();
      this.GdkWindow.SetDecorations(Gdk.WMDecoration.All | Gdk.WMDecoration.Maximize | Gdk.WMDecoration.Minimize | Gdk.WMDecoration.Resizeh | Gdk.WMDecoration.Menu);
      this.GdkWindow.Functions = Gdk.WMFunction.All | Gdk.WMFunction.Maximize | Gdk.WMFunction.Minimize | Gdk.WMFunction.Resize;
      this.WindowStateEvent += HandleWindowStateEvent;
      sckVerInfo = new RestrictionLibrary.CookieAwareWebClient();
      sckVerInfo.DownloadStringCompleted += sckVerInfo_DownloadStringCompleted;
      sckVerInfo.Failure += sckVerInfo_Failure;
      Ret = false;


      this.Close += frmUpdate_FormClosing;
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

    public void NewUpdate(string Version, bool BETA)
    {
      Ret = false;
      if (BETA)
      {
        this.Title = "New BETA Version Available";
        lblTitle.LabelProp = modFunctions.ProductName() + " BETA Update";
      }
      else
      {
        this.Title = "New Version Available";
        lblTitle.LabelProp = modFunctions.ProductName() + " Update";
      }
      string newVer = "Version %v has been released and is available for download.\n" +
        "To keep up-to-date with the latest features, improvements, bug fixes, and\n" +
        "meter compliance, please update %p immediately.";
      newVer = newVer.Replace("%v", modFunctions.DisplayVersion(Version));
      newVer = newVer.Replace("%p", modFunctions.ProductName());
      lblNewVer.LabelProp = newVer;
      txtInfo.Buffer.Text = "Loading Update Information\n\nPlease Wait...";
      lblBETA.Visible = BETA;
      chkStopBETA.Visible = BETA;
    }

    protected void cmdDownload_Click(object o, EventArgs e)
    {
      Ret = true;
      if (chkStopBETA.Visible & chkStopBETA.Active)
      {
        this.Respond(Gtk.ResponseType.Ok);
      }
      else
      {
        this.Respond(Gtk.ResponseType.Yes);
      }
    }

    protected void cmdCancel_Click(object o, EventArgs e)
    {
      Ret = true;
      if (chkStopBETA.Visible & chkStopBETA.Active)
      {
        this.Respond(Gtk.ResponseType.Cancel);
      }
      else
      {
        this.Respond(Gtk.ResponseType.No);
      }
    }

    protected void cmdChanges_Click (object sender, EventArgs e)
    {
      if (scrInfo.Visible)
      {
        ((Gtk.Label)((Gtk.HBox)((Gtk.Alignment) cmdChanges.Children[0]).Children[0]).Children[1]).LabelProp = "_Changes >>";
        scrInfo.Visible = false;
      }
      else
      {
        txtInfo.HeightRequest = 100;
        ((Gtk.Label)((Gtk.HBox)((Gtk.Alignment) cmdChanges.Children[0]).Children[0]).Children[1]).LabelProp = "_Changes <<";
        scrInfo.Visible = true;
        if (!txtInfo.Buffer.Text.StartsWith("Released:"))
        {
          cmdDownload.GrabFocus();
          cmdChanges.Sensitive = false;
          txtInfo.Buffer.Text = "Loading Update Information\n\nPlease Wait...";
          Uri loadPage = new Uri("http://update.realityripple.com/Satellite_Restriction_Tracker/info");
          if (lblBETA.Visible)
          {
            loadPage = new Uri("http://update.realityripple.com/Satellite_Restriction_Tracker/infob");
          }
          sckVerInfo.DownloadStringAsync(loadPage);
        }
      }
    }

    protected void frmUpdate_FormClosing(object o, EventArgs e)
    {
      if (!Ret)
      {
        this.Respond(Gtk.ResponseType.No);
      }
    }

    private void sckVerInfo_DownloadStringCompleted(object o, System.Net.DownloadStringCompletedEventArgs e)
    {
      if (e.Cancelled)
      {
        txtInfo.Buffer.Text = "Info Request Cancelled";
      }
      else if (e.Error != null)
      {
        txtInfo.Buffer.Text = "Info Request Error\n" + e.Error.Message;
      }
      else
      {
        txtInfo.Buffer.Text = e.Result;
      }
      cmdChanges.Sensitive = true;
      cmdChanges.GrabFocus();
    }

    private void sckVerInfo_Failure(object o, RestrictionLibrary.CookieAwareWebClient.ErrorEventArgs e)
    {
      txtInfo.Buffer.Text = "Info Request Error\n" + e.Error.Message;
      cmdChanges.Sensitive = true;
      cmdChanges.GrabFocus();
    }


  }
}