using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
namespace RestrictionTrackerGTK
{
  /// <summary>
  /// TaskbarNotifier allows to display MSN style/Skinned instant messaging popups
  /// </summary>
  public class TaskbarNotifier : Gtk.Window
  {
#region TaskbarNotifier Protected Members
    protected Bitmap BackgroundBitmap = null;
    protected Bitmap ScreenTransBitmap = null;
    protected Bitmap CloseBitmap = null;
    protected Point CloseBitmapLocation;
    protected Size CloseBitmapSize;
    protected Rectangle RealTitleRectangle;
    protected Rectangle RealContentRectangle;
    protected Rectangle WorkAreaRectangle;
    protected uint timer = 0;
    protected TaskbarStates taskbarState = TaskbarStates.hidden;
    protected string titleText;
    protected string contentText;
    protected Color normalTitleColor = Color.FromArgb(0,0,0);
    protected Color hoverTitleColor = Color.FromArgb(0,0,0xFF);
    protected Color normalContentColor = Color.FromArgb(0,0,0);
    protected Color hoverContentColor = Color.FromArgb(0,0,0xFF);
    protected Color transparencyKey = Color.FromArgb(0xFF,0,0xFF);
    protected Font normalTitleFont = new Font("Arial",12,FontStyle.Bold,GraphicsUnit.Pixel);
    protected Font hoverTitleFont = new Font("Arial",12,FontStyle.Bold,GraphicsUnit.Pixel);
    protected Font normalContentFont = new Font("Arial",11,FontStyle.Regular,GraphicsUnit.Pixel);
    protected Font hoverContentFont = new Font("Arial",11,FontStyle.Regular,GraphicsUnit.Pixel);
    protected uint nShowEvents;
    protected uint nHideEvents;
    protected uint nVisibleEvents;
    protected uint nIncrementShow;
    protected uint nIncrementHide;
    protected bool bIsMouseOverPopup = false;
    protected bool bIsMouseOverClose = false;
    protected bool bIsMouseOverContent = false;
    protected bool bIsMouseOverTitle = false;
    protected bool bIsMouseDown = false;
    protected bool bKeepVisibleOnMouseOver = true;    
    protected bool bReShowOnMouseOver = true;        
#endregion            
#region TaskbarNotifier Public Members
    public Rectangle TitleRectangle;
    public Rectangle ContentRectangle;
    public bool TitleClickable = false;
    public bool ContentClickable = true;
    public bool CloseClickable = true;
    public event EventHandler CloseClick = null;
    public event EventHandler TitleClick = null;
    public event EventHandler ContentClick = null;
#endregion
#region TaskbarNotifier enums
    /// <summary>
    /// List of the different popup animation status
    /// </summary>
    public enum TaskbarStates
    {
      hidden = 0,
      appearing = 1,
      visible = 2,
      disappearing = 3
    }
#endregion
 
#region TaskbarNotifier Constructor
    /// <summary>
    /// The Constructor for TaskbarNotifier
    /// </summary>
    public TaskbarNotifier():base(Gtk.WindowType.Popup)
    {
      AppPaintable = true;
      SkipPagerHint = true;
      SkipTaskbarHint = true;
      Decorated = false;
      Resizable = false;
      FocusOnMap = false;
      TypeHint = Gdk.WindowTypeHint.Desktop;

      this.AddEvents((int)Gdk.EventMask.ButtonPressMask);
      this.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
      this.AddEvents((int)Gdk.EventType.ButtonPress);
      this.AddEvents((int)Gdk.EventType.ButtonRelease);

      this.AddEvents((int)Gdk.EventType.MotionNotify);

      KeepAbove = true;
      timer = GLib.Timeout.Add(100, OnTimer);
    }
#endregion
 
#region TaskbarNotifier Properties
    /// <summary>
    /// Get the current TaskbarState (hidden, showing, visible, hiding)
    /// </summary>
    public TaskbarStates TaskbarState
    {
      get
      {
        return taskbarState;
      }
    }
 
    /// <summary>
    /// Get/Set the popup Title Text
    /// </summary>
    public string TitleText
    {
      get
      {
        return titleText;
      }
      set
      {
        titleText=value;
        Refresh(true);
      }
    }
 
    /// <summary>
    /// Get/Set the popup Content Text
    /// </summary>
    public string ContentText
    {
      get
      {
        return contentText;
      }
      set
      {
        contentText=value;
        Refresh(true);
      }
    }
 
