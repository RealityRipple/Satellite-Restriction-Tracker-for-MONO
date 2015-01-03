using System;
using RestrictionLibrary;
using System.Drawing;

namespace RestrictionTrackerGTK
{
  public partial class dlgCustomColors : Gtk.Dialog
  {
    private long lDown;
    private long lUp;
    private long lDownLim;
    private long lUpLim;
    private int iD;
    private int iU;
    private bool dDown;
    private bool dUp;
    internal AppSettings mySettings;
    private bool HasSaved = false;
    private localRestrictionTracker.SatHostTypes DisplayAs;
    private System.Collections.Generic.List<DataBase.DataRow> FakeData;
    private Random rGen;
    private Gtk.ColorButton cbSelected;
    private Gtk.Menu mnuColorOpts;
    private Gtk.MenuItem mnuChoose;
    private Gtk.SeparatorMenuItem mnuSpace;
    private Gtk.MenuItem mnuDefault;
    private Gtk.Menu mnuDefaultOpts;
    private Gtk.MenuItem mnuThisDefault;
    private Gtk.MenuItem mnuGraphDefault;
    private Gtk.MenuItem mnuAllDefault;
    private localRestrictionTracker.SatHostTypes useStyle = localRestrictionTracker.SatHostTypes.Other;
    internal dlgCustomColors(AppSettings settings)
    {
      mySettings = settings;
      rGen = new Random();
      mnuColorOpts = new Gtk.Menu();
      mnuChoose = new Gtk.MenuItem("_Choose Color");
      mnuSpace = new Gtk.SeparatorMenuItem();
      mnuDefault = new Gtk.MenuItem("Use _Default Color for");
      mnuDefaultOpts = new Gtk.Menu();
      mnuDefault.Submenu = mnuDefaultOpts;
      mnuThisDefault = new Gtk.MenuItem("This _Color");
      mnuGraphDefault = new Gtk.MenuItem("This _Graph");
      mnuAllDefault = new Gtk.MenuItem("_All Graphs");
      mnuColorOpts.Append(mnuChoose);
      mnuColorOpts.Append(mnuSpace);
      mnuDefaultOpts.Append(mnuThisDefault);
      mnuDefaultOpts.Append(mnuGraphDefault);
      mnuDefaultOpts.Append(mnuAllDefault);
      mnuColorOpts.Append(mnuDefault);

      mnuChoose.Activated += mnuChoose_Click;
      mnuThisDefault.Activated += mnuDefault_Click;
      mnuGraphDefault.Activated += mnuGraphDefault_Click;
      mnuAllDefault.Activated += mnuAllDefault_Click;

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
      this.WindowStateEvent += HandleWindowStateEvent;

      if (lDownLim == 0 & lUpLim == 0)
      {
        lDown = 8;
        lDownLim = 16;
        dDown = true;
        iD = 1;
        lUp = 4;
        lUpLim = 16;
        dUp = true;
        iU = 1;
      }
      useStyle = mySettings.AccountType;
      switch (useStyle)
      {
        case localRestrictionTracker.SatHostTypes.WildBlue_LEGACY:
        case localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY:
          lblMainTitle.Text = "Main Window Current Usage Graphs";
          lblMainDownTitle.Text = "Download Colors";
          lblMainUpTitle.Text = "Upload Colors";
          grpMainUp.Visible = true;
          lblTrayTitle.Text = "Tray Icon Current Usage Graph Overlay";
          lblTrayDownTitle.Text = "Download Colors";
          lblTrayUpTitle.Text = "Upload Colors";
          grpTrayUp.Visible = true;
          lblHistoryTitle.Text = "History Window Graphs";
          lblHistoryDownTitle.Text = "Download Colors";
          lblHistoryUpTitle.Text = "Upload Colors";
          grpHistoryUp.Visible = true;
          lblHistoryUpMax.Visible = true;
          cmdHistoryUpMax.Visible = true;
          DisplayAs = localRestrictionTracker.SatHostTypes.WildBlue_LEGACY;
          SetTextBGAlignments(true);
          break;
        case localRestrictionTracker.SatHostTypes.DishNet_EXEDE:
          lblMainTitle.Text = "Main Window Current Usage Graphs";
          lblMainDownTitle.Text = "Anytime Colors";
          lblMainUpTitle.Text = "Off-Peak Colors";
          grpMainUp.Visible = true;
          lblTrayTitle.Text = "Tray Icon Current Usage Graph Overlay";
          lblTrayDownTitle.Text = "Anytime Colors";
          lblTrayUpTitle.Text = "Off-Peak Colors";
          grpTrayUp.Visible = true;
          lblHistoryTitle.Text = "History Window Graphs";
          lblHistoryDownTitle.Text = "Anytime Colors";
          lblHistoryUpTitle.Text = "Off-Peak Colors";
          grpHistoryUp.Visible = true;
          lblHistoryUpMax.Visible = true;
          cmdHistoryUpMax.Visible = true;
          DisplayAs = localRestrictionTracker.SatHostTypes.DishNet_EXEDE;
          SetTextBGAlignments(true);
          break;
        case localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE:
        case localRestrictionTracker.SatHostTypes.WildBlue_EXEDE:
          lblMainTitle.Text = "Main Window Graph";
          lblMainDownTitle.Text = "Usage Colors";
          grpMainUp.Visible = false;
          lblTrayTitle.Text = "Tray Icon Graph";
          lblTrayDownTitle.Text = "Usage Colors";
          grpTrayUp.Visible = false;
          lblHistoryTitle.Text = "History Graph";
          lblHistoryDownTitle.Text = "Usage Colors";
          grpHistoryUp.Visible = false;
          lblHistoryUpMax.Visible = false;
          cmdHistoryUpMax.Visible = false;
          DisplayAs = localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE;
          SetTextBGAlignments(false);
          break;
        default:
          lblMainTitle.Text = "Main Window Current Usage Graphs";
          lblMainDownTitle.Text = "Download Colors";
          lblMainUpTitle.Text = "Upload Colors";
          grpMainUp.Visible = true;
          lblTrayTitle.Text = "Tray Icon Current Usage Graph Overlay";
          lblTrayDownTitle.Text = "Download Colors";
          lblTrayUpTitle.Text = "Upload Colors";
          grpTrayUp.Visible = true;
          lblHistoryTitle.Text = "History Window Graphs";
          lblHistoryDownTitle.Text = "Download Colors";
          lblHistoryUpTitle.Text = "Upload Colors";
          grpHistoryUp.Visible = true;
          lblHistoryUpMax.Visible = true;
          cmdHistoryUpMax.Visible = true;
          DisplayAs = localRestrictionTracker.SatHostTypes.Other;
          SetTextBGAlignments(true);
          break;
      }
      cmdMainDownA.ColorSet += cmdColor_SelectedColor;
      cmdMainDownA.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdMainDownB.ColorSet += cmdColor_SelectedColor;
      cmdMainDownB.ButtonReleaseEvent += cmdColor_MouseUp;
      chkMainDownB.Toggled += chkB_CheckedChanged;
      cmdMainDownC.ColorSet += cmdColor_SelectedColor;
      cmdMainDownC.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdMainUpA.ColorSet += cmdColor_SelectedColor;
      cmdMainUpA.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdMainUpB.ColorSet += cmdColor_SelectedColor;
      cmdMainUpB.ButtonReleaseEvent += cmdColor_MouseUp;
      chkMainUpB.Toggled += chkB_CheckedChanged;
      cmdMainUpC.ColorSet += cmdColor_SelectedColor;
      cmdMainUpC.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdMainText.ColorSet += cmdColor_SelectedColor;
      cmdMainBG.ButtonReleaseEvent += cmdColor_MouseUp;

      cmdTrayDownA.ColorSet += cmdColor_SelectedColor;
      cmdTrayDownA.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdTrayDownB.ColorSet += cmdColor_SelectedColor;
      cmdTrayDownB.ButtonReleaseEvent += cmdColor_MouseUp;
      chkTrayDownB.Toggled += chkB_CheckedChanged;
      cmdTrayDownC.ColorSet += cmdColor_SelectedColor;
      cmdTrayDownC.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdTrayUpA.ColorSet += cmdColor_SelectedColor;
      cmdTrayUpA.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdTrayUpB.ColorSet += cmdColor_SelectedColor;
      cmdTrayUpB.ButtonReleaseEvent += cmdColor_MouseUp;
      chkTrayUpB.Toggled += chkB_CheckedChanged;
      cmdTrayUpC.ColorSet += cmdColor_SelectedColor;
      cmdTrayUpC.ButtonReleaseEvent += cmdColor_MouseUp;

      cmdHistoryDownA.ColorSet += cmdColor_SelectedColor;
      cmdHistoryDownA.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryDownB.ColorSet += cmdColor_SelectedColor;
      cmdHistoryDownB.ButtonReleaseEvent += cmdColor_MouseUp;
      chkHistoryDownB.Toggled += chkB_CheckedChanged;
      cmdHistoryDownC.ColorSet += cmdColor_SelectedColor;
      cmdHistoryDownC.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryDownMax.ColorSet += cmdColor_SelectedColor;
      cmdHistoryDownMax.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryUpA.ColorSet += cmdColor_SelectedColor;
      cmdHistoryUpA.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryUpB.ColorSet += cmdColor_SelectedColor;
      cmdHistoryUpB.ButtonReleaseEvent += cmdColor_MouseUp;
      chkHistoryUpB.Toggled += chkB_CheckedChanged;
      cmdHistoryUpC.ColorSet += cmdColor_SelectedColor;
      cmdHistoryUpC.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryUpMax.ColorSet += cmdColor_SelectedColor;
      cmdHistoryUpMax.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryText.ColorSet += cmdColor_SelectedColor;
      cmdHistoryBG.ButtonReleaseEvent += cmdColor_MouseUp;

      if (mySettings.Colors.MainDownA == Color.Transparent)
      {
        mnuAllDefault_Click(mnuAllDefault, new EventArgs());
      }
      else
      {
        SetElColor(ref cmdMainDownA, mySettings.Colors.MainDownA);
        SetElColor(ref cmdMainDownB, mySettings.Colors.MainDownB);
        SetElColor(ref cmdMainDownC, mySettings.Colors.MainDownC);
        SetElColor(ref cmdMainUpA, mySettings.Colors.MainUpA);
        SetElColor(ref cmdMainUpB, mySettings.Colors.MainUpB);
        SetElColor(ref cmdMainUpC, mySettings.Colors.MainUpC);
        SetElColor(ref cmdMainText, mySettings.Colors.MainText);
        SetElColor(ref cmdMainBG, mySettings.Colors.MainBackground);

        SetElColor(ref cmdTrayDownA, mySettings.Colors.TrayDownA);
        SetElColor(ref cmdTrayDownB, mySettings.Colors.TrayDownB);
        SetElColor(ref cmdTrayDownC, mySettings.Colors.TrayDownC);
        SetElColor(ref cmdTrayUpA, mySettings.Colors.TrayUpA);
        SetElColor(ref cmdTrayUpB, mySettings.Colors.TrayUpB);
        SetElColor(ref cmdTrayUpC, mySettings.Colors.TrayUpC);

        SetElColor(ref cmdHistoryDownA, mySettings.Colors.HistoryDownA);
        SetElColor(ref cmdHistoryDownB, mySettings.Colors.HistoryDownB);
        SetElColor(ref cmdHistoryDownC, mySettings.Colors.HistoryDownC);
        SetElColor(ref cmdHistoryDownMax, mySettings.Colors.HistoryDownMax);
        SetElColor(ref cmdHistoryUpA, mySettings.Colors.HistoryUpA);
        SetElColor(ref cmdHistoryUpB, mySettings.Colors.HistoryUpB);
        SetElColor(ref cmdHistoryUpC, mySettings.Colors.HistoryUpC);
        SetElColor(ref cmdHistoryUpMax, mySettings.Colors.HistoryUpMax);
        SetElColor(ref cmdHistoryText, mySettings.Colors.HistoryText);
        SetElColor(ref cmdHistoryBG, mySettings.Colors.HistoryBackground);
      }
      RedrawImages();
      cmdSave.Sensitive = SettingsChanged();
      HasSaved = false;
      this.Show();
      this.GdkWindow.SetDecorations(Gdk.WMDecoration.All | Gdk.WMDecoration.Maximize | Gdk.WMDecoration.Minimize | Gdk.WMDecoration.Resizeh | Gdk.WMDecoration.Menu);
      this.GdkWindow.Functions = Gdk.WMFunction.All | Gdk.WMFunction.Maximize | Gdk.WMFunction.Minimize | Gdk.WMFunction.Resize;
      RedrawImages();
      this.Response += dlgCustomColors_Response;
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

    protected void dlgCustomColors_Response(object o, Gtk.ResponseArgs args)
    {
      this.Response -= dlgCustomColors_Response;
      if (cmdSave.Sensitive)
      {
        Gtk.ResponseType saveRet = modFunctions.ShowMessageBoxYNC(this, "Some settings have been changed but not saved.\n\nDo you want to save the changes to your color scheme?", "Save Colors?", Gtk.DialogFlags.Modal);
        if (saveRet == Gtk.ResponseType.Yes)
        {
          cmdSave.Click();
        }
        else if (saveRet == Gtk.ResponseType.No)
        {
          this.Respond(Gtk.ResponseType.No);
        }
        else if (saveRet == Gtk.ResponseType.Cancel)
        {
          this.Respond(Gtk.ResponseType.None);
          this.Response += dlgCustomColors_Response;
          return;
        }
      }
      if (HasSaved)
      {
        this.Respond(Gtk.ResponseType.Yes);
      }
      else
      {
        this.Respond(Gtk.ResponseType.No);
      }
    }

    private void RedrawImages()
    {
      Color mda = modFunctions.GdkColorToDrawingColor(cmdMainDownA.Color);
      Color mdb = modFunctions.GdkColorToDrawingColor(cmdMainDownB.Color);
      if (!chkMainDownB.Active)
      {
        mdb = Color.Transparent;
      }
      Color mdc = modFunctions.GdkColorToDrawingColor(cmdMainDownC.Color);

      Color mua = modFunctions.GdkColorToDrawingColor(cmdMainUpA.Color);
      Color mub = modFunctions.GdkColorToDrawingColor(cmdMainUpB.Color);
      if (!chkMainUpB.Active)
      {
        mub = Color.Transparent;
      }
      Color muc = modFunctions.GdkColorToDrawingColor(cmdMainUpC.Color);

      Color mt = modFunctions.GdkColorToDrawingColor(cmdMainText.Color);
      Color mbg = modFunctions.GdkColorToDrawingColor(cmdMainBG.Color);

     
      Color tda = modFunctions.GdkColorToDrawingColor(cmdTrayDownA.Color);
      Color tdb = modFunctions.GdkColorToDrawingColor(cmdTrayDownB.Color);
      if (!chkTrayDownB.Active)
      {
        tdb = Color.Transparent;
      }
      Color tdc = modFunctions.GdkColorToDrawingColor(cmdTrayDownC.Color);

      Color tua = modFunctions.GdkColorToDrawingColor(cmdTrayUpA.Color);
      Color tub = modFunctions.GdkColorToDrawingColor(cmdTrayUpB.Color);
      if (!chkTrayUpB.Active)
      {
        tub = Color.Transparent;
      }
      Color tuc = modFunctions.GdkColorToDrawingColor(cmdTrayUpC.Color);

     
      Color hda = modFunctions.GdkColorToDrawingColor(cmdHistoryDownA.Color);
      Color hdb = modFunctions.GdkColorToDrawingColor(cmdHistoryDownB.Color);
      if (!chkHistoryDownB.Active)
      {
        hdb = Color.Transparent;
      }
      Color hdc = modFunctions.GdkColorToDrawingColor(cmdHistoryDownC.Color);
      Color hdm = modFunctions.GdkColorToDrawingColor(cmdHistoryDownMax.Color);

      Color hua = modFunctions.GdkColorToDrawingColor(cmdHistoryUpA.Color);
      Color hub = modFunctions.GdkColorToDrawingColor(cmdHistoryUpB.Color);
      if (!chkHistoryUpB.Active)
      {
        hub = Color.Transparent;
      }
      Color huc = modFunctions.GdkColorToDrawingColor(cmdHistoryUpC.Color);
      Color hum = modFunctions.GdkColorToDrawingColor(cmdHistoryUpMax.Color);

      Color ht = modFunctions.GdkColorToDrawingColor(cmdHistoryText.Color);
      Color hbg = modFunctions.GdkColorToDrawingColor(cmdHistoryBG.Color);

      int iWidth = pctMain.Allocation.Size.Width ; 
      int iHeight = pctMain.Allocation.Size.Height;
      int iHalfW = (int) Math.Floor(iWidth / 2d);
      int iHalfH = (int) Math.Floor(iHeight / 2d);
     
      Graphics g;
      if (DisplayAs == localRestrictionTracker.SatHostTypes.WildBlue_LEGACY)
      {
        Rectangle FakeMRect = new Rectangle(0, 0, iWidth - 1, iHeight * 2);
        Image FakeD = modFunctions.DisplayProgress(modFunctions.DrawingSizeToGdkSize(FakeMRect.Size), lDown, lDownLim, mySettings.Accuracy, mda, mdb, mdc, mt, mbg);
        Image FakeU = modFunctions.DisplayProgress(modFunctions.DrawingSizeToGdkSize(FakeMRect.Size), lUp, lUpLim, mySettings.Accuracy, mua, mub, muc, mt, mbg);
        Bitmap fakeI = new Bitmap(pctHistory.Allocation.Width, pctHistory.Allocation.Height);
        g = Graphics.FromImage(fakeI);
        g.Clear(Color.Black);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        System.Drawing.Rectangle dRect = new System.Drawing.Rectangle(0, 0, iHalfW, iHeight );
        System.Drawing.Rectangle uRect = new System.Drawing.Rectangle(iHalfW + 1, 0, iHalfW, iHeight);
        g.DrawImage(FakeD, dRect, FakeMRect, GraphicsUnit.Pixel);
        g.DrawImage(FakeU, uRect, FakeMRect, GraphicsUnit.Pixel);
        pctMain.Pixbuf = modFunctions.ImageToPixbuf(fakeI);
      }
      else if (DisplayAs == localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE)
      {
        pctMain.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayRProgress(pctMain.Allocation.Size, lDown, lDownLim, mySettings.Accuracy, mda, mdb, mdc, mt, mbg));
      }
      else if (DisplayAs == localRestrictionTracker.SatHostTypes.DishNet_EXEDE)
      {
        Rectangle FakeMRect = new Rectangle(0, 0, iWidth - 1, iHeight * 2);
        Image FakeD = modFunctions.DisplayProgress(modFunctions.DrawingSizeToGdkSize(FakeMRect.Size), lDown, lDownLim, mySettings.Accuracy, mda, mdb, mdc, mt, mbg);
        Image FakeU = modFunctions.DisplayProgress(modFunctions.DrawingSizeToGdkSize(FakeMRect.Size), lUp, lUpLim, mySettings.Accuracy, mua, mub, muc, mt, mbg);
        Bitmap fakeI = new Bitmap(pctHistory.Allocation.Width, pctHistory.Allocation.Height);
        g = Graphics.FromImage(fakeI);
        g.Clear(Color.Black);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        Rectangle dRect = new Rectangle(0, 0, iHalfW, iHeight);
        Rectangle uRect = new Rectangle(iHalfW + 1, 0, iHalfW, iHeight);
        g.DrawImage(FakeD, dRect, FakeMRect, GraphicsUnit.Pixel);
        g.DrawImage(FakeU, uRect, FakeMRect, GraphicsUnit.Pixel);
        pctMain.Pixbuf = modFunctions.ImageToPixbuf(fakeI);
      }
      else
      {
        pctMain.SetFromStock(Gtk.Stock.DialogError, Gtk.IconSize.Dialog);
      }
      Bitmap imgTray = new Bitmap(16, 16);
      g = Graphics.FromImage(imgTray);
      
      g.Clear(Color.Transparent);
      if (DisplayAs == localRestrictionTracker.SatHostTypes.WildBlue_LEGACY)
      {
        if (lDown >= lDownLim | lUp >= lUpLim)
        {
          g.DrawIconUnstretched(new System.Drawing.Icon(GetType(), "Resources.tray_16.restricted.ico"), new Rectangle(0, 0, 16, 16));
        }
        else
        {
          g.DrawIconUnstretched(new System.Drawing.Icon(GetType(), "Resources.tray_16.norm.ico"), new Rectangle(0, 0, 16, 16));
        }
        modFunctions.CreateTrayIcon_Left(ref g, lDown, lDownLim, tda, tdb, tdc, 16);
        modFunctions.CreateTrayIcon_Right(ref g, lUp, lUpLim, tua, tub, tuc, 16);
        pctTray.Pixbuf = modFunctions.ImageToPixbuf(imgTray);
      }
      else if (DisplayAs == localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE)
      {
        if (lDown >= lDownLim)
        {
          g.DrawIconUnstretched(new System.Drawing.Icon(GetType(), "Resources.tray_16.restricted.ico"), new Rectangle(0, 0, 16, 16));
        }
        else
        {
          g.DrawIconUnstretched(new System.Drawing.Icon(GetType(), "Resources.tray_16.norm.ico"), new Rectangle(0, 0, 16, 16));
        }
        modFunctions.CreateTrayIcon_Left(ref g, lDown, lDownLim, tda, tdb, tdc, 16);
        modFunctions.CreateTrayIcon_Right(ref g, lDown, lDownLim, tda, tdb, tdc, 16);
        pctTray.Pixbuf = modFunctions.ImageToPixbuf(imgTray);
      }
      else if (DisplayAs == localRestrictionTracker.SatHostTypes.DishNet_EXEDE)
      {
        if (lDown >= lDownLim)
        {
          g.DrawIconUnstretched(new System.Drawing.Icon(GetType(), "Resources.tray_16.restricted.ico"), new Rectangle(0, 0, 16, 16));
        }
        else
        {
          g.DrawIconUnstretched(new System.Drawing.Icon(GetType(), "Resources.tray_16.norm.ico"), new Rectangle(0, 0, 16, 16));
        }
        modFunctions.CreateTrayIcon_Left(ref g, lDown, lDownLim, tda, tdb, tdc, 16);
        modFunctions.CreateTrayIcon_Right(ref g, lDown, lDownLim, tda, tdb, tdc, 16);
        pctTray.Pixbuf = modFunctions.ImageToPixbuf(imgTray);
      }
      else
      {
        pctTray.SetFromStock(Gtk.Stock.DialogError, Gtk.IconSize.Dialog);
      }
      if (FakeData == null || FakeData.Count == 0)
      {
        MakeFakeData();
      }
      Rectangle FakeHRect = new Rectangle(0, 0, 500, 200);
      if (DisplayAs == localRestrictionTracker.SatHostTypes.WildBlue_LEGACY)
      {
        Image FakeD = modFunctions.DrawLineGraph(FakeData.ToArray(), true, FakeHRect.Size, hda, hdb, hdc, ht, hbg, hdm);
        Image FakeU = modFunctions.DrawLineGraph(FakeData.ToArray(), false, FakeHRect.Size, hua, hub, huc, ht, hbg, hum);
        Bitmap fakeI = new Bitmap(pctHistory.Allocation.Width, pctHistory.Allocation.Height);
        g = Graphics.FromImage(fakeI);
        
        g.Clear(Color.Black);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        Rectangle dRect = new Rectangle(0, (int)(iHalfH * 0.1d), iWidth, (int)(iHalfH * 0.85d));
        Rectangle uRect = new Rectangle(0, (int)(iHalfH + (iHalfH * 0.05d)), iWidth, (int)(iHalfH * 0.85d));
        g.DrawImage(FakeD, dRect, FakeHRect, GraphicsUnit.Pixel);
        g.DrawImage(FakeU, uRect, FakeHRect, GraphicsUnit.Pixel);

        pctHistory.Pixbuf = modFunctions.ImageToPixbuf(fakeI);
      }
      else if (DisplayAs == localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE)
      {
        Image FakeR = modFunctions.DrawRGraph(FakeData.ToArray(), FakeHRect.Size, hda, hdb, hdc, ht, hbg, hdm);
        Bitmap fakeI = new Bitmap(pctHistory.Allocation.Width, pctHistory.Allocation.Height);
        g = Graphics.FromImage(fakeI);

        g.Clear(Color.Black);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        Rectangle dRect;
        if (iHeight == 75)
          dRect = new Rectangle(0, iHalfH / 2, iWidth, iHalfH);
        else
          dRect = new Rectangle(0, 0, iWidth, iHeight);
        g.DrawImage(FakeR, dRect, FakeHRect, GraphicsUnit.Pixel);
        pctHistory.Pixbuf = modFunctions.ImageToPixbuf(fakeI);
      }
      else if (DisplayAs == localRestrictionTracker.SatHostTypes.DishNet_EXEDE)
      {
        Image FakeD = modFunctions.DrawLineGraph(FakeData.ToArray(), true, FakeHRect.Size, hda, hdb, hdc, ht, hbg, hdm);
        Image FakeU = modFunctions.DrawLineGraph(FakeData.ToArray(), false, FakeHRect.Size, hua, hub, huc, ht, hbg, hum);
        Bitmap fakeI = new Bitmap(pctHistory.Allocation.Width, pctHistory.Allocation.Height);
        g = Graphics.FromImage(fakeI);

        g.Clear(Color.Black);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        Rectangle dRect = new Rectangle(0, (int)(iHalfH * 0.1d), iWidth, (int)(iHalfH * 0.85d));
        Rectangle uRect = new Rectangle(0, (int)(iHalfH + (iHalfH * 0.05d)), iWidth, (int)(iHalfH * 0.85d));
        g.DrawImage(FakeD, dRect, FakeHRect, GraphicsUnit.Pixel);
        g.DrawImage(FakeU, uRect, FakeHRect, GraphicsUnit.Pixel);
        pctHistory.Pixbuf = modFunctions.ImageToPixbuf(fakeI);
      }
      else
      {
        pctHistory.SetFromStock(Gtk.Stock.DialogError, Gtk.IconSize.Dialog);
      }
    }

#region "Buttons"
    private void cmdSave_Click(System.Object sender, System.EventArgs e)
    {
      mySettings.Colors.MainDownA = modFunctions.GdkColorToDrawingColor(cmdMainDownA.Color);
      if (chkMainDownB.Active)
      {
        mySettings.Colors.MainDownB = modFunctions.GdkColorToDrawingColor(cmdMainDownB.Color);
      }
      else
      {
        mySettings.Colors.MainDownB = Color.Transparent;
      }
      mySettings.Colors.MainDownC = modFunctions.GdkColorToDrawingColor(cmdMainDownC.Color);
      mySettings.Colors.MainUpA = modFunctions.GdkColorToDrawingColor(cmdMainUpA.Color);
      if (chkMainUpB.Active)
      {
        mySettings.Colors.MainUpB = modFunctions.GdkColorToDrawingColor(cmdMainUpB.Color);
      }
      else
      {
        mySettings.Colors.MainUpB = Color.Transparent;
      }
      mySettings.Colors.MainUpC = modFunctions.GdkColorToDrawingColor(cmdMainUpC.Color);
      mySettings.Colors.MainText = modFunctions.GdkColorToDrawingColor(cmdMainText.Color);
      mySettings.Colors.MainBackground = modFunctions.GdkColorToDrawingColor(cmdMainBG.Color);
      mySettings.Colors.TrayDownA = modFunctions.GdkColorToDrawingColor(cmdTrayDownA.Color);
      if (chkTrayDownB.Active)
      {
        mySettings.Colors.TrayDownB = modFunctions.GdkColorToDrawingColor(cmdTrayDownB.Color);
      }
      else
      {
        mySettings.Colors.TrayDownB = Color.Transparent;
      }
      mySettings.Colors.TrayDownC = modFunctions.GdkColorToDrawingColor(cmdTrayDownC.Color);
      mySettings.Colors.TrayUpA = modFunctions.GdkColorToDrawingColor(cmdTrayUpA.Color);
      if (chkTrayUpB.Active)
      {
        mySettings.Colors.TrayUpB = modFunctions.GdkColorToDrawingColor(cmdTrayUpB.Color);
      }
      else
      {
        mySettings.Colors.TrayUpB = Color.Transparent;
      }
      mySettings.Colors.TrayUpC = modFunctions.GdkColorToDrawingColor(cmdTrayUpC.Color);
      mySettings.Colors.HistoryDownA = modFunctions.GdkColorToDrawingColor(cmdHistoryDownA.Color);
      if (chkHistoryDownB.Active)
      {
        mySettings.Colors.HistoryDownB = modFunctions.GdkColorToDrawingColor(cmdHistoryDownB.Color);
      }
      else
      {
        mySettings.Colors.HistoryDownB = Color.Transparent;
      }
      mySettings.Colors.HistoryDownC = modFunctions.GdkColorToDrawingColor(cmdHistoryDownC.Color);
      mySettings.Colors.HistoryDownMax = modFunctions.GdkColorToDrawingColor(cmdHistoryDownMax.Color);
      mySettings.Colors.HistoryUpA = modFunctions.GdkColorToDrawingColor(cmdHistoryUpA.Color);
      if (chkHistoryUpB.Active)
      {
        mySettings.Colors.HistoryUpB = modFunctions.GdkColorToDrawingColor(cmdHistoryUpB.Color);
      }
      else
      {
        mySettings.Colors.HistoryUpB = Color.Transparent;
      }
      mySettings.Colors.HistoryUpC = modFunctions.GdkColorToDrawingColor(cmdHistoryUpC.Color);
      mySettings.Colors.HistoryUpMax = modFunctions.GdkColorToDrawingColor(cmdHistoryUpMax.Color);
      mySettings.Colors.HistoryText = modFunctions.GdkColorToDrawingColor(cmdHistoryText.Color);
      mySettings.Colors.HistoryBackground = modFunctions.GdkColorToDrawingColor(cmdHistoryBG.Color);
      mySettings.Save();
      cmdSave.Sensitive = false;
      HasSaved = true;
    }

