using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using RestrictionLibrary;

namespace RestrictionTrackerGTK
{
  static class modFunctions
  {
    public enum DateInterval
    {
      Day,
      DayOfYear,
      Hour,
      Minute,
      Month,
      Quarter,
      Second,
      Weekday,
      WeekOfYear,
      Year
    }

    #region "Alert Notifier"
    public static NotifierStyle NOTIFIER_STYLE = null;
    public static NotifierStyle LoadAlertStyle(string Path)
    {
      if (File.Exists(AppData + System.IO.Path.DirectorySeparatorChar + Path + ".tgz"))
      {
        Path = AppData + System.IO.Path.DirectorySeparatorChar + Path + ".tgz";
      }
      else if (File.Exists(AppData + System.IO.Path.DirectorySeparatorChar + Path + ".tar.gz"))
      {
        Path = AppData + System.IO.Path.DirectorySeparatorChar + Path + ".tar.gz";
      }
      else
      {
        return new NotifierStyle();
      }
      try
      {
        string TempAlertDir = AppData + "/notifier/";
        string TempAlertTAR = AppData + "/notifier.tar";
        ExtractGZ(Path, TempAlertTAR);
        ExtractTar(TempAlertTAR, TempAlertDir);
        File.Delete(TempAlertTAR);
        NotifierStyle ns = new NotifierStyle(TempAlertDir + "alert.png", TempAlertDir + "close.png", TempAlertDir + "loc");
        Directory.Delete(TempAlertDir, true);
        return ns;
      }
      catch (Exception)
      {
        return new NotifierStyle();
      }
    }

    private static void ExtractGZ(string sGZ, string sDestFile)
    {
      using (FileStream sourceTGZ = new FileStream(sGZ, FileMode.Open, FileAccess.Read,FileShare.Read))
      {
        using (GZipStream sourceGZ = new GZipStream(sourceTGZ,CompressionMode.Decompress))
        {
          using (FileStream destTAR = File.Create (sDestFile))
          {
            byte[] buffer = new byte[4095];
            int numRead = sourceGZ.Read(buffer, 0, buffer.Length);
            while (numRead != 0)
            {
              destTAR.Write(buffer, 0, numRead);
              numRead = sourceGZ.Read(buffer, 0, buffer.Length);
            }
            destTAR.Flush(true);
            destTAR.Close();
          }
        }
      }
    }

    private static void ExtractTar(string sTAR, string sDestPath)
    {
      if (!Directory.Exists(sDestPath))
      {
        Directory.CreateDirectory(sDestPath);
      }
      if (!sDestPath.EndsWith("" + System.IO.Path.DirectorySeparatorChar))
      {
        sDestPath += System.IO.Path.DirectorySeparatorChar;
      }
      using (FileStream sourceTAR = new FileStream(sTAR, FileMode.Open,FileAccess.Read,FileShare.Read))
      {
        using (BinaryReader binTar = new BinaryReader(sourceTAR))
        {
          while (binTar.BaseStream.Position < binTar.BaseStream.Length)
          {
            TarFileData tarFile = new TarFileData(binTar);
            if (!string.IsNullOrEmpty(tarFile.FileName))
            {
              if (tarFile.LinkIndicator == 0)
              {
                File.WriteAllBytes(sDestPath + tarFile.FileName, tarFile.FileData);
                File.SetLastWriteTime(sDestPath + tarFile.FileName, new DateTime(1970, 1, 1).AddSeconds(tarFile.LastMod));
              }
            }
          }
        }
      }
    }

    public static void MakeNotifier(ref TaskbarNotifier taskNotifier, bool ContentClickable)
    {
      if (NOTIFIER_STYLE.Background == null || NOTIFIER_STYLE.CloseButton == null)
      {
        taskNotifier = null;
        return;
      }
      taskNotifier = new TaskbarNotifier();
      taskNotifier.TitleClickable = false;
      taskNotifier.ContentClickable = ContentClickable;
      taskNotifier.TitleRectangle = NOTIFIER_STYLE.TitleLocation;
      taskNotifier.ContentRectangle = NOTIFIER_STYLE.ContentLocation;
      taskNotifier.SetBackgroundBitmap(NOTIFIER_STYLE.Background, NOTIFIER_STYLE.TransparencyKey);
      taskNotifier.SetCloseBitmap(NOTIFIER_STYLE.CloseButton, NOTIFIER_STYLE.TransparencyKey, NOTIFIER_STYLE.CloseLocation);
      taskNotifier.NormalTitleColor = NOTIFIER_STYLE.TitleColor;
      taskNotifier.NormalContentColor = NOTIFIER_STYLE.ContentColor;
      taskNotifier.HoverContentColor = NOTIFIER_STYLE.ContentHoverColor;
    }

    public static void MakeNotifier(ref TaskbarNotifier taskNotifier, bool ContentClickable, NotifierStyle customStyle)
    {
      if (customStyle.Background == null || customStyle.CloseButton == null)
      {
        taskNotifier = null;
        return;
      }
      taskNotifier = new TaskbarNotifier();
      taskNotifier.TitleClickable = false;
      taskNotifier.ContentClickable = ContentClickable;
      taskNotifier.TitleRectangle = customStyle.TitleLocation;
      taskNotifier.ContentRectangle = customStyle.ContentLocation;
      taskNotifier.SetBackgroundBitmap(customStyle.Background, customStyle.TransparencyKey);
      taskNotifier.SetCloseBitmap(customStyle.CloseButton, customStyle.TransparencyKey, customStyle.CloseLocation);
      taskNotifier.NormalTitleColor = customStyle.TitleColor;
      taskNotifier.NormalContentColor = customStyle.ContentColor;
      taskNotifier.HoverContentColor = customStyle.ContentHoverColor;
    }
    #endregion 

    static string static_AppData_sTmp;