    /// <summary>
    /// Get/Set the Normal Title Color
    /// </summary>
    public Color NormalTitleColor
    {
      get
      {
        return normalTitleColor;
      }
      set
      {
        normalTitleColor = value;
        Refresh(true);
      }
    }
 
    /// <summary>
    /// Get/Set the Hover Title Color
    /// </summary>
    public Color HoverTitleColor
    {
      get
      {
        return hoverTitleColor;
      }
      set
      {
        hoverTitleColor = value;
        Refresh(true);
      }
    }
 
    /// <summary>
    /// Get/Set the Normal Content Color
    /// </summary>
    public Color NormalContentColor
    {
      get
      {
        return normalContentColor;
      }
      set
      {
        normalContentColor = value;
        Refresh(true);
      }
    }
 
    /// <summary>
    /// Get/Set the Hover Content Color
    /// </summary>
    public Color HoverContentColor
    {
      get
      {
        return hoverContentColor;
      }
      set
      {
        hoverContentColor = value;
        Refresh(true);
      }
    }
 
    /// <summary>
    /// Get/Set the Normal Title Font
    /// </summary>
    public Font NormalTitleFont
    {
      get
      {
        return normalTitleFont;
      }
      set
      {
        normalTitleFont = value;
        Refresh(true);
      }
    }
 
    /// <summary>
    /// Get/Set the Hover Title Font
    /// </summary>
    public Font HoverTitleFont
    {
      get
      {
        return hoverTitleFont;
      }
      set
      {
        hoverTitleFont = value;
        Refresh(true);
      }
    }
 
    /// <summary>
    /// Get/Set the Normal Content Font
    /// </summary>
    public Font NormalContentFont
    {
      get
      {
        return normalContentFont;
      }
      set
      {
        normalContentFont = value;
        Refresh(true);
      }
    }
 
    /// <summary>
    /// Get/Set the Hover Content Font
    /// </summary>
    public Font HoverContentFont
    {
      get
      {
        return hoverContentFont;
      }
      set
      {
        hoverContentFont = value;
        Refresh(true);
      }
    }
 
    /// <summary>
    /// Indicates if the popup should remain visible when the mouse pointer is over it.
    /// Added Rev 002
    /// </summary>
    public bool KeepVisibleOnMousOver
    {
      get
      {
        return bKeepVisibleOnMouseOver;
      }
      set
      {
        bKeepVisibleOnMouseOver=value;
      }
    }
 
    /// <summary>
    /// Indicates if the popup should appear again when mouse moves over it while it's disappearing.
    /// Added Rev 002
    /// </summary>
    public bool ReShowOnMouseOver
    {
      get
      {
        return bReShowOnMouseOver;
      }
      set
      {
        bReShowOnMouseOver=value;
      }
    }
#endregion
 
#region TaskbarNotifier Public Methods
    //[DllImport("user32.dll")]
    //private static extern Boolean ShowWindow(IntPtr hWnd,Int32 nCmdShow);

    /// <summary>
    /// Displays the popup for a certain amount of time.
    /// </summary>
    /// <param name="strTitle">The string which will be shown as the title of the popup.</param>
    /// <param name="strContent">The string which will be shown as the content of the popup.</param>
    /// <param name="nTimeToShow">Duration of the showing animation (in milliseconds).</param>
    /// <param name="nTimeToStay">Duration of the visible state before collapsing (in milliseconds). Use 0 to stay visible.</param>
    /// <param name="nTimeToHide">Duration of the hiding animation (in milliseconds).</param>
    /// <returns>Nothing</returns>
    public void Show(string strTitle, string strContent, uint nTimeToShow, uint nTimeToStay, uint nTimeToHide)
    {
      ScreenTransBitmap = null;
      Gtk.Window wndMax = new Gtk.Window(Gtk.WindowType.Toplevel);
      wndMax.SizeAllocated += HandleSizeAllocated;
      wndMax.WindowStateEvent += HandleWindowStateEvent;
      wndMax.Opacity = 0;
      wndMax.SkipPagerHint=true;
      wndMax.SkipTaskbarHint=true;
      wndMax.FocusOnMap = false;
      wndMax.Decorated = CurrentOS.IsMac;
      wndMax.Show();
      if (!CurrentOS.IsMac)
        wndMax.Maximize();
      titleText = strTitle;
      contentText = strContent;
      nVisibleEvents = nTimeToStay;
      CalculateMouseRectangles();
 
      uint nEvents;
      if (nTimeToShow > 10)
      {
        nEvents = Math.Min((nTimeToShow / 10), 100);
        nShowEvents = nTimeToShow / nEvents;
        nIncrementShow = 100 / nEvents;
      }
      else
      {
        nShowEvents = 10;
        nIncrementShow = 100;
      }
 
      if( nTimeToHide > 10)
      {
        nEvents = Math.Min((nTimeToHide / 10), 100);
        nHideEvents = nTimeToHide / nEvents;
        nIncrementHide = 100 / nEvents;
      }
      else
      {
        nHideEvents = 10;
        nIncrementHide = 100;
      }
    }

