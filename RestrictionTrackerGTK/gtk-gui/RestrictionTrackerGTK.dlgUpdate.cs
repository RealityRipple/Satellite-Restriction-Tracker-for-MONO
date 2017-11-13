
// This file has been generated by the GUI designer. Do not modify.
namespace RestrictionTrackerGTK
{
	public partial class dlgUpdate
	{
		private global::Gtk.VBox pnlUpdate;
		private global::Gtk.Label lblTitle;
		private global::Gtk.Label lblNewVer;
		private global::Gtk.Label lblBETA;
		private global::Gtk.CheckButton chkStopBETA;
		private global::Gtk.ScrolledWindow scrInfo;
		private global::Gtk.TextView txtInfo;
		private global::Gtk.Button cmdDownload;
		private global::Gtk.Button cmdCancel;
		private global::Gtk.Button cmdChanges;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget RestrictionTrackerGTK.dlgUpdate
			this.Name = "RestrictionTrackerGTK.dlgUpdate";
			this.Icon = global::Gdk.Pixbuf.LoadFromResource ("RestrictionTrackerGTK.Resources.norm.ico");
			this.TypeHint = ((global::Gdk.WindowTypeHint)(1));
			this.WindowPosition = ((global::Gtk.WindowPosition)(1));
			this.Resizable = false;
			this.AllowGrow = false;
			// Internal child RestrictionTrackerGTK.dlgUpdate.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "pnlUpdateDialog";
			w1.BorderWidth = ((uint)(2));
			// Container child pnlUpdateDialog.Gtk.Box+BoxChild
			this.pnlUpdate = new global::Gtk.VBox ();
			this.pnlUpdate.Name = "pnlUpdate";
			this.pnlUpdate.Spacing = 6;
			// Container child pnlUpdate.Gtk.Box+BoxChild
			this.lblTitle = new global::Gtk.Label ();
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Xpad = 3;
			this.lblTitle.Ypad = 3;
			this.lblTitle.Xalign = 0F;
			this.lblTitle.LabelProp = global::Mono.Unix.Catalog.GetString ("<span size=\"14000\">Satellite Restriction Tracker Update</span>");
			this.lblTitle.UseMarkup = true;
			this.pnlUpdate.Add (this.lblTitle);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.pnlUpdate [this.lblTitle]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child pnlUpdate.Gtk.Box+BoxChild
			this.lblNewVer = new global::Gtk.Label ();
			this.lblNewVer.Name = "lblNewVer";
			this.lblNewVer.Xpad = 3;
			this.lblNewVer.Ypad = 3;
			this.lblNewVer.Xalign = 0F;
			this.lblNewVer.LabelProp = global::Mono.Unix.Catalog.GetString ("<span size=\"9000\">Version %v has been released and is available for download.\nTo keep up-to-date with the latest features, improvements, bug fixes, and\nmeter compliance, please update %p immediately.</span>");
			this.lblNewVer.UseMarkup = true;
			this.pnlUpdate.Add (this.lblNewVer);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.pnlUpdate [this.lblNewVer]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child pnlUpdate.Gtk.Box+BoxChild
			this.lblBETA = new global::Gtk.Label ();
			this.lblBETA.Name = "lblBETA";
			this.lblBETA.Xpad = 3;
			this.lblBETA.Xalign = 0F;
			this.lblBETA.LabelProp = global::Mono.Unix.Catalog.GetString ("<span color=\"Firebrick\">BETA updates may have bugs and other issues that haven't been\nworked out yet, but need testing on a wide range of accounts.\nPlease back up your history before using BETAs.</span>");
			this.lblBETA.UseMarkup = true;
			this.pnlUpdate.Add (this.lblBETA);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.pnlUpdate [this.lblBETA]));
			w4.Position = 2;
			w4.Expand = false;
			w4.Fill = false;
			// Container child pnlUpdate.Gtk.Box+BoxChild
			this.chkStopBETA = new global::Gtk.CheckButton ();
			this.chkStopBETA.TooltipMarkup = "Disable notifications of BETA version updates.";
			this.chkStopBETA.CanFocus = true;
			this.chkStopBETA.Name = "chkStopBETA";
			this.chkStopBETA.Label = global::Mono.Unix.Catalog.GetString ("Don't notify me of _BETA updates.");
			this.chkStopBETA.DrawIndicator = true;
			this.chkStopBETA.UseUnderline = true;
			this.pnlUpdate.Add (this.chkStopBETA);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.pnlUpdate [this.chkStopBETA]));
			w5.Position = 3;
			w5.Expand = false;
			w5.Fill = false;
			w1.Add (this.pnlUpdate);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(w1 [this.pnlUpdate]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child pnlUpdateDialog.Gtk.Box+BoxChild
			this.scrInfo = new global::Gtk.ScrolledWindow ();
			this.scrInfo.Name = "scrInfo";
			this.scrInfo.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrInfo.Gtk.Container+ContainerChild
			this.txtInfo = new global::Gtk.TextView ();
			this.txtInfo.CanFocus = true;
			this.txtInfo.Name = "txtInfo";
			this.txtInfo.Editable = false;
			this.txtInfo.AcceptsTab = false;
			this.txtInfo.WrapMode = ((global::Gtk.WrapMode)(3));
			this.scrInfo.Add (this.txtInfo);
			w1.Add (this.scrInfo);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(w1 [this.scrInfo]));
			w8.PackType = ((global::Gtk.PackType)(1));
			w8.Position = 1;
			// Internal child RestrictionTrackerGTK.dlgUpdate.ActionArea
			global::Gtk.HButtonBox w9 = this.ActionArea;
			w9.Name = "pnlUpdateButtons";
			w9.Homogeneous = true;
			w9.Spacing = 10;
			w9.BorderWidth = ((uint)(5));
			w9.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child pnlUpdateButtons.Gtk.ButtonBox+ButtonBoxChild
			this.cmdDownload = new global::Gtk.Button ();
			this.cmdDownload.TooltipMarkup = "Download the new version.";
			this.cmdDownload.CanDefault = true;
			this.cmdDownload.CanFocus = true;
			this.cmdDownload.Name = "cmdDownload";
			this.cmdDownload.UseUnderline = true;
			this.cmdDownload.Label = global::Mono.Unix.Catalog.GetString ("Download _Update");
			this.AddActionWidget (this.cmdDownload, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w10 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w9 [this.cmdDownload]));
			w10.Expand = false;
			w10.Fill = false;
			// Container child pnlUpdateButtons.Gtk.ButtonBox+ButtonBoxChild
			this.cmdCancel = new global::Gtk.Button ();
			this.cmdCancel.TooltipMarkup = "Ignore the new version for now.";
			this.cmdCancel.CanDefault = true;
			this.cmdCancel.CanFocus = true;
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.UseUnderline = true;
			this.cmdCancel.Label = global::Mono.Unix.Catalog.GetString ("_Not Now");
			this.AddActionWidget (this.cmdCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w11 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w9 [this.cmdCancel]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			// Container child pnlUpdateButtons.Gtk.ButtonBox+ButtonBoxChild
			this.cmdChanges = new global::Gtk.Button ();
			this.cmdChanges.TooltipMarkup = "View the latest version's Change Log.";
			this.cmdChanges.CanFocus = true;
			this.cmdChanges.Name = "cmdChanges";
			this.cmdChanges.UseUnderline = true;
			this.cmdChanges.Label = global::Mono.Unix.Catalog.GetString ("_Changes >>");
			w9.Add (this.cmdChanges);
			global::Gtk.ButtonBox.ButtonBoxChild w12 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w9 [this.cmdChanges]));
			w12.Position = 2;
			w12.Expand = false;
			w12.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 471;
			this.DefaultHeight = 334;
			this.scrInfo.Hide ();
			this.Hide ();
			this.cmdDownload.Clicked += new global::System.EventHandler (this.cmdDownload_Click);
			this.cmdCancel.Clicked += new global::System.EventHandler (this.cmdCancel_Click);
			this.cmdChanges.Clicked += new global::System.EventHandler (this.cmdChanges_Click);
		}
	}
}
