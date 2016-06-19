
// This file has been generated by the GUI designer. Do not modify.
namespace RestrictionTrackerGTK
{
	public partial class frmMain
	{
		private global::Gtk.VBox pnlDetails;
		
		private global::Gtk.HBox pnlButtons;
		
		private global::Gtk.Button cmdRefresh;
		
		private global::Gtk.Button cmdHistory;
		
		private global::Gtk.Button cmdConfig;
		
		private global::Gtk.Button cmdAbout;
		
		private global::Gtk.Frame gbUsage;
		
		private global::Gtk.Alignment algnUsage;
		
		private global::Gtk.VBox pnlDisplays;
		
		private global::Gtk.VBox pnlNothing;
		
		private global::Gtk.Label lblNothing;
		
		private global::Gtk.Label lblRRS;
		
		private global::Gtk.HBox pnlWildBlue;
		
		private global::Gtk.Frame gbDld;
		
		private global::Gtk.Alignment algnDld;
		
		private global::Gtk.HBox pnlDld;
		
		private global::Gtk.VBox pnlDldText;
		
		private global::Gtk.HBox pnlDldTextUsed;
		
		private global::Gtk.Label lblDUsed;
		
		private global::Gtk.Label lblDldUsed;
		
		private global::Gtk.HBox pnlDldTextFree;
		
		private global::Gtk.Label lblDFree;
		
		private global::Gtk.Label lblDldFree;
		
		private global::Gtk.HBox pnlDldTextLimit;
		
		private global::Gtk.Label lblDLimit;
		
		private global::Gtk.Label lblDldLimit;
		
		private global::Gtk.EventBox evnDld;
		
		private global::Gtk.Image pctDld;
		
		private global::Gtk.Label lblDld;
		
		private global::Gtk.Frame gbUld;
		
		private global::Gtk.Alignment algnUld;
		
		private global::Gtk.HBox pnlUld;
		
		private global::Gtk.VBox pnlUldText;
		
		private global::Gtk.HBox pnlUldTextUsed;
		
		private global::Gtk.Label lblUUsed;
		
		private global::Gtk.Label lblUldUsed;
		
		private global::Gtk.HBox pnlUldTextFree;
		
		private global::Gtk.Label lblUFree;
		
		private global::Gtk.Label lblUldFree;
		
		private global::Gtk.HBox pnlUldTextLimit;
		
		private global::Gtk.Label lblULimit;
		
		private global::Gtk.Label lblUldLimit;
		
		private global::Gtk.EventBox evnUld;
		
		private global::Gtk.Image pctUld;
		
		private global::Gtk.Label lblUld;
		
		private global::Gtk.HBox pnlExede;
		
		private global::Gtk.VBox pnlExedeNumbers;
		
		private global::Gtk.HBox pnlExedeDown;
		
		private global::Gtk.Label lblExedeDown;
		
		private global::Gtk.Label lblExedeDownVal;
		
		private global::Gtk.HBox pnlExedeUp;
		
		private global::Gtk.Label lblExedeUp;
		
		private global::Gtk.Label lblExedeUpVal;
		
		private global::Gtk.HBox pnlExedeUsed;
		
		private global::Gtk.Label lblExedeTotal;
		
		private global::Gtk.Label lblExedeTotalVal;
		
		private global::Gtk.HBox pnlExedeFree;
		
		private global::Gtk.Label lblExedeRemain;
		
		private global::Gtk.Label lblExedeRemainVal;
		
		private global::Gtk.HBox pnlExedeTotal;
		
		private global::Gtk.Label lblExedeAllowed;
		
		private global::Gtk.Label lblExedeAllowedVal;
		
		private global::Gtk.EventBox evnExede;
		
		private global::Gtk.Image pctExede;
		
		private global::Gtk.HBox pnlRural;
		
		private global::Gtk.VBox pnlRuralNumbers;
		
		private global::Gtk.HBox pnlRuralUsed;
		
		private global::Gtk.Label lblRuralUsed;
		
		private global::Gtk.Label lblRuralUsedVal;
		
		private global::Gtk.HBox pnlRuralFree;
		
		private global::Gtk.Label lblRuralRemain;
		
		private global::Gtk.Label lblRuralRemainVal;
		
		private global::Gtk.HBox pnlRuralTotal;
		
		private global::Gtk.Label lblRuralAllowed;
		
		private global::Gtk.Label lblRuralAllowedVal;
		
		private global::Gtk.EventBox evnRural;
		
		private global::Gtk.Image pctRural;
		
		private global::Gtk.Label lblStatus;
		
		private global::Gtk.Statusbar sbMainStatus;
		
		private global::Gtk.ProgressBar pbMainStatus;
		
		private global::Gtk.Button cmdNetTest;
		
