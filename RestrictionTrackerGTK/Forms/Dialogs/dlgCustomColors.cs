using System;
using RestrictionLibrary;
using System.Drawing;
namespace RestrictionTrackerGTK
{
  public partial class dlgCustomColors:
    Gtk.Dialog
  {
    private long lUsed;
    private long lLimit;
    private int iD;
    private bool dDown;
    internal AppSettings mySettings;
    private bool HasSaved = false;
    private System.Collections.Generic.List<DataRow> FakeData;
    private Random rGen;
    private Gtk.ColorButton cbSelected;
    private Gtk.Menu mnuColorOpts;
    private Gtk.MenuItem mnuChoose;
    private Gtk.SeparatorMenuItem mnuSpace;
    private Gtk.MenuItem mnuThisDefault;
    private Gtk.MenuItem mnuGraphDefault;
    private Gtk.MenuItem mnuAllDefault;
    internal dlgCustomColors(AppSettings settings)
    {
      mySettings = settings;
      rGen = new Random();
      mnuColorOpts = new Gtk.Menu();
      mnuChoose = new Gtk.MenuItem("_Choose Color");
      mnuSpace = new Gtk.SeparatorMenuItem();
      mnuThisDefault = new Gtk.MenuItem("Use Defaut for _This Color");
      mnuGraphDefault = new Gtk.MenuItem("Use Defaut for This _Graph");
      mnuAllDefault = new Gtk.MenuItem("Use Defaut for _All Graphs");
      mnuColorOpts.Append(mnuChoose);
      mnuColorOpts.Append(mnuSpace);
      mnuColorOpts.Append(mnuThisDefault);
      mnuColorOpts.Append(mnuGraphDefault);
      mnuColorOpts.Append(mnuAllDefault);

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

      if (lLimit == 0)
      {
        lUsed = 8;
        lLimit = 16;
        dDown = true;
        iD = 1;
      }
      cmdMainUsedA.ColorSet += cmdColor_SelectedColor;
      cmdMainUsedA.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdMainUsedB.ColorSet += cmdColor_SelectedColor;
      cmdMainUsedB.ButtonReleaseEvent += cmdColor_MouseUp;
      chkMainUsedB.Toggled += chkB_CheckedChanged;
      cmdMainUsedC.ColorSet += cmdColor_SelectedColor;
      cmdMainUsedC.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdMainText.ColorSet += cmdColor_SelectedColor;
      cmdMainText.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdMainBG.ColorSet += cmdColor_SelectedColor;
      cmdMainBG.ButtonReleaseEvent += cmdColor_MouseUp;

      cmdTrayUsedA.ColorSet += cmdColor_SelectedColor;
      cmdTrayUsedA.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdTrayUsedB.ColorSet += cmdColor_SelectedColor;
      cmdTrayUsedB.ButtonReleaseEvent += cmdColor_MouseUp;
      chkTrayUsedB.Toggled += chkB_CheckedChanged;
      cmdTrayUsedC.ColorSet += cmdColor_SelectedColor;
      cmdTrayUsedC.ButtonReleaseEvent += cmdColor_MouseUp;

      cmdHistoryUsedLine.ColorSet += cmdColor_SelectedColor;
      cmdHistoryUsedLine.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryUsedA.ColorSet += cmdColor_SelectedColor;
      cmdHistoryUsedA.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryUsedB.ColorSet += cmdColor_SelectedColor;
      cmdHistoryUsedB.ButtonReleaseEvent += cmdColor_MouseUp;
      chkHistoryUsedB.Toggled += chkB_CheckedChanged;
      cmdHistoryUsedC.ColorSet += cmdColor_SelectedColor;
      cmdHistoryUsedC.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryUsedMax.ColorSet += cmdColor_SelectedColor;
      cmdHistoryUsedMax.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryText.ColorSet += cmdColor_SelectedColor;
      cmdHistoryText.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryBG.ColorSet += cmdColor_SelectedColor;
      cmdHistoryBG.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryLightGrid.ColorSet += cmdColor_SelectedColor;
      cmdHistoryLightGrid.ButtonReleaseEvent += cmdColor_MouseUp;
      cmdHistoryDarkGrid.ColorSet += cmdColor_SelectedColor;
      cmdHistoryDarkGrid.ButtonReleaseEvent += cmdColor_MouseUp;


      if (mySettings.Colors.MainDownA == Color.Transparent)
      {
        mnuAllDefault_Click(mnuAllDefault, new EventArgs());
      }
      else
      {
        SetElColor(ref cmdMainUsedA, mySettings.Colors.MainDownA);
        SetElColor(ref cmdMainUsedB, mySettings.Colors.MainDownB);
        SetElColor(ref cmdMainUsedC, mySettings.Colors.MainDownC);
        SetElColor(ref cmdMainText, mySettings.Colors.MainText);
        SetElColor(ref cmdMainBG, mySettings.Colors.MainBackground);

        SetElColor(ref cmdTrayUsedA, mySettings.Colors.TrayDownA);
        SetElColor(ref cmdTrayUsedB, mySettings.Colors.TrayDownB);
        SetElColor(ref cmdTrayUsedC, mySettings.Colors.TrayDownC);

        SetElColor(ref cmdHistoryUsedLine, mySettings.Colors.HistoryDownLine);
        SetElColor(ref cmdHistoryUsedA, mySettings.Colors.HistoryDownA);
        SetElColor(ref cmdHistoryUsedB, mySettings.Colors.HistoryDownB);
        SetElColor(ref cmdHistoryUsedC, mySettings.Colors.HistoryDownC);
        SetElColor(ref cmdHistoryUsedMax, mySettings.Colors.HistoryDownMax);
        SetElColor(ref cmdHistoryText, mySettings.Colors.HistoryText);
        SetElColor(ref cmdHistoryBG, mySettings.Colors.HistoryBackground);
        SetElColor(ref cmdHistoryLightGrid, mySettings.Colors.HistoryLightGrid);
        SetElColor(ref cmdHistoryDarkGrid, mySettings.Colors.HistoryDarkGrid);
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
      if (pctMain.Allocation.Width == 1 & pctMain.Allocation.Height == 1 &
        pctHistory.Allocation.Width == 1 & pctHistory.Allocation.Height == 1)
        return;
      Color mda = modFunctions.GdkColorToDrawingColor(cmdMainUsedA.Color);
      Color mdb = modFunctions.GdkColorToDrawingColor(cmdMainUsedB.Color);
      if (!chkMainUsedB.Active)
      {
        mdb = Color.Transparent;
      }
      Color mdc = modFunctions.GdkColorToDrawingColor(cmdMainUsedC.Color);

      Color mt = modFunctions.GdkColorToDrawingColor(cmdMainText.Color);
      Color mbg = modFunctions.GdkColorToDrawingColor(cmdMainBG.Color);


      Color tda = modFunctions.GdkColorToDrawingColor(cmdTrayUsedA.Color);
      Color tdb = modFunctions.GdkColorToDrawingColor(cmdTrayUsedB.Color);
      if (!chkTrayUsedB.Active)
      {
        tdb = Color.Transparent;
      }
      Color tdc = modFunctions.GdkColorToDrawingColor(cmdTrayUsedC.Color);

      Color hdl = modFunctions.GdkColorToDrawingColor(cmdHistoryUsedLine.Color);
      Color hda = modFunctions.GdkColorToDrawingColor(cmdHistoryUsedA.Color);
      Color hdb = modFunctions.GdkColorToDrawingColor(cmdHistoryUsedB.Color);
      if (!chkHistoryUsedB.Active)
      {
        hdb = Color.Transparent;
      }
      Color hdc = modFunctions.GdkColorToDrawingColor(cmdHistoryUsedC.Color);
      Color hdm = modFunctions.GdkColorToDrawingColor(cmdHistoryUsedMax.Color);

      Color ht = modFunctions.GdkColorToDrawingColor(cmdHistoryText.Color);
      Color hbg = modFunctions.GdkColorToDrawingColor(cmdHistoryBG.Color);

      Color hgl = modFunctions.GdkColorToDrawingColor(cmdHistoryLightGrid.Color);
      Color hgd = modFunctions.GdkColorToDrawingColor(cmdHistoryDarkGrid.Color);

      int iWidth = pctMain.WidthRequest;
      if (iWidth > pctMain.Allocation.Width)
        iWidth = pctMain.Allocation.Width;
      int iHeight = pctMain.HeightRequest;
      if (iHeight > pctMain.Allocation.Height)
        iHeight = pctMain.Allocation.Height;
      int iHalfW = (int)Math.Floor(iWidth / 2d);
      int iHalfH = (int)Math.Floor(iHeight / 2d);

      Graphics g;
      pctMain.Pixbuf = modFunctions.ImageToPixbuf(modFunctions.DisplayRProgress(pctMain.Allocation.Size, lUsed, lLimit, mySettings.Accuracy, mda, mdb, mdc, mt, mbg));
      Bitmap imgTray = new Bitmap(16, 16);
      g = Graphics.FromImage(imgTray);

      g.Clear(Color.Transparent);

      if (lUsed >= lLimit)
      {
        g.DrawIconUnstretched(new System.Drawing.Icon(GetType(), "Resources.tray_16.restricted.ico"), new Rectangle(0, 0, 16, 16));
      }
      else
      {
        g.DrawIconUnstretched(new System.Drawing.Icon(GetType(), "Resources.tray_16.norm.ico"), new Rectangle(0, 0, 16, 16));
        modFunctions.CreateTrayIcon_Left(ref g, lUsed, lLimit, tda, tdb, tdc, 16);
        modFunctions.CreateTrayIcon_Right(ref g, lUsed, lLimit, tda, tdb, tdc, 16);
      }
      pctTray.Pixbuf = modFunctions.ImageToPixbuf(imgTray);
    

      iWidth = pctHistory.WidthRequest;
      if (iWidth > pctHistory.Allocation.Width)
        iWidth = pctHistory.Allocation.Width;
      iHeight = pctHistory.HeightRequest;
      if (iHeight > pctHistory.Allocation.Height)
        iHeight = pctHistory.Allocation.Height;
      iHalfW = (int)Math.Floor(iWidth / 2d);
      iHalfH = (int)Math.Floor(iHeight / 2d);
      if (FakeData == null || FakeData.Count == 0)
      {
        MakeFakeData();
      }
      Rectangle FakeHRect = new Rectangle(0, 0, 500, 200);

      Image FakeR = modFunctions.DrawRGraph(FakeData.ToArray(), FakeHRect.Size, hdl, hda, hdb, hdc, ht, hbg, hdm, hgl, hgd);

      Rectangle dRect;
      if (iWidth / FakeR.Width > iHeight / FakeR.Height)
        dRect = new Rectangle((iWidth - (int)(iHeight * (FakeR.Width / (double)FakeR.Height))) / 2, 0, (int)(iHeight * (FakeR.Width / (double)FakeR.Height)), iHeight);
      else
        dRect = new Rectangle(0, (iHeight - (int)(iWidth * (FakeR.Height / (double)FakeR.Width))) / 2, iWidth, (int)(iWidth * (FakeR.Height / (double)FakeR.Width)));

      Bitmap fakeI = new Bitmap(dRect.Width, dRect.Height);
      g = Graphics.FromImage(fakeI);

      g.Clear(Color.Black);
      g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
      g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;



      g.DrawImage(FakeR, new Rectangle(0, 0, dRect.Width, dRect.Height), FakeHRect, GraphicsUnit.Pixel);
      pctHistory.Pixbuf = modFunctions.ImageToPixbuf(fakeI);
      
    }
    #region "Buttons"
    private void cmdSave_Click(System.Object sender, System.EventArgs e)
    {
      mySettings.Colors.MainDownA = modFunctions.GdkColorToDrawingColor(cmdMainUsedA.Color);
      if (chkMainUsedB.Active)
      {
        mySettings.Colors.MainDownB = modFunctions.GdkColorToDrawingColor(cmdMainUsedB.Color);
      }
      else
      {
        mySettings.Colors.MainDownB = Color.Transparent;
      }
      mySettings.Colors.MainDownC = modFunctions.GdkColorToDrawingColor(cmdMainUsedC.Color);
      mySettings.Colors.MainText = modFunctions.GdkColorToDrawingColor(cmdMainText.Color);
      mySettings.Colors.MainBackground = modFunctions.GdkColorToDrawingColor(cmdMainBG.Color);
      mySettings.Colors.TrayDownA = modFunctions.GdkColorToDrawingColor(cmdTrayUsedA.Color);
      if (chkTrayUsedB.Active)
      {
        mySettings.Colors.TrayDownB = modFunctions.GdkColorToDrawingColor(cmdTrayUsedB.Color);
      }
      else
      {
        mySettings.Colors.TrayDownB = Color.Transparent;
      }
      mySettings.Colors.TrayDownC = modFunctions.GdkColorToDrawingColor(cmdTrayUsedC.Color);
      mySettings.Colors.HistoryDownLine = modFunctions.GdkColorToDrawingColor(cmdHistoryUsedLine.Color);
      mySettings.Colors.HistoryDownA = modFunctions.GdkColorToDrawingColor(cmdHistoryUsedA.Color);
      if (chkHistoryUsedB.Active)
      {
        mySettings.Colors.HistoryDownB = modFunctions.GdkColorToDrawingColor(cmdHistoryUsedB.Color);
      }
      else
      {
        mySettings.Colors.HistoryDownB = Color.Transparent;
      }
      mySettings.Colors.HistoryDownC = modFunctions.GdkColorToDrawingColor(cmdHistoryUsedC.Color);
      mySettings.Colors.HistoryDownMax = modFunctions.GdkColorToDrawingColor(cmdHistoryUsedMax.Color);
      mySettings.Colors.HistoryText = modFunctions.GdkColorToDrawingColor(cmdHistoryText.Color);
      mySettings.Colors.HistoryBackground = modFunctions.GdkColorToDrawingColor(cmdHistoryBG.Color);
      mySettings.Colors.HistoryLightGrid = modFunctions.GdkColorToDrawingColor(cmdHistoryLightGrid.Color);
      mySettings.Colors.HistoryDarkGrid = modFunctions.GdkColorToDrawingColor(cmdHistoryDarkGrid.Color);
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
    private void cmdColor_MouseUp(Object sender, Gtk.ButtonReleaseEventArgs e)
    {
      if (e.Event.Button == 3)
      {
        cbSelected = (Gtk.ColorButton)sender;
        mnuColorOpts.ShowAll();
        mnuColorOpts.Popup();
      }
      RedrawImages();
    }
    private void pctMain_MouseUp(object sender, Gtk.ButtonReleaseEventArgs e)
    {
      if (e.Event.Button == 1)
      {
        if (dDown)
        {
          lUsed += iD;
          if (lUsed >= lLimit)
          {
            dDown = false;
          }
        }
        else
        {
          lUsed -= iD;
          if (lUsed <= 0)
          {
            dDown = true;
          }
        }
      }
      RedrawImages();
    }
    private void pctTray_MouseUp(object sender, Gtk.ButtonReleaseEventArgs e)
    {
      if (e.Event.Button == 1)
      {
        if (dDown)
        {
          lUsed += iD;
          if (lUsed >= lLimit)
          {
            dDown = false;
          }
        }
        else
        {
          lUsed -= iD;
          if (lUsed <= 0)
          {
            dDown = true;
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
        SetElColor(ref cmdColor, DefaultColorForElement(cmdColor.Name));
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
        ColorList = new Gtk.ColorButton[] { cmdMainUsedA, cmdMainUsedB, cmdMainUsedC, cmdMainText, cmdMainBG };
      }
      else if (cmdColor.Name.StartsWith("cmdTray"))
      {
        ColorList = new Gtk.ColorButton[] { cmdTrayUsedA, cmdTrayUsedB, cmdTrayUsedC };
      }
      else
      {
        ColorList = new Gtk.ColorButton[] { cmdHistoryUsedLine, cmdHistoryUsedA, cmdHistoryUsedB, cmdHistoryUsedC, cmdHistoryUsedMax, cmdHistoryText, cmdHistoryBG, cmdHistoryLightGrid, cmdHistoryDarkGrid };
      }
      for (int i = 0; i < ColorList.Length; i++)
      {
        Gtk.ColorButton pColor = ColorList[i];
        Color bColor = DefaultColorForElement(pColor.Name);
        SetElColor(ref pColor, bColor);
      }
      RedrawImages();
      cmdSave.Sensitive = SettingsChanged();
    }
    private void mnuAllDefault_Click(System.Object sender, System.EventArgs e)
    {
      Gtk.ColorButton[] ColorList = null;
      ColorList = new Gtk.ColorButton[] { cmdMainUsedA, cmdMainUsedB, cmdMainUsedC, cmdMainText, cmdMainBG, cmdTrayUsedA, cmdTrayUsedB, cmdTrayUsedC, cmdHistoryUsedLine, cmdHistoryUsedA, cmdHistoryUsedB, cmdHistoryUsedC, cmdHistoryUsedMax, cmdHistoryText, cmdHistoryBG, cmdHistoryLightGrid, cmdHistoryDarkGrid };
      for (int i = 0; i < ColorList.Length; i++)
      {
        Gtk.ColorButton pColor = ColorList[i];
        Color bColor = DefaultColorForElement(pColor.Name);
        SetElColor(ref pColor, bColor);
      }
      RedrawImages();
      cmdSave.Sensitive = SettingsChanged();
    }
    #endregion
    #region "Functions"
    private bool SettingsChanged()
    {
      if (!modFunctions.CompareColors(mySettings.Colors.MainDownA, cmdMainUsedA.Color))
        return true;
      if (chkMainUsedB.Active)
      {
        if (!modFunctions.CompareColors(mySettings.Colors.MainDownB, cmdMainUsedB.Color))
          return true;
      }
      else
      {
        if (mySettings.Colors.MainDownB != Color.Transparent)
          return true;
      }
      if (!modFunctions.CompareColors(mySettings.Colors.MainDownC, cmdMainUsedC.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.MainText, cmdMainText.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.MainBackground, cmdMainBG.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.TrayDownA, cmdTrayUsedA.Color))
        return true;
      if (chkTrayUsedB.Active)
      {
        if (!modFunctions.CompareColors(mySettings.Colors.TrayDownB, cmdTrayUsedB.Color))
          return true;
      }
      else
      {
        if (mySettings.Colors.TrayDownB != Color.Transparent)
          return true;
      }
      if (!modFunctions.CompareColors(mySettings.Colors.TrayDownC, cmdTrayUsedC.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryDownLine, cmdHistoryUsedLine.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryDownA, cmdHistoryUsedA.Color))
        return true;
      if (chkHistoryUsedB.Active)
      {
        if (!modFunctions.CompareColors(mySettings.Colors.HistoryDownB, cmdHistoryUsedB.Color))
          return true;
      }
      else
      {
        if (mySettings.Colors.HistoryDownB != Color.Transparent)
          return true;
      }
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryDownC, cmdHistoryUsedC.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryDownMax, cmdHistoryUsedMax.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryText, cmdHistoryText.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryBackground, cmdHistoryBG.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryLightGrid, cmdHistoryLightGrid.Color))
        return true;
      if (!modFunctions.CompareColors(mySettings.Colors.HistoryDarkGrid, cmdHistoryDarkGrid.Color))
        return true;
      return false;

    }
    private Gtk.Widget getControlFromName(ref Gtk.Table wIn, string name)
    {
      try
      {
        if (String.Compare(wIn.Name.Trim(), name.Trim()) == 0)
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
      catch (Exception)
      {
        return null;
      }
    }
    private Gtk.Widget getControlFromName(ref Gtk.Widget wIn, string name)
    {
      try
      {
        if (String.Compare(wIn.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
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
      catch (Exception)
      {
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
    private Color DefaultColorForElement(string Element)
    {
      switch (Element)
      {
        case "cmdMainUsedA":
          return Color.DarkBlue;
        case "cmdMainUsedB":
          return Color.Blue;
        case "cmdMainUsedC":
          return Color.Aqua;
        case "cmdMainText":
          return Color.White;
        case "cmdMainBG":
          return Color.Black;

        case "cmdTrayUsedA":
          return Color.DarkBlue;
        case "cmdTrayUsedB":
          return Color.Blue;
        case "cmdTrayUsedC":
          return Color.Aqua;

        case "cmdHistoryUsedLine":
          return Color.DarkBlue;
        case "cmdHistoryUsedA":
          return Color.DarkBlue;
        case "cmdHistoryUsedB":
          return Color.Blue;
        case "cmdHistoryUsedC":
          return Color.Aqua;
        case "cmdHistoryUsedMax":
          return Color.Yellow;
        case "cmdHistoryText":
          return Color.Black;
        case "cmdHistoryBG":
          return Color.White;
        case "cmdHistoryLightGrid":
          return Color.LightGray;
        case "cmdHistoryDarkGrid":
          return Color.DarkGray;
        default:
          return Color.Transparent;
      }
    }
    private void MakeFakeData()
    {
      FakeData = new System.Collections.Generic.List<DataRow>();
      System.Collections.Generic.List<int> UsedList = new System.Collections.Generic.List<int>();
      System.DateTime startDate = new System.DateTime(2000, 1, 1, 0, 0, 0);
      int startUsed = 0;
      for (int I = 1; I <= 90; I++)
      {
        DataRow dRow = new DataRow(startDate, startUsed, 15000);
        int iUsed = RandSel(50, 500);
        UsedList.Add(iUsed);
        startUsed += iUsed;
        startDate = startDate.AddDays(1);
        if (I > 29)
        {
          startUsed -= UsedList[I - 30];
        }
        FakeData.Add(dRow);
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
    #endregion
  }
}