    private void cmdClose_Click(Object sender, EventArgs e)
    {
      this.Respond(Gtk.ResponseType.Close);
    }
#endregion

#region "Clickables"
    private void cmdColor_SelectedColor(object sender, EventArgs e)
    {
      RedrawImages();
      cmdSave.Sensitive = SettingsChanged();
    }

    [GLib.ConnectBefore]
    private void cmdColor_MouseUp(Object sender, Gtk.ButtonReleaseEventArgs  e)
    {
      if (e.Event.Button == 3)
      {
        cbSelected = (Gtk.ColorButton)sender;
        mnuColorOpts.ShowAll();
        mnuColorOpts.Popup();
      }
      RedrawImages();
    }

    private void pctMain_MouseUp(object sender, Gtk.ButtonReleaseEventArgs  e)
    {
      Console.WriteLine("Main MouseUp: " + e.Event.Button);
      if (e.Event.Button == 1)
      {
        if (dDown)
        {
          lDown += iD;
          if (lDown >= lDownLim)
          {
            dDown = false;
          }
        }
        else
        {
          lDown -= iD;
          if (lDown <= 0)
          {
            dDown = true;
          }
        }
      }
      else if (e.Event.Button == 3)
      {
        if (dUp)
        {
          lUp += iU;
          if (lUp >= lUpLim)
          {
            dUp = false;
          }
        }
        else
        {
          lUp -= iU;
          if (lUp <= 0)
          {
            dUp = true;
          }
        }
      }
      RedrawImages();
    }

