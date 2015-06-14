using System;
using System.Drawing;
using System.IO;
using Gtk;
namespace RestrictionTrackerGTK
{
  public partial class dlgAlertSelection : Gtk.Dialog
  {
    private ScrolledWindow sclStyles;
    private ListBox lstStyles;
    public string AlertStyle;
    private bool Changed;
    public dlgAlertSelection(string style)
    {
      AlertStyle = style;
      this.Build();
      ((Gtk.Box.BoxChild) this.ActionArea[lblMore]).Position = 0;
      ((Gtk.Box.BoxChild) this.ActionArea[vsButtons]).Position = 1;
      if (CurrentOS.IsMac)
      {
        ((Gtk.Box.BoxChild) this.ActionArea[cmdClose]).Position = 2;
        ((Gtk.Box.BoxChild) this.ActionArea[cmdSave]).Position = 3;
      }
      else
      {
        ((Gtk.Box.BoxChild) this.ActionArea[cmdSave]).Position = 2;
        ((Gtk.Box.BoxChild) this.ActionArea[cmdClose]).Position = 3;
      }
      this.WindowStateEvent += HandleWindowStateEvent;
      cmdSave.Clicked += cmdSave_Click;
      cmdClose.Clicked += cmdClose_Click;
      lstStyles = new ListBox("Style");
      sclStyles = new ScrolledWindow();
      sclStyles.Add(lstStyles);
      pnlListBox.Add(sclStyles);
      lstStyles.SetSizeRequest(150, 0);
      ((Gtk.Box.BoxChild) pnlListBox[sclStyles]).Position = 1;
      ((Gtk.Box.BoxChild) pnlListBox[sclStyles]).Expand = true;
      ((Gtk.Box.BoxChild) pnlListBox[sclStyles]).Fill = true;
      ((Gtk.Box.BoxChild) pnlListBox[pnlListButtons]).Position = 2;
      sclStyles.Visible = true;
      lstStyles.Visible = true;
      lstStyles.Columns[0].Resizable = false;
      lstStyles.Columns[0].Expand = true;
      lstStyles.Selection.Mode = SelectionMode.Single;
      lstStyles.ItemSelected += lstStyles_ItemSelected;
      Gtk.Drag.DestSet(lstStyles, 0, null, Gdk.DragAction.Link | Gdk.DragAction.Move);

      lstStyles.DragMotion += lstStyles_DragMotion;
      lstStyles.DragDrop += lstStyles_DragDrop;

      lstStyles.KeyReleaseEvent += lstStyles_KeyReleased;
      evntPreview.AddEvents((int) Gdk.EventMask.ButtonReleaseMask);
      evntPreview.ButtonReleaseEvent += evntPreview_ButtonRelease;


      lstStyles.ClearItems();
      lstStyles.AddItem("Default");
      TreeIter iter;
      lstStyles.HeadersVisible = false;
      lstStyles.Model.GetIterFirst(out iter);
      if (AlertStyle.ToLower() == "default")
      {
        lstStyles.Selection.SelectIter(iter);
      }

      foreach (string sFile in Directory.GetFiles(modFunctions.AppData))
      {
        string sTitle = System.IO.Path.GetFileNameWithoutExtension(sFile);
        string sExt = System.IO.Path.GetExtension(sFile).ToLower();
        while (!string.IsNullOrEmpty(System.IO.Path.GetExtension(sTitle)))
        {
          sExt = System.IO.Path.GetExtension(sTitle).ToLower() + sExt;
          sTitle = System.IO.Path.GetFileNameWithoutExtension(sTitle);
        }
        if (sExt == ".tgz" || sExt == ".tar.gz" || sExt == ".tar")
        {
          lstStyles.AddItem(sTitle);
          lstStyles.Model.IterNext(ref iter);
          if (sTitle.ToLower() == AlertStyle.ToLower())
          {
            lstStyles.Selection.SelectIter(iter);
          }
        }
      }
      lstStyles.TooltipMarkup = "Select the Alert Window Style you want to use.\n<b>Drag and Drop:</b> Add an Alert Style from a Tarball or GZipped TAR (*.tar, *.tar.gz, *.tgz).\n<b>Delete:</b> Remove an Alert Style from the list.";
      modFunctions.PrepareLink(lblMore);
      lblMore.Markup = "<a href=\"http://srt.realityripple.com/Alert_Styles\">Get More Styles</a>";
      lblMore.TooltipText = "Download new Alert Window Styles from RealityRipple.com.";
      Changed = false;
      cmdSave.Sensitive = false;
      this.Show();
      this.GdkWindow.SetDecorations(Gdk.WMDecoration.All | Gdk.WMDecoration.Maximize | Gdk.WMDecoration.Minimize | Gdk.WMDecoration.Resizeh | Gdk.WMDecoration.Menu);
      this.GdkWindow.Functions = Gdk.WMFunction.All | Gdk.WMFunction.Maximize | Gdk.WMFunction.Minimize | Gdk.WMFunction.Resize;
      lstStyles_ItemSelected(lstStyles.SelectedItems);
    }
    void HandleWindowStateEvent(object o, WindowStateEventArgs args)
    {
      if (args.Event.ChangedMask == Gdk.WindowState.Iconified)
      {
        if ((args.Event.NewWindowState & Gdk.WindowState.Iconified) == Gdk.WindowState.Iconified)
        {
          args.Event.Window.Deiconify();
        }
      }
    }
    void lstStyles_KeyReleased(object o, KeyReleaseEventArgs args)
    {
      if (args.Event.Key == Gdk.Key.Delete)
      {
        if (lstStyles.SelectedItems.Length > 0)
        {
          TreeIter iter;
          lstStyles.Model.GetIterFirst(out iter);
          string sTitle = lstStyles.SelectedItems[0];
          do
          {
            GLib.Value val = new GLib.Value();
            lstStyles.Model.GetValue(iter, 0, ref val);
            if ((string) val.Val == sTitle)
            {
              break;
            }
          } while (lstStyles.Model.IterNext (ref iter));
          string sIndex = lstStyles.Model.GetStringFromIter(iter);
          if (sIndex == "0")
          {
            GdkWindow.Beep();
          }
          else if (modFunctions.ShowMessageBox(this, "Do you want to remove the \"" + sTitle + "\" Alert Window Style?", "Remove Alert Style?", DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo) == ResponseType.Yes)
          {
            TreeIter iFirst = new TreeIter();
            lstStyles.Model.GetIterFirst(out iFirst);
            if (File.Exists(System.IO.Path.Combine(modFunctions.AppDataPath, sTitle + ".tar.gz")))
            {
              lstStyles.Selection.SelectIter(iFirst);
              File.Delete(System.IO.Path.Combine(modFunctions.AppDataPath, sTitle + ".tar.gz"));
              lstStyles.RemoveItem(sTitle);
            }
            else if (File.Exists(System.IO.Path.Combine(modFunctions.AppDataPath, sTitle + ".tgz")))
            {
              lstStyles.Selection.SelectIter(iFirst);
              File.Delete(System.IO.Path.Combine(modFunctions.AppDataPath, sTitle + ".tgz"));
              lstStyles.RemoveItem(sTitle);
            }
            else if (File.Exists(System.IO.Path.Combine(modFunctions.AppDataPath, sTitle + ".tar")))
            {
              lstStyles.Selection.SelectIter(iFirst);
              File.Delete(System.IO.Path.Combine(modFunctions.AppDataPath, sTitle + ".tar"));
              lstStyles.RemoveItem(sTitle);
            }
            else
            {
              modFunctions.ShowMessageBox(this, "No file by that name was found! Alert Window Style may already be removed.", "Couldn't Find Alert Style", DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok);
              lstStyles.Selection.SelectIter(iFirst);
              lstStyles.RemoveItem(sTitle);
            }
          }
        }
      }
    }
    private void cmdSave_Click(object sender, EventArgs e)
    {
      AlertStyle = lstStyles.SelectedItems[0];
      Changed = true;
      cmdSave.Sensitive = false;
    }
    private void cmdClose_Click(object sender, EventArgs e)
    {
      if (Changed)
      {
        this.Respond(Gtk.ResponseType.Yes);
      }
      else
      {
        this.Respond(Gtk.ResponseType.No);
      }
    }
    private void lstStyles_ItemSelected(string[] selected)
    {
      NotifierStyle notifyTest = modFunctions.LoadAlertStyle(selected[0]);
      Gdk.Pixbuf preview = null;
      using (Bitmap bmpBG = new Bitmap(notifyTest.Background.Width + 16, notifyTest.Background.Height + 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
      {
        using (Graphics g = Graphics.FromImage(bmpBG))
        {
          g.Clear(Color.Black);
          g.FillRectangle(new System.Drawing.Drawing2D.LinearGradientBrush(new Point(0, 0), new Point(bmpBG.Width, bmpBG.Height), Color.SeaGreen, Color.Blue), new Rectangle(new Point(0, 0), bmpBG.Size));
          using (Bitmap bmpTest = modFunctions.MakeTransparent(new Bitmap(notifyTest.Background), notifyTest.TransparencyKey))
          {
            using (Graphics gt = Graphics.FromImage(bmpTest))
            {
              Bitmap CloseBitmap = modFunctions.MakeTransparent(new Bitmap(notifyTest.CloseButton), notifyTest.TransparencyKey);
              gt.DrawImageUnscaledAndClipped(CloseBitmap, new Rectangle(notifyTest.CloseLocation, new Size(notifyTest.CloseButton.Width / 3, notifyTest.CloseButton.Height)));
              StringFormat tsf = new StringFormat();
              tsf.Alignment = StringAlignment.Near;
              tsf.LineAlignment = StringAlignment.Center;
              tsf.FormatFlags = StringFormatFlags.NoWrap;
              tsf.Trimming = StringTrimming.EllipsisCharacter;
              gt.DrawString("Important Alert Information", new Font("Arial", 12, FontStyle.Bold, GraphicsUnit.Pixel), new SolidBrush(notifyTest.TitleColor), notifyTest.TitleLocation, tsf);
              StringFormat csf = new StringFormat();
              csf.Alignment = StringAlignment.Center;
              csf.LineAlignment = StringAlignment.Center;
              csf.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
              csf.Trimming = StringTrimming.Word;
              gt.DrawString("This alert is just an example to display what an actual alert would look like using the \"" + selected[0] + "\" style.", new Font("Arial", 11, FontStyle.Regular, GraphicsUnit.Pixel), new SolidBrush(notifyTest.ContentColor), notifyTest.ContentLocation, csf);
            }
            g.DrawImageUnscaled(bmpTest, 8, 8);
          }
        }
        preview = modFunctions.ImageToPixbuf(bmpBG);
      }
      pctPreview.WidthRequest = preview.Width;
      pctPreview.HeightRequest = preview.Height;
      pctPreview.Pixbuf = preview;
      cmdSave.Sensitive = ! (selected[0].ToLower() == AlertStyle.ToLower());
      cmdRemove.Sensitive = !(selected[0].ToLower() == "default");
    }
    private void lstStyles_DragMotion(object sender, DragMotionArgs e)
    {
      bool ok = false;
      foreach (Gdk.Atom t in e.Context.Targets)
      {
        if (t.Name == "STRING")
        {
          ok = true;
          Gdk.Atom targ = t;
          lstStyles.DragDataReceived += lstStyles_DragDataReceived_Motion;
          Gtk.Drag.GetData((Gtk.Widget) sender, e.Context, targ, e.Time);
        }
      }
      e.RetVal = ok;
    }
    private void lstStyles_DragDrop(object sender, DragDropArgs e)
    {
      if (e.Context.Targets.Length != 0)
      {
        Gdk.Atom targ = null;
        foreach (Gdk.Atom t in e.Context.Targets)
        {
          if (t.Name == "STRING")
          {
            targ = t;
            lstStyles.DragDataReceived -= lstStyles_DragDataReceived_Motion;
            lstStyles.DragDataReceived += lstStyles_DragDataReceived_Drop;
            Gtk.Drag.GetData((Gtk.Widget) sender, e.Context, targ, e.Time);
            e.RetVal = true;
            break;
          }
        }
        if (targ == null)
        {
          e.RetVal = false;
        }
      }
      else
      {
        e.RetVal = false;
      }
    }
    private void lstStyles_DragDataReceived_Motion(object sender, DragDataReceivedArgs e)
    {
      if (e.SelectionData.Length > 0 && e.SelectionData.Format == 8)
      {
        bool Added = false;
        byte[] data = e.SelectionData.Data;
        string encoded = System.Text.Encoding.UTF8.GetString(data);
        System.Collections.Generic.List<string> paths = new System.Collections.Generic.List<string>(encoded.Split('\r', '\n'));
        paths.RemoveAll(string.IsNullOrEmpty);
        foreach (string sTP in paths)
        {
          if (sTP.StartsWith("file://"))
          {
            string StylePath = sTP.Substring(7);
            string sTitle = System.IO.Path.GetFileNameWithoutExtension(StylePath);
            string sExt = System.IO.Path.GetExtension(StylePath).ToLower();
            while (!string.IsNullOrEmpty(System.IO.Path.GetExtension(sTitle)))
            {
              sExt = System.IO.Path.GetExtension(sTitle).ToLower() + sExt;
              sTitle = System.IO.Path.GetFileNameWithoutExtension(sTitle);
            }
            if (sExt == ".tgz" || sExt == ".tar.gz" || sExt == ".tar")
            {
              Added = true;
              break;
            }
          }
        }
        if (Added)
        {
          Gdk.Drag.Status(e.Context, Gdk.DragAction.Link, e.Time);
        }
        else
        {
          Gdk.Drag.Status(e.Context, 0, e.Time);
        }
      }
    }
    private void lstStyles_DragDataReceived_Drop(object sender, DragDataReceivedArgs e)
    {
      if (e.SelectionData.Length > 0 && e.SelectionData.Format == 8)
      {
        byte[] data = e.SelectionData.Data;
        string encoded = System.Text.Encoding.UTF8.GetString(data);
        System.Collections.Generic.List<string> paths = new System.Collections.Generic.List<string>(encoded.Split('\r', '\n'));
        paths.RemoveAll(string.IsNullOrEmpty);
        foreach (string sTP in paths)
        {
          if (sTP.StartsWith("file://"))
          {
            string StylePath = sTP.Substring(7);
            string sTitle = System.IO.Path.GetFileNameWithoutExtension(StylePath);
            string sExt = System.IO.Path.GetExtension(StylePath).ToLower();
            while (!string.IsNullOrEmpty(System.IO.Path.GetExtension(sTitle)))
            {
              sExt = System.IO.Path.GetExtension(sTitle).ToLower() + sExt;
              sTitle = System.IO.Path.GetFileNameWithoutExtension(sTitle);
            }
            if (sExt == ".tgz" || sExt == ".tar.gz" || sExt == ".tar")
            {
              File.Copy(StylePath, System.IO.Path.Combine(modFunctions.AppData, sTitle + sExt), true);
              TreeIter iter;
              lstStyles.Model.GetIterFirst(out iter);
              bool Add = true;
              do
              {
                GLib.Value val = new GLib.Value();
                lstStyles.Model.GetValue(iter, 0, ref val);
                if ((string) val.Val == sTitle)
                {
                  Add = false;
                  break;
                }
              } while (lstStyles.Model.IterNext (ref iter));
              if (Add)
              {
                lstStyles.AddItem(sTitle);
              }
              if (paths.Count == 1)
              {
                TreeIter iterSel;
                lstStyles.Model.GetIterFirst(out iterSel);
                do
                {
                  GLib.Value val = new GLib.Value();
                  lstStyles.Model.GetValue(iterSel, 0, ref val);
                  if ((string) val.Val == sTitle)
                  {
                    lstStyles.Selection.SelectIter(iterSel);
                    break;
                  }
                } while (lstStyles.Model.IterNext (ref iterSel));
              }
            }
          }
        }
        Drag.Finish(e.Context, true, false, e.Time);
      }
      lstStyles.DragDataReceived -= lstStyles_DragDataReceived_Drop;
    }
    private void evntPreview_ButtonRelease(object sender, ButtonReleaseEventArgs e)
    {
      if (e.Event.Button == 3)
      {
        Gtk.Clipboard cb = Gtk.Clipboard.Get(Gdk.Selection.Clipboard);
        cb.Image = pctPreview.Pixbuf;
      }
      else if (e.Event.Button == 2)
      {
        TaskbarNotifier taskNotifier = null;
        modFunctions.MakeNotifier(ref taskNotifier, false, modFunctions.LoadAlertStyle(lstStyles.SelectedItems[0]));
        if (taskNotifier != null)
        {
          taskNotifier.TransientFor = this;
          taskNotifier.Modal = true;
          taskNotifier.Show("Hello World", "This alert is a live example of your currently selected alert. What do you think?", 200, 5000, 100);
        }
      }
    }
    protected void cmdAdd_Clicked(object sender, EventArgs e)
    {
      FileChooserDialog cdlOpen = new FileChooserDialog("Add Alert Window Styles", this, FileChooserAction.Open, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Open, Gtk.ResponseType.Ok);
      Gtk.FileFilter fTGZ = new Gtk.FileFilter();
      fTGZ.AddPattern("*.tar");
      fTGZ.AddPattern("*.tgz");
      fTGZ.AddPattern("*.tar.gz");
      fTGZ.Name = "Window Styles";
      cdlOpen.AddFilter(fTGZ);
      cdlOpen.SelectMultiple = true;
      cdlOpen.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
      ResponseType ret = (ResponseType) cdlOpen.Run();
      string[] sRet = cdlOpen.Filenames;
      FileFilter fRet = cdlOpen.Filter;
      cdlOpen.Destroy();
      if (ret == ResponseType.Ok)
      {
        if (fRet == fTGZ)
        {
          foreach (string StylePath in sRet)
          {
            string sTitle = System.IO.Path.GetFileNameWithoutExtension(StylePath);
            string sExt = System.IO.Path.GetExtension(StylePath).ToLower();
            while (!string.IsNullOrEmpty(System.IO.Path.GetExtension(sTitle)))
            {
              sExt = System.IO.Path.GetExtension(sTitle).ToLower() + sExt;
              sTitle = System.IO.Path.GetFileNameWithoutExtension(sTitle);
            }
            if (sExt == ".tgz" || sExt == ".tar.gz" || sExt == ".tar")
            {
              File.Copy(StylePath, System.IO.Path.Combine(modFunctions.AppData, sTitle + sExt), true);
              TreeIter iter;
              lstStyles.Model.GetIterFirst(out iter);
              bool Add = true;
              do
              {
                GLib.Value val = new GLib.Value();
                lstStyles.Model.GetValue(iter, 0, ref val);
                if ((string) val.Val == sTitle)
                {
                  Add = false;
                  break;
                }
              } while (lstStyles.Model.IterNext (ref iter));
              if (Add)
              {
                lstStyles.AddItem(sTitle);
              }
              if (sRet.Length == 1)
              {
                TreeIter iterSel;
                lstStyles.Model.GetIterFirst(out iterSel);
                do
                {
                  GLib.Value val = new GLib.Value();
                  lstStyles.Model.GetValue(iterSel, 0, ref val);
                  if ((string) val.Val == sTitle)
                  {
                    lstStyles.Selection.SelectIter(iterSel);
                    break;
                  }
                } while (lstStyles.Model.IterNext (ref iterSel));
              }
            }
          }
        }
      }
    }
    protected void cmdRemove_Clicked(object sender, EventArgs e)
    {
      if (lstStyles.SelectedItems.Length > 0)
      {
        TreeIter iter;
        lstStyles.Model.GetIterFirst(out iter);
        string sTitle = lstStyles.SelectedItems[0];
        do
        {
          GLib.Value val = new GLib.Value();
          lstStyles.Model.GetValue(iter, 0, ref val);
          if ((string) val.Val == sTitle)
          {
            break;
          }
        } while (lstStyles.Model.IterNext (ref iter));
        string sIndex = lstStyles.Model.GetStringFromIter(iter);
        if (sIndex == "0")
        {
          GdkWindow.Beep();
        }
        else if (modFunctions.ShowMessageBox(this, "Do you want to remove the \"" + sTitle + "\" Alert Window Style?", "Remove Alert Style?", DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo) == ResponseType.Yes)
        {
          TreeIter iFirst = new TreeIter();
          lstStyles.Model.GetIterFirst(out iFirst);
          if (File.Exists(System.IO.Path.Combine(modFunctions.AppData, sTitle + ".tar.gz")))
          {
            lstStyles.Selection.SelectIter(iFirst);
            File.Delete(System.IO.Path.Combine(modFunctions.AppData, sTitle + ".tar.gz"));
            lstStyles.RemoveItem(sTitle);
          }
          else if (File.Exists(System.IO.Path.Combine(modFunctions.AppData, sTitle + ".tgz")))
          {
            lstStyles.Selection.SelectIter(iFirst);
            File.Delete(System.IO.Path.Combine(modFunctions.AppData, sTitle + ".tgz"));
            lstStyles.RemoveItem(sTitle);
          }
          else if (File.Exists(System.IO.Path.Combine(modFunctions.AppData, sTitle + ".tar")))
          {
            lstStyles.Selection.SelectIter(iFirst);
            File.Delete(System.IO.Path.Combine(modFunctions.AppData, sTitle + ".tar"));
            lstStyles.RemoveItem(sTitle);
          }
          else
          {
            modFunctions.ShowMessageBox(this, "No file by that name was found! Alert Window Style may already be removed.", "Couldn't Find Alert Style", DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok);
            lstStyles.Selection.SelectIter(iFirst);
            lstStyles.RemoveItem(sTitle);
          }
        }
      }
    }
  }
  class ListBox : ListView<string>
  {
    public ListBox(params string[] columnNames) : base(columnNames)
    {

    }
    protected override void RenderCell(CellRendererText render, int index, string item)
    {
      render.Text = item;
    }
  }
}