    public static string AppData
    {
      get
      {
        if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar.ToString() + CompanyName()))
        {
          System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar.ToString() + CompanyName());
        }
        if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar.ToString() + CompanyName() + System.IO.Path.DirectorySeparatorChar.ToString() + ProductName()))
        {
          System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar.ToString() + CompanyName() + System.IO.Path.DirectorySeparatorChar.ToString() + ProductName());
        }
        if (string.IsNullOrEmpty(static_AppData_sTmp))
        {
          static_AppData_sTmp = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar.ToString() + CompanyName() + System.IO.Path.DirectorySeparatorChar.ToString() + ProductName();
        }
        return static_AppData_sTmp;
      }
    }

    public static string MySaveDir
    {
      get
      {
        AppSettings mySettings = new AppSettings();
        if (string.IsNullOrEmpty(mySettings.HistoryDir))
        {
          mySettings.HistoryDir = AppData;
        }
        if (!System.IO.Directory.Exists(mySettings.HistoryDir))
        {
          System.IO.Directory.CreateDirectory(mySettings.HistoryDir);
        }
        return mySettings.HistoryDir;
      }
    }

    public static string ByteSize(UInt64 InBytes)
    {
      if (InBytes >= 1000)
      {
        if (InBytes / 1024 >= 1000)
        {
          if (InBytes / 1024 / 1024 >= 1000)
          {
            if (InBytes / 1024 / 1024 / 1024 >= 1000)
            {
              return string.Format("{0,0:0.##}", (InBytes) / 1024 / 1024 / 1024 / 1024) + " TB";
            }
            else
            {
              return string.Format("{0,0:0.##}", (InBytes) / 1024 / 1024 / 1024) + " GB";
            }
          }
          else
          {
            return string.Format("{0,0:0.##}", (InBytes) / 1024 / 1024) + " MB";
          }
        }
        else
        {
          return string.Format("{0,0:0.#}", (InBytes) / 1024) + " KB";
        }
      }
      else
      {
        return InBytes.ToString() + " B";
      }
    }

    public static localRestrictionTracker.SatHostTypes StringToHostType(string st)
    {
      switch (st.ToUpper())
      {
        case "WBL":
        case "WILDBLUE":
          return localRestrictionTracker.SatHostTypes.WildBlue_LEGACY;
        case "WBX":
        case "EXEDE":
          return localRestrictionTracker.SatHostTypes.WildBlue_EXEDE;
        case "WBV":
          return localRestrictionTracker.SatHostTypes.WildBlue_EVOLUTION;
        case "RPL":
          return localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY;
        case "RPX":
        case "RURALPORTAL":
          return localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE;
        case "DNX":
        case "DISHNET":
          return localRestrictionTracker.SatHostTypes.DishNet_EXEDE;
        default:
          return localRestrictionTracker.SatHostTypes.Other ;
      }
    }

    public static string HostTypeToString(localRestrictionTracker.SatHostTypes ht)
    {
      switch (ht)
      {
        case  localRestrictionTracker.SatHostTypes.WildBlue_LEGACY:
          return "WBL";
        case localRestrictionTracker.SatHostTypes.WildBlue_EXEDE:
          return "WBX";
        case localRestrictionTracker.SatHostTypes.WildBlue_EVOLUTION:
          return "WBV";
        case localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY:
          return "RPL";
        case localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE:
          return "RPX";
        case localRestrictionTracker.SatHostTypes.DishNet_EXEDE:
          return "DNX";
        default:
          return "O";
      }
    }

    public static string ConvertTime(UInt64 lngMS, bool Abbreviated = false, bool Trimmed = true)
    {
      UInt64 lngSeconds = lngMS / 1000;
      UInt64 lngWeeks = lngSeconds / (60 * 60 * 24 * 7);
      lngSeconds = lngSeconds % (60 * 60 * 24 * 7);
      UInt64 lngDays = lngSeconds / (60 * 60 * 24);
      lngSeconds = lngSeconds % (60 * 60 * 24);
      UInt64 lngHours = lngSeconds / (60 * 60);
      lngSeconds = lngSeconds % (60 * 60);
      UInt64 lngMins = lngSeconds / 60;
      lngSeconds = lngSeconds % 60;
      if (Abbreviated)
      {
        if (Trimmed)
        {
          if (lngWeeks > 0)
          {
            return lngWeeks + "w " + lngDays + "d";
          }
          else if (lngDays > 0)
          {
            if (lngHours > 20)
            {
              if (lngDays >= 6)
              {
                return "1 w";
              }
              else
              {
                return lngDays + 1 + " d";
              }
            }
            else
            {
              return lngDays + (lngHours > 14 ? "¾ d" : (lngHours > 8 ? "½ d" : (lngHours > 2 ? "¼ d" : " d")));
            }
          }
          else if (lngHours > 0)
          {
            if (lngHours >= 22 | (lngHours == 21 & lngMins > 50))
            {
              return "1 d";
            }
            else if (lngMins > 50)
            {
              return lngHours + 1 + " h";
            }
            else
            {
              return lngHours + (lngMins > 35 ? "¾ h" : (lngMins > 20 ? "½ h" : (lngMins > 5 ? "¼ h" : " h")));
            }
          }
          else if (lngMins > 0)
          {
            if (lngMins >= 55 | (lngMins == 54 & lngSeconds > 50))
            {
              return "1 h";
            }
            else if (lngSeconds > 50)
            {
              return lngMins + 1 + " m";
            }
            else
            {
              return lngMins + (lngSeconds > 35 ? "¾ m" : (lngSeconds > 20 ? "½ m" : (lngSeconds > 5 ? "¼ m" : " m")));
            }
          }
          else
          {
            if (lngSeconds > 55)
            {
              return "1 m";
            }
            else
            {
              return lngSeconds + "s";
            }
          }
        }
        else
        {
          if (lngWeeks > 0)
          {
            return lngWeeks + "w " + lngDays + "d " + lngHours + "h " + lngMins + "m " + lngSeconds + "s";
          }
          else if (lngDays > 0)
          {
            return lngDays + "d " + lngHours + "h " + lngMins + "m " + lngSeconds + "s";
          }
          else if (lngHours > 0)
          {
            return lngHours + "h " + lngMins + "m " + lngSeconds + "s";
          }
          else if (lngMins > 0)
          {
            return lngMins + "m " + lngSeconds + "s";
          }
          else
          {
            return lngSeconds + "s";
          }
        }
      }
      else
      {
        string strWeeks = (lngWeeks == 1 ? "" : "s");
        string strDays = (lngDays == 1 ? "" : "s");
        string strHours = (lngHours == 1 ? "" : "s");
        string strMins = (lngMins == 1 ? "" : "s");
        string strSeconds = (lngSeconds == 1 ? "" : "s");
        if (Trimmed)
        {
          if (lngWeeks > 0)
          {
            return lngWeeks + " Week" + strWeeks + " and " + lngDays + " Day" + strDays;
          }
          else if (lngDays > 0)
          {
            if (lngHours > 20)
            {
              if (lngDays >= 6)
              {
                return "1 Week";
              }
              else
              {
                return lngDays + 1 + " Days";
              }
            }
            else
            {
              return lngDays + (lngHours > 14 ? " and Three Quarter Days" : (lngHours > 8 ? " and a Half Days" : (lngHours > 2 ? " and a Quarter Days" : " Day" + strDays)));
            }
          }
          else if (lngHours > 0)
          {
            if (lngHours >= 22 | (lngHours == 21 & lngMins > 50))
            {
              return "1 Day";
            }
            else if (lngMins > 50)
            {
              return lngHours + 1 + " Hours";
            }
            else
            {
              return lngHours + (lngMins > 35 ? " and Three Quarter Hours" : (lngMins > 20 ? " and a Half Hours" : (lngMins > 5 ? " and a Quarter Hours" : " Hour" + strHours)));
            }
          }
          else if (lngMins > 0)
          {
            if (lngMins >= 55 | (lngMins == 54 & lngSeconds > 55))
            {
              return "1 hour";
            }
            else if (lngSeconds > 50)
            {
              return lngMins + 1 + " Minutes";
            }
            else
            {
              return lngMins + (lngSeconds > 35 ? " and Three Quarter Minutes" : (lngSeconds > 20 ? " and a Half Minutes" : (lngSeconds > 5 ? " and a Quarter Minutes" : " Minute" + strMins)));
            }
          }
          else
          {
            if (lngSeconds > 55)
            {
              return "1 Minute";
            }
            else
            {
              return lngSeconds + "Second" + strSeconds;
            }
          }
        }
        else
        {
          if (lngWeeks > 0)
          {
            return lngWeeks + " Week" + strWeeks + ", " + lngDays + " Day" + strDays + ", " + lngHours + " Hour" + strHours + ", " + lngMins + " Minute" + strMins + ", and " + lngSeconds + " Second" + strSeconds;
          }
          else if (lngDays > 0)
          {
            return lngDays + " Day" + strDays + ", " + lngHours + " Hour" + strHours + ", " + lngMins + " Minute" + strMins + ", and " + lngSeconds + " Second" + strSeconds;
          }
          else if (lngHours > 0)
          {
            return lngHours + " Hour" + strHours + ", " + lngMins + " Minute" + strMins + ", and " + lngSeconds + " Second" + strSeconds;
          }
          else if (lngMins > 0)
          {
            return lngMins + " Minute" + strMins + " and " + lngSeconds + " Second" + strSeconds;
          }
          else
          {
            return lngSeconds + " Second" + strSeconds;
          }
        }
      }
    }

    public static string DisplayVersion(string sVersion)
    {
      string ApplicationVersion = sVersion;
      while (!String.IsNullOrEmpty(ApplicationVersion) && ApplicationVersion.Length > 3 && ApplicationVersion.EndsWith(".0"))
      {
        ApplicationVersion = ApplicationVersion.Substring(0, ApplicationVersion.Length-2);
      }
      if (string.IsNullOrEmpty(ApplicationVersion))
        return sVersion;
      else
        return ApplicationVersion;
    }

    public static bool CompareVersions(string sRemote)
    {
      string sLocal = ProductVersion();
      string[] LocalVer = new string[4];
      if (sLocal.Contains("."))
      {
        for (int I = 0; I <= 3; I++)
        {
          if (sLocal.Split('.').Length > I)
          {
            string sTmp = sLocal.Split('.') [I].Trim();
            int iV;
            if (int.TryParse(sTmp, out iV) & sTmp.Length < 4)
            {
              sTmp += new string('0', 4 - sTmp.Length);
            }
            LocalVer [I] = sTmp;
          }
          else
          {
            LocalVer [I] = "0000";
          }
        }
      }
      else if (sLocal.Contains(","))
      {
        for (int I = 0; I <= 3; I++)
        {
          if (sLocal.Split(',').Length > I)
          {
            string sTmp = sLocal.Split(',') [I].Trim();
            int iV;
            if (int.TryParse(sTmp, out iV) & sTmp.Length < 4)
            {
              sTmp += new string('0', 4 - sTmp.Length);
            }
            LocalVer [I] = sTmp;
          }
          else
          {
            LocalVer [I] = "0000";
          }
        }
      }
      string[] RemoteVer = new string[4];
      if (sRemote.Contains("."))
      {
        for (int I = 0; I <= 3; I++)
        {
          if (sRemote.Split('.').Length > I)
          {
            string sTmp = sRemote.Split('.') [I].Trim();
            int iV;
            if (int.TryParse(sTmp, out iV) & sTmp.Length < 4)
            {
              sTmp += new string('0', 4 - sTmp.Length);
            }
            RemoteVer [I] = sTmp;
          }
          else
          {
            RemoteVer [I] = "0000";
          }
        }
      }
      else if (sRemote.Contains(","))
      {
        for (int I = 0; I <= 3; I++)
        {
          if (sRemote.Split(',').Length > I)
          {
            string sTmp = sRemote.Split(',') [I].Trim();
            int iV;
            if (int.TryParse(sTmp, out iV) & sTmp.Length < 4)
            {
              sTmp += new string('0', 4 - sTmp.Length);
            }
            RemoteVer [I] = sTmp;
          }
          else
          {
            RemoteVer [I] = "0000";
          }
        }
      }
      bool bUpdate = false;
      int[] LocalVal = new int[4];
      int[] RemoteVal = new int[4];
      for (int I = 0; I <= 3; I++)
      {
        LocalVal [I] = int.Parse(LocalVer [I]);
        RemoteVal [I] = int.Parse(RemoteVer [I]);
      }

      if (LocalVal [0] > RemoteVal [0])
      {
        //Local's OK
      }
      else if (LocalVal [0] == RemoteVal [0])
      {
        if (LocalVal [1] > RemoteVal [1])
        {
          //Local's OK
        }
        else if (LocalVal [1] == RemoteVal [1])
        {
          if (LocalVal [2] > RemoteVal [2])
          {
            //Local's OK
          }
          else if (LocalVal [2] == RemoteVal [2])
          {
            if (LocalVal [3] >= RemoteVal [3])
            {
              //Local's OK
            }
            else
            {
              bUpdate = true;
            }
          }
          else
          {
            bUpdate = true;
          }
        }
        else
        {
          bUpdate = true;
        }
      }
      else
      {
        bUpdate = true;
      }
      return bUpdate;
    }

    public static void SaveToFTP(object sData)
    {
      string sFailFile = "WB-ReadFail-" + DateTime.Now.ToString("G") + "-v" + ProductVersion() + ".txt";
      sFailFile = sFailFile.Replace("/", "-");
      sFailFile = sFailFile.Replace(":", "-");
      System.Net.WebRequest ftpSave = System.Net.FtpWebRequest.Create("ftp://realityripple.com/" + sFailFile);
      ftpSave.Proxy = new System.Net.WebProxy();
      ftpSave.Credentials = modCreds.FTPCredentials(); //Use [new System.Net.NetworkCredential("FTPUSER", "FTPPASS");] to upload failures to a FTP location.
      ftpSave.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
      using (System.IO.Stream ftpStream = ftpSave.GetRequestStream())
      {
        byte[] bHTTP = System.Text.Encoding.UTF8.GetBytes(sData.ToString());
        ftpStream.Write(bHTTP, 0, bHTTP.Length);
        ftpStream.Close();
      }
    }

    public static string PercentEncode(string inString)
    {
      string sRet = string.Empty;
      if (string.IsNullOrEmpty(inString))
        return inString;
      for (int I = inString.Length - 1; I >= 0; I += -1)
      {
        int iChar = Convert.ToInt32(inString[I]);
        if ((iChar >= 48 && iChar <= 57) || (iChar <= 65 && iChar >= 90) || (iChar <= 97 && iChar >= 122))
          sRet = inString [I].ToString() + sRet;
        else if (iChar == 32)
          sRet = "+" + sRet;
        else
          sRet = "%" + PadHex(iChar, 2) + sRet;
      }
      return sRet;
    }

    private static string PadHex(Int32 Value, UInt16 Length)
    {
      string sVal = Convert.ToString(Value, 16);
      while (sVal.Length < Length)
      {
        sVal = "0" + sVal;
      }
      return sVal;
    }

    public static byte CopyDirectory(string FromDir, string ToDir)
    {
      if (System.IO.Directory.Exists(FromDir))
      {
        bool bDidSomething = false;
        if (System.IO.Directory.Exists(ToDir))
        {
          string[] wbFiles = System.IO.Directory.GetFiles(FromDir);
          if (wbFiles.Length > 0)
          {
            string[] srtFiles = System.IO.Directory.GetFiles(ToDir);
            System.Collections.ObjectModel.Collection<string> spareFiles = null;
            if (srtFiles.Length > 0)
            {
              spareFiles = new System.Collections.ObjectModel.Collection<string>();
              for (int I = 0; I <= wbFiles.Length - 1; I++)
              {
                bool isUnique = true;
                for (int J = 0; J <= srtFiles.Length - 1; J++)
                {
                  if (System.IO.Path.GetFileName(srtFiles [J]).CompareTo(System.IO.Path.GetFileName(wbFiles [I])) == 0)
                  {
                    isUnique = false;
                    break;
                  }
                }
                if (isUnique)
                {
                  spareFiles.Add(wbFiles [I]);
                }
              }
            }
            else
            {
              spareFiles = new System.Collections.ObjectModel.Collection<string>(srtFiles);
            }
            if (spareFiles.Count > 0)
            {
              System.IO.Directory.CreateDirectory(ToDir);
              for (int I = 0; I <= spareFiles.Count - 1; I++)
              {
                string file = spareFiles [I];
                string sFName = System.IO.Path.GetFileName(file);
                System.IO.File.Copy(file, ToDir + System.IO.Path.DirectorySeparatorChar.ToString() + sFName, true);
                bDidSomething = true;
              }
            }
          }
          else
          {
            //no files in WB folder to copy
          }
        }
        else
        {
          string[] wbFiles = System.IO.Directory.GetFiles(FromDir);
          if (wbFiles.Length > 0)
          {
            System.IO.Directory.CreateDirectory(ToDir);
            for (int I = 0; I <= wbFiles.Length - 1; I++)
            {
              string file = wbFiles [I];
              string sFName = System.IO.Path.GetFileName(file);
              System.IO.File.Copy(file, ToDir + System.IO.Path.DirectorySeparatorChar.ToString() + sFName, true);
              bDidSomething = true;
            }
          }
        }
        if (!bDidSomething)
        {
          return 2;
        }
        string[] wFileTmp = System.IO.Directory.GetFiles(FromDir);
        string[] sFileTmp = System.IO.Directory.GetFiles(ToDir);
        bool Equal = true;
        if (wFileTmp.Length == sFileTmp.Length)
        {
          for (int I = 0; I <= wFileTmp.Length - 1; I++)
          {
            if ((System.IO.Path.GetFileName(wFileTmp [I]).CompareTo(System.IO.Path.GetFileName(sFileTmp [I])) == 0) & (new FileInfo(wFileTmp [I]).Length == new FileInfo(sFileTmp [I]).Length))
            {
              continue;
            }
            else
            {
              Equal = false;
              break;
            }
          }
        }
        else
        {
          Equal = false;
        }
        if (Equal)
        {
          return 1;
        }
        else
        {
          return 0;
        }
      }
      else
      {
        return 2;
      }
    }

    public static string NetError(string Message)
    {
      if (Message.StartsWith("Error: ConnectFailure (") & Message.EndsWith(")"))
      {
        return "Unable to connect to the remote server " + Message.Substring(Message.IndexOf("("));
      }
      else
      {
        return Message;
      }
    }

    #region "Graphs"
    #region "History"
    private static Rectangle dGraph;
    private static Rectangle uGraph;
    private static System.DateTime oldDate;
    private static System.DateTime newDate;
    private static DataBase.DataRow[] dData;
    private static DataBase.DataRow[] uData;

    public static Rectangle GetGraphRect(bool DownGraph, out System.DateTime firstX, out System.DateTime lastX)
    {
      firstX = oldDate;
      lastX = newDate;
      if (DownGraph)
      {
        return dGraph;
      }
      else
      {
        return uGraph;
      }
    }

    static internal DataBase.DataRow GetGraphData(System.DateTime fromDate, bool DownGraph)
    {
      if (DownGraph)
      {
        DataBase.DataRow closestRow = default(DataBase.DataRow);
        int closestVal = int.MaxValue;
        foreach (DataBase.DataRow dRow in dData)
        {
          if (Math.Abs(DateDiff(DateInterval.Minute, dRow.DATETIME, fromDate)) < closestVal)
          {
            closestRow = dRow;
            closestVal = (int)Math.Abs(DateDiff(DateInterval.Minute, dRow.DATETIME, fromDate));
          }
        }
        return closestRow;
      }
      else
      {
        DataBase.DataRow closestRow = default(DataBase.DataRow);
        int closestVal = int.MaxValue;
        foreach (DataBase.DataRow dRow in uData)
        {
          if (Math.Abs(DateDiff(DateInterval.Minute, dRow.DATETIME, fromDate)) < closestVal)
          {
            closestRow = dRow;
            closestVal = (int)Math.Abs(DateDiff(DateInterval.Minute, dRow.DATETIME, fromDate));
          }
        }
        return closestRow;
      }
    }

    public static Image DrawLineGraph(DataBase.DataRow[] Data, bool Down, Size ImgSize, Color ColorA, Color ColorB, Color ColorC, Color ColorText, Color ColorBG, Color ColorMax)
    {
      if (Data == null || Data.Length == 0)
      {
        return new Bitmap(1, 1);
      }
      long yDMax = 0;
      long yUMax = 0;
      for (long I = 0; I <= Data.Length - 1; I++)
      {
        if (yDMax < Data [I].DOWNLOAD)
        {
          yDMax = Data [I].DOWNLOAD;
        }
        if (yUMax < Data [I].UPLOAD)
        {
          yUMax = Data [I].UPLOAD;
        }
        if (yDMax < Data [I].DOWNLIM)
        {
          yDMax = Data [I].DOWNLIM;
        }
        if (yUMax < Data [I].UPLIM)
        {
          yUMax = Data [I].UPLIM;
        }
      }
      long yMax = (yDMax > yUMax ? yDMax : yUMax);
      if (!(yMax % 1000 == 0))
      {
        yMax = (int)(((double)yMax / 1000) * 1000);
      }
      long lMax = (Down ? yDMax : yUMax);
      if (!(lMax % 1000 == 0))
      {
        lMax = (int)(((double)lMax / 1000) * 1000 + 1000);
      }
      Image iPic = new Bitmap(ImgSize.Width, ImgSize.Height);
      Graphics g = Graphics.FromImage(iPic);
      Font tFont = new Font(FontFamily.GenericSansSerif, 7);
      int lYWidth = (int)g.MeasureString(yMax.ToString().Trim() + " MB", tFont).Width + 10;
      int lXHeight = (int)g.MeasureString(DateTime.Now.ToString("g"), tFont).Height + 10;
      g.Clear(ColorBG);
      int yTop = lXHeight / 2;
      int yHeight = (int)(ImgSize.Height - (lXHeight * 1.5));
      if (Down)
      {
        dGraph = new Rectangle(lYWidth, yTop, (ImgSize.Width - 4) - lYWidth, yHeight);
        dData = Data;
      }
      else
      {
        uGraph = new Rectangle(lYWidth, yTop, (ImgSize.Width - 4) - lYWidth, yHeight);
        uData = Data;
      }
      g.DrawLine(new Pen(ColorText), lYWidth, yTop, lYWidth, yTop + yHeight);
      g.DrawLine(new Pen(ColorText), lYWidth, yTop + yHeight, ImgSize.Width, yTop + yHeight);
      oldDate = Data [0].DATETIME;
      newDate = Data [Data.Length - 1].DATETIME;
      for (int I = 0; I <= (int) lMax; I += (int) ((((lMax / ((double) yHeight / (tFont.Size + 12)))) / 100) * 100))
      {
        int iY = (int)(yTop + yHeight - (I / (double)lMax * yHeight));
        g.DrawString(I.ToString().Trim() + " MB", tFont, new SolidBrush(ColorText), lYWidth - g.MeasureString(I.ToString().Trim() + " MB", tFont).Width - 5, iY - (g.MeasureString(I.ToString().Trim() + " MB", tFont).Height / 2));
        g.DrawLine(new Pen(ColorText), lYWidth - 3, iY, lYWidth, iY);
      }
      System.DateTime lStart = Data [0].DATETIME;
      System.DateTime lEnd = Data [Data.Length - 1].DATETIME;
      DateInterval dInterval = DateInterval.Minute;
      uint lInterval = 1;
      uint lLabelInterval = 5;
      if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 61)
      {
        //Under an Hour
        lInterval = 1;
        lLabelInterval = 5;
        dInterval = DateInterval.Minute;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) < 60 * 13)
      {
        //Under 12 hours
        lInterval = 15;
        lLabelInterval = 60;
        dInterval = DateInterval.Minute;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 60 * 25)
      {
        //Under a Day
        lInterval = 60;
        lLabelInterval = 60 * 6;
        dInterval = DateInterval.Minute;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 60 * 24 * 8)
      {
        //Under a Week
        lInterval = 12;
        lLabelInterval = 24;
        dInterval = DateInterval.Hour;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 60 * 24 * 31)
      {
        //Under 30 Days
        lInterval = 24;
        lLabelInterval = 24 * 7;
        dInterval = DateInterval.Hour;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 60 * 24 * 366)
      {
        //Under a Year
        lInterval = 7;
        lLabelInterval = 30;
        dInterval = DateInterval.Day;
      }
      else
      {
        //Over a year
        lInterval = 30;
        lLabelInterval = 365;
        dInterval = DateInterval.Day;
      }
      long lMaxTime = Math.Abs(DateDiff(dInterval, lStart, lEnd));
      if (lMaxTime == 0)
      {
        return new Bitmap(1, 1);
      }
      long lLineWidth = (ImgSize.Width - 4) - lYWidth - 1;
      double dCompInter = lLineWidth / (double)lMaxTime;

      for (long I = 0; I <= lMaxTime; I += lInterval)
      {
        int lX = (int)(lYWidth + (I * dCompInter) + 1);
        g.DrawLine(SystemPens.GrayText, lX, ImgSize.Height - (lXHeight - 3), lX, ImgSize.Height - lXHeight);
      }
      long lastI = (long)(lYWidth + (lMaxTime * dCompInter));
      if (lastI >= (ImgSize.Width - 4))
      {
        lastI = (ImgSize.Width - 4);
      }
      string sDispV = "g";
      if (DateDiff(DateInterval.Day, lStart, lEnd) > 1)
      {
        sDispV = "d";
      }
      else if (DateDiff(DateInterval.Day, lStart, lEnd) == 1)
      {
        sDispV = "g";
      }
      else if (DateDiff(DateInterval.Day, lStart, lEnd) < 1)
      {
        sDispV = "t";
      }
      string sLastDisp = lEnd.ToString(sDispV);
      float iLastDispWidth = g.MeasureString(sLastDisp, tFont).Width;
      for (long I = 0; I <= lMaxTime; I += lLabelInterval)
      {
        int lX = (int)(lYWidth + (I * dCompInter) + 1);
        if (lX >= (ImgSize.Width - 4))
        {
          lX = (ImgSize.Width - 4);
          g.DrawLine(new Pen(ColorText), lX, ImgSize.Height - (lXHeight - 5), lX, ImgSize.Height - lXHeight);
          string sDisp = DateAdd(dInterval, I, lStart).ToString(sDispV);
          g.DrawString(sDisp, tFont, new SolidBrush(ColorText), lX - g.MeasureString(sDisp, tFont).Width, ImgSize.Height - lXHeight + 5);
          if (lX >= lastI - (iLastDispWidth * 1.6))
          {
            lastI = -1;
          }
        }
        else
        {
          g.DrawLine(new Pen(ColorText), lX, ImgSize.Height - (lXHeight - 5), lX, ImgSize.Height - lXHeight);
          string sDisp = DateAdd(dInterval, I, lStart).ToString(sDispV);
          g.DrawString(sDisp, tFont, new SolidBrush(ColorText), lX - (g.MeasureString(sDisp, tFont).Width / 2), ImgSize.Height - lXHeight + 5);
          if (lX >= lastI - (iLastDispWidth * 1.6))
          {
            lastI = -1;
          }
        }
      }
      if (lastI > -1)
      {
        g.DrawLine(new Pen(ColorText), lastI, ImgSize.Height - (lXHeight - 5), lastI, ImgSize.Height - lXHeight);
        g.DrawString(sLastDisp, tFont, new SolidBrush(ColorText), lastI - iLastDispWidth + 3, ImgSize.Height - lXHeight + 5);
      }

      int MaxY = (int)(yTop + yHeight - ((Down ? Data [Data.Length - 1].DOWNLIM : Data [Data.Length - 1].UPLIM) / (double)lMax * yHeight));
      Point[] lMaxPoints = new Point[lMaxTime + 1];
      Point[] lPoints = new Point[lMaxTime + 4];
      byte[] lTypes = new byte[lMaxTime + 4];
      long lastLVal = 0;

      for (long I = 0; I <= lMaxTime; I++)
      {
        long lVal = -1;
        long lLow = long.MaxValue;
        long lHigh = 0;
        for (int J = 0; J <= Data.Length - 1; J++)
        {
          if (Math.Abs(DateDiff(dInterval, Data [J].DATETIME, DateAdd(dInterval, I, lStart))) == 0)
          {
            long jLim = (Down ? Data [J].DOWNLIM : Data [J].UPLIM);
            if (lHigh < jLim)
            {
              lHigh = jLim;
            }
            if (lLow > jLim)
            {
              lLow = jLim;
            }
          }
        }
        if (lHigh > 0 & lLow < long.MaxValue)
        {
          lVal = (lHigh + lLow) / 2;
        }
        if (lVal == -1 & lastLVal > 0)
        {
          lVal = lastLVal;
        }
        lMaxPoints [I].X = (int)(lYWidth + (I * dCompInter) + 1);
        lMaxPoints [I].Y = (int)(yTop + yHeight - (lVal / (double)lMax * yHeight));
        if (I > 0 && (lMaxPoints [I - 1].X == 0 & lMaxPoints [I - 1].Y == 0))
        {
          long J = 1;
          while (lMaxPoints[I - J].Y == 0)
          {
            J += 1;
          }
          for (long K = 1; K <= J - 1; K++)
          {
            lMaxPoints [I - K].X = (int)(lYWidth + ((I - K) * dCompInter) + 1);
            lMaxPoints [I - K].Y = (lMaxPoints [I - J].Y + lMaxPoints [I].Y) / 2;
          }
        }
        lastLVal = lVal;
      }

      lastLVal = 0;
      for (long I = 0; I <= lMaxTime; I++)
      {
        long lVal = -1;
        long lLow = long.MaxValue;
        long lHigh = 0;
        for (int J = 0; J <= Data.Length - 1; J++)
        {
          if (Math.Abs(DateDiff(dInterval, Data [J].DATETIME, DateAdd(dInterval, I, lStart))) == 0)
          {
            long jVal = (Down ? Data [J].DOWNLOAD : Data [J].UPLOAD);
            if (lHigh < jVal)
            {
              lHigh = jVal;
            }
            if (lLow > jVal)
            {
              lLow = jVal;
            }
          }
        }
        if (lHigh > 0 & lLow < long.MaxValue)
        {
          lVal = (lHigh + lLow) / 2;
        }
        if (lVal == -1 & lastLVal > 0)
        {
          lVal = lastLVal;
        }
        lPoints [I].X = (int)(lYWidth + (I * dCompInter) + 1);
        lPoints [I].Y = (int)(yTop + yHeight - ((double)lVal / lMax * yHeight));
        if (I > 0 && (lPoints [I - 1].X == 0 & lPoints [I - 1].Y == 0))
        {
          long J = 1;
          while (lPoints[I - J].Y == 0)
          {
            J += 1;
          }
          for (long K = 1; K <= J - 1; K++)
          {
            lPoints [I - K].X = (int)(lYWidth + ((I - K) * dCompInter) + 1);
            lPoints [I - K].Y = (lPoints [I - J].Y + lPoints [I].Y) / 2;
          }
        }
        lastLVal = lVal;
      }
      if (lPoints [lMaxTime].IsEmpty)
      {
        lPoints [lMaxTime] = new Point(ImgSize.Width - 1, yTop + yHeight - 1);
      }
      lPoints [lMaxTime + 1] = new Point(ImgSize.Width - 1, yTop + yHeight - 1);
      lPoints [lMaxTime + 2] = new Point(lYWidth + 1, yTop + yHeight - 1);
      lPoints [lMaxTime + 3] = lPoints [0];
      lTypes [0] = (byte)PathPointType.Start;
      for (long I = 1; I <= lMaxTime + 2; I++)
      {
        lTypes [I] = (byte)PathPointType.Line;
      }
      lTypes [lMaxTime + 3] = (byte)(PathPointType.Line | PathPointType.CloseSubpath);
      LinearGradientBrush fBrush = TriGradientBrush(new Point(lYWidth, MaxY), new Point(lYWidth, yTop + yHeight), ColorA, ColorB, ColorC);
      fBrush.WrapMode = WrapMode.TileFlipX;
      g.DrawLines(new Pen(new SolidBrush(ColorMax), 5), lMaxPoints);
      g.FillPath(fBrush, new GraphicsPath(lPoints, lTypes));
      g.DrawLines(new Pen(new SolidBrush(Color.FromArgb(96, ColorMax)), 5), lMaxPoints);
      g.Dispose();
      return iPic;
    }

    public static Image DrawEGraph(DataBase.DataRow[] Data, bool Invert, Size ImgSize, Color ColorDA, Color ColorDB, Color ColorDC, Color ColorUA, Color ColorUB, Color ColorUC, Color ColorText,
                                   Color ColorBG, Color ColorMax)
    {
      if (Data == null || Data.Length == 0)
      {
        return new Bitmap(1, 1);
      }
      long yVMax = 0;
      for (long I = 0; I <= Data.Length - 1; I++)
      {
        if (Invert)
        {
          if (yVMax < Data [I].DOWNLOAD + Data [I].UPLOAD + Data [I].DOWNLIM)
          {
            yVMax = Data [I].DOWNLOAD + Data [I].UPLOAD + Data [I].DOWNLIM;
          }
        }
        else
        {
          if (yVMax < Data [I].DOWNLOAD + Data [I].UPLOAD + Data [I].UPLIM)
          {
            yVMax = Data [I].DOWNLOAD + Data [I].UPLOAD + Data [I].UPLIM;
          }
        }
      }
      if (Invert)
      {
        if (yVMax < Data [Data.Length - 1].UPLIM)
        {
          yVMax = Data [Data.Length - 1].UPLIM;
        }
      }
      else
      {
        if (yVMax < Data [Data.Length - 1].DOWNLIM)
        {
          yVMax = Data [Data.Length - 1].DOWNLIM;
        }
      }
      long yMax = yVMax;
      if (!(yMax % 1000 == 0))
      {
        yMax = (int)(((double)yMax / 1000) * 1000);
      }
      long lMax = yVMax;
      if (!(lMax % 1000 == 0))
      {
        lMax = (int)(((double)lMax / 1000) * 1000 + 1000);
      }
      Image iPic = new Bitmap(ImgSize.Width, ImgSize.Height);
      Graphics g = Graphics.FromImage(iPic);
      Font tFont = new Font(FontFamily.GenericSansSerif, 7);
      int lYWidth = (int)g.MeasureString(yMax.ToString().Trim() + " MB", tFont).Width + 10;
      int lXHeight = (int)g.MeasureString(DateTime.Now.ToString("g"), tFont).Height + 10;
      g.Clear(ColorBG);
      int yTop = lXHeight / 2;
      int yHeight = (int)(ImgSize.Height - (lXHeight * 1.5));
      dGraph = new Rectangle(lYWidth, yTop, ImgSize.Width - lYWidth, yHeight);
      dData = Data;
      uGraph = new Rectangle(0, 0, 0, 0);
      uData = null;
      g.DrawLine(new Pen(ColorText), lYWidth, yTop, lYWidth, yTop + yHeight);
      g.DrawLine(new Pen(ColorText), lYWidth, yTop + yHeight, ImgSize.Width, yTop + yHeight);
      oldDate = Data [0].DATETIME;
      newDate = Data [Data.Length - 1].DATETIME;
      for (int I = 0; I <= (int) lMax; I += (int) ((((lMax / ((double) yHeight / (tFont.Size + 12)))) / 100) * 100))
      {
        int iY = (int)(yTop + yHeight - (I / (double)lMax * yHeight));
        g.DrawString(I.ToString().Trim() + " MB", tFont, new SolidBrush(ColorText), lYWidth - g.MeasureString(I.ToString().Trim() + " MB", tFont).Width - 5, iY - (g.MeasureString(I.ToString().Trim() + " MB", tFont).Height / 2));
        g.DrawLine(new Pen(ColorText), lYWidth - 3, iY, lYWidth, iY);
      }
      System.DateTime lStart = Data [0].DATETIME;
      System.DateTime lEnd = Data [Data.Length - 1].DATETIME;
      DateInterval dInterval = DateInterval.Minute;
      uint lInterval = 1;
      uint lLabelInterval = 5;
      if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 61)
      {
        //Under an Hour
        lInterval = 1;
        lLabelInterval = 5;
        dInterval = DateInterval.Minute;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) < 60 * 13)
      {
        //Under 12 hours
        lInterval = 15;
        lLabelInterval = 60;
        dInterval = DateInterval.Minute;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 60 * 25)
      {
        //Under a Day
        lInterval = 60;
        lLabelInterval = 60 * 6;
        dInterval = DateInterval.Minute;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 60 * 24 * 8)
      {
        //Under a Week
        lInterval = 12;
        lLabelInterval = 24;
        dInterval = DateInterval.Hour;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 60 * 24 * 31)
      {
        //Under 30 Days
        lInterval = 24;
        lLabelInterval = 24 * 7;
        dInterval = DateInterval.Hour;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 60 * 24 * 366)
      {
        //Under a Year
        lInterval = 7;
        lLabelInterval = 30;
        dInterval = DateInterval.Day;
      }
      else
      {
        //Over a year
        lInterval = 30;
        lLabelInterval = 365;
        dInterval = DateInterval.Day;
      }
      long lMaxTime = Math.Abs(DateDiff(dInterval, lStart, lEnd));
      if (lMaxTime == 0)
      {
        return new Bitmap(1, 1);
      }
      long lLineWidth = ImgSize.Width - lYWidth;
      double dCompInter = lLineWidth / (double)lMaxTime;
      for (long I = 0; I <= lMaxTime; I += lInterval)
      {
        int lX = (int)(lYWidth + (I * dCompInter));
        g.DrawLine(new Pen(ColorText), lX, ImgSize.Height - (lXHeight - 3), lX, ImgSize.Height - lXHeight);
      }
      for (long I = 0; I <= lMaxTime; I += lLabelInterval)
      {
        int lX = (int)(lYWidth + (I * dCompInter));
        g.DrawLine(new Pen(ColorText), lX, ImgSize.Height - (lXHeight - 5), lX, ImgSize.Height - lXHeight);
        string sDisp = null;
        if (DateDiff(DateInterval.Day, lStart, lEnd) > 1)
        {
          sDisp = DateAdd(dInterval, I, lStart).ToString("d");
        }
        else if (DateDiff(DateInterval.Day, lStart, lEnd) < 1)
        {
          sDisp = DateAdd(dInterval, I, lStart).ToString("t");
        }
        else
        {
          sDisp = DateAdd(dInterval, I, lStart).ToString("g");
        }
        g.DrawString(sDisp, tFont, new SolidBrush(ColorText), lX - (g.MeasureString(sDisp, tFont).Width / 2), ImgSize.Height - lXHeight + 5);
      }
      int MaxY = 0;
      if (Invert)
      {
        MaxY = (int)(yTop + yHeight - (Data [Data.Length - 1].UPLIM / (double)lMax * yHeight));
      }
      else
      {
        MaxY = (int)(yTop + yHeight - (Data [Data.Length - 1].DOWNLIM / (double)lMax * yHeight));
      }

      Point[] lMaxPoints = new Point[lMaxTime + 1];
      long lastLVal = 0;

      for (long I = 0; I <= lMaxTime; I++)
      {
        long lVal = -1;
        long lLow = long.MaxValue;
        long lHigh = 0;
        for (int J = 0; J <= Data.Length - 1; J++)
        {
          if (Math.Abs(DateDiff(dInterval, Data [J].DATETIME, DateAdd(dInterval, I, lStart))) == 0)
          {
            if (Invert)
            {
              if (lHigh < Data [J].UPLIM)
              {
                lHigh = Data [J].UPLIM;
              }
              if (lLow > Data [J].UPLIM)
              {
                lLow = Data [J].UPLIM;
              }
            }
            else
            {
              if (lHigh < Data [J].DOWNLIM)
              {
                lHigh = Data [J].DOWNLIM;
              }
              if (lLow > Data [J].DOWNLIM)
              {
                lLow = Data [J].DOWNLIM;
              }
            }
          }
        }

        if (lHigh > 0 & lLow < long.MaxValue)
        {
          lVal = (lHigh + lLow) / 2;
        }
        if (lVal == -1 & lastLVal > 0)
        {
          lVal = lastLVal;
        }
        lMaxPoints [I].X = (int)(lYWidth + (I * dCompInter));
        lMaxPoints [I].Y = (int)(yTop + yHeight - ((double)lVal / lMax * yHeight));
        lastLVal = lVal;
      }

      Point[] lUPoints = new Point[lMaxTime + 4];
      Point[] lDPoints = new Point[lMaxTime + 4];
      Point[] lOPoints = new Point[lMaxTime + 4];
      byte[] lTypes = new byte[lMaxTime + 4];
      long lastDVal = 0;
      long lastUVal = 0;
      long lastOVal = 0;
      for (long I = 0; I <= lMaxTime; I++)
      {
        System.Collections.Generic.List<long> lDRange = new System.Collections.Generic.List<long>();
        long lDVal = -1;
        System.Collections.Generic.List<long> lURange = new System.Collections.Generic.List<long>();
        long lUVal = -1;
        System.Collections.Generic.List<long> lORange = new System.Collections.Generic.List<long>();
        long lOVal = -1;
        long lDLow = long.MaxValue;
        long lDHigh = -1;
        long lULow = long.MaxValue;
        long lUHigh = -1;
        long lOLow = long.MaxValue;
        long lOHigh = -1;

        for (int J = 0; J <= Data.Length - 1; J++)
        {
          if (Math.Abs(DateDiff(dInterval, Data [J].DATETIME, DateAdd(dInterval, I, lStart))) == 0)
          {
            long jDVal = Data [J].DOWNLOAD;
            if (lDHigh < jDVal)
            {
              lDHigh = jDVal;
            }
            if (lDLow > jDVal)
            {
              lDLow = jDVal;
            }

            long jUVal = Data [J].UPLOAD;
            if (lUHigh < jUVal)
            {
              lUHigh = jUVal;
            }
            if (lULow > jUVal)
            {
              lULow = jUVal;
            }

            long jOVal = 0;
            if (Data [J].DOWNLIM == Data [J].UPLIM)
            {
              jOVal = 0;
            }
            else
            {
              if (Invert)
              {
                jOVal = Data [J].DOWNLIM;
              }
              else
              {
                jOVal = Data [J].UPLIM;
              }
            }
            if (lOHigh < jOVal)
            {
              lOHigh = jOVal;
            }
            if (lOLow > jOVal)
            {
              lOLow = jOVal;
            }

          }
        }

        if (lDHigh > -1 & lDLow < long.MaxValue)
        {
          int Nulls = lDRange.FindAll((long val) => val == 0).Count;
          if (Nulls > lDRange.Count * 0.4)
          {
            lDVal = 0;
          }
          else
          {
            lastDVal = lDHigh;
          }
        }
        else if (lastDVal > 0)
        {
          lDVal = lastDVal;
        }

        if (lUHigh > -1 & lULow < long.MaxValue)
        {
          int Nulls = lURange.FindAll((long val) => val == 0).Count;
          if (Nulls > lURange.Count * 0.4)
          {
            lUVal = 0;
          }
          else
          {
            lastUVal = lUHigh;
          }
        }
        else if (lastUVal > 0)
        {
          lUVal = lastUVal;
        }

        if (lOHigh > -1 & lOLow < long.MaxValue)
        {
          int Nulls = lORange.FindAll((long val) => val == 0).Count;
          if (Nulls > lORange.Count * 0.4)
          {
            lOVal = 0;
          }
          else
          {
            lastOVal = lOHigh;
          }
        }
        else if (lastOVal > 0)
        {
          lOVal = lastOVal;
        }

        lUPoints [I].X = (int)(lYWidth + (I * dCompInter));
        lUPoints [I].Y = (int)(yTop + yHeight - (lUVal / (double)lMax * yHeight));
        lDPoints [I].X = (int)(lYWidth + (I * dCompInter));
        lDPoints [I].Y = (int)(yTop + yHeight - (lDVal / (double)lMax * yHeight) - (yTop + yHeight - lUPoints [I].Y));
        lOPoints [I].X = (int)(lYWidth + (I * dCompInter));
        lOPoints [I].Y = (int)(yTop + yHeight - (lOVal / (double)lMax * yHeight) - (yTop + yHeight - lDPoints [I].Y));
        if (lDVal > 0)
          lastDVal = lDVal;
        if (lUVal > 0)
          lastUVal = lUVal;
        if (lOVal > 0)
        lastOVal = lOVal;
      }
      lTypes [0] = (byte)PathPointType.Start;
      for (long I = 1; I <= lMaxTime + 2; I++)
      {
        lTypes [I] = (byte)PathPointType.Line;
      }

      if (lUPoints [lMaxTime].IsEmpty)
      {
        lUPoints [lMaxTime] = new Point(ImgSize.Width, yTop + yHeight);
      }
      lUPoints [lMaxTime + 1] = new Point(ImgSize.Width, yTop + yHeight);
      lUPoints [lMaxTime + 2] = new Point(lYWidth, yTop + yHeight);
      lUPoints [lMaxTime + 3] = lUPoints [0];
      lTypes [lMaxTime + 3] = (byte)(PathPointType.Line | PathPointType.CloseSubpath);
      LinearGradientBrush uBrush = TriGradientBrush(new Point(lYWidth, MaxY), new Point(lYWidth, yTop + yHeight), ColorUA, ColorUB, ColorUC);
      uBrush.WrapMode = WrapMode.TileFlipX;

      if (lDPoints [lMaxTime].IsEmpty)
      {
        lDPoints [lMaxTime] = new Point(ImgSize.Width, yTop + yHeight);
      }
      lDPoints [lMaxTime + 1] = new Point(ImgSize.Width, yTop + yHeight);
      lDPoints [lMaxTime + 2] = new Point(lYWidth, yTop + yHeight);
      lDPoints [lMaxTime + 3] = lDPoints [0];
      LinearGradientBrush dBrush = TriGradientBrush(new Point(lYWidth, MaxY), new Point(lYWidth, yTop + yHeight), ColorDA, ColorDB, ColorDC);
      dBrush.WrapMode = WrapMode.TileFlipX;

      if (lOPoints [lMaxTime].IsEmpty)
      {
        lOPoints [lMaxTime] = new Point(ImgSize.Width, yTop + yHeight);
      }
      lOPoints [lMaxTime + 1] = new Point(ImgSize.Width, yTop + yHeight);
      lOPoints [lMaxTime + 2] = new Point(lYWidth, yTop + yHeight);
      lOPoints [lMaxTime + 3] = lOPoints [0];
      LinearGradientBrush oBrush = TriGradientBrush(new Point(lYWidth, MaxY), new Point(lYWidth, yTop + yHeight), ColorDA, ColorDB, ColorDC);
      oBrush.WrapMode = WrapMode.TileFlipX;

      g.DrawLines(new Pen(new SolidBrush(ColorMax), 5), lMaxPoints);
      g.FillPath(oBrush, new GraphicsPath(lOPoints, lTypes));
      g.FillPath(dBrush, new GraphicsPath(lDPoints, lTypes));
      g.FillPath(uBrush, new GraphicsPath(lUPoints, lTypes));
      g.DrawLines(new Pen(new SolidBrush(Color.FromArgb(96, ColorMax)), 5), lMaxPoints);
      g.Dispose();
      return iPic;
    }

    public static Image DrawRGraph(DataBase.DataRow[] Data, Size ImgSize, Color ColorA, Color ColorB, Color ColorC, Color ColorText, Color ColorBG, Color ColorMax)
    {
      if (Data == null || Data.Length == 0)
      {
        return new Bitmap(1, 1);
      }
      long yVMax = 0;
      for (long I = 0; I <= Data.Length - 1; I++)
      {
        if (yVMax < Data [I].DOWNLOAD)
        {
          yVMax = Data [I].DOWNLOAD;
        }
      }
      if (yVMax < Data [Data.Length - 1].DOWNLIM)
      {
        yVMax = Data [Data.Length - 1].DOWNLIM;
      }
      long yMax = yVMax;
      if (!(yMax % 1000 == 0))
      {
        yMax = (int)(((double)yMax / 1000) * 1000);
      }
      long lMax = yVMax;
      if (!(lMax % 1000 == 0))
      {
        lMax = (int)(((double)lMax / 1000) * 1000 + 1000);
      }
      Image iPic = new Bitmap(ImgSize.Width, ImgSize.Height);
      Graphics g = Graphics.FromImage(iPic);
      Font tFont = new Font(FontFamily.GenericSansSerif, 7);
      int lYWidth = (int)g.MeasureString(yMax.ToString().Trim() + " MB", tFont).Width + 10;
      int lXHeight = (int)g.MeasureString(DateTime.Now.ToString("g"), tFont).Height + 10;
      g.Clear(ColorBG);
      int yTop = lXHeight / 2;
      int yHeight = (int)(ImgSize.Height - (lXHeight * 1.5));
      dGraph = new Rectangle(lYWidth, yTop, ImgSize.Width - lYWidth, yHeight);
      dData = Data;
      uGraph = new Rectangle(0, 0, 0, 0);
      uData = null;
      g.DrawLine(new Pen(ColorText), lYWidth, yTop, lYWidth, yTop + yHeight);
      g.DrawLine(new Pen(ColorText), lYWidth, yTop + yHeight, ImgSize.Width, yTop + yHeight);
      oldDate = Data [0].DATETIME;
      newDate = Data [Data.Length - 1].DATETIME;
      for (int I = 0; I <= (int) lMax; I += (int) ((((lMax / ((double) yHeight / (tFont.Size + 12)))) / 100) * 100))
      {
        int iY = (int)(yTop + yHeight - (I / (double)lMax * yHeight));
        g.DrawString(I.ToString().Trim() + " MB", tFont, new SolidBrush(ColorText), lYWidth - g.MeasureString(I.ToString().Trim() + " MB", tFont).Width, iY - (g.MeasureString(I.ToString().Trim() + " MB", tFont).Height / 2));
        g.DrawLine(new Pen(ColorText), lYWidth - 3, iY, lYWidth, iY);
      }
      System.DateTime lStart = Data [0].DATETIME;
      System.DateTime lEnd = Data [Data.Length - 1].DATETIME;
      DateInterval dInterval = DateInterval.Minute;
      uint lInterval = 1;
      uint lLabelInterval = 5;
      if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 61)
      {
        //Under an Hour
        lInterval = 1;
        lLabelInterval = 5;
        dInterval = DateInterval.Minute;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) < 60 * 13)
      {
        //Under 12 hours
        lInterval = 15;
        lLabelInterval = 60;
        dInterval = DateInterval.Minute;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 60 * 25)
      {
        //Under a Day
        lInterval = 60;
        lLabelInterval = 60 * 6;
        dInterval = DateInterval.Minute;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 60 * 24 * 8)
      {
        //Under a Week
        lInterval = 12;
        lLabelInterval = 24;
        dInterval = DateInterval.Hour;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 60 * 24 * 31)
      {
        //Under 30 Days
        lInterval = 24;
        lLabelInterval = 24 * 7;
        dInterval = DateInterval.Hour;
      }
      else if (Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd)) <= 60 * 24 * 366)
      {
        //Under a Year
        lInterval = 7;
        lLabelInterval = 30;
        dInterval = DateInterval.Day;
      }
      else
      {
        //Over a year
        lInterval = 30;
        lLabelInterval = 365;
        dInterval = DateInterval.Day;
      }
      long lMaxTime = Math.Abs(DateDiff(dInterval, lStart, lEnd));
      if (lMaxTime == 0)
      {
        return new Bitmap(1, 1);
      }
      long lLineWidth = ImgSize.Width - lYWidth;
      double dCompInter = lLineWidth / (double)lMaxTime;
      for (long I = 0; I <= lMaxTime; I += lInterval)
      {
        int lX = (int)(lYWidth + (I * dCompInter));
        g.DrawLine(new Pen(ColorText), lX, ImgSize.Height - (lXHeight - 3), lX, ImgSize.Height - lXHeight);
      }
      for (long I = 0; I <= lMaxTime; I += lLabelInterval)
      {
        int lX = (int)(lYWidth + (I * dCompInter));
        g.DrawLine(new Pen(ColorText), lX, ImgSize.Height - (lXHeight - 5), lX, ImgSize.Height - lXHeight);
        string sDisp = null;
        if (DateDiff(DateInterval.Day, lStart, lEnd) > 1)
        {
          sDisp = DateAdd(dInterval, I, lStart).ToString("d");
        }
        else if (DateDiff(DateInterval.Day, lStart, lEnd) < 1)
        {
          sDisp = DateAdd(dInterval, I, lStart).ToString("t");
        }
        else
        {
          sDisp = DateAdd(dInterval, I, lStart).ToString("g");
        }
        g.DrawString(sDisp, tFont, new SolidBrush(ColorText), lX - (g.MeasureString(sDisp, tFont).Width / 2), ImgSize.Height - lXHeight + 5);
      }
      int MaxY = (int)(yTop + yHeight - (Data [Data.Length - 1].DOWNLIM / (double)lMax * yHeight));
      Point[] lMaxPoints = new Point[lMaxTime + 1];
      long lastLVal = 0;
      for (long I = 0; I <= lMaxTime; I++)
      {
        long lVal = 0;
        long lLow = long.MaxValue;
        long lHigh = 0;
        for (int J = 0; J <= Data.Length - 1; J++)
        {
          if (Math.Abs(DateDiff(dInterval, Data [J].DATETIME, DateAdd(dInterval, I, lStart))) == 0)
          {
            if (lHigh < Data [J].DOWNLIM)
            {
              lHigh = Data [J].DOWNLIM;
            }
            if (lLow > Data [J].DOWNLIM)
            {
              lLow = Data [J].DOWNLIM;
            }
          }
        }
        if (lHigh > 0 & lLow < long.MaxValue)
        {
          lVal = (lHigh + lLow) / 2;
        }
        if (lVal == -1 & lastLVal > 0)
        {
          lVal = lastLVal;
        }
        lMaxPoints [I].X = (int)(lYWidth + (I * dCompInter));
        lMaxPoints [I].Y = (int)(yTop + yHeight - (lVal / (double)lMax * yHeight));
        lastLVal = lVal;
      }

      lastLVal = 0;
      Point[] lDPoints = new Point[lMaxTime + 4];
      byte[] lTypes = new byte[lMaxTime + 4];
      for (long I = 0; I <= lMaxTime; I++)
      {
        long lDVal = 0;
        long lLow = long.MaxValue;
        long lHigh = 0;
        for (int J = 0; J <= Data.Length - 1; J++)
        {
          if (Math.Abs(DateDiff(dInterval, Data [J].DATETIME, DateAdd(dInterval, I, lStart))) == 0)
          {
            if (lHigh < Data [J].DOWNLOAD)
            {
              lHigh = Data [J].DOWNLOAD;
            }
            if (lLow > Data [J].DOWNLOAD)
            {
              lLow = Data [J].DOWNLOAD;
            }
          }
        }
        if (lHigh > 0 & lLow < long.MaxValue)
        {
          lDVal = (lHigh + lLow) / 2;
        }
        if (lDVal == -1 & lastLVal > 0)
        {
          lDVal = lastLVal;
        }
        lDPoints [I].X = (int)(lYWidth + (I * dCompInter));
        lDPoints [I].Y = (int)(yTop + yHeight - (lDVal / (double)lMax * yHeight));
        lastLVal = lDVal;
      }
      if (lDPoints [lMaxTime].IsEmpty)
      {
        lDPoints [lMaxTime] = new Point(ImgSize.Width, yTop + yHeight);
      }
      lDPoints [lMaxTime + 1] = new Point(ImgSize.Width, yTop + yHeight);
      lDPoints [lMaxTime + 2] = new Point(lYWidth, yTop + yHeight);
      lDPoints [lMaxTime + 3] = lDPoints [0];
      lTypes [0] = (byte)PathPointType.Start;
      for (long I = 1; I <= lMaxTime + 2; I++)
      {
        lTypes [I] = (byte)PathPointType.Line;
      }
      lTypes [lMaxTime + 3] = (byte)(PathPointType.Line | PathPointType.CloseSubpath);

      LinearGradientBrush fBrush = TriGradientBrush(new Point(lYWidth, MaxY), new Point(lYWidth, yTop + yHeight), ColorA, ColorB, ColorC);
      fBrush.WrapMode = WrapMode.TileFlipX;
      g.DrawLines(new Pen(new SolidBrush(ColorMax), 5), lMaxPoints);
      g.FillPath(fBrush, new GraphicsPath(lDPoints, lTypes));
      g.DrawLines(new Pen(new SolidBrush(Color.FromArgb(96,ColorMax)), 5), lMaxPoints);
      g.Dispose();
      return iPic;
    }

    #endregion

    #region "Progress"
    public static System.Drawing.Font MonospaceFont(float Size)
    {
      try
      {
        if (System.Drawing.FontFamily.GenericMonospace.IsStyleAvailable(FontStyle.Regular))
          return (new Font(System.Drawing.FontFamily.GenericMonospace, Size));
        else
        {
          System.Collections.Generic.List<string> fontList = new System.Collections.Generic.List<string>();
          foreach (System.Drawing.FontFamily fam in System.Drawing.FontFamily.Families)
          {
            if (fam.IsStyleAvailable(FontStyle.Regular))
              fontList.Add(fam.Name);
          }
          if (fontList.Contains("Courier New"))
            return new Font("Courier New", Size);
          else if (fontList.Contains("Consolas"))
            return new Font("Consolas", Size);
          else if (fontList.Contains("Lucida Console"))
            return new Font("Lucida Console",Size);
          else
            return new Font(SystemFonts.DefaultFont.Name,Size);
        }
      }
      catch
      {
        return (new Font(SystemFonts.DefaultFont.Name, Size));
      }
    }

    public static Image DisplayProgress(Gdk.Size ImgSize, long Current, long Total, int Accuracy, Color ColorA, Color ColorB, Color ColorC, Color ColorText, Color ColorBG)
    {
      if (ImgSize.IsEmpty)
      {
        return null;
      }
      double dTotal = (double)Total;
      double dCurrent = (double)Current;
      if (Total == 0)
      {
        Image bmpTmp = new Bitmap(ImgSize.Width, ImgSize.Height);
        float fontSize = 8;
        if (ImgSize.Width < ImgSize.Height)
        {
          if (ImgSize.Width / 12 > 8)
          {
            fontSize = ImgSize.Width / 12;
          }
          else
          {
            fontSize = 8;
          }
        }
        else
        {
          if (ImgSize.Height / 12 > 8)
          {
            fontSize = ImgSize.Height / 12;
          }
          else
          {
            fontSize = 8;
          }
        }
        using (Graphics g = Graphics.FromImage(bmpTmp))
        {
          g.Clear(ColorBG);
          Font fFont = MonospaceFont(fontSize);
          LinearGradientBrush linGrBrush = TriGradientBrush(new Point(0, 0), new Point(0, ImgSize.Height), ColorA, ColorB, ColorC);
          g.FillRectangle(linGrBrush, 0, 0, ImgSize.Width, ImgSize.Height);
          string Msg = "Loading...";
          PointF pF = new PointF(ImgSize.Width / 2 - g.MeasureString(Msg, fFont).Width / 2, ImgSize.Height / 2 - g.MeasureString(Msg, fFont).Height / 2);
          g.DrawString(Msg, fFont, new SolidBrush(Color.FromArgb(128, ColorBG)), pF.X + 2, pF.Y + 2);
          g.DrawString(Msg, fFont, new SolidBrush(ColorBG), pF.X + 1, pF.Y + 1);
          g.DrawString(Msg, fFont, new SolidBrush(ColorText), pF);
        }
        return bmpTmp;
      }
      else
      {
        long Val = 0;
        string Msg = null;
        Image bmpTmp = new Bitmap(ImgSize.Width, ImgSize.Height);
        float fontSize = 8;
        if (ImgSize.Width < ImgSize.Height)
        {
          if (ImgSize.Width / 12 > 8)
          {
            fontSize = ImgSize.Width / 12;
          }
          else
          {
            fontSize = 8;
          }
        }
        else
        {
          if (ImgSize.Height / 12 > 8)
          {
            fontSize = ImgSize.Height / 12;
          }
          else
          {
            fontSize = 8;
          }
        }
        Font fFont = MonospaceFont(fontSize);
        LinearGradientBrush linGrBrush = TriGradientBrush(new Point(0, 0), new Point(0, ImgSize.Height), ColorA, ColorB, ColorC);
        if (Current < Total)
        {
          using (Graphics g = Graphics.FromImage(bmpTmp))
          {
            g.Clear(ColorBG);
            if (Total > 0 & Current > 0)
            {
              Val = (long)Math.Round((dTotal - dCurrent) / dTotal * ImgSize.Height);
              g.FillRectangle(linGrBrush, 0, Val, ImgSize.Width, ImgSize.Height - Val);
              for (double I = -1; I <= ImgSize.Height + 1; I += ((ImgSize.Height + 2.0) / 10.0))
              {
                g.DrawLine(new Pen(ColorBG), 0, Convert.ToInt32(I), ImgSize.Width, Convert.ToInt32(I));
              }
              Msg = FormatPercent(dCurrent / dTotal, Accuracy);
            }
            else
            {
              Msg = "0%";
            }
            PointF pF = new PointF(ImgSize.Width / 2 - g.MeasureString(Msg, fFont).Width / 2, ImgSize.Height / 2 - g.MeasureString(Msg, fFont).Height / 2);
            g.DrawString(Msg, fFont, new SolidBrush(Color.FromArgb(128, ColorBG)), pF.X + 2, pF.Y + 2);
            g.DrawString(Msg, fFont, new SolidBrush(ColorBG), pF.X + 1, pF.Y + 1);
            g.DrawString(Msg, fFont, new SolidBrush(ColorText), pF);
          }
        }
        else
        {
          using (Graphics g = Graphics.FromImage(bmpTmp))
          {
            g.Clear(ColorBG);
            if (Total > 0 & Current > 0)
            {
              g.FillRectangle(linGrBrush, 0, 0, ImgSize.Width, ImgSize.Height);
              for (double I = -1; I <= ImgSize.Height + 1; I += ((ImgSize.Height + 2.0) / 10.0))
              {
                g.DrawLine(new Pen(ColorBG), 0, Convert.ToInt32(I), ImgSize.Width, Convert.ToInt32(I));
              }
              Msg = FormatPercent(dCurrent / dTotal, Accuracy);
            }
            else
            {
              Msg = "0%";
            }
            PointF pF = new PointF(ImgSize.Width / 2 - g.MeasureString(Msg, fFont).Width / 2, ImgSize.Height / 2 - g.MeasureString(Msg, fFont).Height / 2);
            g.DrawString(Msg, fFont, new SolidBrush(Color.FromArgb(128, ColorBG)), pF.X + 2, pF.Y + 2);
            g.DrawString(Msg, fFont, new SolidBrush(ColorBG), pF.X + 1, pF.Y + 1);
            g.DrawString(Msg, fFont, new SolidBrush(ColorText), pF);
          }
        }
        return bmpTmp;
      }
    }

    public static Image DisplayEProgress(Gdk.Size ImgSize, long Down, long Up, long Over, long Total, int Accuracy, Color ColorDA, Color ColorDB, Color ColorDC, Color ColorUA, Color ColorUB, Color ColorUC, Color ColorText, Color ColorBG)
    {
      if (ImgSize.IsEmpty)
      {
        return null;
      }
      string Msg = null;
      Image bmpTmp = new Bitmap(ImgSize.Width, ImgSize.Height);
      float fontSize = 8;
      if (ImgSize.Width < ImgSize.Height)
      {
        if (ImgSize.Width / 12 > 8)
        {
          fontSize = ImgSize.Width / 12;
        }
        else
        {
          fontSize = 8;
        }
      }
      else
      {
        if (ImgSize.Height / 12 > 8)
        {
          fontSize = ImgSize.Height / 12;
        }
        else
        {
          fontSize = 8;
        }
      }
      Font fFont = MonospaceFont(fontSize);
      LinearGradientBrush downBrush = TriGradientBrush(new Point(0, 0), new Point(ImgSize.Width, 0), ColorDC, ColorDB, ColorDA);
      LinearGradientBrush upBrush = TriGradientBrush(new Point(0, 0), new Point(ImgSize.Width, 0), ColorUC, ColorUB, ColorUA);
      if (Down + Up + Over < Total)
      {
        using (Graphics g = Graphics.FromImage(bmpTmp))
        {
          g.Clear(ColorBG);
          if (Total > 0 & (Down > 0 | Up > 0))
          {
            long upWidth = (long)Math.Round(ImgSize.Width - ((Total - (double)Up) / Total * ImgSize.Width));
            g.FillRectangle(upBrush, 0, 0, upWidth, ImgSize.Height);
            long downWidth = (long)Math.Round(ImgSize.Width - ((Total - (double)Down) / Total * ImgSize.Width));
            g.FillRectangle(downBrush, upWidth, 0, downWidth, ImgSize.Height);
            for (double I = -1; I <= ImgSize.Width + 1; I += ((ImgSize.Width + 2.0) / 10.0))
            {
              g.DrawLine(new Pen(ColorBG), Convert.ToInt32(I), 0, Convert.ToInt32(I), ImgSize.Height);
            }
            if (Over > 0)
            {
              Msg = "Down:  " + FormatPercent((double)Down / Total, Accuracy) + "\nUp:    " + FormatPercent((double)Up / Total, Accuracy) + "\nOver:  " + FormatPercent((double)Over / Total, Accuracy) + "\nTotal: " + FormatPercent(((double)Down + Up + Over) / Total, Accuracy);
            }
            else
            {
              Msg = "Down:  " + FormatPercent((double)Down / Total, Accuracy) + "\nUp:    " + FormatPercent((double)Up / Total, Accuracy) + "\nTotal: " + FormatPercent(((double)Down + Up) / Total, Accuracy);
            }
          }
          else
          {
            Msg = "0%";
          }
          PointF pF = new PointF(ImgSize.Width / 2 - g.MeasureString(Msg, fFont).Width / 2, ImgSize.Height / 2 - g.MeasureString(Msg, fFont).Height / 2);
          g.DrawString(Msg, fFont, new SolidBrush(Color.FromArgb(128, ColorBG)), pF.X + 2, pF.Y + 2);
          g.DrawString(Msg, fFont, new SolidBrush(ColorBG), pF.X + 1, pF.Y + 1);
          g.DrawString(Msg, fFont, new SolidBrush(ColorText), pF);
        }
      }
      else
      {
        using (Graphics g = Graphics.FromImage(bmpTmp))
        {
          g.Clear(ColorBG);
          if (Total > 0 & (Down > 0 | Up > 0))
          {
            long fillT = Down + Up;
            long upWidth = (long)Math.Round(ImgSize.Width - ((fillT - (double)Up) / fillT * ImgSize.Width));
            g.FillRectangle(upBrush, 0, 0, upWidth, ImgSize.Height);
            long downWidth = (long)Math.Round(ImgSize.Width - ((fillT - (double)Down) / fillT * ImgSize.Width));
            g.FillRectangle(downBrush, upWidth, 0, downWidth, ImgSize.Height);
            for (double I = -1; I <= ImgSize.Width + 1; I += ((ImgSize.Width + 2.0) / 10.0))
            {
              g.DrawLine(new Pen(ColorBG), Convert.ToInt32(I), 0, Convert.ToInt32(I), ImgSize.Height);
            }
            if (Over > 0)
            {
              Msg = "Down:  " + FormatPercent((double)Down / Total, Accuracy) + "\nUp:    " + FormatPercent((double)Up / Total, Accuracy) + "\nOver:  " + FormatPercent((double)Over / Total, Accuracy) + "\nTotal: " + FormatPercent(((double)Down + Up + Over) / Total, Accuracy);
            }
            else
            {
              Msg = "Down:  " + FormatPercent((double)Down / Total, Accuracy) + "\nUp:    " + FormatPercent((double)Up / Total, Accuracy) + "\nTotal: " + FormatPercent(((double)Down + Up) / Total, Accuracy);
            }

          }
          else
          {
            Msg = "0%";
          }
          PointF pF = new PointF(ImgSize.Width / 2 - g.MeasureString(Msg, fFont).Width / 2, ImgSize.Height / 2 - g.MeasureString(Msg, fFont).Height / 2);
          g.DrawString(Msg, fFont, new SolidBrush(Color.FromArgb(128, ColorBG)), pF.X + 2, pF.Y + 2);
          g.DrawString(Msg, fFont, new SolidBrush(ColorBG), pF.X + 1, pF.Y + 1);
          g.DrawString(Msg, fFont, new SolidBrush(ColorText), pF);
        }
      }
      return bmpTmp;
    }

    public static Image DisplayRProgress(Gdk.Size ImgSize, long Down, long Total, int Accuracy, Color ColorA, Color ColorB, Color ColorC, Color ColorText, Color ColorBG)
    {
      if (ImgSize.IsEmpty)
      {
        return null;
      }
      string Msg = null;
      Image bmpTmp = new Bitmap(ImgSize.Width, ImgSize.Height);
      float fontSize = 8;
      if (ImgSize.Width < ImgSize.Height)
      {
        if (ImgSize.Width / 12 > 8)
        {
          fontSize = ImgSize.Width / 12;
        }
        else
        {
          fontSize = 8;
        }
      }
      else
      {
        if (ImgSize.Height / 12 > 8)
        {
          fontSize = ImgSize.Height / 12;
        }
        else
        {
          fontSize = 8;
        }
      }
      Font fFont = MonospaceFont(fontSize);
      LinearGradientBrush downBrush = TriGradientBrush(new Point(0, 0), new Point(ImgSize.Width, 0), ColorC, ColorB, ColorA);
      if (Down < Total)
      {
        using (Graphics g = Graphics.FromImage(bmpTmp))
        {
          g.Clear(ColorBG);
          if (Total > 0 & Down > 0)
          {
            long downWidth = (long)Math.Round(ImgSize.Width - (((Total - (double)Down) / Total) * ImgSize.Width));
            g.FillRectangle(downBrush, 0, 0, downWidth, ImgSize.Height);
            for (double I = -1; I <= ImgSize.Width + 1; I += ((ImgSize.Width + 2.0) / 10.0))
            {
              g.DrawLine(new Pen(ColorBG), Convert.ToInt32(I), 0, Convert.ToInt32(I), ImgSize.Height);
            }
            Msg = FormatPercent((double)Down / Total, Accuracy);
          }
          else
          {
            Msg = "0%";
          }
          PointF pF = new PointF(ImgSize.Width / 2 - g.MeasureString(Msg, fFont).Width / 2, ImgSize.Height / 2 - g.MeasureString(Msg, fFont).Height / 2);
          g.DrawString(Msg, fFont, new SolidBrush(Color.FromArgb(128, ColorBG)), pF.X + 2, pF.Y + 2);
          g.DrawString(Msg, fFont, new SolidBrush(ColorBG), pF.X + 1, pF.Y + 1);
          g.DrawString(Msg, fFont, new SolidBrush(ColorText), pF);
        }
      }
      else
      {
        using (Graphics g = Graphics.FromImage(bmpTmp))
        {
          g.Clear(ColorBG);
          if (Total > 0 & Down > 0)
          {
            g.FillRectangle(downBrush, 0, 0, ImgSize.Width, ImgSize.Height);
            for (double I = -1; I <= ImgSize.Width + 1; I += ((ImgSize.Width + 2.0) / 10.0))
            {
              g.DrawLine(new Pen(ColorBG), Convert.ToInt32(I), 0, Convert.ToInt32(I), ImgSize.Height);
            }
            Msg = FormatPercent((double)Down / Total, Accuracy);
          }
          else
          {
            Msg = "0%";
          }
          PointF pF = new PointF(ImgSize.Width / 2 - g.MeasureString(Msg, fFont).Width / 2, ImgSize.Height / 2 - g.MeasureString(Msg, fFont).Height / 2);
          g.DrawString(Msg, fFont, new SolidBrush(Color.FromArgb(128, ColorBG)), pF.X + 2, pF.Y + 2);
          g.DrawString(Msg, fFont, new SolidBrush(ColorBG), pF.X + 1, pF.Y + 1);
          g.DrawString(Msg, fFont, new SolidBrush(ColorText), pF);
        }
      }
      return bmpTmp;
    }

    private static LinearGradientBrush TriGradientBrush(Point point1, Point point2, Color ColorA, Color ColorB, Color ColorC)
    {
      if (point1.Equals(point2))
      {
        return new LinearGradientBrush(point1, new Point(point2.X, point2.Y + 1), ColorC, ColorA);
      }
      LinearGradientBrush tBrush = default(LinearGradientBrush);
      if (ColorB.A == 0)
      {
        tBrush = new LinearGradientBrush(point1, point2, ColorC, ColorA);
      }
      else
      {
        tBrush = new LinearGradientBrush(point1, point2, Color.Black, Color.Black);
        ColorBlend cb = new ColorBlend();
        cb.Positions = new float[] {0f, 0.5f, 1f};
        cb.Colors = new Color[] {ColorC, ColorB, ColorA};
        tBrush.InterpolationColors = cb;
      }
      return tBrush;
    }

    public static string FormatPercent(double val, int Accuracy)
    {
      System.Globalization.NumberFormatInfo nfi = new System.Globalization.CultureInfo("en-us").NumberFormat;
      nfi.PercentDecimalDigits = Accuracy;
      int[] groupSize = {0};
      nfi.PercentGroupSizes = groupSize;
      return (val.ToString("P", nfi));
    }
    #endregion

    #region "Tray"
    private const int Alpha = 192;

    public static void CreateTrayIcon_Left(ref Graphics g, long lUsed, long lLim, Color cA, Color cB, Color cC, int iSize)
    {
      LinearGradientBrush fillBrush = default(LinearGradientBrush);
      if (cB.A == 0)
      {
        fillBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, iSize), Color.FromArgb(Alpha, cC), Color.FromArgb(Alpha, cA));
      }
      else
      {
        fillBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, iSize), Color.Black, Color.Black);
        ColorBlend cBlend = new ColorBlend();
        cBlend.Positions = new float[] {0f, 0.5f, 1f};
        cBlend.Colors = new Color[]
        {
          Color.FromArgb(Alpha, cC),
          Color.FromArgb(Alpha, cB),
          Color.FromArgb(Alpha, cA)
        };
        fillBrush.InterpolationColors = cBlend;
      }
      long yUsed = (long) Math.Round(iSize - ((double) lUsed / lLim * iSize));
      if (yUsed < 0)
      {
        yUsed = 0;
      }
      if (yUsed > iSize)
      {
        yUsed = iSize;
      }
      g.FillRectangle(fillBrush, 0, yUsed, (float) Math.Floor(iSize / 2d), iSize - yUsed);
    }

    public static void CreateTrayIcon_Right(ref Graphics g, long lUsed, long lLim, Color cA, Color cB, Color cC, int iSize)
    {
      LinearGradientBrush fillBrush = default(LinearGradientBrush);
      if (cB.A == 0)
      {
        fillBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, iSize), Color.FromArgb(Alpha, cC), Color.FromArgb(Alpha, cA));
      }
      else
      {
        fillBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, iSize), Color.Black, Color.Black);
        ColorBlend cBlend = new ColorBlend();
        cBlend.Positions = new float[] {0f, 0.5f, 1f};
        cBlend.Colors = new Color[]
        {
          Color.FromArgb(Alpha, cC),
          Color.FromArgb(Alpha, cB),
          Color.FromArgb(Alpha, cA)
        };
        fillBrush.InterpolationColors = cBlend;
      }
      long yUsed = (long) Math.Round(iSize - ((double) lUsed / lLim * iSize));
      if (yUsed < 0)
      {
        yUsed = 0;
      }
      if (yUsed > iSize)
      {
        yUsed = iSize;
      }
      g.FillRectangle(fillBrush, (float) Math.Floor(iSize / 2d), yUsed, (float) Math.Floor(iSize / 2d), iSize - yUsed);
    }

    public static void CreateTrayIcon_Dual(ref Graphics g, long lDown, long lUp, long lLim, Color cDA, Color cDB, Color cDC, Color cUA, Color cUB, Color cUC, int iSize)
    {
      LinearGradientBrush upBrush = default(LinearGradientBrush);
      if (cUB.A == 0)
      {
        upBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, iSize), Color.FromArgb(Alpha, cUC), Color.FromArgb(Alpha, cUA));
      }
      else
      {
        upBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, iSize), Color.Black, Color.Black);
        ColorBlend cBlend = new ColorBlend();
        cBlend.Positions = new float[] {0f ,0.5f ,1f};
        cBlend.Colors = new Color[]
        {
          Color.FromArgb(Alpha, cUC),
          Color.FromArgb(Alpha, cUB),
          Color.FromArgb(Alpha, cUA)
        };
        upBrush.InterpolationColors = cBlend;
      }
      LinearGradientBrush downBrush = default(LinearGradientBrush);
      if (cDB.A == 0)
      {
        downBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, iSize), Color.FromArgb(Alpha, cDC), Color.FromArgb(Alpha, cDA));
      }
      else
      {
        downBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, iSize), Color.Black, Color.Black);
        ColorBlend cBlend = new ColorBlend();
        cBlend.Positions = new float[] {0f, 0.5f, 1f};
        cBlend.Colors = new Color[]
        {
          Color.FromArgb(Alpha, cDC),
          Color.FromArgb(Alpha, cDB),
          Color.FromArgb(Alpha, cDA)
        };
        downBrush.InterpolationColors = cBlend;
      }
      if (lDown + lUp > lLim)
      {
        //Maxed
        long fillLim = lDown + lUp;
        long yUp = (long) Math.Round(iSize - ((double) lUp / fillLim * iSize));
        long yDown = (long) Math.Round(yUp - ((double) lDown / fillLim * iSize));
        g.FillRectangle(downBrush, 0, yDown, iSize, iSize - ((iSize - yUp) - 1) - yDown);
        g.FillRectangle(upBrush, 0, yUp, iSize, iSize - yUp);
      }
      else
      {
        long yUp = (long) Math.Round(iSize - ((double) lUp / lLim * iSize));
        long yDown = (long) Math.Round(yUp - ((double) lDown / lLim * iSize));
        g.FillRectangle(downBrush, 0, yDown, iSize, iSize - ((iSize - yUp) - 1) - yDown);
        g.FillRectangle(upBrush, 0, yUp, iSize, iSize - yUp);
      }
    }
    #endregion
    #endregion

    /// <summary>
    /// Attempts to see if a file is in use, waiting up to five seconds for it to be freed.
    /// </summary>
    /// <param name="Filename">The exact path to the file which needs to be checked.</param>
    /// <param name="access">Write permissions required for checking.</param>
    /// <returns>True on available, false on in use.</returns>
    /// <remarks></remarks>
    public static bool InUseChecker(string Filename, System.IO.FileAccess access)
    {
      if (!System.IO.File.Exists(Filename))
      {
        return true;
      }
      long iStart = TickCount();
      do
      {
        try
        {
          switch (access)
          {
            case FileAccess.Read:
              //only check for ability to read
              using (FileStream fs = System.IO.File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
              {
                if (fs.CanRead)
                {
                  return true;
                }
              }

              break;
              case FileAccess.Write:
              case FileAccess.ReadWrite:
              //check for ability to write
              using (FileStream fs = System.IO.File.Open(Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
              {
                if (fs.CanWrite)
                {
                  return true;
                }
              }
              break;
          }
        }
        catch (Exception)
        {
        }
        System.Threading.Thread.Sleep(0);
        System.Threading.Thread.Sleep(100);
      } while (TickCount() - iStart < 5000);
      return false;
    }

    public static DateTime DateAdd(DateInterval interval, double number, DateTime dateValue)
    {
      switch (interval)
      {
        case DateInterval.Day:
          return dateValue.AddDays(number);
          case DateInterval.DayOfYear:
          return dateValue.AddDays(number);
          case DateInterval.Hour:
          return dateValue.AddHours(number);
          case DateInterval.Minute:
          return dateValue.AddMinutes(number);
          case DateInterval.Month:
          return dateValue.AddMonths((int)number);
          case DateInterval.Quarter:
          return dateValue.AddMonths((int)number * 3);
          case DateInterval.Second:
          return dateValue.AddSeconds(number);
          case DateInterval.Weekday:
          return dateValue;
          case DateInterval.WeekOfYear:
          return dateValue;
          default:
          return dateValue;
      }
    }

    public static long DateDiff(DateInterval interval, DateTime date1, DateTime date2)
    {
      switch (interval)
      { 
        case DateInterval.Day:
          case DateInterval.DayOfYear:
          System.TimeSpan spanForDays = date2 - date1;
          return (long)spanForDays.TotalDays;
          case DateInterval.Hour:
          System.TimeSpan spanForHours = date2 - date1;
          return (long)spanForHours.TotalHours;
          case DateInterval.Minute:
          System.TimeSpan spanForMinutes = date2 - date1;
          return (long)spanForMinutes.TotalMinutes;
          case DateInterval.Month:
          return ((date2.Year - date1.Year) * 12) + (date2.Month - date1.Month);
          case DateInterval.Quarter:
          long dateOneQuarter = (long)System.Math.Ceiling(date1.Month / 3.0);
          long dateTwoQuarter = (long)System.Math.Ceiling(date2.Month / 3.0);
          return (4 * (date2.Year - date1.Year)) + dateTwoQuarter - dateOneQuarter;
          case DateInterval.Second:
          System.TimeSpan spanForSeconds = date2 - date1;
          return (long)spanForSeconds.TotalSeconds;
          case DateInterval.Weekday:
          System.TimeSpan spanForWeekdays = date2 - date1;
          return (long)(spanForWeekdays.TotalDays / 7.0);
          case DateInterval.WeekOfYear:
          System.DateTime dateOneModified = date1;
          System.DateTime dateTwoModified = date2;
          while (dateTwoModified.DayOfWeek != System.Globalization.DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek)
          {
            dateTwoModified = dateTwoModified.AddDays(-1);
          }
          while (dateOneModified.DayOfWeek != System.Globalization.DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek)
          {
            dateOneModified = dateOneModified.AddDays(-1);
          }
          System.TimeSpan spanForWeekOfYear = dateTwoModified - dateOneModified;
          return (long)(spanForWeekOfYear.TotalDays / 7.0);
          case DateInterval.Year:
          return date2.Year - date1.Year;
          default:
          return 0;
      }
    }

    public static bool IsNumeric(string value)
    {
      string ret = value;
      for (int i = 0; i < 10; i++)
      {
        ret = ret.Replace(i.ToString().Trim(), "");
      }
      return ret.Length == 0;
    }

    public static long TickCount()
    {
      return (System.Diagnostics.Stopwatch.GetTimestamp() / System.Diagnostics.Stopwatch.Frequency) * 1000;
    }

    public static string ProductVersion()
    {
      System.Reflection.Assembly srt = System.Reflection.Assembly.GetExecutingAssembly();
      System.Diagnostics.FileVersionInfo fInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(srt.Location);
      return fInfo.FileVersion;
    }

    public static string CompanyName()
    {
      System.Reflection.Assembly srt = System.Reflection.Assembly.GetExecutingAssembly();
      System.Diagnostics.FileVersionInfo fInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(srt.Location);
      return fInfo.CompanyName;
    }

    public static string ProductName()
    {
      System.Reflection.Assembly srt = System.Reflection.Assembly.GetExecutingAssembly();
      System.Diagnostics.FileVersionInfo fInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(srt.Location);
      return fInfo.ProductName;
    }

    public static Gtk.ResponseType ShowMessageBox(Gtk.Window parent, string text, string title, Gtk.DialogFlags flags, Gtk.MessageType icon, Gtk.ButtonsType buttons)
    {
      Gtk.MessageDialog dlg = new Gtk.MessageDialog(parent, flags, icon, buttons, text);
      if (String.IsNullOrEmpty(title))
      {
        dlg.Title = modFunctions.ProductName();
      }
      else
      {
        dlg.Title = title;
      }
      Gtk.ResponseType ret = (Gtk.ResponseType)dlg.Run();
      dlg.Destroy();
      return ret;
    }

    public static Gtk.ResponseType ShowMessageBoxYNC(Gtk.Window parent, string text, string title, Gtk.DialogFlags flags)
    {
      Gtk.MessageDialog dlg = new Gtk.MessageDialog(parent, flags, Gtk.MessageType.Question, Gtk.ButtonsType.None, text);
      dlg.AddButton(Gtk.Stock.Yes, Gtk.ResponseType.Yes);
      dlg.AddButton(Gtk.Stock.No, Gtk.ResponseType.No);
      dlg.AddButton(Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
      if (String.IsNullOrEmpty(title))
      {
        dlg.Title = modFunctions.ProductName();
      }
      else
      {
        dlg.Title = title;
      }
      Gtk.ResponseType ret = (Gtk.ResponseType)dlg.Run();
      dlg.Destroy();
      return ret;
    }

    public static Bitmap GetScreenRect(Rectangle rectIn)
    {
      Gdk.Window window = Gdk.Global.DefaultRootWindow;
      if (window != null)
      {           
        Gdk.Pixbuf scrnBuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, rectIn.Width, rectIn.Height);          
        scrnBuf.GetFromDrawable(window, Gdk.Colormap.System, rectIn.X, rectIn.Y, 0, 0, rectIn.Width, rectIn.Height);  
        return (Bitmap) PixbufToImage(scrnBuf);
      }
      return null;
    }

    public static Bitmap ReplaceColors(Bitmap bitIn, Color TransparencyKey, Bitmap bitBG)
    {
      Bitmap bitOut = new Bitmap(bitIn.Width, bitIn.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      using (Graphics g = Graphics.FromImage(bitOut))
      {
        g.DrawImageUnscaled(bitIn, 0, 0);
      }
      for (int cY = 0; cY < bitOut.Height; cY++)
      {
        for (int cX = 0; cX < bitOut.Width; cX++)
        {
          if (bitOut.GetPixel(cX, cY).ToArgb() == TransparencyKey.ToArgb())
          {
            if (bitBG == null)
            {
              bitOut.SetPixel(cX, cY, Color.Black);
            }
            else
            {
              bitOut.SetPixel(cX, cY, bitBG.GetPixel(cX, cY));
            }
          }
        }
      }
      return bitOut; 
    }

    public static Bitmap MakeTransparent(Bitmap bitIn, Color TransparencyKey)
    {
      return SwapColors(bitIn, TransparencyKey, Color.FromArgb(0, 0, 0, 0));
    }

    public static Bitmap SwapColors(Bitmap bitIn, Color InColor, Color OutColor)
    {
      Bitmap bitOut = new Bitmap(bitIn.Width, bitIn.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
      using (Graphics g = Graphics.FromImage(bitOut))
      {
        g.DrawImageUnscaled(bitIn, 0, 0);
      }
      for (int cY = 0; cY < bitOut.Height; cY++)
      {
        for (int cX = 0; cX < bitOut.Width; cX++)
        {
          if (bitOut.GetPixel(cX, cY).ToArgb() == InColor.ToArgb())
          {
            bitOut.SetPixel(cX, cY, OutColor);
          }
        }
      }
      return bitOut;
    }

    public static Color GdkColorToDrawingColor(Gdk.Color c)
    {
      return Color.FromArgb(255, (int)Math.Floor(c.Red / 256d), (int)Math.Floor(c.Green / 256d), (int)Math.Floor(c.Blue / 256d));
    }

    public static Gdk.Color DrawingColorToGdkColor(Color c)
    {
      Gdk.Color ret = new Gdk.Color((byte)c.R, (byte)c.G, (byte)c.B);
      ret.Pixel = (uint)c.A;
      return ret;
    }

    public static Size GdkSizeToDrawingSize(Gdk.Size s)
    {
      return new Size(s.Width, s.Height);
    }

    public static Gdk.Size DrawingSizeToGdkSize(Size s)
    {
      return new Gdk.Size(s.Width, s.Height);
    }

    public static Gdk.Pixbuf ImageToPixbuf(Image img)
    {
      using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
      {
        img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        ms.Position = 0;
        Gdk.Pixbuf ret = new Gdk.Pixbuf(ms);
        return ret;
      }
    }

    public static Image PixbufToImage(Gdk.Pixbuf pbf)
    {
      byte[] img = pbf.SaveToBuffer("bmp");
      using (System.IO.MemoryStream ms = new System.IO.MemoryStream(img))
      {
        Image ret = Image.FromStream(ms);
        return ret;
      }
    }

    public static bool IterativeEqualityCheck(byte[] inArray1, byte[] inArray2)
    {
      if (inArray1.Length == inArray2.Length)
      {
        for (int I = 0; I <= inArray1.Length - 1; I++)
        {
          if (!(inArray1 [I] == inArray2 [I]))
          {
            return false;
          }
        }
        return true;
      }
      else
      {
        return false;
      }
    }
  }
}