    void HandleWindowStateEvent(object o, Gtk.WindowStateEventArgs args)
    {
      if (args.Event.ChangedMask == Gdk.WindowState.Maximized)
      {
        Gtk.Window wndMax = (Gtk.Window)o;

        Gdk.Rectangle displayRect = wndMax.GdkWindow.FrameExtents;
        wndMax.Hide();

        if (displayRect.Width == 200)
        {
          int w, h;
          wndMax.GetSize(out w, out h);
          System.Threading.Thread.Sleep(0);
          Gtk.Application.RunIteration();
          displayRect = wndMax.GdkWindow.FrameExtents;
        }

        Gdk.Rectangle windowRect = Screen.GetMonitorGeometry(Screen.GetMonitorAtPoint(wndMax.Allocation.X, wndMax.Allocation.Y));

        if (displayRect.Width == 200)
        {
          Console.WriteLine("Still Bad");
          displayRect.Location = Gdk.Point.Zero;
          displayRect.Width = windowRect.Width;
          displayRect.Height = windowRect.Height - 32;
        }

        int xLoc = windowRect.Right - BackgroundBitmap.Width - 1;
        int yLoc = windowRect.Bottom - BackgroundBitmap.Height - 1;
        if (displayRect.Right < windowRect.Right)
        {
          xLoc = displayRect.Right - BackgroundBitmap.Width - 1;
        }
        else if (displayRect.Left > windowRect.Left)
        {
          xLoc = displayRect.Left + 1;
        }
        if (displayRect.Top > windowRect.Top)
        {
          yLoc = displayRect.Top + 1;
        }
        else if (displayRect.Bottom < windowRect.Bottom)
        {
          yLoc = displayRect.Bottom - BackgroundBitmap.Height - 1;
        }

        switch (taskbarState)
        {
          case TaskbarStates.hidden:
            GLib.Source.Remove(timer);
            timer = 0;
            taskbarState = TaskbarStates.appearing;
            Opacity = 0;
            this.WidthRequest = BackgroundBitmap.Width;
            this.HeightRequest = BackgroundBitmap.Height;
            this.Move(xLoc, yLoc);
            timer = GLib.Timeout.Add(nShowEvents, OnTimer);
            ShowAll();
            Refresh(true);
            break;
          case TaskbarStates.appearing:
            Refresh(true);
            break;
          case TaskbarStates.visible:
            GLib.Source.Remove(timer);
            timer = 0;
            if (nVisibleEvents > 0)
            {
              timer = GLib.Timeout.Add(nVisibleEvents, OnTimer);
            }
            Refresh(true);
            break;
          case TaskbarStates.disappearing:
            GLib.Source.Remove(timer);
            timer = 0;
            taskbarState = TaskbarStates.visible;
            Opacity = 1;
            this.WidthRequest = BackgroundBitmap.Width;
            this.HeightRequest = BackgroundBitmap.Height;
            this.Move(xLoc, yLoc);
            if (nVisibleEvents > 0)
            {
              timer = GLib.Timeout.Add(nVisibleEvents, OnTimer);
            }
            Refresh(true);
            break;
        }
      }
    }

    void HandleSizeAllocated(object o, Gtk.SizeAllocatedArgs args)
    {
      Gtk.Window wndMax = (Gtk.Window)o;
      if (args.Allocation.Width == 200)
      {
        if (CurrentOS.IsMac)
          wndMax.Maximize();
      }
      else
      {
        Console.WriteLine(args.Allocation.Width);
      }
    }