    private void pctTray_MouseUp(object sender, Gtk.ButtonReleaseEventArgs  e)
    {
      Console.WriteLine("Tray MouseUp: " + e.Event.Button);
      if (e.Event.Button == 1)
      {
        if (dDown)
        {
          lDown += iD;
          if (lDown >= lDownLim)
          {
            dDown = false;
          }
        }
        else
        {
          lDown -= iD;
          if (lDown <= 0)
          {
            dDown = true;
          }
        }
      }
      else if (e.Event.Button == 3)
      {
        if (dUp)
        {
          lUp += iU;
          if (lUp >= lUpLim)
          {
            dUp = false;
          }
        }
        else
        {
          lUp -= iU;
          if (lUp <= 0)
          {
            dUp = true;
          }
        }
      }
      RedrawImages();
    }

    private void chkB_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Gtk.CheckButton chkThis = (Gtk.CheckButton)sender;
      Gtk.ColorButton cmdThis = (Gtk.ColorButton)getControlFromName(ref pnlCustomColors, chkThis.Name.Replace("chk", "cmd"));
      if (cmdThis == null)
      {
        return;
      }
      if (chkThis.Active)
      {
        cmdThis.Sensitive = true;
      }
      else
      {
        cmdThis.Sensitive = false;
      }
      cmdSave.Sensitive = SettingsChanged();
      RedrawImages();
    }
#endregion

#region "Menus"
    private void mnuChoose_Click(System.Object sender, System.EventArgs e)
    {
      Gtk.ColorButton cmdColor = cbSelected;
      if (cmdColor != null)
      {
        cmdColor.Click();
      }
    }