		private global::Gtk.Label lblMainStatus;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget RestrictionTrackerGTK.frmMain
			this.Name = "RestrictionTrackerGTK.frmMain";
			this.Title = global::Mono.Unix.Catalog.GetString ("Satellite Restriction Tracker");
			this.Icon = global::Gdk.Pixbuf.LoadFromResource ("RestrictionTrackerGTK.Resources.norm.ico");
			this.WindowPosition = ((global::Gtk.WindowPosition)(1));
			this.AllowShrink = true;
			this.Role = "";
			// Container child RestrictionTrackerGTK.frmMain.Gtk.Container+ContainerChild
			this.pnlDetails = new global::Gtk.VBox ();
			this.pnlDetails.Name = "pnlDetails";
			// Container child pnlDetails.Gtk.Box+BoxChild
			this.pnlButtons = new global::Gtk.HBox ();
			this.pnlButtons.Name = "pnlButtons";
			this.pnlButtons.Homogeneous = true;
			this.pnlButtons.Spacing = 3;
			this.pnlButtons.BorderWidth = ((uint)(3));
			// Container child pnlButtons.Gtk.Box+BoxChild
			this.cmdRefresh = new global::Gtk.Button ();
			this.cmdRefresh.TooltipMarkup = "Reload bandwidth level information immediately.\n(Hold CTRL to reload database.)";
			this.cmdRefresh.CanFocus = true;
			this.cmdRefresh.Name = "cmdRefresh";
			this.cmdRefresh.UseUnderline = true;
			this.cmdRefresh.Label = global::Mono.Unix.Catalog.GetString ("_Refresh");
			this.pnlButtons.Add (this.cmdRefresh);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.pnlButtons [this.cmdRefresh]));
			w1.Position = 0;
			// Container child pnlButtons.Gtk.Box+BoxChild
			this.cmdHistory = new global::Gtk.Button ();
			this.cmdHistory.TooltipMarkup = "View your bandwidth history.";
			this.cmdHistory.CanFocus = true;
			this.cmdHistory.Name = "cmdHistory";
			this.cmdHistory.UseUnderline = true;
			this.cmdHistory.Label = global::Mono.Unix.Catalog.GetString ("_History");
			this.pnlButtons.Add (this.cmdHistory);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.pnlButtons [this.cmdHistory]));
			w2.Position = 1;
			// Container child pnlButtons.Gtk.Box+BoxChild
			this.cmdConfig = new global::Gtk.Button ();
			this.cmdConfig.TooltipMarkup = "Change program settings.";
			this.cmdConfig.CanFocus = true;
			this.cmdConfig.Name = "cmdConfig";
			this.cmdConfig.UseUnderline = true;
			this.cmdConfig.Label = global::Mono.Unix.Catalog.GetString ("_Configuration");
			this.pnlButtons.Add (this.cmdConfig);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.pnlButtons [this.cmdConfig]));
			w3.Position = 2;
			// Container child pnlButtons.Gtk.Box+BoxChild
			this.cmdAbout = new global::Gtk.Button ();
			this.cmdAbout.TooltipMarkup = "View information about Satellite Restriction Tracker and check for updates.";
			this.cmdAbout.CanFocus = true;
			this.cmdAbout.Name = "cmdAbout";
			this.cmdAbout.UseUnderline = true;
			this.cmdAbout.Label = global::Mono.Unix.Catalog.GetString ("_About");
			this.pnlButtons.Add (this.cmdAbout);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.pnlButtons [this.cmdAbout]));
			w4.PackType = ((global::Gtk.PackType)(1));
			w4.Position = 3;
			this.pnlDetails.Add (this.pnlButtons);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.pnlDetails [this.pnlButtons]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child pnlDetails.Gtk.Box+BoxChild
			this.gbUsage = new global::Gtk.Frame ();
			this.gbUsage.Name = "gbUsage";
			this.gbUsage.BorderWidth = ((uint)(3));
			// Container child gbUsage.Gtk.Container+ContainerChild
			this.algnUsage = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.algnUsage.Name = "algnUsage";
			this.algnUsage.LeftPadding = ((uint)(3));
			this.algnUsage.TopPadding = ((uint)(3));
			this.algnUsage.RightPadding = ((uint)(3));
			this.algnUsage.BottomPadding = ((uint)(3));
			// Container child algnUsage.Gtk.Container+ContainerChild
			this.pnlDisplays = new global::Gtk.VBox ();
			this.pnlDisplays.Name = "pnlDisplays";
			this.pnlDisplays.Spacing = 6;
			// Container child pnlDisplays.Gtk.Box+BoxChild
			this.pnlNothing = new global::Gtk.VBox ();
			this.pnlNothing.Name = "pnlNothing";
			this.pnlNothing.Homogeneous = true;
			this.pnlNothing.Spacing = 6;
			// Container child pnlNothing.Gtk.Box+BoxChild
			this.lblNothing = new global::Gtk.Label ();
			this.lblNothing.Name = "lblNothing";
			this.lblNothing.LabelProp = global::Mono.Unix.Catalog.GetString ("Satellite Restriction Tracker");
			this.pnlNothing.Add (this.lblNothing);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.pnlNothing [this.lblNothing]));
			w6.Position = 0;
			// Container child pnlNothing.Gtk.Box+BoxChild
			this.lblRRS = new global::Gtk.Label ();
			this.lblRRS.CanFocus = true;
			this.lblRRS.Name = "lblRRS";
			this.lblRRS.Xalign = 1F;
			this.lblRRS.LabelProp = global::Mono.Unix.Catalog.GetString ("by RealityRipple Software");
			this.pnlNothing.Add (this.lblRRS);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.pnlNothing [this.lblRRS]));
			w7.PackType = ((global::Gtk.PackType)(1));
			w7.Position = 1;
			w7.Expand = false;
			w7.Fill = false;
			this.pnlDisplays.Add (this.pnlNothing);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.pnlDisplays [this.pnlNothing]));
			w8.Position = 0;
			// Container child pnlDisplays.Gtk.Box+BoxChild
			this.pnlWildBlue = new global::Gtk.HBox ();
			this.pnlWildBlue.Name = "pnlWildBlue";
			this.pnlWildBlue.Homogeneous = true;
			// Container child pnlWildBlue.Gtk.Box+BoxChild
			this.gbDld = new global::Gtk.Frame ();
			this.gbDld.Name = "gbDld";
			this.gbDld.ShadowType = ((global::Gtk.ShadowType)(2));
			// Container child gbDld.Gtk.Container+ContainerChild
			this.algnDld = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.algnDld.Name = "algnDld";
			this.algnDld.LeftPadding = ((uint)(3));
			this.algnDld.TopPadding = ((uint)(3));
			this.algnDld.RightPadding = ((uint)(3));
			this.algnDld.BottomPadding = ((uint)(3));
			// Container child algnDld.Gtk.Container+ContainerChild
			this.pnlDld = new global::Gtk.HBox ();
			this.pnlDld.Name = "pnlDld";
			this.pnlDld.Spacing = 6;
			// Container child pnlDld.Gtk.Box+BoxChild
			this.pnlDldText = new global::Gtk.VBox ();
			this.pnlDldText.Name = "pnlDldText";
			this.pnlDldText.Homogeneous = true;
			this.pnlDldText.Spacing = 6;
			// Container child pnlDldText.Gtk.Box+BoxChild
			this.pnlDldTextUsed = new global::Gtk.HBox ();
			this.pnlDldTextUsed.Name = "pnlDldTextUsed";
			this.pnlDldTextUsed.Spacing = 6;
			// Container child pnlDldTextUsed.Gtk.Box+BoxChild
			this.lblDUsed = new global::Gtk.Label ();
			this.lblDUsed.Name = "lblDUsed";
			this.lblDUsed.LabelProp = global::Mono.Unix.Catalog.GetString ("Used:");
			this.pnlDldTextUsed.Add (this.lblDUsed);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.pnlDldTextUsed [this.lblDUsed]));
			w9.Position = 0;
			w9.Expand = false;
			w9.Fill = false;
			// Container child pnlDldTextUsed.Gtk.Box+BoxChild
			this.lblDldUsed = new global::Gtk.Label ();
			this.lblDldUsed.Name = "lblDldUsed";
			this.lblDldUsed.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlDldTextUsed.Add (this.lblDldUsed);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.pnlDldTextUsed [this.lblDldUsed]));
			w10.PackType = ((global::Gtk.PackType)(1));
			w10.Position = 1;
			w10.Expand = false;
			w10.Fill = false;
			this.pnlDldText.Add (this.pnlDldTextUsed);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.pnlDldText [this.pnlDldTextUsed]));
			w11.Position = 0;
			w11.Expand = false;
			w11.Fill = false;
			// Container child pnlDldText.Gtk.Box+BoxChild
			this.pnlDldTextFree = new global::Gtk.HBox ();
			this.pnlDldTextFree.Name = "pnlDldTextFree";
			this.pnlDldTextFree.Spacing = 6;
			// Container child pnlDldTextFree.Gtk.Box+BoxChild
			this.lblDFree = new global::Gtk.Label ();
			this.lblDFree.Name = "lblDFree";
			this.lblDFree.LabelProp = global::Mono.Unix.Catalog.GetString ("Free:");
			this.pnlDldTextFree.Add (this.lblDFree);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.pnlDldTextFree [this.lblDFree]));
			w12.Position = 0;
			w12.Expand = false;
			w12.Fill = false;
			// Container child pnlDldTextFree.Gtk.Box+BoxChild
			this.lblDldFree = new global::Gtk.Label ();
			this.lblDldFree.Name = "lblDldFree";
			this.lblDldFree.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlDldTextFree.Add (this.lblDldFree);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.pnlDldTextFree [this.lblDldFree]));
			w13.PackType = ((global::Gtk.PackType)(1));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			this.pnlDldText.Add (this.pnlDldTextFree);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.pnlDldText [this.pnlDldTextFree]));
			w14.Position = 1;
			w14.Expand = false;
			w14.Fill = false;
			// Container child pnlDldText.Gtk.Box+BoxChild
			this.pnlDldTextLimit = new global::Gtk.HBox ();
			this.pnlDldTextLimit.Name = "pnlDldTextLimit";
			this.pnlDldTextLimit.Spacing = 6;
			// Container child pnlDldTextLimit.Gtk.Box+BoxChild
			this.lblDLimit = new global::Gtk.Label ();
			this.lblDLimit.Name = "lblDLimit";
			this.lblDLimit.LabelProp = global::Mono.Unix.Catalog.GetString ("Limit:");
			this.pnlDldTextLimit.Add (this.lblDLimit);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.pnlDldTextLimit [this.lblDLimit]));
			w15.Position = 0;
			w15.Expand = false;
			w15.Fill = false;
			// Container child pnlDldTextLimit.Gtk.Box+BoxChild
			this.lblDldLimit = new global::Gtk.Label ();
			this.lblDldLimit.Name = "lblDldLimit";
			this.lblDldLimit.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlDldTextLimit.Add (this.lblDldLimit);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.pnlDldTextLimit [this.lblDldLimit]));
			w16.PackType = ((global::Gtk.PackType)(1));
			w16.Position = 1;
			w16.Expand = false;
			w16.Fill = false;
			this.pnlDldText.Add (this.pnlDldTextLimit);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.pnlDldText [this.pnlDldTextLimit]));
			w17.Position = 2;
			w17.Expand = false;
			w17.Fill = false;
			this.pnlDld.Add (this.pnlDldText);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.pnlDld [this.pnlDldText]));
			w18.Position = 0;
			w18.Expand = false;
			w18.Fill = false;
			// Container child pnlDld.Gtk.Box+BoxChild
			this.evnDld = new global::Gtk.EventBox ();
			this.evnDld.Name = "evnDld";
			// Container child evnDld.Gtk.Container+ContainerChild
			this.pctDld = new global::Gtk.Image ();
			this.pctDld.TooltipMarkup = "Graph representing your download usage.";
			this.pctDld.Name = "pctDld";
			this.evnDld.Add (this.pctDld);
			this.pnlDld.Add (this.evnDld);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.pnlDld [this.evnDld]));
			w20.Position = 1;
			this.algnDld.Add (this.pnlDld);
			this.gbDld.Add (this.algnDld);
			this.lblDld = new global::Gtk.Label ();
			this.lblDld.Name = "lblDld";
			this.lblDld.Xalign = 0F;
			this.lblDld.LabelProp = global::Mono.Unix.Catalog.GetString ("Download (X%)");
			this.lblDld.UseMarkup = true;
			this.gbDld.LabelWidget = this.lblDld;
			this.pnlWildBlue.Add (this.gbDld);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.pnlWildBlue [this.gbDld]));
			w23.Position = 0;
			w23.Padding = ((uint)(3));
			// Container child pnlWildBlue.Gtk.Box+BoxChild
			this.gbUld = new global::Gtk.Frame ();
			this.gbUld.Name = "gbUld";
			this.gbUld.ShadowType = ((global::Gtk.ShadowType)(2));
			// Container child gbUld.Gtk.Container+ContainerChild
			this.algnUld = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.algnUld.Name = "algnUld";
			this.algnUld.LeftPadding = ((uint)(3));
			this.algnUld.TopPadding = ((uint)(3));
			this.algnUld.RightPadding = ((uint)(3));
			this.algnUld.BottomPadding = ((uint)(3));
			// Container child algnUld.Gtk.Container+ContainerChild
			this.pnlUld = new global::Gtk.HBox ();
			this.pnlUld.Name = "pnlUld";
			this.pnlUld.Spacing = 6;
			// Container child pnlUld.Gtk.Box+BoxChild
			this.pnlUldText = new global::Gtk.VBox ();
			this.pnlUldText.Name = "pnlUldText";
			this.pnlUldText.Homogeneous = true;
			this.pnlUldText.Spacing = 6;
			// Container child pnlUldText.Gtk.Box+BoxChild
			this.pnlUldTextUsed = new global::Gtk.HBox ();
			this.pnlUldTextUsed.Name = "pnlUldTextUsed";
			this.pnlUldTextUsed.Spacing = 6;
			// Container child pnlUldTextUsed.Gtk.Box+BoxChild
			this.lblUUsed = new global::Gtk.Label ();
			this.lblUUsed.Name = "lblUUsed";
			this.lblUUsed.LabelProp = global::Mono.Unix.Catalog.GetString ("Used:");
			this.pnlUldTextUsed.Add (this.lblUUsed);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.pnlUldTextUsed [this.lblUUsed]));
			w24.Position = 0;
			w24.Expand = false;
			w24.Fill = false;
			// Container child pnlUldTextUsed.Gtk.Box+BoxChild
			this.lblUldUsed = new global::Gtk.Label ();
			this.lblUldUsed.Name = "lblUldUsed";
			this.lblUldUsed.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlUldTextUsed.Add (this.lblUldUsed);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.pnlUldTextUsed [this.lblUldUsed]));
			w25.PackType = ((global::Gtk.PackType)(1));
			w25.Position = 1;
			w25.Expand = false;
			w25.Fill = false;
			this.pnlUldText.Add (this.pnlUldTextUsed);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.pnlUldText [this.pnlUldTextUsed]));
			w26.Position = 0;
			w26.Expand = false;
			w26.Fill = false;
			// Container child pnlUldText.Gtk.Box+BoxChild
			this.pnlUldTextFree = new global::Gtk.HBox ();
			this.pnlUldTextFree.Name = "pnlUldTextFree";
			this.pnlUldTextFree.Spacing = 6;
			// Container child pnlUldTextFree.Gtk.Box+BoxChild
			this.lblUFree = new global::Gtk.Label ();
			this.lblUFree.Name = "lblUFree";
			this.lblUFree.LabelProp = global::Mono.Unix.Catalog.GetString ("Free:");
			this.pnlUldTextFree.Add (this.lblUFree);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.pnlUldTextFree [this.lblUFree]));
			w27.Position = 0;
			w27.Expand = false;
			w27.Fill = false;
			// Container child pnlUldTextFree.Gtk.Box+BoxChild
			this.lblUldFree = new global::Gtk.Label ();
			this.lblUldFree.Name = "lblUldFree";
			this.lblUldFree.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlUldTextFree.Add (this.lblUldFree);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.pnlUldTextFree [this.lblUldFree]));
			w28.PackType = ((global::Gtk.PackType)(1));
			w28.Position = 1;
			w28.Expand = false;
			w28.Fill = false;
			this.pnlUldText.Add (this.pnlUldTextFree);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.pnlUldText [this.pnlUldTextFree]));
			w29.Position = 1;
			w29.Expand = false;
			w29.Fill = false;
			// Container child pnlUldText.Gtk.Box+BoxChild
			this.pnlUldTextLimit = new global::Gtk.HBox ();
			this.pnlUldTextLimit.Name = "pnlUldTextLimit";
			this.pnlUldTextLimit.Spacing = 6;
			// Container child pnlUldTextLimit.Gtk.Box+BoxChild
			this.lblULimit = new global::Gtk.Label ();
			this.lblULimit.Name = "lblULimit";
			this.lblULimit.LabelProp = global::Mono.Unix.Catalog.GetString ("Limit:");
			this.pnlUldTextLimit.Add (this.lblULimit);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.pnlUldTextLimit [this.lblULimit]));
			w30.Position = 0;
			w30.Expand = false;
			w30.Fill = false;
			// Container child pnlUldTextLimit.Gtk.Box+BoxChild
			this.lblUldLimit = new global::Gtk.Label ();
			this.lblUldLimit.Name = "lblUldLimit";
			this.lblUldLimit.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlUldTextLimit.Add (this.lblUldLimit);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.pnlUldTextLimit [this.lblUldLimit]));
			w31.PackType = ((global::Gtk.PackType)(1));
			w31.Position = 1;
			w31.Expand = false;
			w31.Fill = false;
			this.pnlUldText.Add (this.pnlUldTextLimit);
			global::Gtk.Box.BoxChild w32 = ((global::Gtk.Box.BoxChild)(this.pnlUldText [this.pnlUldTextLimit]));
			w32.Position = 2;
			w32.Expand = false;
			w32.Fill = false;
			this.pnlUld.Add (this.pnlUldText);
			global::Gtk.Box.BoxChild w33 = ((global::Gtk.Box.BoxChild)(this.pnlUld [this.pnlUldText]));
			w33.Position = 0;
			w33.Expand = false;
			w33.Fill = false;
			// Container child pnlUld.Gtk.Box+BoxChild
			this.evnUld = new global::Gtk.EventBox ();
			this.evnUld.Name = "evnUld";
			// Container child evnUld.Gtk.Container+ContainerChild
			this.pctUld = new global::Gtk.Image ();
			this.pctUld.TooltipMarkup = "Graph representing your upload usage.";
			this.pctUld.Name = "pctUld";
			this.evnUld.Add (this.pctUld);
			this.pnlUld.Add (this.evnUld);
			global::Gtk.Box.BoxChild w35 = ((global::Gtk.Box.BoxChild)(this.pnlUld [this.evnUld]));
			w35.Position = 1;
			this.algnUld.Add (this.pnlUld);
			this.gbUld.Add (this.algnUld);
			this.lblUld = new global::Gtk.Label ();
			this.lblUld.Name = "lblUld";
			this.lblUld.Xalign = 0F;
			this.lblUld.LabelProp = global::Mono.Unix.Catalog.GetString ("Upload (X%)");
			this.lblUld.UseMarkup = true;
			this.gbUld.LabelWidget = this.lblUld;
			this.pnlWildBlue.Add (this.gbUld);
			global::Gtk.Box.BoxChild w38 = ((global::Gtk.Box.BoxChild)(this.pnlWildBlue [this.gbUld]));
			w38.Position = 1;
			w38.Padding = ((uint)(3));
			this.pnlDisplays.Add (this.pnlWildBlue);
			global::Gtk.Box.BoxChild w39 = ((global::Gtk.Box.BoxChild)(this.pnlDisplays [this.pnlWildBlue]));
			w39.Position = 1;
			// Container child pnlDisplays.Gtk.Box+BoxChild
			this.pnlExede = new global::Gtk.HBox ();
			this.pnlExede.Name = "pnlExede";
			this.pnlExede.Spacing = 6;
			// Container child pnlExede.Gtk.Box+BoxChild
			this.pnlExedeNumbers = new global::Gtk.VBox ();
			this.pnlExedeNumbers.Name = "pnlExedeNumbers";
			this.pnlExedeNumbers.Homogeneous = true;
			this.pnlExedeNumbers.Spacing = 6;
			// Container child pnlExedeNumbers.Gtk.Box+BoxChild
			this.pnlExedeDown = new global::Gtk.HBox ();
			this.pnlExedeDown.Name = "pnlExedeDown";
			this.pnlExedeDown.Spacing = 6;
			// Container child pnlExedeDown.Gtk.Box+BoxChild
			this.lblExedeDown = new global::Gtk.Label ();
			this.lblExedeDown.Name = "lblExedeDown";
			this.lblExedeDown.LabelProp = global::Mono.Unix.Catalog.GetString ("Download:");
			this.pnlExedeDown.Add (this.lblExedeDown);
			global::Gtk.Box.BoxChild w40 = ((global::Gtk.Box.BoxChild)(this.pnlExedeDown [this.lblExedeDown]));
			w40.Position = 0;
			w40.Expand = false;
			w40.Fill = false;
			// Container child pnlExedeDown.Gtk.Box+BoxChild
			this.lblExedeDownVal = new global::Gtk.Label ();
			this.lblExedeDownVal.Name = "lblExedeDownVal";
			this.lblExedeDownVal.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlExedeDown.Add (this.lblExedeDownVal);
			global::Gtk.Box.BoxChild w41 = ((global::Gtk.Box.BoxChild)(this.pnlExedeDown [this.lblExedeDownVal]));
			w41.PackType = ((global::Gtk.PackType)(1));
			w41.Position = 1;
			w41.Expand = false;
			w41.Fill = false;
			this.pnlExedeNumbers.Add (this.pnlExedeDown);
			global::Gtk.Box.BoxChild w42 = ((global::Gtk.Box.BoxChild)(this.pnlExedeNumbers [this.pnlExedeDown]));
			w42.Position = 0;
			w42.Expand = false;
			w42.Fill = false;
			// Container child pnlExedeNumbers.Gtk.Box+BoxChild
			this.pnlExedeUp = new global::Gtk.HBox ();
			this.pnlExedeUp.Name = "pnlExedeUp";
			this.pnlExedeUp.Spacing = 6;
			// Container child pnlExedeUp.Gtk.Box+BoxChild
			this.lblExedeUp = new global::Gtk.Label ();
			this.lblExedeUp.Name = "lblExedeUp";
			this.lblExedeUp.LabelProp = global::Mono.Unix.Catalog.GetString ("Upload:");
			this.pnlExedeUp.Add (this.lblExedeUp);
			global::Gtk.Box.BoxChild w43 = ((global::Gtk.Box.BoxChild)(this.pnlExedeUp [this.lblExedeUp]));
			w43.Position = 0;
			w43.Expand = false;
			w43.Fill = false;
			// Container child pnlExedeUp.Gtk.Box+BoxChild
			this.lblExedeUpVal = new global::Gtk.Label ();
			this.lblExedeUpVal.Name = "lblExedeUpVal";
			this.lblExedeUpVal.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlExedeUp.Add (this.lblExedeUpVal);
			global::Gtk.Box.BoxChild w44 = ((global::Gtk.Box.BoxChild)(this.pnlExedeUp [this.lblExedeUpVal]));
			w44.PackType = ((global::Gtk.PackType)(1));
			w44.Position = 1;
			w44.Expand = false;
			w44.Fill = false;
			this.pnlExedeNumbers.Add (this.pnlExedeUp);
			global::Gtk.Box.BoxChild w45 = ((global::Gtk.Box.BoxChild)(this.pnlExedeNumbers [this.pnlExedeUp]));
			w45.Position = 1;
			w45.Expand = false;
			w45.Fill = false;
			// Container child pnlExedeNumbers.Gtk.Box+BoxChild
			this.pnlExedeUsed = new global::Gtk.HBox ();
			this.pnlExedeUsed.Name = "pnlExedeUsed";
			this.pnlExedeUsed.Spacing = 6;
			// Container child pnlExedeUsed.Gtk.Box+BoxChild
			this.lblExedeTotal = new global::Gtk.Label ();
			this.lblExedeTotal.Name = "lblExedeTotal";
			this.lblExedeTotal.LabelProp = global::Mono.Unix.Catalog.GetString ("Used:");
			this.pnlExedeUsed.Add (this.lblExedeTotal);
			global::Gtk.Box.BoxChild w46 = ((global::Gtk.Box.BoxChild)(this.pnlExedeUsed [this.lblExedeTotal]));
			w46.Position = 0;
			w46.Expand = false;
			w46.Fill = false;
			// Container child pnlExedeUsed.Gtk.Box+BoxChild
			this.lblExedeTotalVal = new global::Gtk.Label ();
			this.lblExedeTotalVal.Name = "lblExedeTotalVal";
			this.lblExedeTotalVal.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlExedeUsed.Add (this.lblExedeTotalVal);
			global::Gtk.Box.BoxChild w47 = ((global::Gtk.Box.BoxChild)(this.pnlExedeUsed [this.lblExedeTotalVal]));
			w47.PackType = ((global::Gtk.PackType)(1));
			w47.Position = 1;
			w47.Expand = false;
			w47.Fill = false;
			this.pnlExedeNumbers.Add (this.pnlExedeUsed);
			global::Gtk.Box.BoxChild w48 = ((global::Gtk.Box.BoxChild)(this.pnlExedeNumbers [this.pnlExedeUsed]));
			w48.Position = 2;
			w48.Expand = false;
			w48.Fill = false;
			// Container child pnlExedeNumbers.Gtk.Box+BoxChild
			this.pnlExedeFree = new global::Gtk.HBox ();
			this.pnlExedeFree.Name = "pnlExedeFree";
			this.pnlExedeFree.Spacing = 6;
			// Container child pnlExedeFree.Gtk.Box+BoxChild
			this.lblExedeRemain = new global::Gtk.Label ();
			this.lblExedeRemain.Name = "lblExedeRemain";
			this.lblExedeRemain.LabelProp = global::Mono.Unix.Catalog.GetString ("Free:");
			this.pnlExedeFree.Add (this.lblExedeRemain);
			global::Gtk.Box.BoxChild w49 = ((global::Gtk.Box.BoxChild)(this.pnlExedeFree [this.lblExedeRemain]));
			w49.Position = 0;
			w49.Expand = false;
			w49.Fill = false;
			// Container child pnlExedeFree.Gtk.Box+BoxChild
			this.lblExedeRemainVal = new global::Gtk.Label ();
			this.lblExedeRemainVal.Name = "lblExedeRemainVal";
			this.lblExedeRemainVal.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlExedeFree.Add (this.lblExedeRemainVal);
			global::Gtk.Box.BoxChild w50 = ((global::Gtk.Box.BoxChild)(this.pnlExedeFree [this.lblExedeRemainVal]));
			w50.PackType = ((global::Gtk.PackType)(1));
			w50.Position = 1;
			w50.Expand = false;
			w50.Fill = false;
			this.pnlExedeNumbers.Add (this.pnlExedeFree);
			global::Gtk.Box.BoxChild w51 = ((global::Gtk.Box.BoxChild)(this.pnlExedeNumbers [this.pnlExedeFree]));
			w51.Position = 3;
			w51.Expand = false;
			w51.Fill = false;
			// Container child pnlExedeNumbers.Gtk.Box+BoxChild
			this.pnlExedeTotal = new global::Gtk.HBox ();
			this.pnlExedeTotal.Name = "pnlExedeTotal";
			this.pnlExedeTotal.Spacing = 6;
			// Container child pnlExedeTotal.Gtk.Box+BoxChild
			this.lblExedeAllowed = new global::Gtk.Label ();
			this.lblExedeAllowed.Name = "lblExedeAllowed";
			this.lblExedeAllowed.LabelProp = global::Mono.Unix.Catalog.GetString ("Limit:");
			this.pnlExedeTotal.Add (this.lblExedeAllowed);
			global::Gtk.Box.BoxChild w52 = ((global::Gtk.Box.BoxChild)(this.pnlExedeTotal [this.lblExedeAllowed]));
			w52.Position = 0;
			w52.Expand = false;
			w52.Fill = false;
			// Container child pnlExedeTotal.Gtk.Box+BoxChild
			this.lblExedeAllowedVal = new global::Gtk.Label ();
			this.lblExedeAllowedVal.Name = "lblExedeAllowedVal";
			this.lblExedeAllowedVal.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlExedeTotal.Add (this.lblExedeAllowedVal);
			global::Gtk.Box.BoxChild w53 = ((global::Gtk.Box.BoxChild)(this.pnlExedeTotal [this.lblExedeAllowedVal]));
			w53.PackType = ((global::Gtk.PackType)(1));
			w53.Position = 1;
			w53.Expand = false;
			w53.Fill = false;
			this.pnlExedeNumbers.Add (this.pnlExedeTotal);
			global::Gtk.Box.BoxChild w54 = ((global::Gtk.Box.BoxChild)(this.pnlExedeNumbers [this.pnlExedeTotal]));
			w54.Position = 4;
			w54.Expand = false;
			w54.Fill = false;
			this.pnlExede.Add (this.pnlExedeNumbers);
			global::Gtk.Box.BoxChild w55 = ((global::Gtk.Box.BoxChild)(this.pnlExede [this.pnlExedeNumbers]));
			w55.Position = 0;
			w55.Expand = false;
			w55.Fill = false;
			// Container child pnlExede.Gtk.Box+BoxChild
			this.evnExede = new global::Gtk.EventBox ();
			this.evnExede.Name = "evnExede";
			// Container child evnExede.Gtk.Container+ContainerChild
			this.pctExede = new global::Gtk.Image ();
			this.pctExede.TooltipMarkup = "Graph representing your bandwidth usage.";
			this.pctExede.Name = "pctExede";
			this.evnExede.Add (this.pctExede);
			this.pnlExede.Add (this.evnExede);
			global::Gtk.Box.BoxChild w57 = ((global::Gtk.Box.BoxChild)(this.pnlExede [this.evnExede]));
			w57.Position = 1;
			this.pnlDisplays.Add (this.pnlExede);
			global::Gtk.Box.BoxChild w58 = ((global::Gtk.Box.BoxChild)(this.pnlDisplays [this.pnlExede]));
			w58.Position = 2;
			// Container child pnlDisplays.Gtk.Box+BoxChild
			this.pnlRural = new global::Gtk.HBox ();
			this.pnlRural.Name = "pnlRural";
			this.pnlRural.Spacing = 6;
			// Container child pnlRural.Gtk.Box+BoxChild
			this.pnlRuralNumbers = new global::Gtk.VBox ();
			this.pnlRuralNumbers.Name = "pnlRuralNumbers";
			this.pnlRuralNumbers.Homogeneous = true;
			this.pnlRuralNumbers.Spacing = 6;
			// Container child pnlRuralNumbers.Gtk.Box+BoxChild
			this.pnlRuralUsed = new global::Gtk.HBox ();
			this.pnlRuralUsed.Name = "pnlRuralUsed";
			this.pnlRuralUsed.Spacing = 6;
			// Container child pnlRuralUsed.Gtk.Box+BoxChild
			this.lblRuralUsed = new global::Gtk.Label ();
			this.lblRuralUsed.Name = "lblRuralUsed";
			this.lblRuralUsed.LabelProp = global::Mono.Unix.Catalog.GetString ("Used:");
			this.pnlRuralUsed.Add (this.lblRuralUsed);
			global::Gtk.Box.BoxChild w59 = ((global::Gtk.Box.BoxChild)(this.pnlRuralUsed [this.lblRuralUsed]));
			w59.Position = 0;
			w59.Expand = false;
			w59.Fill = false;
			// Container child pnlRuralUsed.Gtk.Box+BoxChild
			this.lblRuralUsedVal = new global::Gtk.Label ();
			this.lblRuralUsedVal.Name = "lblRuralUsedVal";
			this.lblRuralUsedVal.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlRuralUsed.Add (this.lblRuralUsedVal);
			global::Gtk.Box.BoxChild w60 = ((global::Gtk.Box.BoxChild)(this.pnlRuralUsed [this.lblRuralUsedVal]));
			w60.PackType = ((global::Gtk.PackType)(1));
			w60.Position = 1;
			w60.Expand = false;
			w60.Fill = false;
			this.pnlRuralNumbers.Add (this.pnlRuralUsed);
			global::Gtk.Box.BoxChild w61 = ((global::Gtk.Box.BoxChild)(this.pnlRuralNumbers [this.pnlRuralUsed]));
			w61.Position = 0;
			w61.Expand = false;
			w61.Fill = false;
			// Container child pnlRuralNumbers.Gtk.Box+BoxChild
			this.pnlRuralFree = new global::Gtk.HBox ();
			this.pnlRuralFree.Name = "pnlRuralFree";
			this.pnlRuralFree.Spacing = 6;
			// Container child pnlRuralFree.Gtk.Box+BoxChild
			this.lblRuralRemain = new global::Gtk.Label ();
			this.lblRuralRemain.Name = "lblRuralRemain";
			this.lblRuralRemain.LabelProp = global::Mono.Unix.Catalog.GetString ("Free:");
			this.pnlRuralFree.Add (this.lblRuralRemain);
			global::Gtk.Box.BoxChild w62 = ((global::Gtk.Box.BoxChild)(this.pnlRuralFree [this.lblRuralRemain]));
			w62.Position = 0;
			w62.Expand = false;
			w62.Fill = false;
			// Container child pnlRuralFree.Gtk.Box+BoxChild
			this.lblRuralRemainVal = new global::Gtk.Label ();
			this.lblRuralRemainVal.Name = "lblRuralRemainVal";
			this.lblRuralRemainVal.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlRuralFree.Add (this.lblRuralRemainVal);
			global::Gtk.Box.BoxChild w63 = ((global::Gtk.Box.BoxChild)(this.pnlRuralFree [this.lblRuralRemainVal]));
			w63.PackType = ((global::Gtk.PackType)(1));
			w63.Position = 1;
			w63.Expand = false;
			w63.Fill = false;
			this.pnlRuralNumbers.Add (this.pnlRuralFree);
			global::Gtk.Box.BoxChild w64 = ((global::Gtk.Box.BoxChild)(this.pnlRuralNumbers [this.pnlRuralFree]));
			w64.Position = 1;
			w64.Expand = false;
			w64.Fill = false;
			// Container child pnlRuralNumbers.Gtk.Box+BoxChild
			this.pnlRuralTotal = new global::Gtk.HBox ();
			this.pnlRuralTotal.Name = "pnlRuralTotal";
			this.pnlRuralTotal.Spacing = 6;
			// Container child pnlRuralTotal.Gtk.Box+BoxChild
			this.lblRuralAllowed = new global::Gtk.Label ();
			this.lblRuralAllowed.Name = "lblRuralAllowed";
			this.lblRuralAllowed.LabelProp = global::Mono.Unix.Catalog.GetString ("Limit:");
			this.pnlRuralTotal.Add (this.lblRuralAllowed);
			global::Gtk.Box.BoxChild w65 = ((global::Gtk.Box.BoxChild)(this.pnlRuralTotal [this.lblRuralAllowed]));
			w65.Position = 0;
			w65.Expand = false;
			w65.Fill = false;
			// Container child pnlRuralTotal.Gtk.Box+BoxChild
			this.lblRuralAllowedVal = new global::Gtk.Label ();
			this.lblRuralAllowedVal.Name = "lblRuralAllowedVal";
			this.lblRuralAllowedVal.LabelProp = global::Mono.Unix.Catalog.GetString (" -- ");
			this.pnlRuralTotal.Add (this.lblRuralAllowedVal);
			global::Gtk.Box.BoxChild w66 = ((global::Gtk.Box.BoxChild)(this.pnlRuralTotal [this.lblRuralAllowedVal]));
			w66.PackType = ((global::Gtk.PackType)(1));
			w66.Position = 1;
			w66.Expand = false;
			w66.Fill = false;
			this.pnlRuralNumbers.Add (this.pnlRuralTotal);
			global::Gtk.Box.BoxChild w67 = ((global::Gtk.Box.BoxChild)(this.pnlRuralNumbers [this.pnlRuralTotal]));
			w67.Position = 2;
			w67.Expand = false;
			w67.Fill = false;
			this.pnlRural.Add (this.pnlRuralNumbers);
			global::Gtk.Box.BoxChild w68 = ((global::Gtk.Box.BoxChild)(this.pnlRural [this.pnlRuralNumbers]));
			w68.Position = 0;
			w68.Expand = false;
			w68.Fill = false;
			// Container child pnlRural.Gtk.Box+BoxChild
			this.evnRural = new global::Gtk.EventBox ();
			this.evnRural.Name = "evnRural";
			// Container child evnRural.Gtk.Container+ContainerChild
			this.pctRural = new global::Gtk.Image ();
			this.pctRural.TooltipMarkup = "Graph representing your bandwidth usage.";
			this.pctRural.Name = "pctRural";
			this.evnRural.Add (this.pctRural);
			this.pnlRural.Add (this.evnRural);
			global::Gtk.Box.BoxChild w70 = ((global::Gtk.Box.BoxChild)(this.pnlRural [this.evnRural]));
			w70.Position = 1;
			this.pnlDisplays.Add (this.pnlRural);
			global::Gtk.Box.BoxChild w71 = ((global::Gtk.Box.BoxChild)(this.pnlDisplays [this.pnlRural]));
			w71.Position = 3;
			this.algnUsage.Add (this.pnlDisplays);
			this.gbUsage.Add (this.algnUsage);
			this.lblStatus = new global::Gtk.Label ();
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Xalign = 0F;
			this.lblStatus.LabelProp = global::Mono.Unix.Catalog.GetString ("Usage Levels");
			this.lblStatus.UseMarkup = true;
			this.gbUsage.LabelWidget = this.lblStatus;
			this.pnlDetails.Add (this.gbUsage);
			global::Gtk.Box.BoxChild w74 = ((global::Gtk.Box.BoxChild)(this.pnlDetails [this.gbUsage]));
			w74.Position = 1;
			w74.Padding = ((uint)(3));
			// Container child pnlDetails.Gtk.Box+BoxChild
			this.sbMainStatus = new global::Gtk.Statusbar ();
			this.sbMainStatus.Name = "sbMainStatus";
			this.sbMainStatus.Spacing = 6;
			// Container child sbMainStatus.Gtk.Box+BoxChild
			this.pbMainStatus = new global::Gtk.ProgressBar ();
			this.pbMainStatus.Name = "pbMainStatus";
			this.sbMainStatus.Add (this.pbMainStatus);
			global::Gtk.Box.BoxChild w75 = ((global::Gtk.Box.BoxChild)(this.sbMainStatus [this.pbMainStatus]));
			w75.Position = 1;
			// Container child sbMainStatus.Gtk.Box+BoxChild
			this.cmdNetTest = new global::Gtk.Button ();
			this.cmdNetTest.CanFocus = true;
			this.cmdNetTest.Name = "cmdNetTest";
			this.cmdNetTest.UseUnderline = true;
			this.cmdNetTest.Relief = ((global::Gtk.ReliefStyle)(2));
			global::Gtk.Image w76 = new global::Gtk.Image ();
			this.cmdNetTest.Image = w76;
			this.sbMainStatus.Add (this.cmdNetTest);
			global::Gtk.Box.BoxChild w77 = ((global::Gtk.Box.BoxChild)(this.sbMainStatus [this.cmdNetTest]));
			w77.Position = 2;
			w77.Expand = false;
			w77.Fill = false;
			// Container child sbMainStatus.Gtk.Box+BoxChild
			this.lblMainStatus = new global::Gtk.Label ();
			this.lblMainStatus.Name = "lblMainStatus";
			this.lblMainStatus.SingleLineMode = true;
			this.sbMainStatus.Add (this.lblMainStatus);
			global::Gtk.Box.BoxChild w78 = ((global::Gtk.Box.BoxChild)(this.sbMainStatus [this.lblMainStatus]));
			w78.Position = 3;
			w78.Expand = false;
			w78.Fill = false;
			this.pnlDetails.Add (this.sbMainStatus);
			global::Gtk.Box.BoxChild w79 = ((global::Gtk.Box.BoxChild)(this.pnlDetails [this.sbMainStatus]));
			w79.Position = 2;
			w79.Expand = false;
			w79.Fill = false;
			this.Add (this.pnlDetails);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 455;
			this.DefaultHeight = 454;
			this.Show ();
			this.cmdNetTest.Clicked += new global::System.EventHandler (this.cmdNetTest_Clicked);
		}
	}
}