    /// <summary>
    /// Hides the popup
    /// </summary>
    /// <returns>Nothing</returns>
    public new void Hide()
    {
      if (taskbarState != TaskbarStates.hidden)
      {
        GLib.Source.Remove(timer);
        timer = 0;
        taskbarState = TaskbarStates.hidden;
        base.Hide();
      }
    }

    public void SlowHide()
    {
      GLib.Source.Remove(timer);
      timer = 0;
      taskbarState = TaskbarStates.disappearing;
      timer = GLib.Timeout.Add(nHideEvents, OnTimer);
    }
 
    /// <summary>
    /// Sets the background bitmap and its transparency color
    /// </summary>
    /// <param name="strFilename">Path of the Background Bitmap on the disk</param>
    /// <param name="transparencyColor">Color of the Bitmap which won't be visible</param>
    /// <returns>Nothing</returns>
    public void SetBackgroundBitmap(string strFilename, Color transparencyColor)
    {
      BackgroundBitmap = new Bitmap(strFilename);
      this.WidthRequest = BackgroundBitmap.Width;
      this.HeightRequest = BackgroundBitmap.Height;
      transparencyKey = transparencyColor;
      if (BackgroundBitmap == null)
        SetDefaultBitmaps();
      //Region = BitmapToRegion(BackgroundBitmap, transparencyColor);
      Refresh(true);
    }
 
    /// <summary>
    /// Sets the background bitmap and its transparency color
    /// </summary>
    /// <param name="image">Image/Bitmap object which represents the Background Bitmap</param>
    /// <param name="transparencyColor">Color of the Bitmap which won't be visible</param>
    /// <returns>Nothing</returns>
    public void SetBackgroundBitmap(Image image, Color transparencyColor)
    {
      BackgroundBitmap = new Bitmap(image);
      this.WidthRequest = BackgroundBitmap.Width;
      this.HeightRequest = BackgroundBitmap.Height;
      transparencyKey = transparencyColor;
      if (BackgroundBitmap == null)
        SetDefaultBitmaps();
      //Region = BitmapToRegion(BackgroundBitmap, transparencyColor);
      Refresh(true);
    }
 
    /// <summary>
    /// Sets the 3-State Close Button bitmap, its transparency color and its coordinates
    /// </summary>
    /// <param name="strFilename">Path of the 3-state Close button Bitmap on the disk (width must a multiple of 3)</param>
    /// <param name="transparencyColor">Color of the Bitmap which won't be visible</param>
    /// <param name="position">Location of the close button on the popup</param>
    /// <returns>Nothing</returns>
    public void SetCloseBitmap(string strFilename, Color transparencyColor, Point position)
    {
      CloseBitmap = modFunctions.MakeTransparent(new Bitmap(strFilename), transparencyColor);
      //CloseBitmap.MakeTransparent(transparencyColor);
      //transparencyKey = transparencyColor;
      CloseBitmapSize = new Size(CloseBitmap.Width / 3, CloseBitmap.Height);
      CloseBitmapLocation = position;
      if (CloseBitmap == null || CloseBitmapSize.IsEmpty)
        SetDefaultBitmaps();
      Refresh(true);
    }
 
    /// <summary>
    /// Sets the 3-State Close Button bitmap, its transparency color and its coordinates
    /// </summary>
    /// <param name="image">Image/Bitmap object which represents the 3-state Close button Bitmap (width must be a multiple of 3)</param>
    /// <param name="transparencyColor">Color of the Bitmap which won't be visible</param>
    /// /// <param name="position">Location of the close button on the popup</param>
    /// <returns>Nothing</returns>
    public void SetCloseBitmap(Image image, Color transparencyColor, Point position)
    {
      CloseBitmap = modFunctions.MakeTransparent(new Bitmap(image), transparencyColor);
      CloseBitmapSize = new Size(CloseBitmap.Width / 3, CloseBitmap.Height);
      CloseBitmapLocation = position;
      if (CloseBitmap == null || CloseBitmapSize.IsEmpty)
        SetDefaultBitmaps();
      Refresh(true);
    }

    private void SetDefaultBitmaps()
    {
      SetDefaultBackgroundBitmap();
      SetDefaultCloseBitmap();
    }

    private void SetDefaultBackgroundBitmap()
    {
      BackgroundBitmap = new Bitmap(GetType(), "RestrictionTrackerGTK.Resources.default_alert.png");
      this.WidthRequest = BackgroundBitmap.Width;
      this.HeightRequest = BackgroundBitmap.Height;
      transparencyKey = Color.Fuchsia;
      TitleRectangle = new Rectangle(7, 3, 188, 24);
      ContentRectangle = new Rectangle(9, 31, 227, 66);
    }

