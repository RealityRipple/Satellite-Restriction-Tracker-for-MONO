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
    public static string HistoryPath
    {
      get
      {
        return sFile;
      }
    }
    public static void LOG_Add(System.DateTime dTime, long lDown, long lDownLim, long lUp, long lUpLim, bool Save = true)
    {
      if (!isLoaded)
      {
        return;
      }
      if (Math.Abs(LOG_GetLast().Subtract(dTime).TotalMinutes) >= HistoryAge)
      {
        if (lDownLim > 0 | lUpLim > 0)
        {
          if (usageDB == null)
          {
            usageDB = new DataBase();
          }
          usageDB.Add(new DataBase.DataRow(dTime, lDown, lDownLim, lUp, lUpLim));
          if (Save)
          {
            LOG_Sort();
            System.Threading.Thread tX = new System.Threading.Thread(new System.Threading.ThreadStart(LOG_Save));
            tX.Start();
          }
        }
      }
    }
    public static void LOG_Get(long lngIndex, out System.DateTime dtDate, out long lngDown, out long lngDownLim, out long lngUp, out long lngUpLim)
    {
      if (isLoaded)
      {
        if (LOG_GetCount() > lngIndex)
        {
          DataBase.DataRow dbRow = usageDB.ToArray()[lngIndex];
          dtDate = dbRow.DATETIME;
          lngDown = dbRow.DOWNLOAD;
          lngDownLim = dbRow.DOWNLIM;
          lngUp = dbRow.UPLOAD;
          lngUpLim = dbRow.UPLIM;
          return;
        }
      }
      dtDate = new DateTime(1970, 1, 1);
      lngDown = 0;
      lngDownLim = 0;
      lngUp = 0;
      lngUpLim = 0;
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
      if (LOG_GetCount() > 0)
      {
        return usageDB.ToArray()[LOG_GetCount() - 1].DATETIME;
      }
      else
      {
        return new System.DateTime(1970, 1, 1);
      }
    }
    public static void LOG_Initialize(string sAccount, bool withDisplay)
    {
      isLoaded = false;
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
          usageDB.StopNew = true;
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
    public static void LOG_Sort()
    {
      if (!isLoaded)
      {
        return;
      }
      while (isSaving)
      {
        System.Threading.Thread.Sleep(0);
        System.Threading.Thread.Sleep(100);
      }
      if (usageDB != null)
      {
        usageDB.Sort();
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
    private static long FileLen(string Path)
    {
      if (File.Exists(Path))
      {
        return new FileInfo(Path).Length;
      }
      return -1;
    }
    private static void usageDB_ProgressState(object o, DataBase.ProgressStateEventArgs e)
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