    private void mnuDefault_Click(System.Object sender, System.EventArgs e)
    {
      Gtk.ColorButton cmdColor = cbSelected;
      if (cmdColor != null)
      {
        SetElColor(ref cmdColor, DefaultColorForElement(cmdColor.Name, useStyle));
        cmdSave.Sensitive = SettingsChanged();
      }
      RedrawImages();
    }

    private void mnuGraphDefault_Click(System.Object sender, System.EventArgs e)
    {
      Gtk.ColorButton cmdColor = cbSelected;
      Gtk.ColorButton[] ColorList = null;
      if (cmdColor.Name.StartsWith("cmdMain"))
      {
        ColorList = new Gtk.ColorButton[] {
          cmdMainDownA,
          cmdMainDownB,
          cmdMainDownC,
          cmdMainUpA,
          cmdMainUpB,
          cmdMainUpC,
          cmdMainText,
          cmdMainBG
        };
      }
      else if (cmdColor.Name.StartsWith("cmdTray"))
      {
        ColorList = new Gtk.ColorButton[] {
          cmdTrayDownA,
          cmdTrayDownB,
          cmdTrayDownC,
          cmdTrayUpA,
          cmdTrayUpB,
          cmdTrayUpC
        };
      }
      else
      {
        ColorList = new Gtk.ColorButton[] {
          cmdHistoryDownA,
          cmdHistoryDownB,
          cmdHistoryDownC,
          cmdHistoryDownMax,
          cmdHistoryUpA,
          cmdHistoryUpB,
          cmdHistoryUpC,
          cmdHistoryUpMax,
          cmdHistoryText,
          cmdHistoryBG
        };
      }
      for (int i = 0; i < ColorList.Length; i++)
      {
        Gtk.ColorButton pColor = ColorList[i];
        Color bColor = DefaultColorForElement(pColor.Name, useStyle);
        SetElColor(ref pColor, bColor);
      }
      RedrawImages();
      cmdSave.Sensitive = SettingsChanged();
    }