    private void SetDefaultCloseBitmap()
    {
      Bitmap srcBitmap = new Bitmap(GetType(), "RestrictionTrackerGTK.Resources.default_close.png");
      CloseBitmap = modFunctions.MakeTransparent(srcBitmap, Color.Fuchsia);
      CloseBitmapSize = new Size(srcBitmap.Width / 3, srcBitmap.Height);
      CloseBitmapLocation = new Point(190, 0);
    }


#endregion
 
#region TaskbarNotifier Protected Methods
    protected void DrawCloseButton(ref Graphics grfx)
    {
      if (CloseBitmap == null || CloseBitmapSize.IsEmpty)
        SetDefaultBitmaps();
      if (CloseBitmap != null)
      {  
        Rectangle rectDest = new Rectangle(CloseBitmapLocation, CloseBitmapSize);
        Rectangle rectSrc;
 
        if (bIsMouseOverClose)
        {
          if (bIsMouseDown)
          {
            rectSrc = new Rectangle(new Point(CloseBitmapSize.Width * 2, 0), CloseBitmapSize);
          }
          else
          {
            rectSrc = new Rectangle(new Point(CloseBitmapSize.Width, 0), CloseBitmapSize);
          }
        }
        else
        {
          rectSrc = new Rectangle(new Point(0, 0), CloseBitmapSize);
        }
        grfx.DrawImage(CloseBitmap, rectDest, rectSrc, GraphicsUnit.Pixel);
      }
    }
 
    protected void DrawText(ref Graphics grfx)
    {
      if (titleText != null && titleText.Length != 0)
      {
        StringFormat sf = new StringFormat();
        sf.Alignment = StringAlignment.Near;
        sf.LineAlignment = StringAlignment.Center;
        sf.FormatFlags = StringFormatFlags.NoWrap;
        sf.Trimming = StringTrimming.EllipsisCharacter;     
        if (bIsMouseOverTitle)
        {
          grfx.DrawString(titleText, hoverTitleFont, new SolidBrush(hoverTitleColor), TitleRectangle, sf);
        }
        else
        {
          grfx.DrawString(titleText, normalTitleFont, new SolidBrush(normalTitleColor), TitleRectangle, sf);
        }
      }
 
      if (contentText != null && contentText.Length != 0)
      {
        StringFormat sf = new StringFormat();
        sf.Alignment = StringAlignment.Center;
        sf.LineAlignment = StringAlignment.Center;
        sf.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
        sf.Trimming = StringTrimming.Word;            
        if (bIsMouseOverContent)
        {
          grfx.DrawString(contentText, hoverContentFont, new SolidBrush(hoverContentColor), ContentRectangle, sf);
        }
        else
        {
          grfx.DrawString(contentText, normalContentFont, new SolidBrush(normalContentColor), ContentRectangle, sf);
        }
      }
    }
 
