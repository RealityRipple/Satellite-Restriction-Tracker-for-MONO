using System;
using System.Threading;
using RestrictionLibrary;
namespace RestrictionTrackerGTK
{
  public partial class dlgUpdate:
    Gtk.Dialog
  {
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
        lblTitle.LabelProp = modFunctions.ProductName + " BETA Update";
      }
      else
      {
        this.Title = "New Version Available";
        lblTitle.LabelProp = modFunctions.ProductName + " Update";
      }
      string newVer = "Version %v has been released and is available for download.\n" +
                      "To keep up-to-date with the latest features, improvements, bug fixes, and\n" +
                      "meter compliance, please update %p immediately.";
      newVer = newVer.Replace("%v", modFunctions.DisplayVersion(Version));
      newVer = newVer.Replace("%p", modFunctions.ProductName);
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
    protected void cmdChanges_Click(object sender, EventArgs e)
    {
      if (scrInfo.Visible)
      {
        ((Gtk.Label)cmdChanges.Child).LabelProp = "_Changes >>";
        scrInfo.Visible = false;
      }
      else
      {
        txtInfo.HeightRequest = 100;
        ((Gtk.Label)cmdChanges.Child).LabelProp = "_Changes <<";
        scrInfo.Visible = true;
        if (!txtInfo.Buffer.Text.StartsWith("Released:"))
        {
          cmdDownload.GrabFocus();
          cmdChanges.Sensitive = false;
          txtInfo.Buffer.Text = "Loading Update Information\n\nPlease Wait...";
          System.Threading.Thread tInfo = new Thread(GetVerInfo);
          tInfo.Start();
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
    private void GetVerInfo()
    {
      string sRet = null;
      WebClientEx sckVerInfo = new WebClientEx();
      sckVerInfo.KeepAlive = false;
      if (lblBETA.Visible)
      {
        sRet = sckVerInfo.DownloadString("http://update.realityripple.com/Satellite_Restriction_Tracker/infob");
      }
      else
      {
        sRet = sckVerInfo.DownloadString("http://update.realityripple.com/Satellite_Restriction_Tracker/info");
      }
      SetVerInfo(sRet);
    }
    private class VerInfoEventArgs
      : EventArgs
    {
      public string Message;
      public VerInfoEventArgs(string sMsg)
      {
        Message = sMsg;
      }
    }
    private void SetVerInfo(string Message)
    {
      Gtk.Application.Invoke(new object(), new VerInfoEventArgs(Message), Main_SetVerInfo);
    }
    private void Main_SetVerInfo(object o, EventArgs ea)
    {
      VerInfoEventArgs e = (VerInfoEventArgs)ea;
      txtInfo.Buffer.Text = e.Message;
      cmdChanges.Sensitive = true;
      cmdChanges.GrabFocus();
    }
  }
}