    private void mnuAllDefault_Click(System.Object sender, System.EventArgs e)
    {
      Gtk.ColorButton[] ColorList = null;
      ColorList = new Gtk.ColorButton[] {
        cmdMainDownA,
        cmdMainDownB,
        cmdMainDownC,
        cmdMainUpA,
        cmdMainUpB,
        cmdMainUpC,
        cmdMainText,
        cmdMainBG,
        cmdTrayDownA,
        cmdTrayDownB,
        cmdTrayDownC,
        cmdTrayUpA,
        cmdTrayUpB,
        cmdTrayUpC,
        cmdHistoryDownA,
        cmdHistoryDownB,
        cmdHistoryDownC,
        cmdHistoryDownMax,
        cmdHistoryUpA,
        cmdHistoryUpB,
        cmdHistoryUpC,
        cmdHistoryUpMax,
        cmdHistoryText,
        cmdHistoryBG
      };
      for (int i = 0; i < ColorList.Length; i++)
      {
        Gtk.ColorButton pColor = ColorList[i];
        Color bColor = DefaultColorForElement(pColor.Name, useStyle);
        SetElColor(ref pColor, bColor);
      }
      RedrawImages();
      cmdSave.Sensitive = SettingsChanged();
    }
#endregion

#region "Functions"
    private bool SettingsChanged()
    {
      if (!modFunctions.CompareColors(mySettings.Colors.MainDownA, cmdMainDownA.Color))
        return true;
      if (chkMainDownB.Active)
      {
        if (!modFunctions.CompareColors(mySettings.Colors.MainDownB, cmdMainDownB.Color))
          return true;
      }
      else
      {
        if (mySettings.Colors.MainDownB != Color.Transparent)
          return true;
      }
      if (!modFunctions.CompareColors(mySettings.Colors.MainDownC,cmdMainDownC.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.MainUpA,cmdMainUpA.Color))
        return true;
      if (chkMainUpB.Active)
      {
        if (!modFunctions.CompareColors(mySettings.Colors.MainUpB, cmdMainUpB.Color))
          return true;
      }
      else
      {
        if (mySettings.Colors.MainUpB != Color.Transparent)
          return true;
      }
      if (!modFunctions.CompareColors(mySettings.Colors.MainUpC,cmdMainUpC.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.MainText,cmdMainText.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.MainBackground,cmdMainBG.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.TrayDownA,cmdTrayDownA.Color))
        return true;
      if (chkTrayDownB.Active)
      {
        if (!modFunctions.CompareColors(mySettings.Colors.TrayDownB, cmdTrayDownB.Color))
          return true;
      }
      else
      {
        if (mySettings.Colors.TrayDownB != Color.Transparent)
          return true;
      }
      if (!modFunctions.CompareColors(mySettings.Colors.TrayDownC,cmdTrayDownC.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.TrayUpA,cmdTrayUpA.Color))
        return true;
      if (chkTrayUpB.Active)
      {
        if (!modFunctions.CompareColors(mySettings.Colors.TrayUpB, cmdTrayUpB.Color))
          return true;
      }
      else
      {
        if (mySettings.Colors.TrayUpB != Color.Transparent)
          return true;
      }
      if (!modFunctions.CompareColors(mySettings.Colors.TrayUpC,cmdTrayUpC.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryDownA,cmdHistoryDownA.Color))
        return true;
      if (chkHistoryDownB.Active)
      {
        if (!modFunctions.CompareColors(mySettings.Colors.HistoryDownB, cmdHistoryDownB.Color))
          return true;
      }
      else
      {
        if (mySettings.Colors.HistoryDownB != Color.Transparent)
          return true;
      }
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryDownC,cmdHistoryDownC.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryDownMax,cmdHistoryDownMax.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryUpA,cmdHistoryUpA.Color))
        return true;
      if (chkHistoryUpB.Active)
      {
        if (!modFunctions.CompareColors(mySettings.Colors.HistoryUpB, cmdHistoryUpB.Color))
          return true;
      }
      else
      {
        if (mySettings.Colors.HistoryUpB != Color.Transparent)
          return true;
      }
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryUpC,cmdHistoryUpC.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryUpMax,cmdHistoryUpMax.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryText,cmdHistoryText.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryBackground, cmdHistoryBG.Color))
        return true;
      return false;
    }
    private Gtk.Widget getControlFromName(ref Gtk.Table wIn, string name)
    {
      try
      {
        if (wIn.Name.ToUpper().Trim() == name.ToUpper().Trim())
        {
          return wIn;
        }
        else
        {
          if (wIn is Gtk.Container)
          {
            Gtk.Container bItem = (Gtk.Container)wIn;
            if (bItem.Children.Length > 0)
            {
              for (int i = 0; i < bItem.Children.Length; i++)
              {
                Gtk.Widget tC = getControlFromName(ref bItem.Children[i], name);
                if (tC != null)
                {
                  return tC;
                }
              }
            }
          }
        }
        return null;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return null;
      }
    }
    private Gtk.Widget getControlFromName(ref Gtk.Widget wIn, string name)
    {
      try
      {
        if (wIn.Name.ToUpper().Trim() == name.ToUpper().Trim())
        {
          return wIn;
        }
        else
        {
          if (wIn is Gtk.Container)
          {
            Gtk.Container bItem = (Gtk.Container)wIn;
            if (bItem.Children.Length > 0)
            {
              for (int i = 0; i < bItem.Children.Length; i++)
              {
                Gtk.Widget tC = getControlFromName(ref bItem.Children[i], name);
                if (tC != null)
                {
                  return tC;
                }
              }
            }
          }
        }
        return null;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return null;
      }
    }

    private void SetElColor(ref Gtk.ColorButton cmdColor, Color color)
    {
      cmdColor.Color = modFunctions.DrawingColorToGdkColor(color);
      Gtk.CheckButton chkThis = (Gtk.CheckButton)getControlFromName(ref pnlCustomColors, cmdColor.Name.Replace("cmd", "chk"));
      if (chkThis != null)
      {
        chkThis.Active = !(color == Color.Transparent);
        cmdColor.Sensitive = !(color == Color.Transparent);
      }
    }

    private Color DefaultColorForElement(string Element, localRestrictionTracker.SatHostTypes Provider)
    {
      switch (Provider)
      {
        case localRestrictionTracker.SatHostTypes.WildBlue_LEGACY:
        case localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY:
          switch (Element)
          {
            case "cmdMainDownA":
              return Color.DarkBlue;
            case "cmdMainDownB":
              return Color.Blue;
            case "cmdMainDownC":
              return Color.Aqua;
            case "cmdMainUpA":
              return Color.DarkBlue;
            case "cmdMainUpB":
              return Color.Blue;
            case "cmdMainUpC":
              return Color.Aqua;
            case "cmdMainText":
              return Color.White;
            case "cmdMainBG":
              return Color.Black;

            case "cmdTrayDownA":
              return Color.DarkBlue;
            case "cmdTrayDownB":
              return Color.Blue;
            case "cmdTrayDownC":
              return Color.Aqua;
            case "cmdTrayUpA":
              return Color.DarkBlue;
            case "cmdTrayUpB":
              return Color.Blue;
            case "cmdTrayUpC":
              return Color.Aqua;

            case "cmdHistoryDownA":
              return Color.DarkBlue;
            case "cmdHistoryDownB":
              return Color.Blue;
            case "cmdHistoryDownC":
              return Color.Aqua;
            case "cmdHistoryDownMax":
              return Color.Yellow;
            case "cmdHistoryUpA":
              return Color.DarkBlue;
            case "cmdHistoryUpB":
              return Color.Blue;
            case "cmdHistoryUpC":
              return Color.Aqua;
            case "cmdHistoryUpMax":
              return Color.Yellow;
            case "cmdHistoryText":
              return Color.Black;
            case "cmdHistoryBG":
              return Color.White;
            default:
              return Color.Transparent;
          }
        case localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE:
        case localRestrictionTracker.SatHostTypes.WildBlue_EXEDE:
          switch (Element)
          {
            case "cmdMainDownA":
              return Color.DarkBlue;
            case "cmdMainDownB":
              return Color.Blue;
            case "cmdMainDownC":
              return Color.Aqua;
            case "cmdMainUpA":
              return Color.Transparent;
            case "cmdMainUpB":
              return Color.Transparent;
            case "cmdMainUpC":
              return Color.Transparent;
            case "cmdMainText":
              return Color.White;
            case "cmdMainBG":
              return Color.Black;

            case "cmdTrayDownA":
              return Color.DarkBlue;
            case "cmdTrayDownB":
              return Color.Blue;
            case "cmdTrayDownC":
              return Color.Aqua;
            case "cmdTrayUpA":
              return Color.Transparent;
            case "cmdTrayUpB":
              return Color.Transparent;
            case "cmdTrayUpC":
              return Color.Transparent;

            case "cmdHistoryDownA":
              return Color.DarkBlue;
            case "cmdHistoryDownB":
              return Color.Blue;
            case "cmdHistoryDownC":
              return Color.Aqua;
            case "cmdHistoryDownMax":
              return Color.Yellow;
            case "cmdHistoryUpA":
              return Color.Transparent;
            case "cmdHistoryUpB":
              return Color.Transparent;
            case "cmdHistoryUpC":
              return Color.Transparent;
            case "cmdHistoryUpMax":
              return Color.Transparent;
            case "cmdHistoryText":
              return Color.Black;
            case "cmdHistoryBG":
              return Color.White;
            default:
              return Color.Transparent;
          }
        case localRestrictionTracker.SatHostTypes.DishNet_EXEDE:
          switch (Element)
          {
            case "cmdMainDownA":
              return Color.DarkBlue;
            case "cmdMainDownB":
              return Color.Blue;
            case "cmdMainDownC":
              return Color.Aqua;
            case "cmdMainUpA":
              return Color.DarkBlue;
            case "cmdMainUpB":
              return Color.Blue;
            case "cmdMainUpC":
              return Color.Aqua;
            case "cmdMainText":
              return Color.White;
            case "cmdMainBG":
              return Color.Black;

            case "cmdTrayDownA":
              return Color.DarkBlue;
            case "cmdTrayDownB":
              return Color.Blue;
            case "cmdTrayDownC":
              return Color.Aqua;
            case "cmdTrayUpA":
              return Color.DarkBlue;
            case "cmdTrayUpB":
              return Color.Blue;
            case "cmdTrayUpC":
              return Color.Aqua;

            case "cmdHistoryDownA":
              return Color.DarkBlue;
            case "cmdHistoryDownB":
              return Color.Blue;
            case "cmdHistoryDownC":
              return Color.Aqua;
            case "cmdHistoryDownMax":
              return Color.Yellow;
            case "cmdHistoryUpA":
              return Color.DarkBlue;
            case "cmdHistoryUpB":
              return Color.Blue;
            case "cmdHistoryUpC":
              return Color.Aqua;
            case "cmdHistoryUpMax":
              return Color.Yellow;
            case "cmdHistoryText":
              return Color.Black;
            case "cmdHistoryBG":
              return Color.White;
            default:
              return Color.Transparent;
          }
        default:
          return Color.Transparent;
      }
    }

    private void MakeFakeData()
    {
      FakeData = new System.Collections.Generic.List<DataBase.DataRow>();
      System.Collections.Generic.List<int> DownList = new System.Collections.Generic.List<int>();
      System.Collections.Generic.List<int> UpList = new System.Collections.Generic.List<int>();
      System.DateTime startDate = new System.DateTime(2000, 1, 1, 0, 0, 0);
      int startDown = 0;
      int startUp = 0;
      if (DisplayAs == localRestrictionTracker.SatHostTypes.WildBlue_LEGACY)
      {
        for (int I = 1; I <= 90; I++)
        {
          DataBase.DataRow dRow = new DataBase.DataRow(startDate, startDown, 12000, startUp, 3000);
          int DownUsed = RandSel(50, 500);
          int UpUsed = RandSel(10, 120);
          DownList.Add(DownUsed);
          UpList.Add(UpUsed);
          startDown += DownUsed;
          startUp += UpUsed;
          startDate = startDate.AddDays(1);
          if (I > 29)
          {
            startDown -= DownList[I - 30];
            startUp -= UpList[I - 30];
          }
          FakeData.Add(dRow);
        }
      }
      else if (DisplayAs == localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE)
      {
        for (int I = 1; I <= 90; I++)
        {
          DataBase.DataRow dRow = new DataBase.DataRow(startDate, startDown, 15000, startDown, 15000);
          int DownUsed = RandSel(50, 500);
          DownList.Add(DownUsed);
          startDown += DownUsed;
          startDate = startDate.AddDays(1);
          if (I > 29)
          {
            startDown -= DownList[I - 30];
          }
          FakeData.Add(dRow);
        }
      }
      else if (DisplayAs == localRestrictionTracker.SatHostTypes.DishNet_EXEDE)
      {
        for (int I = 1; I <= 90; I++)
        {
          DataBase.DataRow dRow = new DataBase.DataRow(startDate, startDown, 10000, startUp, 10000);
          int DownUsed = RandSel(50, 500);
          int UpUsed = RandSel(50, 450);
          DownList.Add(DownUsed);
          UpList.Add(UpUsed);
          startDown += DownUsed;
          startUp += UpUsed;
          startDate = startDate.AddDays(1);
          if (I % 30 == 0)
          {
            startDown = 0;
            startUp = 0;
          }
          FakeData.Add(dRow);
        }
      }
    }

    private int RandSel(int Low, int High)
    {
      int I = rGen.Next(Low, High);
      if (I == Low)
      {
        I = High;
      }
      return I;
    }

    private void SetTextBGAlignments(bool Horizontal)
    {
      if (Horizontal)
      {
        //Size preSize = new Size(75, 75);

      }
      else
      {
        Size preSize = new Size(100, 55);
        pnlMain.NRows = 3;
        ((Gtk.Table.TableChild)pnlMain[pnlMainStyle]).TopAttach = 2;
        ((Gtk.Table.TableChild)pnlMain[pnlMainStyle]).BottomAttach = 3;
        pnlMain.Remove(grpMainUp);
        ((Gtk.Table.TableChild)pnlMain[grpMainDown]).TopAttach = 1;
        ((Gtk.Table.TableChild)pnlMain[grpMainDown]).BottomAttach = 2;

        ((Gtk.Table.TableChild)pnlMain[evnMain]).TopAttach = 0;
        ((Gtk.Table.TableChild)pnlMain[evnMain]).LeftAttach = 0;
        ((Gtk.Table.TableChild)pnlMain[evnMain]).BottomAttach = 1;
        ((Gtk.Table.TableChild)pnlMain[evnMain]).RightAttach = 1;
        pnlMain.NColumns = 1;
        pctMain.WidthRequest = preSize.Width;
        pctMain.HeightRequest = preSize.Height;

        pnlMainStyle.NRows = 4;
        ((Gtk.Table.TableChild)pnlMainStyle[lblMainBG]).TopAttach = 1;
        ((Gtk.Table.TableChild)pnlMainStyle[lblMainBG]).BottomAttach = 2;
        ((Gtk.Table.TableChild)pnlMainStyle[lblMainBG]).LeftAttach = 0;
        ((Gtk.Table.TableChild)pnlMainStyle[lblMainBG]).RightAttach = 1;

        ((Gtk.Table.TableChild)pnlMainStyle[cmdMainBG]).TopAttach = 1;
        ((Gtk.Table.TableChild)pnlMainStyle[cmdMainBG]).BottomAttach = 2;
        ((Gtk.Table.TableChild)pnlMainStyle[cmdMainBG]).LeftAttach = 1;
        ((Gtk.Table.TableChild)pnlMainStyle[cmdMainBG]).RightAttach = 2;
        pnlMainStyle.NColumns = 2;

        pnlTray.NRows = 2;
        pnlTray.Remove(grpTrayUp);
        ((Gtk.Table.TableChild)pnlTray[grpTrayDown]).TopAttach = 1;
        ((Gtk.Table.TableChild)pnlTray[grpTrayDown]).BottomAttach = 2;

        ((Gtk.Table.TableChild)pnlTray[evnTray]).TopAttach = 0;
        ((Gtk.Table.TableChild)pnlTray[evnTray]).LeftAttach = 0;
        ((Gtk.Table.TableChild)pnlTray[evnTray]).BottomAttach = 1;
        ((Gtk.Table.TableChild)pnlTray[evnTray]).RightAttach = 2;
        pnlTray.NColumns = 1;
        pctTray.WidthRequest = preSize.Width;
        pctTray.HeightRequest = preSize.Height;


        pnlHistory.NRows = 3;
        ((Gtk.Table.TableChild)pnlHistory[pnlHistoryStyle]).TopAttach = 2;
        ((Gtk.Table.TableChild)pnlHistory[pnlHistoryStyle]).BottomAttach = 3;
        pnlHistory.Remove(grpHistoryUp);
        ((Gtk.Table.TableChild)pnlHistory[grpHistoryDown]).TopAttach = 1;
        ((Gtk.Table.TableChild)pnlHistory[grpHistoryDown]).BottomAttach = 2;

        ((Gtk.Table.TableChild)pnlHistory[evnHistory]).TopAttach = 0;
        ((Gtk.Table.TableChild)pnlHistory[evnHistory]).LeftAttach = 0;
        ((Gtk.Table.TableChild)pnlHistory[evnHistory]).BottomAttach = 1;
        ((Gtk.Table.TableChild)pnlHistory[evnHistory]).RightAttach = 2;
        pnlHistory.NColumns = 1;
        pctHistory.WidthRequest = preSize.Width;
        pctHistory.HeightRequest = preSize.Height;

        pnlHistoryStyle.NRows = 4;
        ((Gtk.Table.TableChild)pnlHistoryStyle[lblHistoryBG]).TopAttach = 1;
        ((Gtk.Table.TableChild)pnlHistoryStyle[lblHistoryBG]).BottomAttach = 2;
        ((Gtk.Table.TableChild)pnlHistoryStyle[lblHistoryBG]).LeftAttach = 0;
        ((Gtk.Table.TableChild)pnlHistoryStyle[lblHistoryBG]).RightAttach = 1;

        ((Gtk.Table.TableChild)pnlHistoryStyle[cmdHistoryBG]).TopAttach = 1;
        ((Gtk.Table.TableChild)pnlHistoryStyle[cmdHistoryBG]).BottomAttach = 2;
        ((Gtk.Table.TableChild)pnlHistoryStyle[cmdHistoryBG]).LeftAttach = 1;
        ((Gtk.Table.TableChild)pnlHistoryStyle[cmdHistoryBG]).RightAttach = 2;
        pnlHistoryStyle.NColumns = 2;

        pnlCustomColors.NColumns = 3;
        ((Gtk.Table.TableChild)pnlCustomColors[grpHistory]).RightAttach = 3;
        ((Gtk.Table.TableChild)pnlCustomColors[grpHistory]).LeftAttach = 2;
        ((Gtk.Table.TableChild)pnlCustomColors[grpHistory]).TopAttach = 0;
        ((Gtk.Table.TableChild)pnlCustomColors[grpHistory]).BottomAttach = 1;

        ((Gtk.Table.TableChild)pnlCustomColors[grpTray]).RightAttach = 2;
        ((Gtk.Table.TableChild)pnlCustomColors[grpTray]).LeftAttach = 1;
        ((Gtk.Table.TableChild)pnlCustomColors[grpTray]).TopAttach = 0;
        ((Gtk.Table.TableChild)pnlCustomColors[grpTray]).BottomAttach = 1;

        ((Gtk.Table.TableChild)pnlCustomColors[grpMain]).RightAttach = 1;
        ((Gtk.Table.TableChild)pnlCustomColors[grpMain]).LeftAttach = 0;
        ((Gtk.Table.TableChild)pnlCustomColors[grpMain]).TopAttach = 0;
        ((Gtk.Table.TableChild)pnlCustomColors[grpMain]).BottomAttach = 1;
        pnlCustomColors.NRows = 1;
      }
    }
#endregion
  }
}