    protected void CalculateMouseRectangles()
    {
      Graphics grfx = Graphics.FromImage (new Bitmap(1,1));;
      StringFormat sf = new StringFormat();
      sf.Alignment = StringAlignment.Center;
      sf.LineAlignment = StringAlignment.Center;
      sf.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
      SizeF sizefTitle = grfx.MeasureString(titleText, hoverTitleFont, TitleRectangle.Width, sf);
      SizeF sizefContent = grfx.MeasureString(contentText, hoverContentFont, ContentRectangle.Width, sf);
      grfx.Dispose();

      if (sizefTitle.Height > TitleRectangle.Height)
      {
        RealTitleRectangle = new Rectangle(TitleRectangle.Left, TitleRectangle.Top, TitleRectangle.Width , TitleRectangle.Height );
      } 
      else
      {
        RealTitleRectangle = new Rectangle(TitleRectangle.Left, TitleRectangle.Top, (int)sizefTitle.Width, (int)sizefTitle.Height);
      }
      RealTitleRectangle.Inflate(0,2);
 
      if (sizefContent.Height > ContentRectangle.Height)
      {
        RealContentRectangle = new Rectangle((ContentRectangle.Width-(int)sizefContent.Width)/2+ContentRectangle.Left, ContentRectangle.Top, (int)sizefContent.Width, ContentRectangle.Height );
      }
      else
      {
        RealContentRectangle = new Rectangle((ContentRectangle.Width-(int)sizefContent.Width)/2+ContentRectangle.Left, (ContentRectangle.Height-(int)sizefContent.Height)/2+ContentRectangle.Top, (int)sizefContent.Width, (int)sizefContent.Height);
      }
      RealContentRectangle.Inflate(0,2);
    }
#endregion
 
#region TaskbarNotifier Events Overrides
    protected bool OnTimer()
    {
      switch (taskbarState)
      {
        case TaskbarStates.appearing:
          if (Opacity < 1)
          {
            Opacity += (double)nIncrementShow / 100;
          }
          else
          {
            GLib.Source.Remove(timer);
            timer = 0;
            Opacity = 1;
            if (nVisibleEvents > 0)
            {
              taskbarState = TaskbarStates.visible;
              timer = GLib.Timeout.Add(nVisibleEvents, OnTimer);
            }
            return false;
          }
          break;
        case TaskbarStates.visible:
          GLib.Source.Remove(timer);
          timer = 0;
          if ((bKeepVisibleOnMouseOver && !bIsMouseOverPopup) || (!bKeepVisibleOnMouseOver))
          {
            taskbarState = TaskbarStates.disappearing;
          } 
          timer = GLib.Timeout.Add(nHideEvents, OnTimer);
          return false;
        case TaskbarStates.disappearing:
          if (bReShowOnMouseOver && bIsMouseOverPopup)
          {
            taskbarState = TaskbarStates.appearing;
          }
          else
          {
            if (Opacity > 0)
            {
              Opacity -= (double)nIncrementHide / 100;
            }
            else
            {
              Hide();
            }
          }
          break;
      }
      return true; 
    }

    [GLib.ConnectBefore]
    protected override bool OnEnterNotifyEvent(Gdk.EventCrossing evnt)
    {
      bool ret = base.OnEnterNotifyEvent(evnt);
      bIsMouseOverPopup = true;
      Refresh(true);
      return ret;
    }

    [GLib.ConnectBefore]
    protected override bool OnLeaveNotifyEvent(Gdk.EventCrossing evnt)
    {
      bool ret = base.OnLeaveNotifyEvent(evnt);
      bIsMouseOverPopup = false;
      bIsMouseOverClose = false;
      bIsMouseOverTitle = false;
      bIsMouseOverContent = false;
      Refresh(true);
      return ret;
    }

