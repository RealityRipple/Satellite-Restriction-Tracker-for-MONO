using System;
using System.Collections;
using System.Runtime.InteropServices;
namespace MacInterop
{
  public class GtkOSXApplication
  {
    [DllImport("libigemacintegration.dylib")]
    static extern void gtk_osxapplication_set_menu_bar(IntPtr app, IntPtr menu_shell);
    public static Gtk.MenuShell MenuBar
    {
      set
      {
        gtk_osxapplication_set_menu_bar(IntPtr.Zero, value == null ? IntPtr.Zero : value.Handle);
      }
    }
    [DllImport("libigemacintegration.dylib")]
    static extern IntPtr gtk_osxapplication_add_app_menu_group(IntPtr app);
    public static GtkMacMenuGroup AddAppMenuGroup()
    {
      IntPtr raw_ret = gtk_osxapplication_add_app_menu_group(IntPtr.Zero);
      GtkMacMenuGroup ret = raw_ret == IntPtr.Zero ? null : (GtkMacMenuGroup)GLib.Opaque.GetOpaque(raw_ret, typeof(GtkMacMenuGroup), false);
      return ret;
    }
  }
  public class GtkMacMenuGroup: GLib.Opaque
  {
    [DllImport("libigemacintegration.dylib")]
    static extern void gtk_osxapplication_add_app_menu_item(IntPtr app, IntPtr raw, IntPtr menu_item);
    public void AddMenuItem(Gtk.MenuItem menu_item)
    {
      gtk_osxapplication_add_app_menu_item(IntPtr.Zero, Handle, menu_item == null ? IntPtr.Zero : menu_item.Handle);
    }
    public GtkMacMenuGroup(IntPtr raw) :
      base(raw)
    {
    }
  }
}
