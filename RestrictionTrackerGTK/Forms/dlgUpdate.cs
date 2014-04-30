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

      Uri loadPage = new Uri("http://update.realityripple.com/Satellite_Restriction_Tracker/info");
      if (lblBETA.Visible)
      {
        loadPage = new Uri("http://update.realityripple.com/Satellite_Restriction_Tracker/infob");
      }
      sckVerInfo.DownloadStringAsync(loadPage);
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
      }
      else
      {
        this.Title = "New Version Available";
      }
      lblNewVer.Text = modFunctions.ProductName() + " Update\nVersion " + modFunctions.DisplayVersion(Version);
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
    }

    private void sckVerInfo_Failure(object o, RestrictionLibrary.CookieAwareWebClient.ErrorEventArgs e)
    {
      txtInfo.Buffer.Text = "Info Request Error\n" + e.Error.Message;
    }
  }
}