    [GLib.ConnectBefore]
    protected override bool OnMotionNotifyEvent(Gdk.EventMotion mea)
    {
      bool ret = base.OnMotionNotifyEvent(mea);
 
      bool bContentModified = false;
      if ((mea.X > CloseBitmapLocation.X) && (mea.X < CloseBitmapLocation.X + CloseBitmapSize.Width) && (mea.Y > CloseBitmapLocation.Y) && (mea.Y < CloseBitmapLocation.Y + CloseBitmapSize.Height) && CloseClickable)
      {
        if (!bIsMouseOverClose)
        {
          bIsMouseOverClose = true;
          bIsMouseOverTitle = false;
          bIsMouseOverContent = false;
          this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Hand1);
          bContentModified = true;
        }
      }
      else if (RealContentRectangle.Contains(new Point((int)mea.X, (int)mea.Y)) && ContentClickable)
      {
        if (!bIsMouseOverContent)
        {
          bIsMouseOverClose = false;
          bIsMouseOverTitle = false;
          bIsMouseOverContent = true;
          this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Hand1);
          bContentModified = true;
        }
      }
      else if (RealTitleRectangle.Contains(new Point((int)mea.X, (int)mea.Y)) && TitleClickable)
      {
        if (!bIsMouseOverTitle)
        {
          bIsMouseOverClose = false;
          bIsMouseOverTitle = true;
          bIsMouseOverContent = false;
          this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Hand1);
          bContentModified = true;
        }
      }
      else
      {
        if (bIsMouseOverClose || bIsMouseOverTitle || bIsMouseOverContent)
        {
          bContentModified = true;
        }
 
        bIsMouseOverClose = false;
        bIsMouseOverTitle = false;
        bIsMouseOverContent = false;
        this.GdkWindow.Cursor = null;
      }
 
      if (bContentModified)
      {
        Refresh(true);
      }
      return ret;
    }

    [GLib.ConnectBefore]
    protected override bool OnButtonPressEvent(Gdk.EventButton evnt)
    {
      bool ret = base.OnButtonPressEvent(evnt);
      if (evnt.Button == 1)
      {
        bIsMouseDown = true;
      
        if (bIsMouseOverClose)
        {
          Refresh(true);
        }
      }
      return ret;
    }

    [GLib.ConnectBefore]
    protected override bool OnButtonReleaseEvent(Gdk.EventButton evnt)
    {
      bool ret = base.OnButtonReleaseEvent(evnt);
      if (evnt.Button == 1)
      {
        bIsMouseDown = false;
   
        if (bIsMouseOverClose)
        {
          bReShowOnMouseOver = false;
          SlowHide();
                
          if (CloseClick != null)
          {
            CloseClick(this, new EventArgs());
          }
        }
        else if (bIsMouseOverTitle)
        {
          if (TitleClick != null)
          {
            TitleClick(this, new EventArgs());
          }
        }
        else if (bIsMouseOverContent)
        {
          if (ContentClick != null)
          {
            ContentClick(this, new EventArgs());
          }
        }
      }
      return ret;
    }

    Gdk.Pixbuf background = null;
    [GLib.ConnectBefore]
    protected override bool OnExposeEvent(Gdk.EventExpose args)
    {
      if (background != null)
      {
        GdkWindow.DrawPixbuf(Style.BackgroundGC(State), background, 0, 0, 0, 0, background.Width, background.Height, Gdk.RgbDither.None, 0, 0);
      }
      return base.OnExposeEvent(args);
    }

    private void Refresh(bool Draw)
    {
      Graphics offScreenGraphics;
      Bitmap offscreenBitmap;
      
      offscreenBitmap = new Bitmap(BackgroundBitmap.Width, BackgroundBitmap.Height);
      offScreenGraphics = Graphics.FromImage(offscreenBitmap);
      
      if (BackgroundBitmap != null)
      {
        offScreenGraphics.DrawImage(BackgroundBitmap, 0, 0, BackgroundBitmap.Width, BackgroundBitmap.Height);
      }
      
      DrawCloseButton(ref offScreenGraphics);
      DrawText(ref offScreenGraphics);
 

      if (Gdk.Display.Default.SupportsShapes)
      {
        background = modFunctions.ImageToPixbuf(offscreenBitmap).AddAlpha(true, transparencyKey.R, transparencyKey.G, transparencyKey.B);
        Gdk.Pixmap pixmap = null;
        Gdk.Pixmap mask = null;
        this.WidthRequest = background.Width;
        this.HeightRequest = background.Height;
        background.RenderPixmapAndMask(out pixmap, out mask, 2);
        this.ShapeCombineMask(mask, 0, 0);
      }
      else
      {
        if (this.GdkWindow == null)
        {
          background = modFunctions.ImageToPixbuf(modFunctions.SwapColors(offscreenBitmap, transparencyKey, Color.Black));
        }
        else if (this.GdkWindow.FrameExtents.Location.X <= 0 || this.GdkWindow.FrameExtents.Location.Y <= 0)
        {
          background = modFunctions.ImageToPixbuf(modFunctions.SwapColors(offscreenBitmap, transparencyKey, Color.Black));
        }
        else
        {
          if (ScreenTransBitmap == null)
          {
            ScreenTransBitmap = modFunctions.GetScreenRect(new Rectangle(this.GdkWindow.FrameExtents.Location.X,this.GdkWindow.FrameExtents.Location.Y, offscreenBitmap.Size.Width,offscreenBitmap.Size.Height));
          }
          background = modFunctions.ImageToPixbuf(modFunctions.ReplaceColors(offscreenBitmap, transparencyKey, ScreenTransBitmap));
        }
        this.WidthRequest = background.Width;
        this.HeightRequest = background.Height;
      }
      if (Draw && titleText != null)
      {
        GdkWindow.DrawPixbuf(Style.BackgroundGC(State), background, 0, 0, 0, 0, background.Width, background.Height, Gdk.RgbDither.None, 0, 0);
      }
    }

    [GLib.ConnectBefore]
    protected override void OnSizeAllocated(Gdk.Rectangle sized)
    {
      base.OnSizeAllocated(sized); 
      if (background == null)
      {
        Refresh(false);
      }
    }
#endregion
  }
}