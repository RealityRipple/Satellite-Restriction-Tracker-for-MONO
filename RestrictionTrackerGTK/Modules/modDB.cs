using System;
using System.IO;
using RestrictionLibrary;
namespace RestrictionTrackerGTK
{
  static class modDB
  {
    private const long HistoryAge = 1;
    private static string sFile;
    private static DataBase withEventsField_usageDB;
    static internal DataBase usageDB
    {
      get
      {
        return withEventsField_usageDB;
      }
      set
      {
        if (withEventsField_usageDB != null)
        {
          withEventsField_usageDB.ProgressState -= usageDB_ProgressState;
        }
        withEventsField_usageDB = value;
        if (withEventsField_usageDB != null)
        {
          withEventsField_usageDB.ProgressState += usageDB_ProgressState;
        }
      }
    }
    private static bool isLoaded;
    private static bool isSaving;
    public static byte LOG_State
    {
      get
      {
        if (!isLoaded)
        {
          return 0;
        }
        if (isSaving)
        {
          return 2;
        }
        return 1;
      }
    }
    public static void LOG_Add(System.DateTime dTime, long lUsed, long lLimit, bool Save = true)
    {
      if (!isLoaded)
      {
        return;
      }
      if (lLimit <= 0)
      {
        return;
      }
      if (usageDB == null)
      {
        usageDB = new DataBase();
      }
      usageDB.Add(new DataRow(dTime, lUsed, lLimit));
      if (Save)
      {
        System.Threading.Thread tX = new System.Threading.Thread(new System.Threading.ThreadStart(LOG_Save));
        tX.Start();
      }
    }
    public static void LOG_Get(long lngIndex, out System.DateTime dtDate, out long lngUsed, out long lngLimit)
    {
      if (isLoaded)
      {
        if (LOG_GetCount() > lngIndex)
        {
          DataRow[] dArr = usageDB.ToArray();
          DataRow dbRow = dArr[lngIndex];
          dtDate = dbRow.DATETIME;
          lngUsed = dbRow.USED;
          lngLimit = dbRow.LIMIT;
          return;
        }
      }
      dtDate = new DateTime(1970, 1, 1);
      lngUsed = 0;
      lngLimit = 0;
    }
    public static DataRow[] LOG_GetRange(System.DateTime dtStart, System.DateTime dtEnd)
    {
      System.Collections.Generic.List<DataRow> lRet = new System.Collections.Generic.List<DataRow>();
      if (!isLoaded)
      {
        return lRet.ToArray();
      }
      UInt64 kStart = (UInt64)Math.Floor((double)(dtStart.Ticks / 600000000));
      UInt64 kEnd = (UInt64)Math.Floor((double)(dtEnd.Ticks / 600000000));
      foreach (System.Collections.Generic.KeyValuePair<UInt64, DataRow> dRow in usageDB)
      {
        if (dRow.Key >= kStart && dRow.Key <= kEnd)
        {
          lRet.Add(dRow.Value);
        }
        if (dRow.Key > kEnd)
        {
          break;
        }
      }
      return lRet.ToArray();
    }
    public static int LOG_GetCount()
    {
      if (!isLoaded)
      {
        return 0;
      }
      if (usageDB == null)
      {
        return 0;
      }
      return usageDB.Count;
    }
    public static System.DateTime LOG_GetLast()
    {
      if (!isLoaded)
      {
        return new System.DateTime(1970, 1, 1);
      }
      if (LOG_GetCount() < 1)
      {
        return new System.DateTime(1970, 1, 1);
      }
      return usageDB.LastRow.DATETIME;
    }
    public static void LOG_Initialize(string sAccount, bool withDisplay)
    {
      isLoaded = false;
      if (! File.Exists(Path.Combine(modFunctions.MySaveDir(false), "History-" + sAccount + ".wb")) && File.Exists(Path.Combine(modFunctions.MySaveDir(false), "History-" + sAccount + "@exede.net.wb")))
      {
        try
        {
          File.Move(Path.Combine(modFunctions.MySaveDir(false), "History-" + sAccount + "@exede.net.wb"), Path.Combine(modFunctions.MySaveDir(false), "History-" + sAccount + ".wb"));
        }
        catch(Exception)
        {
          modFunctions.ShowMessageBox(null, "Your history file could not be renamed because another program is using it!", "File in Use", 0, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
        }
      }
      sFile = Path.Combine(modFunctions.MySaveDir(true), "History-" + sAccount + ".wb");
      if (!File.Exists(sFile))
      {
        sFile = Path.Combine(modFunctions.MySaveDir(true), "History-" + sAccount + ".xml");
      }
      if (File.Exists(sFile))
      {
        usageDB = new DataBase(sFile, withDisplay);
        usageDB.StartNew();
        if (sFile.CompareTo(Path.Combine(modFunctions.MySaveDir(true), "History-" + sAccount + ".xml")) == 0)
        {
          sFile = Path.Combine(modFunctions.MySaveDir(true), "History-" + sAccount + ".wb");
          usageDB.Save(sFile, withDisplay);
          if (srlFunctions.InUseChecker(Path.Combine(modFunctions.MySaveDir(true), "History-" + sAccount + ".xml"), FileAccess.Write))
          {
            File.Delete(Path.Combine(modFunctions.MySaveDir(true), "History-" + sAccount + ".xml"));
          }
        }
      }
      else
      {
        if (sFile.CompareTo(Path.Combine(modFunctions.MySaveDir(true), "History-" + sAccount + ".xml")) == 0)
        {
          sFile = Path.Combine(modFunctions.MySaveDir(true), "History-" + sAccount + ".wb");
        }
      }
      isLoaded = true;
    }
    public static void LOG_Terminate(bool withSave)
    {
      if (!isLoaded)
      {
        if (usageDB != null)
        {
          usageDB.StopNew();
        }
        return;
      }
      while (isSaving)
      {
        System.Threading.Thread.Sleep(0);
        System.Threading.Thread.Sleep(100);
      }
      if (usageDB != null)
      {
        if (withSave)
        {
          LOG_Save(false);
        }
        usageDB = null;
      }
    }
    static internal void LOG_Save()
    {
      LOG_Save(false);
    }
    static internal void LOG_Save(bool withDisplay)
    {
      if (!isLoaded)
        return;
      if (!string.IsNullOrEmpty(sFile))
      {
        isSaving = true;
        if (!Directory.Exists(Path.GetDirectoryName(sFile)))
          Directory.CreateDirectory(Path.GetDirectoryName(sFile));
        if (srlFunctions.InUseChecker(sFile, FileAccess.Write))
        {
          usageDB.Save(sFile, withDisplay);
        }
        else
        {
          modFunctions.ShowMessageBox(null, "Your history file could not be saved because another program is using it!", "History Log in Use", Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok);
        }
        isSaving = false;
      }
    }
    private static void usageDB_ProgressState(object o, DataBaseProgressEventArgs e)
    {
      if (MainClass.fHistory != null)
      {
        MainClass.fHistory.SetProgress(e.Value, e.Total, "");
      }
      else
      {
        MainClass.fMain.SetProgress(e.Value, e.Total, "");
      }
    }
  }
}
