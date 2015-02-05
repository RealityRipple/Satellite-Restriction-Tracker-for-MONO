using System;
using Gtk;
using RestrictionLibrary;
namespace RestrictionTrackerGTK
{
  public partial class frmHistory : Gtk.Window
  {
    DateTimeWidget dtwFrom;
    DateTimeWidget dtwTo;
    ScrolledWindow sclUsage;
    DataListView lvUsage;
    internal AppSettings mySettings;
    private Gdk.Rectangle lastRect;
    private System.Drawing.Rectangle graphSpaceD;
    private System.Drawing.Rectangle graphSpaceU;
    private System.DateTime graphMinX;
    private System.DateTime graphMaxX;
    private DataBase.DataRow[] extraData;
    private localRestrictionTracker.SatHostTypes useStyle = localRestrictionTracker.SatHostTypes.Other;
    private delegate void ParameterizedInvoker(object parameter);
    private delegate void ParameterizedInvoker2(object param1,object param2);
    public frmHistory() : base(Gtk.WindowType.Toplevel)
    {
      Gdk.Geometry geo = new Gdk.Geometry();
      geo.MinWidth = 575;
      geo.MinHeight = 350;
      this.SetGeometryHints(null, geo, Gdk.WindowHints.MinSize);


      dtwFrom = new DateTimeWidget(DateTime.Now);
      dtwTo = new DateTimeWidget(DateTime.Now);

      dtwFrom.DateChanged += dtwFrom_DateChanged;
      dtwTo.DateChanged += dtwTo_DateChanged;

      this.Build();

      this.ShowAll();

      mySettings = new AppSettings();
      useStyle = mySettings.AccountType;

      if (mySettings.Colors.HistoryDownA == System.Drawing.Color.Transparent)
      {
        SetDefaultColors();
      }

      if (mySettings.Gr == "id")
      {
        optGrid.Active = true;
      }
      else
      {
        optGraph.Active = true;
      }
      cmdQuery.Clicked += cmdQuery_Click;
      cmdToday.Clicked += cmdToday_Click;
      cmd30Days.Clicked += cmd30Days_Click;
      cmd60Days.Clicked += cmd60Days_Click;
      cmdAllTime.Clicked += cmdAllTime_Click;


      evnDld.AddEvents((int)(Gdk.EventMask.PointerMotionMask));
      evnDld.MotionNotifyEvent += pctDld_MouseMove;
      evnUld.AddEvents((int)(Gdk.EventMask.PointerMotionMask));
      evnUld.MotionNotifyEvent += pctUld_MouseMove;

      algnFrom.Add(dtwFrom);
      ((Gtk.Table.TableChild)pnlAge[algnFrom]).LeftAttach = 1;
      ((Gtk.Table.TableChild)pnlAge[algnFrom]).TopAttach = 0;
      dtwFrom.TooltipText = "Oldest date to display records from.";

      algnTo.Add(dtwTo);
      ((Gtk.Table.TableChild)pnlAge[algnTo]).LeftAttach = 1;
      ((Gtk.Table.TableChild)pnlAge[algnTo]).TopAttach = 1;
      dtwTo.TooltipText = "Most recent date to display records from.";

      dtwFrom.ShowAll();
      dtwTo.ShowAll();

      ResetDates();
      dtwFrom.Date = dtwFrom.MinDate;
      dtwTo.Date = dtwTo.MaxDate;

      switch (useStyle)
      {
        case localRestrictionTracker.SatHostTypes.DishNet_EXEDE:
        case localRestrictionTracker.SatHostTypes.WildBlue_EXEDE:
        case localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE:
          ((Gtk.Label) cmd30Days.Child).LabelProp = "This Period";
          ((Gtk.Label) cmd60Days.Child).LabelProp = "Last Period";
          break;
        default:
          ((Gtk.Label) cmd30Days.Child).LabelProp = "30 Days";
          ((Gtk.Label) cmd60Days.Child).LabelProp = "60 Days";
          break;
      }

      this.SizeAllocated += frmHistory_ResizeBegin;
      this.Hidden += frmHistory_Hidden;

      switch (mySettings.Ago)
      {
        case 1:
          cmdToday.Click();
          break;
        case 30:
          cmd30Days.Click();
          break;
        case 60:
          cmd60Days.Click();
          break;
        case uint.MaxValue:
          cmdAllTime.Click();
          break;
        default:
          if (modFunctions.DateAdd(modFunctions.DateInterval.Day, -1 * mySettings.Ago, DateTime.Now) > dtwFrom.MinDate & modFunctions.DateAdd(modFunctions.DateInterval.Day, -1 * mySettings.Ago, DateTime.Now) < dtwFrom.MaxDate)
          {
            dtwFrom.Date = modFunctions.DateAdd(modFunctions.DateInterval.Day, -1 * mySettings.Ago, DateTime.Now);
          }
          else
          {
            dtwFrom.Date = dtwFrom.MinDate;
          }
          cmdQuery.Click();
          break;
      }
    }
    #region "Form Events"
    private uint tmrResizeCheck;
    private void frmHistory_ResizeBegin(object sender, SizeAllocatedArgs e)
    {
      if (e.Allocation.Equals(lastRect))
      {
        return;
      }
      lastRect = e.Allocation;
      if (tmrResizeCheck != 0)
      {
        GLib.Source.Remove(tmrResizeCheck);
        tmrResizeCheck = 0;
      }
      pctDld.Pixbuf = ResizingNote(pctDld.Allocation.Size);
      pctUld.Pixbuf = ResizingNote(pctUld.Allocation.Size);
      tmrResizeCheck = GLib.Timeout.Add(1000, frmHistory_ResizeEnd);
    }
    private bool frmHistory_ResizeEnd()
    {
      if (tmrResizeCheck != 0)
      {
        GLib.Source.Remove(tmrResizeCheck);
        tmrResizeCheck = 0;
      }
      DoResize(true);
      return false;
    }
    private void frmHistory_Hidden(object sender, EventArgs e)
    {
      HideProgress();
      UInt32 SetAgo = 30;
      if (mySettings != null)
        SetAgo = mySettings.Ago;
      mySettings = new AppSettings();
      mySettings.Ago = SetAgo;
      mySettings.Gr = (optGraph.Active ? "aph" : "id");
      mySettings.Save();
      MainClass.fMain.ReLoadSettings();
    }
    #endregion
    #region "Graph"
    private class DidResizeEventArgs :EventArgs
    {
      public System.Drawing.Image downRet;
      public System.Drawing.Image upRet;
      public DidResizeEventArgs(System.Drawing.Image down, System.Drawing.Image up)
      {
        downRet = down;
        upRet = up;
      }
    }
    private void DidResize(object o, EventArgs ea)
    {
      DidResizeEventArgs e = (DidResizeEventArgs)ea;
      if (e.downRet == null)
      {
        pctDld.Pixbuf = null;
      }
      if (e.upRet == null)
      {
        pctUld.Pixbuf = null;
      }
      if (e.downRet != null)
      {
        pctDld.Pixbuf = modFunctions.ImageToPixbuf(e.downRet);
      }
      if (e.upRet != null)
      {
        pctUld.Pixbuf = modFunctions.ImageToPixbuf(e.upRet);
      }
      graphSpaceD = modFunctions.GetGraphRect(true, out graphMinX, out graphMaxX);
      DateTime nul;
      graphSpaceU = modFunctions.GetGraphRect(false, out nul, out nul);
      HideProgress();
    }
    private void DoGraph(object o)
    {
      object[] state = (object[])o;
      byte graphStyle = (byte)(state[0]);
      DataBase.DataRow[] graphData = (DataBase.DataRow[])state[1];
      System.Drawing.Size downSize = (System.Drawing.Size)state[2];
      switch (graphStyle)
      {
        case 0:
          System.Drawing.Size upSize = (System.Drawing.Size)state[3];
          System.Drawing.Image bDown = modFunctions.DrawLineGraph(graphData, true, downSize, mySettings.Colors.HistoryDownA, mySettings.Colors.HistoryDownB, mySettings.Colors.HistoryDownC, mySettings.Colors.HistoryText, mySettings.Colors.HistoryBackground, mySettings.Colors.HistoryDownMax);
          System.Drawing.Image bUp = modFunctions.DrawLineGraph(extraData, false, upSize, mySettings.Colors.HistoryUpA, mySettings.Colors.HistoryUpB, mySettings.Colors.HistoryUpC, mySettings.Colors.HistoryText, mySettings.Colors.HistoryBackground, mySettings.Colors.HistoryUpMax);
          if (!this.Visible)
          {
            return;
          }
          Gtk.Application.Invoke(null, new DidResizeEventArgs(bDown, bUp), DidResize);
          break;
        case 1:
          System.Drawing.Image bRural = modFunctions.DrawRGraph(graphData, downSize, mySettings.Colors.HistoryDownA, mySettings.Colors.HistoryDownB, mySettings.Colors.HistoryDownC, mySettings.Colors.HistoryText, mySettings.Colors.HistoryBackground, mySettings.Colors.HistoryDownMax);
          if (!this.Visible)
          {
            return;
          }
          Gtk.Application.Invoke(null, new DidResizeEventArgs(bRural, null), DidResize);
          break;
      }
    }
    internal void DoResize(bool Forced = false)
    {
      if (this.Visible)
      {
        if (pnlGraph.Visible)
        {
          if (!(this.Allocation.Equals(lastRect)) | Forced)
          {
            ShowProgress("Drawing Graph...", "Collecting data, estimating, and resizing...");
            DataBase.DataRow[] lItems = extraData;
            if ((lItems == null || lItems.Length == 0) | (modDB.usageDB == null || modDB.usageDB.Count == 0))
            {
              evnUld.Visible = false;
              pnlGraph.Homogeneous = false;
              pnlGraph.CheckResize();
              lastRect = this.Allocation;
              pctDld.Pixbuf = NoDataNote(pctDld.Allocation.Size);
              HideProgress();
            }
            else
            {
              ParameterizedInvoker GraphInvoker = new ParameterizedInvoker(DoGraph);
              bool bDisplayed = false;
              switch (useStyle)
              {
                case localRestrictionTracker.SatHostTypes.DishNet_EXEDE:

                  evnUld.Visible = true;
                  pnlGraph.Homogeneous = true;
                  pnlGraph.CheckResize();
                  lastRect = this.Allocation;
                  pctDld.Pixbuf = ResizingNote(pctDld.Allocation.Size);
                  pctUld.Pixbuf = ResizingNote(pctUld.Allocation.Size);
                  object DGraphData = (object)new object[] { (byte)0, extraData, modFunctions.GdkSizeToDrawingSize(pctDld.Allocation.Size), modFunctions.GdkSizeToDrawingSize(pctUld.Allocation.Size) };
                  GraphInvoker.BeginInvoke(DGraphData, null, null);
                  bDisplayed = true;
                  break;
                case localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE:
                case localRestrictionTracker.SatHostTypes.WildBlue_EXEDE:
                  evnUld.Visible = false;
                  pnlGraph.Homogeneous = false;
                  pnlGraph.CheckResize();
                  lastRect = this.Allocation;
                  pctDld.Pixbuf = ResizingNote(pctDld.Allocation.Size);
                  object RGraphData = (object)new object[] { (byte)1, extraData, modFunctions.GdkSizeToDrawingSize(pctDld.Allocation.Size) };
                  GraphInvoker.BeginInvoke(RGraphData, null, null);
                  bDisplayed = true;
                  break;
                case localRestrictionTracker.SatHostTypes.WildBlue_LEGACY:
                case localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY:
                  evnUld.Visible = true;
                  pnlGraph.Homogeneous = true;
                  pnlGraph.CheckResize();
                  lastRect = this.Allocation;
                  pctDld.Pixbuf = ResizingNote(pctDld.Allocation.Size);
                  pctUld.Pixbuf = ResizingNote(pctUld.Allocation.Size);
                  object WGraphData = (object)new object[] { (byte)0, extraData, modFunctions.GdkSizeToDrawingSize(pctDld.Allocation.Size), modFunctions.GdkSizeToDrawingSize(pctUld.Allocation.Size) };
                  GraphInvoker.BeginInvoke(WGraphData, null, null);
                  bDisplayed = true;
                  break;
              }
              if (!bDisplayed)
              {
                if (modDB.usageDB.GetLast().DOWNLIM == modDB.usageDB.GetLast().UPLIM)
                {
                  if (modDB.usageDB.GetLast().DOWNLOAD == modDB.usageDB.GetLast().UPLOAD)
                  {
                    //RuralPortal
                    evnUld.Visible = false;
                    pnlGraph.Homogeneous = false;
                    lastRect = this.Allocation;
                    pctDld.Pixbuf = ResizingNote(pctDld.Allocation.Size);
                    object RGraphData = (object)new object[] { (byte)1, extraData, modFunctions.GdkSizeToDrawingSize(pctDld.Allocation.Size) };
                    GraphInvoker.BeginInvoke(RGraphData, null, null);
                  }
                  else
                  {
                    //Exede
                    evnUld.Visible = false;
                    pnlGraph.Homogeneous = false;
                    lastRect = this.Allocation;
                    pctDld.Pixbuf = ResizingNote(pctDld.Allocation.Size);
                    object EGraphData = (object)new object[] { (byte)2, extraData, modFunctions.GdkSizeToDrawingSize(pctDld.Allocation.Size) };
                    GraphInvoker.BeginInvoke(EGraphData, null, null);
                  }
                }
                else
                {
                  evnUld.Visible = true;
                  pnlGraph.Homogeneous = true;
                  lastRect = this.Allocation;
                  pctDld.Pixbuf = ResizingNote(pctDld.Allocation.Size);
                  pctUld.Pixbuf = ResizingNote(pctUld.Allocation.Size);
                  object WGraphData = (object)new object[] { (byte)0, extraData, modFunctions.GdkSizeToDrawingSize(pctDld.Allocation.Size), modFunctions.GdkSizeToDrawingSize(pctUld.Allocation.Size) };
                  GraphInvoker.BeginInvoke(WGraphData, null, null);
                }
              }
            }
          }
        }
      }
    }
    string static_pctDld_MouseMove_lastShow;
    private void pctDld_MouseMove(object sender, MotionNotifyEventArgs e)
    {
      if (graphSpaceD.IsEmpty)
      {
        return;
      }
      if (graphSpaceD.Contains((int)e.Event.X, (int)e.Event.Y))
      {
        System.DateTime dNow = CalculateNow(graphSpaceD, graphMinX, graphMaxX, e.Event.X);
        DataBase.DataRow gShow = modFunctions.GetGraphData(dNow, true);
        string showTime = gShow.sDATETIME;
        string Show = showTime + " : " + gShow.sDOWNLOAD + " MB / " + gShow.sDOWNLIM + " MB";
        if (static_pctDld_MouseMove_lastShow == Show)
        {
          return;
        }
        static_pctDld_MouseMove_lastShow = Show;
        pctDld.TooltipText = null;
        pctDld.TriggerTooltipQuery();
        pctDld.TooltipText = Show;
        pctDld.TriggerTooltipQuery();
      }
      else
      {
        pctDld.TooltipText = null;
        pctDld.TriggerTooltipQuery();
      }
    }
    string static_pctUld_MouseMove_lastShow;
    private void pctUld_MouseMove(object sender, MotionNotifyEventArgs e)
    {
      if (graphSpaceU.IsEmpty)
      {
        return;
      }
      if (graphSpaceU.Contains((int)e.Event.X, (int)e.Event.Y))
      {
        System.DateTime dNow = CalculateNow(graphSpaceU, graphMinX, graphMaxX, e.Event.X);
        DataBase.DataRow gShow = modFunctions.GetGraphData(dNow, false);
        string Show = gShow.sDATETIME + " : " + gShow.sUPLOAD + " MB / " + gShow.sUPLIM + " MB";
        if (static_pctUld_MouseMove_lastShow == Show)
        {
          return;
        }
        static_pctUld_MouseMove_lastShow = Show;
        pctUld.TooltipText = null;
        pctUld.TriggerTooltipQuery();
        pctUld.TooltipText = Show;
        pctUld.TriggerTooltipQuery();
      }
      else
      {
        pctUld.TooltipText = null;
        pctUld.TriggerTooltipQuery();
      }
    }
    private System.DateTime CalculateNow(System.Drawing.Rectangle GraphSpace, System.DateTime StartX, System.DateTime EndX, double X)
    {
      long DateSpan = modFunctions.DateDiff(modFunctions.DateInterval.Minute, StartX, EndX);
      return modFunctions.DateAdd(modFunctions.DateInterval.Minute, ((X - GraphSpace.Left) / GraphSpace.Width) * DateSpan, StartX);
    }
    #endregion
    #region "Buttons"
    private void cmdQuery_Click(System.Object sender, System.EventArgs e)
    {
      grpAge.Sensitive = false;
      System.DateTime dFrom = System.DateTime.Parse(dtwFrom.Date.Date.ToShortDateString() + " 00:00:00 AM");
      System.DateTime dTo = System.DateTime.Parse(dtwTo.Date.Date.ToShortDateString() + " 11:59:59 PM");
      DataBase.DataRow[] lItems = null;
      ShowProgress("Querying DataBase...", "Reading Rows...");
      bool runResize = false;
      if (optGraph.Active)
      {
        if (sclUsage != null)
        {
          if (sclUsage.Visible)
          {
            sclUsage.Visible = false;
            pnlHistory.Remove(sclUsage);
          }
        }
        if (!pnlGraph.Visible)
        {
          pnlHistory.Add(pnlGraph);
          ((Gtk.Box.BoxChild)pnlHistory[pnlGraph]).Position = 1;
          pnlGraph.Visible = true;
        }
        if (modDB.usageDB != null && modDB.usageDB.Count > 0)
        {
          lItems = Array.FindAll(modDB.usageDB.ToArray(), (DataBase.DataRow satRow) => satRow.DATETIME.CompareTo(dFrom) >= 0 & satRow.DATETIME.CompareTo(dTo) <= 0);
          extraData = lItems;
          DoResize(true);
        }
        else
        {
          extraData = null;
          DoResize(true);
        }
        runResize = true;
      }
      else
      {
        if (pnlGraph.Visible)
        {
          pnlGraph.Visible = false;
          pnlHistory.Remove(pnlGraph);
        }
        if (modDB.usageDB != null && modDB.usageDB.Count > 0)
        {
          lItems = Array.FindAll(modDB.usageDB.ToArray(), (DataBase.DataRow satRow) => satRow.DATETIME.CompareTo(dFrom) >= 0 & satRow.DATETIME.CompareTo(dTo) <= 0);
        }
        else
        {
          lItems = null;
        }
        if (sclUsage == null)
        {
          bool SameLim = true;
          switch (useStyle)
          {
            case localRestrictionTracker.SatHostTypes.DishNet_EXEDE:
              if (lItems != null)
              {
                long dnDLim = 0;
                long dnULim = 0;
                foreach (DataBase.DataRow lItem in lItems)
                {
                  if (dnDLim == 0)
                  {
                    dnDLim = lItem.DOWNLIM;
                  }
                  else
                  {
                    if (!(dnDLim == lItem.DOWNLIM))
                    {
                      SameLim = false;
                      break;
                    }
                  }
                  if (dnULim == 0)
                  {
                    dnULim = lItem.UPLIM;
                  }
                  else
                  {
                    if (!(dnULim == lItem.UPLIM))
                    {
                      SameLim = false;
                      break;
                    }
                  }
                }
              }
              else
              {
                SameLim = true;
              }
              if (SameLim)
              {
                lvUsage = (DataListView)new DishNetView();
              }
              else
              {
                lvUsage = (DataListView)new DishNetLimsView();
              }
              break;
            case localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE:
            case localRestrictionTracker.SatHostTypes.WildBlue_EXEDE:
              lvUsage = (DataListView)new RuralPortalView();
              break;
            default:
              if (lItems != null)
              {
                long wbDLim = 0;
                long wbULim = 0;
                foreach (DataBase.DataRow lItem in lItems)
                {
                  if (wbDLim == 0)
                  {
                    wbDLim = lItem.DOWNLIM;
                  }
                  else
                  {
                    if (!(wbDLim == lItem.DOWNLIM))
                    {
                      SameLim = false;
                      break;
                    }
                  }
                  if (wbULim == 0)
                  {
                    wbULim = lItem.UPLIM;
                  }
                  else
                  {
                    if (!(wbULim == lItem.UPLIM))
                    {
                      SameLim = false;
                      break;
                    }
                  }
                }
              }
              else
              {
                SameLim = false;
              }
              if (SameLim)
              {
                lvUsage = (DataListView)new WildBlueView();
              }
              else
              {
                lvUsage = (DataListView)new WildBlueLimsView();
              }
              break;
          }
          sclUsage = new ScrolledWindow();
          sclUsage.Add(lvUsage);
          pnlHistory.Add(sclUsage);
          ((Gtk.Box.BoxChild)pnlHistory[sclUsage]).Position = 1;
          sclUsage.Visible = true;
          lvUsage.Visible = true;

          lvUsage.Columns[0].Resizable = true;
          lvUsage.Columns[1].Resizable = true;
          lvUsage.Columns[2].Resizable = true;

          lvUsage.Columns[0].Expand = true;
          lvUsage.Columns[1].Alignment = 0.5f;
          lvUsage.Columns[1].CellRenderers[0].Xalign = 1.0f;
          lvUsage.Columns[2].Alignment = 0.5f;
          lvUsage.Columns[2].CellRenderers[0].Xalign = 1.0f;
        }
        else if (!sclUsage.Visible)
        {
          pnlHistory.Add(sclUsage);
          ((Gtk.Box.BoxChild)pnlHistory[sclUsage]).Position = 1;
          sclUsage.Visible = true;
          lvUsage.Visible = true;
        }

        if (modDB.usageDB != null && modDB.usageDB.Count > 0)
        {
          lvUsage.ClearItems();
          ShowProgress("", "Populating Table...");
          foreach (DataBase.DataRow lItem in lItems)
          {
            lvUsage.AddItem(lItem);
          }
        }
        else
        {
          lvUsage.ClearItems();
        }
      }
      if (runResize)
      {
        sbHistoryStatus.Pop(0);
      }
      else
      {
        HideProgress();
      }
      mySettings.Ago = (uint)Math.Abs(modFunctions.DateDiff(modFunctions.DateInterval.Day, dtwFrom.Date.Date, dtwTo.Date.Date));
      if (mySettings.Ago == 0)
      {
        mySettings.Ago = 1;
      }
      grpAge.Sensitive = true;
    }
    private void cmdToday_Click(System.Object sender, System.EventArgs e)
    {
      System.DateTime RightNow = new System.DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
      System.DateTime FromNow = default(System.DateTime);
      if (RightNow > dtwFrom.MaxDate)
      {
        FromNow = dtwFrom.MaxDate;
      }
      else if (RightNow < dtwFrom.MinDate)
      {
        FromNow = dtwFrom.MinDate;
      }
      else
      {
        FromNow = RightNow;
      }
      System.DateTime ToNow = default(System.DateTime);
      if (RightNow > dtwTo.MaxDate)
      {
        ToNow = dtwTo.MaxDate;
      }
      else if (RightNow < dtwTo.MinDate)
      { 
        ToNow = dtwTo.MinDate;
      }
      else
      {
        ToNow = RightNow;
      } 
      if (FromNow.Year < 2000)
        FromNow = dtwFrom.MinDate;
      if (ToNow.Year < 2000)
        ToNow = dtwTo.MinDate;
      dtwFrom.Date = FromNow;
      dtwTo.Date = ToNow;
      cmdQuery.Click();
    }
    private void cmd30Days_Click(System.Object sender, System.EventArgs e)
    {
      System.DateTime RightNow = new System.DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
      System.DateTime From30DaysAgo = default(System.DateTime);
      if (useStyle == localRestrictionTracker.SatHostTypes.WildBlue_LEGACY || useStyle == localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY)
      {
        if (modFunctions.DateAdd(modFunctions.DateInterval.Day, -30, RightNow) > dtwFrom.MaxDate)
        {
          From30DaysAgo = dtwFrom.MaxDate;
        }
        else if (modFunctions.DateAdd(modFunctions.DateInterval.Day, -30, RightNow) < dtwFrom.MinDate)
        {
          From30DaysAgo = dtwFrom.MinDate;
        }
        else
        {
          From30DaysAgo = modFunctions.DateAdd(modFunctions.DateInterval.Day, -30, RightNow);
        } 
      }
      else
      {
        if (modDB.usageDB != null)
        {
          if (modDB.usageDB.Count > 1)
          {
            ShowProgress("Querying DataBase...", "Scanning for Resets...");
            for (int i = modDB.usageDB.Count - 1; i > 1; i--)
            {
              SetProgress(modDB.usageDB.Count - i, modDB.usageDB.Count, "");
              if ((modDB.usageDB.ToArray()[i].DOWNLOAD < 500 && modDB.usageDB.ToArray()[i].UPLOAD < 500) && (modDB.usageDB.ToArray()[i - 1].DOWNLOAD > 500 || modDB.usageDB.ToArray()[i - 1].UPLOAD > 500))
              {
                if (modDB.usageDB.ToArray()[i].DATETIME > dtwFrom.Date)
                {
                  From30DaysAgo = dtwFrom.MaxDate;
                }
                else if (modDB.usageDB.ToArray()[i].DATETIME < dtwFrom.MinDate)
                {
                  From30DaysAgo = dtwFrom.MinDate;
                }
                else
                {
                  From30DaysAgo = modDB.usageDB.ToArray()[i].DATETIME;
                }
                break;
              }
            }
          }
          else if (modDB.usageDB.Count > 0)
          {
            if (modDB.usageDB.ToArray()[0].DATETIME > dtwFrom.MaxDate)
            {
              From30DaysAgo = dtwFrom.MaxDate;
            }
            else if (modDB.usageDB.ToArray()[0].DATETIME < dtwFrom.MinDate)
            {
              From30DaysAgo = dtwFrom.MinDate;
            }
            else
            {
              From30DaysAgo = modDB.usageDB.ToArray()[0].DATETIME;
            }
          }
          else
          {
            From30DaysAgo = dtwFrom.MinDate;
          }
        }
        else
        {
          From30DaysAgo = dtwFrom.MinDate;
        }
      }

      System.DateTime ToNow = default(System.DateTime);
      if (RightNow > dtwTo.MaxDate)
      {
        ToNow = dtwTo.MaxDate;
      }
      else if (RightNow < dtwTo.MinDate)
      {
        ToNow = dtwTo.MinDate;
      }
      else
      {
        ToNow = RightNow;
      }
      if (From30DaysAgo.Year < 2000)
        From30DaysAgo = dtwFrom.MinDate;
      if (ToNow.Year < 2000)
        ToNow = dtwTo.MinDate;
      dtwFrom.Date = From30DaysAgo;
      dtwTo.Date = ToNow;
      HideProgress();
      cmdQuery.Click();
    }
    private void cmd60Days_Click(System.Object sender, System.EventArgs e)
    {
      System.DateTime RightNow = new System.DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
      System.DateTime From60DaysAgo = default(System.DateTime);
      if (useStyle == localRestrictionTracker.SatHostTypes.WildBlue_LEGACY || useStyle == localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY)
      {
        if (modFunctions.DateAdd(modFunctions.DateInterval.Day, -60, RightNow) > dtwFrom.MaxDate)
        {
          From60DaysAgo = dtwFrom.MaxDate;
        }
        else if (modFunctions.DateAdd(modFunctions.DateInterval.Day, -60, RightNow) < dtwFrom.MinDate)
        {
          From60DaysAgo = dtwFrom.MinDate;
        }
        else
        {
          From60DaysAgo = modFunctions.DateAdd(modFunctions.DateInterval.Day, -60, RightNow);
        }
      }
      else
      {
        if (modDB.usageDB != null)
        {
          if (modDB.usageDB.Count > 1)
          {
            ShowProgress("Querying DataBase...", "Scanning for Resets...");
            int Finds = 0;
            for (int i = modDB.usageDB.Count - 1; i > 1; i--)
            {
              SetProgress(modDB.usageDB.Count - i, modDB.usageDB.Count, "");
              if ((modDB.usageDB.ToArray()[i].DOWNLOAD < 500 && modDB.usageDB.ToArray()[i].UPLOAD < 500) && (modDB.usageDB.ToArray()[i - 1].DOWNLOAD > 500 || modDB.usageDB.ToArray()[i - 1].UPLOAD > 500))
              {
                if (DateTime.Today.Subtract(modDB.usageDB.ToArray()[i].DATETIME).TotalDays > 6)
                {
                  Finds ++;
                  if (Finds == 2)
                  {
                    if (modDB.usageDB.ToArray()[i].DATETIME > dtwFrom.Date)
                    {
                      From60DaysAgo = dtwFrom.MaxDate;
                    }
                    else if (modDB.usageDB.ToArray()[i].DATETIME < dtwFrom.MinDate)
                    {
                      From60DaysAgo = dtwFrom.MinDate;
                    }
                    else
                    {
                      From60DaysAgo = modDB.usageDB.ToArray()[i].DATETIME;
                    }
                    break;
                  }
                }
              }
            }
            if (Finds < 2)
            {
              From60DaysAgo = dtwFrom.MinDate;
            }
          }
          else if (modDB.usageDB.Count > 0)
          {
            if (modDB.usageDB.ToArray()[0].DATETIME > dtwFrom.MaxDate)
            {
              From60DaysAgo = dtwFrom.MaxDate;
            }
            else if (modDB.usageDB.ToArray()[0].DATETIME < dtwFrom.MinDate)
            {
              From60DaysAgo = dtwFrom.MinDate;
            }
            else
            {
              From60DaysAgo = modDB.usageDB.ToArray()[0].DATETIME;
            }
          }
          else
          {
            From60DaysAgo = dtwFrom.MinDate;
          }
        }
        else
        {
          From60DaysAgo = dtwFrom.MinDate;
        }
      }
      System.DateTime ToNow = default(System.DateTime);
      if (RightNow > dtwTo.MaxDate)
      {
        ToNow = dtwTo.MaxDate;
      }
      else if (RightNow < dtwTo.MinDate)
      {
        ToNow = dtwTo.MinDate;
      }
      else
      {
        ToNow = RightNow;
      }
      if (From60DaysAgo.Year < 2000)
        From60DaysAgo = dtwFrom.MinDate;
      if (ToNow.Year < 2000)
        ToNow = dtwTo.MinDate;
      dtwFrom.Date = From60DaysAgo;
      dtwTo.Date = ToNow;
      HideProgress();
      cmdQuery.Click();
    }
    private void cmdAllTime_Click(System.Object sender, System.EventArgs e)
    {
      dtwFrom.Date = dtwFrom.MinDate;
      dtwTo.Date = dtwTo.MaxDate;
      cmdQuery.Click();
    }
    private void cmdImport_Click(System.Object sender, System.EventArgs e)
    {
      if ((modDB.usageDB == null || modDB.usageDB.Count == 0))
      {
        modFunctions.ShowMessageBox(this, "The Database has not been loaded yet, please wait.", "", DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok);
        return;
      }
      FileChooserDialog cdlOpen = new FileChooserDialog("Import History Database", this, FileChooserAction.Open, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Open, Gtk.ResponseType.Ok);
      Gtk.FileFilter fXML = new Gtk.FileFilter();
      fXML.AddPattern("*.xml");
      fXML.Name = "XML Files";
      Gtk.FileFilter fCSV = new Gtk.FileFilter();
      fCSV.AddPattern("*.csv");
      fCSV.Name = "CSV Files";
      Gtk.FileFilter fWB = new Gtk.FileFilter();
      fWB.AddPattern("*.wb");
      fWB.Name = "WB Files";
      cdlOpen.AddFilter(fXML);
      cdlOpen.AddFilter(fCSV);
      cdlOpen.AddFilter(fWB);
      cdlOpen.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
      ResponseType ret = (ResponseType)cdlOpen.Run();
      string sRet = cdlOpen.Filename;
      FileFilter fRet = cdlOpen.Filter;
      cdlOpen.Destroy();
      if (ret == ResponseType.Ok)
      {
        ShowProgress("Importing DataBase...", "Opening File...");
        DataBase wbImport = new DataBase(sRet, true);
        wbImport.ProgressState += usageDB_ProgressState;
        wbImport.StartNew();
        if (wbImport.Count > 0)
        {
          if (modDB.usageDB != null)
          {
            ShowProgress("", "Merging File and DataBase...");
            modDB.usageDB.Merge(wbImport, true);
          }
          else
          {
            ShowProgress("", "Loading File into DataBase...");
            modDB.usageDB = wbImport;
          }
          ShowProgress("", "Sorting DataBase...");
          modDB.LOG_Sort();
          ShowProgress("", "Saving DataBase...");
          modDB.LOG_Save(true);
          HideProgress();
          modFunctions.ShowMessageBox(this, System.IO.Path.GetFileName(sRet) + " has been merged into your history database.", "", DialogFlags.Modal, MessageType.Info, ButtonsType.Ok);
          System.Threading.Thread.Sleep(0);
          ResetDates();
        }
        else
        {
          HideProgress();
          modFunctions.ShowMessageBox(this, "Could not import " + System.IO.Path.GetFileName(sRet), "", DialogFlags.Modal, MessageType.Error, ButtonsType.Ok);
        }
      }
    }
    private void cmdExport_Click(System.Object sender, System.EventArgs e)
    {
      if ((modDB.usageDB == null || modDB.usageDB.Count == 0))
      {
        modFunctions.ShowMessageBox(this, "The Database has not been loaded yet, please wait.", "", DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok);
        return;
      }
      FileChooserDialog cdlSave = new FileChooserDialog("Export History Database", this, FileChooserAction.Save, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Save, Gtk.ResponseType.Ok);
      Gtk.FileFilter fXML = new Gtk.FileFilter();
      fXML.AddPattern("*.xml");
      fXML.Name = "XML File";
      Gtk.FileFilter fCSV = new Gtk.FileFilter();
      fCSV.AddPattern("*.csv");
      fCSV.Name = "CSV File";
      Gtk.FileFilter fWB = new Gtk.FileFilter();
      fWB.AddPattern("*.wb");
      fWB.Name = "WB File";
      cdlSave.AddFilter(fXML);
      cdlSave.AddFilter(fCSV);
      cdlSave.AddFilter(fWB);
      cdlSave.CurrentName = "Backup-" + mySettings.Account;
      cdlSave.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
      ResponseType ret = (ResponseType)cdlSave.Run();
      string sRet = cdlSave.Filename;
      FileFilter fRet = cdlSave.Filter;
      cdlSave.Destroy();
      if (ret == ResponseType.Ok)
      {
        if (fRet == fCSV)
        {
          if (!sRet.ToLower().EndsWith(".csv"))
          {
            sRet += ".csv";
          }
        }
        else if (fRet == fWB)
        {
          if (!sRet.ToLower().EndsWith(".wb"))
          {
            sRet += ".wb";
          }
        }
        else
        {
          if (!sRet.ToLower().EndsWith(".xml"))
          {
            sRet += ".xml";
          }
        }

        ShowProgress("Exporting DataBase...", "Saving File...");
        if (chkExportRange.Active)
        {
          System.DateTime dFrom = System.DateTime.Parse(dtwFrom.Date.Date + " 00:00:00 AM");
          System.DateTime dTo = System.DateTime.Parse(dtwTo.Date.Date + " 11:59:59 PM");
          DataBase newDB = new DataBase();
          newDB.ProgressState += usageDB_ProgressState;
          foreach (DataBase.DataRow satRow in modDB.usageDB)
          {
            if (satRow.DATETIME.CompareTo(dFrom) >= 0 & satRow.DATETIME.CompareTo(dTo) <= 0)
            {
              newDB.Add(satRow);
            }
          }
          newDB.Save(sRet, true);
          newDB = null;
        }
        else
        {
          modDB.usageDB.Save(sRet, true);
        }
        HideProgress();
        modFunctions.ShowMessageBox(this, "Your history has been exported to " + System.IO.Path.GetFileName(sRet), "", DialogFlags.Modal, MessageType.Info, ButtonsType.Ok);
      }
    }
    private void usageDB_ProgressState(object o, DataBase.ProgressStateEventArgs e)
    {
      SetProgress(e.Value, e.Total, "");
      Gtk.Main.IterationDo(false);
    }
    private void cmdClose_Click(System.Object sender, System.EventArgs e)
    {
      this.Hide();
    }
    #endregion
    #region "Date Pickers"
    private void dtwTo_DateChanged(object sender, DateTimeWidget.DateChangedEventArgs  e)
    {
      if (dtwFrom.Date > dtwTo.Date)
      {
        if (dtwTo.Date > dtwFrom.MaxDate)
        {
          dtwFrom.Date = dtwFrom.MaxDate;
        }
        else if (dtwTo.Date < dtwFrom.MinDate)
        {
          dtwFrom.Date = dtwFrom.MinDate;
        }
        else
        {
          dtwFrom.Date = dtwTo.Date;
        }
      }
    }
    private void dtwFrom_DateChanged(object sender, DateTimeWidget.DateChangedEventArgs e)
    {
      if (dtwFrom.Date > dtwTo.Date)
      {
        if (dtwFrom.Date > dtwTo.MaxDate)
        {
          dtwTo.Date = dtwTo.MaxDate;
        }
        else if (dtwFrom.Date < dtwTo.MinDate)
        {
          dtwTo.Date = dtwTo.MinDate;
        }
        else
        {
          dtwTo.Date = dtwFrom.Date;
        }
      }
    }
    private void ResetDates()
    {
      System.DateTime dMin = default(System.DateTime);
      System.DateTime dMax = default(System.DateTime);
      long nul;
      modDB.LOG_Get(0, out dMin, out nul, out nul, out nul, out nul);
      System.DateTime fDate = new DateTime(2000, 1, 1);
      if (modFunctions.DateDiff(modFunctions.DateInterval.Year, fDate, dMin) < 0)
      {
        dMin = fDate;
      }
      if (modFunctions.DateDiff(modFunctions.DateInterval.Second, dMin, DateTime.Now) < 0)
      {
        pctErr.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.error.png");
        pctErr.TooltipText = "The Log history is more recent than your system clock.\nPlease update your computer's time.";
      }
      else
      {
        dMax = DateTime.Now;
        pctErr.Pixbuf = null;
        pctErr.TooltipText = "";
        if (modFunctions.DateDiff(modFunctions.DateInterval.Second, dMin, dMax) > 0)
        {
          dtwFrom.MaxDate = dMax;
          dtwTo.MaxDate = dMax;
          dtwFrom.MinDate = dMin;
          dtwTo.MinDate = dMin;
        }
        else
        {
          pctErr.Pixbuf = Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.error.png");
          pctErr.TooltipText = "The Log history is more recent than your system clock.\nPlease update your computer's time.";
        }
      }
      if (dtwFrom.MinDate.Year < 2000)
        dtwFrom.MinDate = new DateTime(2000, 1, 1);
      if (dtwTo.MinDate.Year < 2000)
        dtwTo.MinDate = new DateTime(2000, 1, 1);
    }
    #endregion
    #region "Notices"
    private Gdk.Pixbuf NoDataNote(Gdk.Size ImgSize)
    {
      if (ImgSize.Width == 0 | ImgSize.Height == 0)
      {
        return null;
      }
      System.Drawing.Image iPic = new System.Drawing.Bitmap(ImgSize.Width, ImgSize.Height);
      using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(iPic))
      {
        const string sNote = "No data has yet been accumulated.";
        g.Clear(System.Drawing.SystemColors.ButtonFace);
        g.DrawString(sNote, System.Drawing.SystemFonts.DefaultFont, System.Drawing.SystemBrushes.ControlText, (ImgSize.Width / 2) - (g.MeasureString(sNote, System.Drawing.SystemFonts.DefaultFont).Width / 2), (ImgSize.Height / 2) - (g.MeasureString(sNote, System.Drawing.SystemFonts.DefaultFont).Height / 2));
      }
      return modFunctions.ImageToPixbuf(iPic);
    }
    private Gdk.Pixbuf ResizingNote(Gdk.Size ImgSize)
    {
      if (ImgSize.Width == 0 | ImgSize.Height == 0)
      {
        return null;
      }
      System.Drawing.Image iPic = new System.Drawing.Bitmap(ImgSize.Width, ImgSize.Height);
      using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(iPic))
      {
        g.Clear(System.Drawing.SystemColors.ButtonFace);
        g.FillRectangle(new System.Drawing.Drawing2D.LinearGradientBrush(new System.Drawing.Rectangle(0, 0, ImgSize.Width, ImgSize.Height), System.Drawing.Color.White, System.Drawing.Color.MidnightBlue, System.Drawing.Drawing2D.LinearGradientMode.Vertical), new System.Drawing.Rectangle(0, 0, ImgSize.Width, ImgSize.Height));
      }
      return modFunctions.ImageToPixbuf(iPic);
    }
    #endregion
    private void SetDefaultColors()
    {
      Gtk.Application.Invoke(Main_SetDefaultColors);
    }
    private void Main_SetDefaultColors(object o, EventArgs e)
    {
      switch (useStyle)
      {
        case localRestrictionTracker.SatHostTypes.WildBlue_LEGACY:
        case localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY:
          mySettings.Colors.MainDownA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.MainDownB = System.Drawing.Color.Blue;
          mySettings.Colors.MainDownC = System.Drawing.Color.Aqua;
          mySettings.Colors.MainUpA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.MainUpB = System.Drawing.Color.Blue;
          mySettings.Colors.MainUpC = System.Drawing.Color.Aqua;
          mySettings.Colors.MainText = System.Drawing.Color.White;
          mySettings.Colors.MainBackground = System.Drawing.Color.Black;

          mySettings.Colors.TrayDownA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.TrayDownB = System.Drawing.Color.Blue;
          mySettings.Colors.TrayDownC = System.Drawing.Color.Aqua;
          mySettings.Colors.TrayUpA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.TrayUpB = System.Drawing.Color.Blue;
          mySettings.Colors.TrayUpC = System.Drawing.Color.Aqua;

          mySettings.Colors.HistoryDownA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.HistoryDownB = System.Drawing.Color.Blue;
          mySettings.Colors.HistoryDownC = System.Drawing.Color.Aqua;
          mySettings.Colors.HistoryDownMax = System.Drawing.Color.Yellow;
          mySettings.Colors.HistoryUpA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.HistoryUpB = System.Drawing.Color.Blue;
          mySettings.Colors.HistoryUpC = System.Drawing.Color.Aqua;
          mySettings.Colors.HistoryUpMax = System.Drawing.Color.Yellow;
          mySettings.Colors.HistoryText = System.Drawing.Color.Black;
          mySettings.Colors.HistoryBackground = System.Drawing.Color.White;

          break;
        case localRestrictionTracker.SatHostTypes.WildBlue_EXEDE:
        case localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE:
          mySettings.Colors.MainDownA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.MainDownB = System.Drawing.Color.Blue;
          mySettings.Colors.MainDownC = System.Drawing.Color.Aqua;
          mySettings.Colors.MainUpA = System.Drawing.Color.Transparent;
          mySettings.Colors.MainUpB = System.Drawing.Color.Transparent;
          mySettings.Colors.MainUpC = System.Drawing.Color.Transparent;
          mySettings.Colors.MainText = System.Drawing.Color.White;
          mySettings.Colors.MainBackground = System.Drawing.Color.Black;

          mySettings.Colors.TrayDownA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.TrayDownB = System.Drawing.Color.Blue;
          mySettings.Colors.TrayDownC = System.Drawing.Color.Aqua;
          mySettings.Colors.TrayUpA = System.Drawing.Color.Transparent;
          mySettings.Colors.TrayUpB = System.Drawing.Color.Transparent;
          mySettings.Colors.TrayUpC = System.Drawing.Color.Transparent;

          mySettings.Colors.HistoryDownA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.HistoryDownB = System.Drawing.Color.Blue;
          mySettings.Colors.HistoryDownC = System.Drawing.Color.Aqua;
          mySettings.Colors.HistoryDownMax = System.Drawing.Color.Yellow;
          mySettings.Colors.HistoryUpA = System.Drawing.Color.Transparent;
          mySettings.Colors.HistoryUpB = System.Drawing.Color.Transparent;
          mySettings.Colors.HistoryUpC = System.Drawing.Color.Transparent;
          mySettings.Colors.HistoryUpMax = System.Drawing.Color.Transparent;
          mySettings.Colors.HistoryText = System.Drawing.Color.Black;
          mySettings.Colors.HistoryBackground = System.Drawing.Color.White;

          break;
        case localRestrictionTracker.SatHostTypes.DishNet_EXEDE:
          mySettings.Colors.MainDownA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.MainDownB = System.Drawing.Color.Blue;
          mySettings.Colors.MainDownC = System.Drawing.Color.Aqua;
          mySettings.Colors.MainUpA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.MainUpB = System.Drawing.Color.Blue;
          mySettings.Colors.MainUpC = System.Drawing.Color.Aqua;
          mySettings.Colors.MainText = System.Drawing.Color.White;
          mySettings.Colors.MainBackground = System.Drawing.Color.Black;

          mySettings.Colors.TrayDownA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.TrayDownB = System.Drawing.Color.Blue;
          mySettings.Colors.TrayDownC = System.Drawing.Color.Aqua;
          mySettings.Colors.TrayUpA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.TrayUpB = System.Drawing.Color.Blue;
          mySettings.Colors.TrayUpC = System.Drawing.Color.Aqua;

          mySettings.Colors.HistoryDownA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.HistoryDownB = System.Drawing.Color.Blue;
          mySettings.Colors.HistoryDownC = System.Drawing.Color.Aqua;
          mySettings.Colors.HistoryDownMax = System.Drawing.Color.Yellow;
          mySettings.Colors.HistoryUpA = System.Drawing.Color.DarkBlue;
          mySettings.Colors.HistoryUpB = System.Drawing.Color.Blue;
          mySettings.Colors.HistoryUpC = System.Drawing.Color.Aqua;
          mySettings.Colors.HistoryUpMax = System.Drawing.Color.Yellow;
          mySettings.Colors.HistoryText = System.Drawing.Color.Black;
          mySettings.Colors.HistoryBackground = System.Drawing.Color.White;
          break;
        default:
          break;
      }
    }
    #region "StatusBar"
    private uint tmrPulse;
    public void ShowProgress(string Title, string Subtitle)
    {
      if (!String.IsNullOrEmpty(Title))
      {
        sbHistoryStatus.Push(0, Title);
      }
      if (!String.IsNullOrEmpty(Subtitle))
      {
        lblHistoryStatus.Text = Subtitle;
      }
      lblHistoryStatus.Visible = true;
      pbHistoryStatus.Visible = true;
      if (tmrPulse == 0)
      {
        tmrPulse = GLib.Timeout.Add(150, tmrPulse_Tick);
      }
    }
    private bool tmrPulse_Tick()
    {
      pbHistoryStatus.Pulse();
      Gtk.Main.IterationDo(false);
      return true;
    }
    public void SetProgress(int value, int max, string Subtitle)
    {
      if (max > 0)
      {
        if (tmrPulse != 0)
        {
          GLib.Source.Remove(tmrPulse);
          tmrPulse = 0;
        }
        pbHistoryStatus.Fraction = ((double)value / max);
      }
      else
      {
        if (tmrPulse != 0)
        {
          GLib.Source.Remove(tmrPulse);
          tmrPulse = 0;
        }
        tmrPulse = GLib.Timeout.Add(150, tmrPulse_Tick);
      }
      if (!String.IsNullOrEmpty(Subtitle))
      {
        lblHistoryStatus.Text = Subtitle;
      }
    }
    public void HideProgress()
    {
      if (tmrPulse != 0)
      {
        GLib.Source.Remove(tmrPulse);
        tmrPulse = 0;
      }
      sbHistoryStatus.Pop(0);
      pbHistoryStatus.Fraction = 0d;
      lblHistoryStatus.Text = "";
      pbHistoryStatus.Visible = false;
      lblHistoryStatus.Visible = false;
    }
    #endregion
    #region "DateTime Widget"
    class DateTimeWidget : VBox
    {
      HBox pnlDT;
      EventBox evnDTEvents;
      HBox pnlDropdown;
      ToggleButton cmdToggle;
      Gtk.Window frmDT;
      DateTime dtSelectedDate;
      DateTime dtMinimumDate;
      DateTime dtMaximumDate;
      Calendar calDate;
      Entry txtDate;
      public class DateChangedEventArgs : EventArgs
      {
        public DateTime date;
        public DateChangedEventArgs(DateTime d)
        {
          date = d;
        }
      }
      public delegate void DateChangedHandler(object o,DateChangedEventArgs date);
      public event DateChangedHandler DateChanged;
      public DateTime Date
      {
        get
        {
          return dtSelectedDate;
        }
        set
        {
          dtSelectedDate = value;
          txtDate.Text = dtSelectedDate.ToString("D");
        }
      }
      public DateTime MinDate
      {
        get
        {
          return dtMinimumDate;
        }
        set
        {
          dtMinimumDate = value;
          if (dtSelectedDate < dtMinimumDate)
          {
            dtSelectedDate = dtMinimumDate;
            txtDate.Text = dtSelectedDate.ToString("D");
          }
        }
      }
      public DateTime MaxDate
      {
        get
        {
          return dtMaximumDate;
        }
        set
        {
          dtMaximumDate = value;
          if (dtSelectedDate > dtMaximumDate)
          {
            dtSelectedDate = dtMaximumDate;
            txtDate.Text = dtSelectedDate.ToString("D");
          }
        }
      }
      public DateTimeWidget(DateTime date)
      {
        dtSelectedDate = date;
        dtMinimumDate = new DateTime(1970, 1, 1);
        dtMaximumDate = DateTime.Today;
        pnlDT = new HBox();
        pnlDropdown = new HBox(false, 0);
        evnDTEvents = new EventBox();
        cmdToggle = new ToggleButton();
        cmdToggle.Pressed += cmdToggle_Pressed;
        cmdToggle.Toggled += cmdToggle_Released;
        Arrow arrow = new Arrow(ArrowType.Down, ShadowType.None);
        txtDate = new Entry(dtSelectedDate.ToString("D"));
        txtDate.IsEditable = false;
        txtDate.Alignment = 1.0f;
        pnlDropdown.PackStart(txtDate, true, true, 0);
        pnlDropdown.PackStart(cmdToggle, false, false, 0);
        cmdToggle.Add(arrow);
        evnDTEvents.Add(pnlDropdown);
        pnlDT.PackStart(evnDTEvents, true, true, 0);
        this.PackStart(pnlDT, false, false, 0);
        frmDT = new Gtk.Window(Gtk.WindowType.Toplevel);
        frmDT.Modal = false;
        frmDT.SkipPagerHint = true;
        frmDT.SkipTaskbarHint = true;
        frmDT.BorderWidth = 2;
        frmDT.Resizable = false;
        frmDT.Decorated = false;
        frmDT.TypeHint = Gdk.WindowTypeHint.PopupMenu;
        calDate = new Calendar();
        frmDT.Add(calDate);
        this.Shown += this_shown;
      }
      private void this_shown(object o, EventArgs e)
      {
        this.Shown -= this_shown;
        popup();
        System.Threading.Thread.Sleep(0);
        popdown();
      }
      private void on_shown(object o, EventArgs e)
      {
        calDate.KeyPressEvent += on_keypress;
        calDate.ButtonPressEvent += on_click;
        frmDT.FocusOutEvent += focus_out;
        evnDTEvents.FocusOutEvent += focus_out; 
      }
      private int lastClick;
      private void on_hidden(object o, EventArgs e)
      {
        frmDT.FocusOutEvent -= focus_out;
        evnDTEvents.FocusOutEvent -= focus_out;    
        calDate.KeyPressEvent -= on_keypress;
        calDate.ButtonPressEvent -= on_click;
        cmdToggle.Active = false;
        lastClick = Environment.TickCount;
      }
      private bool skipToggle;
      private void cmdToggle_Pressed(object o, EventArgs e)
      {
        skipToggle = (Environment.TickCount - lastClick < 50);
      }
      private void cmdToggle_Released(object o, EventArgs e)
      {
        if (skipToggle)
        {
          cmdToggle.Active = false;
          return;
        }
        if (cmdToggle.Active)
        {    
          popup();
        }
        else
        {
          popdown();
        }
      }
      private void on_keypress(object o, Gtk.KeyPressEventArgs args)
      {
        if (args.Event.Key == Gdk.Key.Return || args.Event.Key == Gdk.Key.Escape)
        {
          popdown();
        }
      }
      private void on_click(object o, Gtk.ButtonPressEventArgs args)
      {
        if (args.Event.Type == Gdk.EventType.TwoButtonPress)
        {
          popdown();
        }
      }
      private void focus_out(object o, Gtk.FocusOutEventArgs args)
      {
        popdown();
      }
      private void popup()
      {
        calDate.Date = dtSelectedDate;
        int x = 0;
        int y = 0;
        int width = 0;
        int height = 0;
        int depth = 0;
        if (evnDTEvents != null)
        {
          if (evnDTEvents.GdkWindow != null)
          {
            evnDTEvents.GdkWindow.GetGeometry(out x, out y, out width, out height, out depth);
            evnDTEvents.GdkWindow.GetOrigin(out x, out y);
          }
        }
        frmDT.Move(x + (width - frmDT.Allocation.Width), y + height);
        frmDT.ShowAll();
        frmDT.GrabFocus();
        evnDTEvents.GrabFocus();
        frmDT.Shown += on_shown;
        frmDT.Hidden += on_hidden;
        Gdk.Pointer.Grab(frmDT.GdkWindow, true, Gdk.EventMask.FocusChangeMask, null, null, Gtk.Global.CurrentEventTime);
      }
      private void popdown()
      {
        Gdk.Pointer.Ungrab(Gtk.Global.CurrentEventTime);
        frmDT.Hide();
        if (dtSelectedDate == calDate.Date)
        {
          return;
        }
        dtSelectedDate = calDate.Date;
        if (dtSelectedDate < dtMinimumDate)
        {
          dtSelectedDate = dtMinimumDate;
        }
        if (dtSelectedDate > dtMaximumDate)
        {
          dtSelectedDate = dtMaximumDate;
        }
        txtDate.Text = dtSelectedDate.ToString("D");
        if (DateChanged != null)
        {
          DateChanged(null, new DateChangedEventArgs(dtSelectedDate));
        }
      }
    }
    #endregion
    #region "List Views"
    class DataListView : ListView<DataBase.DataRow>
    {
      public DataListView(params string[] columnNames) : base(columnNames)
      {
      }
      protected override void RenderCell(CellRendererText render, int index, DataBase.DataRow item)
      {
        switch (index)
        {
          case 0:
            render.Text = item.sDATETIME;
            break;
          default:
            render.Text = "";
            break;
        }
      }
    }
    class WildBlueView : DataListView
    {
      public WildBlueView() : base("Date and Time", "Download", "Upload")
      {
      }
      protected override void RenderCell(CellRendererText render, int index, DataBase.DataRow item)
      {
        switch (index)
        {
          case 0:
            render.Text = item.sDATETIME;
            break;
          case 1:
            render.Text = item.sDOWNLOAD;
            break;
          case 2:
            render.Text = item.sUPLOAD;
            break;
        }
      }
    }
    class WildBlueLimsView : DataListView
    {
      public WildBlueLimsView() : base("Date and Time", "Download / Limit", "Upload / Limit")
      {
      }
      protected override void RenderCell(CellRendererText render, int index, DataBase.DataRow item)
      {
        switch (index)
        {
          case 0:
            render.Text = item.sDATETIME;
            break;
          case 1:
            render.Text = item.sDOWNLOAD + " / " + item.sDOWNLIM;
            break;
          case 2:
            render.Text = item.sUPLOAD + " / " + item.sUPLIM;
            break;
        }
      }
    }
    class RuralPortalView : DataListView
    {
      public RuralPortalView() : base("Date and Time", "Used", "Total")
      {
      }
      protected override void RenderCell(CellRendererText render, int index, DataBase.DataRow item)
      {
        switch (index)
        {
          case 0:
            render.Text = item.sDATETIME;
            break;
          case 1:
            render.Text = item.sDOWNLOAD;
            break;
          case 2:
            render.Text = item.sDOWNLIM;
            break;
        }
      }
    }
    class DishNetView : DataListView
    {
      public DishNetView() : base("Date and Time", "Anytime", "Off-Peak")
      {
      }
      protected override void RenderCell(CellRendererText render, int index, DataBase.DataRow item)
      {
        switch (index)
        {
          case 0:
            render.Text = item.sDATETIME;
            break;
          case 1:
            render.Text = item.sDOWNLOAD;
            break;
          case 2:
            render.Text = item.sUPLOAD;
            break;
        }
      }
    }
    class DishNetLimsView : DataListView
    {
      public DishNetLimsView() : base("Date and Time", "Anytime / Limit", "Off-Peak / Limit")
      {
      }
      protected override void RenderCell(CellRendererText render, int index, DataBase.DataRow item)
      {
        switch (index)
        {
          case 0:
            render.Text = item.sDATETIME;
            break;
          case 1:
            render.Text = item.sDOWNLOAD + " / " + item.sDOWNLIM;
            break;
          case 2:
            render.Text = item.sUPLOAD + " / " + item.sUPLIM;
            break;
        }
      }
    }
    #endregion
  }
}

