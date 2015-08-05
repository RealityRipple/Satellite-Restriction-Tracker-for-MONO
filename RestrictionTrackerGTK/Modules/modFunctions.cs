using System;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using RestrictionLibrary;
namespace RestrictionTrackerGTK
{
  static class modFunctions
  {
    #region "Alert Notifier"
    public static NotifierStyle NOTIFIER_STYLE = null;
    public static NotifierStyle LoadAlertStyle(string Path)
    {
      if (File.Exists(AppDataPath + Path + ".tgz"))
      {
        Path = AppDataPath + Path + ".tgz";
      }
      else if (File.Exists(AppDataPath + Path + ".tar.gz"))
      {
        Path = AppDataPath + Path + ".tar.gz";
      }
      else if (File.Exists(AppDataPath + Path + ".tar"))
      {
        Path = AppDataPath + Path + ".tar";
      }
      else
      {
        return new NotifierStyle();
      }
      try
      {
        string TempAlertDir = AppData + "/notifier/";
        string TempAlertTAR = AppData + "/notifier.tar";
        if (Path.EndsWith(".tar"))
        {
          ExtractTar(Path, TempAlertDir);
        }
        else
        {
          try
          {
            ExtractGZ(Path, TempAlertTAR);
            ExtractTar(TempAlertTAR, TempAlertDir);
            File.Delete(TempAlertTAR);
          }
          catch (Exception)
          {
            ExtractTar(Path, TempAlertDir);
          }
        }
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
      if (!sDestPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
      {
        sDestPath += Path.DirectorySeparatorChar.ToString();
      }
      else if (!sDestPath.EndsWith("" + Path.DirectorySeparatorChar))
      {
        sDestPath += Path.DirectorySeparatorChar.ToString();
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
    public static string LocalAppData
    {
      get
      {
        if (CurrentOS.IsMac)
        {
          return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support") + Path.DirectorySeparatorChar.ToString();
        }
        else
        {
          return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar.ToString();
        }
      }
    }
    static string static_AppData_sTmp;
    public static string AppDataPath
    {
      get
      {
        return Path.Combine(LocalAppData, CompanyName, ProductName) + Path.DirectorySeparatorChar.ToString();
      }
    }
    public static string AppData
    {
      get
      {
        if (!Directory.Exists(LocalAppData + CompanyName))
        {
          Directory.CreateDirectory(LocalAppData + CompanyName);
        }
        if (!Directory.Exists(AppDataPath))
        {
          Directory.CreateDirectory(AppDataPath);
        }
        if (string.IsNullOrEmpty(static_AppData_sTmp))
        {
          static_AppData_sTmp = AppDataPath;
        }
        return static_AppData_sTmp;
      }
    }
    public static string MySaveDir(bool Create = false)
    {
      AppSettings mySettings = new AppSettings();
      if (string.IsNullOrEmpty(mySettings.HistoryDir))
      {
        if (Create)
          mySettings.HistoryDir = AppData;
        else
          mySettings.HistoryDir = AppDataPath;
      }
      if (Create)
      {
        if (!Directory.Exists(mySettings.HistoryDir))
        {
          Directory.CreateDirectory(mySettings.HistoryDir);
        }
      }
      return mySettings.HistoryDir;
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
              return string.Format("{0,0:0.##}", (InBytes) / 1024d / 1024d / 1024d / 1024d) + " TB";
            }
            else
            {
              return string.Format("{0,0:0.##}", (InBytes) / 1024d / 1024d / 1024d) + " GB";
            }
          }
          else
          {
            return string.Format("{0,0:0.##}", (InBytes) / 1024d / 1024d) + " MB";
          }
        }
        else
        {
          return string.Format("{0,0:0.#}", (InBytes) / 1024d) + " KB";
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
        case "WBV":
          return localRestrictionTracker.SatHostTypes.WildBlue_EXEDE;
        case "RPL":
          return localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY;
        case "RPX":
        case "RURALPORTAL":
          return localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE;
        case "DNX":
        case "DISHNET":
          return localRestrictionTracker.SatHostTypes.DishNet_EXEDE;
        default:
          return localRestrictionTracker.SatHostTypes.Other;
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
        ApplicationVersion = ApplicationVersion.Substring(0, ApplicationVersion.Length - 2);
      }
      if (string.IsNullOrEmpty(ApplicationVersion))
        return sVersion;
      else
        return ApplicationVersion;
    }
    public static bool CompareVersions(string sRemote)
    {
      string sLocal = ProductVersion;
      string[] LocalVer = new string[4];
      if (sLocal.Contains("."))
      {
        for (int I = 0; I <= 3; I++)
        {
          if (sLocal.Split('.').Length > I)
          {
            string sTmp = sLocal.Split('.')[I].Trim();
            int iV;
            if (int.TryParse(sTmp, out iV) & sTmp.Length < 4)
            {
              sTmp += new string('0', 4 - sTmp.Length);
            }
            LocalVer[I] = sTmp;
          }
          else
          {
            LocalVer[I] = "0000";
          }
        }
      }
      else if (sLocal.Contains(","))
      {
        for (int I = 0; I <= 3; I++)
        {
          if (sLocal.Split(',').Length > I)
          {
            string sTmp = sLocal.Split(',')[I].Trim();
            int iV;
            if (int.TryParse(sTmp, out iV) & sTmp.Length < 4)
            {
              sTmp += new string('0', 4 - sTmp.Length);
            }
            LocalVer[I] = sTmp;
          }
          else
          {
            LocalVer[I] = "0000";
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
            string sTmp = sRemote.Split('.')[I].Trim();
            int iV;
            if (int.TryParse(sTmp, out iV) & sTmp.Length < 4)
            {
              sTmp += new string('0', 4 - sTmp.Length);
            }
            RemoteVer[I] = sTmp;
          }
          else
          {
            RemoteVer[I] = "0000";
          }
        }
      }
      else if (sRemote.Contains(","))
      {
        for (int I = 0; I <= 3; I++)
        {
          if (sRemote.Split(',').Length > I)
          {
            string sTmp = sRemote.Split(',')[I].Trim();
            int iV;
            if (int.TryParse(sTmp, out iV) & sTmp.Length < 4)
            {
              sTmp += new string('0', 4 - sTmp.Length);
            }
            RemoteVer[I] = sTmp;
          }
          else
          {
            RemoteVer[I] = "0000";
          }
        }
      }
      bool bUpdate = false;
      int[] LocalVal = new int[4];
      int[] RemoteVal = new int[4];
      for (int I = 0; I <= 3; I++)
      {
        LocalVal[I] = int.Parse(LocalVer[I]);
        RemoteVal[I] = int.Parse(RemoteVer[I]);
      }

      if (LocalVal[0] > RemoteVal[0])
      {
        //Local's OK
      }
      else if (LocalVal[0] == RemoteVal[0])
      {
        if (LocalVal[1] > RemoteVal[1])
        {
          //Local's OK
        }
        else if (LocalVal[1] == RemoteVal[1])
        {
          if (LocalVal[2] > RemoteVal[2])
          {
            //Local's OK
          }
          else if (LocalVal[2] == RemoteVal[2])
          {
            if (LocalVal[3] >= RemoteVal[3])
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
      try
      {
        string sFailFile = "SRT-MONO-ReadFail-" + DateTime.Now.ToString("G") + "-v" + ProductVersion + ".txt";
        sFailFile = sFailFile.Replace("/", "-");
        sFailFile = sFailFile.Replace(":", "-");
        System.Net.WebRequest ftpSave = System.Net.FtpWebRequest.Create("ftp://realityripple.com/" + sFailFile);
        ftpSave.Proxy = new System.Net.WebProxy();
        ftpSave.Credentials = modCreds.FTPCredentials(); //Use [new System.Net.NetworkCredential("FTPUSER", "FTPPASS");] to upload failures to a FTP location.
        ftpSave.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
        using (Stream ftpStream = ftpSave.GetRequestStream())
        {
          byte[] bHTTP = System.Text.Encoding.UTF8.GetBytes(sData.ToString());
          ftpStream.Write(bHTTP, 0, bHTTP.Length);
          ftpStream.Close();
        }
        MainClass.fMain.FailResponse(true);
      }
      catch
      {
        MainClass.fMain.FailResponse(false);
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
          sRet = inString[I].ToString() + sRet;
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
      if (Directory.Exists(FromDir))
      {
        bool bDidSomething = false;
        if (Directory.Exists(ToDir))
        {
          string[] wbFiles = Directory.GetFiles(FromDir);
          if (wbFiles.Length > 0)
          {
            string[] srtFiles = Directory.GetFiles(ToDir);
            System.Collections.ObjectModel.Collection<string> spareFiles = null;
            if (srtFiles.Length > 0)
            {
              spareFiles = new System.Collections.ObjectModel.Collection<string>();
              for (int I = 0; I <= wbFiles.Length - 1; I++)
              {
                bool isUnique = true;
                for (int J = 0; J <= srtFiles.Length - 1; J++)
                {
                  if (Path.GetFileName(srtFiles[J]).CompareTo(Path.GetFileName(wbFiles[I])) == 0)
                  {
                    isUnique = false;
                    break;
                  }
                }
                if (isUnique)
                {
                  spareFiles.Add(wbFiles[I]);
                }
              }
            }
            else
            {
              spareFiles = new System.Collections.ObjectModel.Collection<string>(srtFiles);
            }
            if (spareFiles.Count > 0)
            {
              Directory.CreateDirectory(ToDir);
              for (int I = 0; I <= spareFiles.Count - 1; I++)
              {
                string file = spareFiles[I];
                string sFName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(ToDir, sFName), true);
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
          string[] wbFiles = Directory.GetFiles(FromDir);
          if (wbFiles.Length > 0)
          {
            Directory.CreateDirectory(ToDir);
            for (int I = 0; I <= wbFiles.Length - 1; I++)
            {
              string file = wbFiles[I];
              string sFName = Path.GetFileName(file);
              File.Copy(file, Path.Combine(ToDir, sFName), true);
              bDidSomething = true;
            }
          }
        }
        if (!bDidSomething)
        {
          return 2;
        }
        string[] wFileTmp = Directory.GetFiles(FromDir);
        string[] sFileTmp = Directory.GetFiles(ToDir);
        bool Equal = true;
        if (wFileTmp.Length == sFileTmp.Length)
        {
          for (int I = 0; I <= wFileTmp.Length - 1; I++)
          {
            if ((Path.GetFileName(wFileTmp[I]).CompareTo(Path.GetFileName(sFileTmp[I])) == 0) & (new FileInfo(wFileTmp[I]).Length == new FileInfo(sFileTmp[I]).Length))
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
    public static void MoveDirectory(string FromDir, string ToDir)
    {
      if (Directory.Exists(FromDir))
      {
        if (!Directory.Exists(ToDir))
          Directory.CreateDirectory(ToDir);
        foreach (string sDir in Directory.GetDirectories(FromDir))
        {
          MoveDirectory(sDir, Path.Combine(ToDir, Path.GetFileName(sDir)));
        }
        foreach (string sFile in Directory.GetFiles(FromDir))
        {
          File.Move(sFile, Path.Combine(ToDir, Path.GetFileName(sFile)));
        }
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
    class ActivateLinkEventArgs : GLib.SignalArgs
    {
      public string Url { get { return (string)base.Args[0]; } }
    }
    [GLib.ConnectBefore]
    static void HandleActivateLink(object o, ActivateLinkEventArgs e)
    {
      System.Diagnostics.Process.Start(e.Url);
      e.RetVal = true;
    }
    public static void PrepareLink(Gtk.Label label)
    {
      var signal = GLib.Signal.Lookup(label, "activate-link", typeof(ActivateLinkEventArgs));
      signal.AddDelegate(new EventHandler<ActivateLinkEventArgs>(HandleActivateLink));
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
    public static Image DrawRGraph(DataBase.DataRow[] Data, Size ImgSize, Color ColorLine, Color ColorA, Color ColorB, Color ColorC, Color ColorText, Color ColorBG, Color ColorMax, Color ColorGridLight, Color ColorGridDark)
    {
      return DrawGraph(Data, 0, ImgSize, ColorLine, ColorA, ColorB, ColorC, ColorText, ColorBG, ColorMax, ColorGridLight, ColorGridDark);
    }
    public static Image DrawLineGraph(DataBase.DataRow[] Data, bool Down, Size ImgSize, Color ColorLine, Color ColorA, Color ColorB, Color ColorC, Color ColorText, Color ColorBG, Color ColorMax, Color ColorGridLight, Color ColorGridDark)
    {
      if (Down)
        return DrawGraph(Data, 1, ImgSize, ColorLine, ColorA, ColorB, ColorC, ColorText, ColorBG, ColorMax, ColorGridLight, ColorGridDark);
      else
        return DrawGraph(Data, 255, ImgSize, ColorLine, ColorA, ColorB, ColorC, ColorText, ColorBG, ColorMax, ColorGridLight, ColorGridDark);
    }
    private static Image DrawGraph(DataBase.DataRow[] Data, byte Down, Size ImgSize, Color ColorLine, Color ColorA, Color ColorB, Color ColorC, Color ColorText, Color ColorBG, Color ColorMax, Color ColorGridLight, Color ColorGridDark)
    {
      if (Data == null || Data.Length == 0)
      {
        return new Bitmap(1, 1);
      }
      long yMax = 0;
      long lMax = 0;
      if (Down == 0)
      {
        long yVMax = 0;
        for (int i = 0; i < Data.Length; i++)
        {
          if (yVMax < Data[i].DOWNLOAD)
            yVMax = Data[i].DOWNLOAD;
          if (yVMax < Data[i].DOWNLIM)
            yVMax = Data[i].DOWNLIM;
        }
        yMax = yVMax;
        if (yMax % 1000 != 0)
          yMax = (int)(((double)yMax / 1000) * 1000);
        lMax = yVMax;
      }
      else
      {
        long yDMax = 0;
        long yUMax = 0;
        for (int i = 0; i < Data.Length; i++)
        {
          if (yDMax < Data[i].DOWNLOAD)
            yDMax = Data[i].DOWNLOAD;
          if (yUMax < Data[i].UPLOAD)
            yUMax = Data[i].UPLOAD;
          if (yDMax < Data[i].DOWNLIM)
            yDMax = Data[i].DOWNLIM;
          if (yUMax < Data[i].UPLIM)
            yUMax = Data[i].UPLIM;
        }
        if (yDMax > yUMax)
          yMax = yDMax;
        else
          yMax = yUMax;
        if (yMax % 1000 != 0)
          yMax = (int)(((double)yMax / 1000) * 1000);
        if (Down == 1)
          lMax = yDMax;
        else
          lMax = yUMax;
      }
      if (lMax % 1000 != 0)
        lMax = (int)(((double)lMax / 1000) * 1000) + 1000;
      Image iPic = new Bitmap(ImgSize.Width, ImgSize.Height);
      Graphics g = Graphics.FromImage(iPic);
      Font tFont = new Font(FontFamily.GenericSansSerif, 7);
      g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
      int lYWidth = (int)g.MeasureString(yMax.ToString().Trim() + " MB", tFont).Width + 10;
      int lXHeight = (int)g.MeasureString(DateTime.Now.ToString("g"), tFont).Height + 10;
      g.Clear(ColorBG);
      int yTop = lXHeight / 2;
      int yHeight = (int)(ImgSize.Height - (lXHeight * 1.5));
      if (Down == 255)
      {
        uGraph = new Rectangle(lYWidth, yTop, (ImgSize.Width - 4) - lYWidth, yHeight);
        uData = Data;
      }
      else
      {
        dGraph = new Rectangle(lYWidth, yTop, (ImgSize.Width - 4) - lYWidth, yHeight);
        dData = Data;
      }
      g.DrawLine(new Pen(ColorText), lYWidth, yTop, lYWidth, yTop + yHeight);
      g.DrawLine(new Pen(ColorText), lYWidth, yTop + yHeight, ImgSize.Width, yTop + yHeight);
      oldDate = Data[0].DATETIME;
      newDate = Data[Data.Length - 1].DATETIME;
      for (int i = 0; i <= (int) lMax; i += (int) ((((lMax / ((double) yHeight / (tFont.Size + 12)))) / 100) * 100))
      {
        int iY = (int)(yTop + yHeight - (i / (double)lMax * yHeight));
        g.DrawString(i.ToString().Trim() + " MB", tFont, new SolidBrush(ColorText), lYWidth - g.MeasureString(i.ToString().Trim() + " MB", tFont).Width - 5, iY - (g.MeasureString(i.ToString().Trim() + " MB", tFont).Height / 2));
        g.DrawLine(new Pen(ColorText), lYWidth - 3, iY, lYWidth, iY);
      }
      for (long i = 0; i <= lMax; i+= (lMax / 10))
      {
        int iY = (int)(yTop + yHeight - (i / (double)lMax * yHeight));
        if (i > 0)
          g.DrawLine(new Pen(ColorGridLight), lYWidth + 1, iY, ImgSize.Width - 4, iY);
      }
      for (long i = 0; i <= lMax; i+= (lMax/5))
      {
        int iY = (int)(yTop + yHeight - (i / (double)lMax * yHeight));
        if (i > 0)
          g.DrawLine(new Pen(ColorGridDark), lYWidth + 1, iY, ImgSize.Width - 4, iY);
      }
      System.DateTime lStart = Data[0].DATETIME;
      System.DateTime lEnd = Data[Data.Length - 1].DATETIME;
      DateInterval dInterval = DateInterval.Minute;
      uint lInterval = 1;
      double lBitInterval = 1.0;
      uint lLabelInterval = 5;
      long lDiff = Math.Abs(DateDiff(DateInterval.Minute, lStart, lEnd));
      if (lDiff <= 61)
      {
        lInterval = 5;
        lBitInterval = 1;
        lLabelInterval = 10;
        dInterval = DateInterval.Minute;
      }
      else if (lDiff < 60 * 13)
      {
        lInterval = 60;
        lBitInterval = 30;
        lLabelInterval = 60 * 2;
        dInterval = DateInterval.Minute;
      }
      else if (lDiff <= 60 * 24)
      {
        lInterval = 6;
        lBitInterval = 1;
        lLabelInterval = 6;
        dInterval = DateInterval.Hour;
      }
      else if (lDiff <= 60 * 24 * 2)
      {
        lInterval = 12;
        lBitInterval = 6;
        lLabelInterval = 24;
        dInterval = DateInterval.Hour;
      }
      else if (lDiff <= 60 * 24 * 6)
      {
        lInterval = 24;
        lBitInterval = 12;
        lLabelInterval = 24;
        dInterval = DateInterval.Hour;
      }
      else if (lDiff <= 60 * 24 * 12)
      {
        lInterval = 2;
        lBitInterval = 1;
        lLabelInterval = 2;
        dInterval = DateInterval.Day;
      }
      else if (lDiff <= 60 * 24 * 20)
      {
        lInterval = 2;
        lBitInterval = 1;
        lLabelInterval = 4;
        dInterval = DateInterval.Day;
      }
      else if (lDiff <= 60 * 24 * 27)
      {
        lInterval = 2;
        lBitInterval = 1;
        lLabelInterval = 7;
        dInterval = DateInterval.Day;
      }
      else if (lDiff <= 60 * 24 * 40)
      {
        lInterval = 4;
        lBitInterval = 2;
        lLabelInterval = 7;
        dInterval = DateInterval.Day;
      }
      else if (lDiff <= 60 * 24 * 90)
      {
        lInterval = 14;
        lBitInterval = 7;
        lLabelInterval = 14;
        dInterval = DateInterval.Day;
      }
      else if (lDiff <= 60 * 24 * 240)
      {
        lInterval = 30;
        lBitInterval = 15;
        lLabelInterval = 30;
        dInterval = DateInterval.Day;
      }
      else if (lDiff <= 60 * 24 * 365)
      {
        lInterval = 60;
        lBitInterval = 30;
        lLabelInterval = 90;
        dInterval = DateInterval.Day;
      }
      else if (lDiff <= 60 * 24 * 365 * 2)
      {
        lInterval = 90;
        lBitInterval = 60;
        lLabelInterval = 180;
        dInterval = DateInterval.Day;
      }
      else
      {
        lInterval = 365;
        lBitInterval = 180;
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
      for (double i = 0; i <= lMaxTime; i+= lBitInterval)
      {
        int lX = (int)(lYWidth + (i * dCompInter)) + 1;
        if (i > 0)
          g.DrawLine(new Pen(ColorGridLight), lX, yTop, lX, ImgSize.Height - (lXHeight + 5));
      }
      for (long i = 0; i <= lMaxTime; i += lInterval)
      {
        int lX = (int)(lYWidth + (i * dCompInter)) + 1;
        g.DrawLine(new Pen(ColorText), lX, ImgSize.Height - (lXHeight - 3), lX, ImgSize.Height - lXHeight);
        if (i > 0)
          g.DrawLine(new Pen(ColorGridDark), lX, yTop, lX, ImgSize.Height - (lXHeight + 5));
      }
      long lastI = (long)(lYWidth + (lMaxTime * dCompInter));
      if (lastI >= (ImgSize.Width - 4))
        lastI = (ImgSize.Width - 4);
      string sDispV = "g";
      long ddRet = DateDiff(DateInterval.Minute, lStart, lEnd);
      if (ddRet > 1)
        sDispV = "d";
      else if (ddRet < 1)
        sDispV = "t";
      else
        sDispV = "g";
      string sLastDisp = lEnd.ToString(sDispV);
      float iLastDispWidth = g.MeasureString(sLastDisp, tFont).Width;
      for (long i = 0; i <= lMaxTime; i += lLabelInterval)
      {
        int lX = (int)(lYWidth + (i * dCompInter)) + 1;
        string sDisp = DateAdd(dInterval, i, lStart).ToString(sDispV);
        g.DrawLine(new Pen(ColorText), lX, ImgSize.Height - (lXHeight - 5), lX, ImgSize.Height - lXHeight);
        if (lX >= (ImgSize.Width - (g.MeasureString(sDisp, tFont).Width / 2)))
        {
          g.DrawString(sDisp, tFont, new SolidBrush(ColorText), (ImgSize.Width - g.MeasureString(sDisp, tFont).Width), ImgSize.Height - lXHeight + 5);
        }
        else if (lX - (g.MeasureString(sDisp, tFont).Width / 2) < lYWidth)
        {
          g.DrawString(sDisp, tFont, new SolidBrush(ColorText), lX - (g.MeasureString(sDisp, tFont).Width / sDisp.Length), ImgSize.Height - lXHeight + 5);
        }
        else
        {
          g.DrawString(sDisp, tFont, new SolidBrush(ColorText), lX - (g.MeasureString(sDisp, tFont).Width / 2), ImgSize.Height - lXHeight + 5);
        }
        if (lX >= lastI - (iLastDispWidth * 1.6))
          lastI = -1;
      }
      if (lastI > -1)
      {
        g.DrawLine(new Pen(ColorText), lastI, ImgSize.Height - (lXHeight - 5), lastI, ImgSize.Height - lXHeight);
        g.DrawString(sLastDisp, tFont, new SolidBrush(ColorText), lastI - iLastDispWidth + 3, ImgSize.Height - lXHeight + 5);
      }
      int MaxY = 0;
      if (Down == 255)
        MaxY = (int)(yTop + yHeight - (Data[Data.Length - 1].UPLIM / (double)lMax * yHeight));
      else
        MaxY = (int)(yTop + yHeight - (Data[Data.Length - 1].DOWNLIM / (double)lMax * yHeight));
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
          if (Math.Abs(DateDiff(dInterval, Data[J].DATETIME, DateAdd(dInterval, I, lStart))) == 0)
          {
            long jLim = 0;
            if (Down == 255)
              jLim = Data[J].UPLIM;
            else
              jLim = Data[J].DOWNLIM;
            if (lHigh < jLim)
              lHigh = jLim;
            if (lLow > jLim)
              lLow = jLim;
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
        lMaxPoints[I].X = (int)(lYWidth + (I * dCompInter) + 1);
        lMaxPoints[I].Y = (int)(yTop + yHeight - (lVal / (double)lMax * yHeight));
        if (I > 0)
        {
          if (lMaxPoints[I - 1].X == 0 & lMaxPoints[I - 1].Y == 0)
          {
            long J = 1;
            while (lMaxPoints[I - J].Y == 0)
              J++;
            for (long K = 1; K < J; K++)
            {
              lMaxPoints[I - K].X = (int)(lYWidth + ((I - K) * dCompInter) + 1);
              lMaxPoints[I - K].Y = (lMaxPoints[I - J].Y + lMaxPoints[I].Y) / 2;
            }
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
          if (Math.Abs(DateDiff(dInterval, Data[J].DATETIME, DateAdd(dInterval, I, lStart))) == 0)
          {
            long jVal = 0;
            if (Down == 255)
              jVal = Data[J].UPLOAD;
            else
              jVal = Data[J].DOWNLOAD;
            if (lHigh < jVal)
              lHigh = jVal;
            if (lLow > jVal)
              lLow = jVal;
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
        lPoints[I].X = (int)(lYWidth + (I * dCompInter) + 1);
        lPoints[I].Y = (int)(yTop + yHeight - (lVal / (double)lMax * yHeight));
        if (I > 0)
        {
          if (lPoints[I - 1].X == 0 & lPoints[I - 1].Y == 0)
          {
            long J = 1;
            while (lPoints[I-J].Y == 0)
              J++;
            for (long K = 1; K < J; K++)
            {
              lPoints[I - K].X = (int)(lYWidth + ((I - K) * dCompInter) + 1);
              lPoints[I - K].Y = (lPoints[I - J].Y + lPoints[I].Y) / 2;
            }
          }
        }
        lastLVal = lVal;
      }
      if (lPoints[lMaxTime].IsEmpty)
      {
        lPoints[lMaxTime] = new Point(ImgSize.Width, yTop + yHeight);
      }
      lPoints[lMaxTime + 1] = new Point(ImgSize.Width, yTop + yHeight);
      lPoints[lMaxTime + 2] = new Point(lYWidth, yTop + yHeight);
      lPoints[lMaxTime + 3] = lPoints[0];
      lTypes[0] = (byte)PathPointType.Start;
      for (long I = 1; I <= lMaxTime + 2; I++)
      {
        lTypes[I] = (byte)PathPointType.Line;
      }
      lTypes[lMaxTime + 3] = (byte)(PathPointType.Line | PathPointType.CloseSubpath);
      g.DrawLines(new Pen(new SolidBrush(ColorMax), 5), lMaxPoints);
      GraphicsPath gPath = new GraphicsPath(lPoints, lTypes);
      LinearGradientBrush fBrush = TriGradientBrush(new Point(lYWidth, MaxY), new Point(lYWidth, yTop + yHeight), Color.FromArgb(192, ColorA), Color.FromArgb(192, ColorB), Color.FromArgb(192, ColorC));
      fBrush.WrapMode = WrapMode.TileFlipX;
      g.FillPath(fBrush, gPath);
      g.SetClip(gPath);
      g.FillRectangle(new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.FromArgb(192, ColorMax), Color.FromArgb(192, ColorC)), lYWidth + 1, yTop - 1, ImgSize.Width, MaxY - yTop + 1);
      g.ResetClip();
      g.DrawPath(new Pen(ColorLine, 1.5f), gPath);
      g.DrawLines(new Pen(new SolidBrush(Color.FromArgb(96, ColorMax)), 5), lMaxPoints);
      g.DrawLine(new Pen(ColorText), lYWidth, yTop, lYWidth, yTop + yHeight);
      g.DrawLine(new Pen(ColorText), lYWidth, yTop + yHeight, ImgSize.Width, yTop + yHeight);
      g.Dispose();
      return iPic;
    }
    #endregion
    #region "Progress"
    public static Font MonospaceFont(float Size)
    {
      try
      {
        if (FontFamily.GenericMonospace.IsStyleAvailable(FontStyle.Regular))
          return (new Font(FontFamily.GenericMonospace, Size));
        else
        {
          System.Collections.Generic.List<string> fontList = new System.Collections.Generic.List<string>();
          foreach (FontFamily fam in FontFamily.Families)
          {
            if (fam.IsStyleAvailable(FontStyle.Regular))
              fontList.Add(fam.Name);
          }
          if (fontList.Contains("Courier New"))
            return new Font("Courier New", Size);
          else if (fontList.Contains("Consolas"))
            return new Font("Consolas", Size);
          else if (fontList.Contains("Lucida Console"))
            return new Font("Lucida Console", Size);
          else
            return new Font(SystemFonts.DefaultFont.Name, Size);
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
          g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
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
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
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
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
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
          g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
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
          g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
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
          g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
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
          g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
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
      if (ColorB == Color.Transparent)
      {
        tBrush = new LinearGradientBrush(point1, point2, ColorC, ColorA);
      }
      else
      {
        tBrush = new LinearGradientBrush(point1, point2, Color.Black, Color.Black);
        ColorBlend cb = new ColorBlend();
        cb.Positions = new float[] { 0f, 0.5f, 1f };
        cb.Colors = new Color[] { ColorC, ColorB, ColorA };
        tBrush.InterpolationColors = cb;
      }
      return tBrush;
    }
    public static string FormatPercent(double val, int Accuracy)
    {
      System.Globalization.NumberFormatInfo nfi = new System.Globalization.CultureInfo("en-us").NumberFormat;
      nfi.PercentDecimalDigits = Accuracy;
      int[] groupSize = { 0 };
      nfi.PercentGroupSizes = groupSize;
      return (val.ToString("P", nfi));
    }
    #endregion
    #region "Tray"
    private const int Alpha = 192;
    public static void CreateTrayIcon_Left(ref Graphics g, long lUsed, long lLim, Color cA, Color cB, Color cC, int iSize)
    {
      LinearGradientBrush fillBrush = default(LinearGradientBrush);
      if (cB == Color.Transparent)
      {
        fillBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, iSize), Color.FromArgb(Alpha, cC), Color.FromArgb(Alpha, cA));
      }
      else
      {
        fillBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, iSize), Color.Black, Color.Black);
        ColorBlend cBlend = new ColorBlend();
        cBlend.Positions = new float[] { 0f, 0.5f, 1f };
        cBlend.Colors = new Color[] { Color.FromArgb(Alpha, cC), Color.FromArgb(Alpha, cB), Color.FromArgb(Alpha, cA) };
        fillBrush.InterpolationColors = cBlend;
      }
      long yUsed = (long)Math.Round(iSize - ((double)lUsed / lLim * iSize));
      if (yUsed < 0)
      {
        yUsed = 0;
      }
      if (yUsed > iSize)
      {
        yUsed = iSize;
      }
      g.FillRectangle(fillBrush, 0, yUsed, (float)Math.Floor(iSize / 2d), iSize - yUsed);
    }
    public static void CreateTrayIcon_Right(ref Graphics g, long lUsed, long lLim, Color cA, Color cB, Color cC, int iSize)
    {
      LinearGradientBrush fillBrush = default(LinearGradientBrush);
      if (cB == Color.Transparent)
      {
        fillBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, iSize), Color.FromArgb(Alpha, cC), Color.FromArgb(Alpha, cA));
      }
      else
      {
        fillBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, iSize), Color.Black, Color.Black);
        ColorBlend cBlend = new ColorBlend();
        cBlend.Positions = new float[] { 0f, 0.5f, 1f };
        cBlend.Colors = new Color[] { Color.FromArgb(Alpha, cC), Color.FromArgb(Alpha, cB), Color.FromArgb(Alpha, cA) };
        fillBrush.InterpolationColors = cBlend;
      }
      long yUsed = (long)Math.Round(iSize - ((double)lUsed / lLim * iSize));
      if (yUsed < 0)
      {
        yUsed = 0;
      }
      if (yUsed > iSize)
      {
        yUsed = iSize;
      }
      g.FillRectangle(fillBrush, (float)Math.Floor(iSize / 2d), yUsed, (float)Math.Ceiling(iSize / 2d), iSize - yUsed);
    }
    #endregion
    public static void ScreenDefaultColors(ref AppSettings.AppColors Colors, localRestrictionTracker.SatHostTypes useStyle)
    {
      AppSettings.AppColors defaultColors = GetDefaultColors(useStyle);
      if (Colors.MainDownA == Color.Transparent | Colors.MainDownA.A < 255)
        Colors.MainDownA = defaultColors.MainDownA;
      if (Colors.MainDownB == Color.Transparent | Colors.MainDownB.A < 255)
        Colors.MainDownB = defaultColors.MainDownB;
      if (Colors.MainDownC == Color.Transparent | Colors.MainDownC.A < 255)
        Colors.MainDownC = defaultColors.MainDownC;
      if (Colors.MainUpA == Color.Transparent | Colors.MainUpA.A < 255)
        Colors.MainUpA = defaultColors.MainUpA;
      if (Colors.MainUpB == Color.Transparent | Colors.MainUpB.A < 255)
        Colors.MainUpB = defaultColors.MainUpB;
      if (Colors.MainUpC == Color.Transparent | Colors.MainUpC.A < 255)
        Colors.MainUpC = defaultColors.MainUpC;
      if (Colors.MainText == Color.Transparent | Colors.MainText.A < 255)
        Colors.MainText = defaultColors.MainText;
      if (Colors.MainBackground == Color.Transparent | Colors.MainBackground.A < 255)
        Colors.MainBackground = defaultColors.MainBackground;

      if (Colors.TrayDownA == Color.Transparent | Colors.TrayDownA.A < 255)
        Colors.TrayDownA = defaultColors.TrayDownA;
      if (Colors.TrayDownB == Color.Transparent | Colors.TrayDownB.A < 255)
        Colors.TrayDownB = defaultColors.TrayDownB;
      if (Colors.TrayDownC == Color.Transparent | Colors.TrayDownC.A < 255)
        Colors.TrayDownC = defaultColors.TrayDownC;
      if (Colors.TrayUpA == Color.Transparent | Colors.TrayUpA.A < 255)
        Colors.TrayUpA = defaultColors.TrayUpA;
      if (Colors.TrayUpB == Color.Transparent | Colors.TrayUpB.A < 255)
        Colors.TrayUpB = defaultColors.TrayUpB;
      if (Colors.TrayUpC == Color.Transparent | Colors.TrayUpC.A < 255)
        Colors.TrayUpC = defaultColors.TrayUpC;

      if (Colors.HistoryDownLine == Color.Transparent | Colors.HistoryDownLine.A < 255)
        Colors.HistoryDownLine = defaultColors.HistoryDownLine;
      if (Colors.HistoryDownA == Color.Transparent | Colors.HistoryDownA.A < 255)
        Colors.HistoryDownA = defaultColors.HistoryDownA;
      if (Colors.HistoryDownB == Color.Transparent | Colors.HistoryDownB.A < 255)
        Colors.HistoryDownB = defaultColors.HistoryDownB;
      if (Colors.HistoryDownC == Color.Transparent | Colors.HistoryDownC.A < 255)
        Colors.HistoryDownC = defaultColors.HistoryDownC;
      if (Colors.HistoryUpLine == Color.Transparent | Colors.HistoryUpLine.A < 255)
        Colors.HistoryUpLine = defaultColors.HistoryUpLine;
      if (Colors.HistoryUpA == Color.Transparent | Colors.HistoryUpA.A < 255)
        Colors.HistoryUpA = defaultColors.HistoryUpA;
      if (Colors.HistoryUpB == Color.Transparent | Colors.HistoryUpB.A < 255)
        Colors.HistoryUpB = defaultColors.HistoryUpB;
      if (Colors.HistoryUpC == Color.Transparent | Colors.HistoryUpC.A < 255)
        Colors.HistoryUpC = defaultColors.HistoryUpC;
      if (Colors.HistoryText == Color.Transparent | Colors.HistoryText.A < 255)
        Colors.HistoryText = defaultColors.HistoryText;
      if (Colors.HistoryBackground == Color.Transparent | Colors.HistoryBackground.A < 255)
        Colors.HistoryBackground = defaultColors.HistoryBackground;
      if (Colors.HistoryLightGrid == Color.Transparent | Colors.HistoryLightGrid.A < 255)
        Colors.HistoryLightGrid = defaultColors.HistoryLightGrid;
      if (Colors.HistoryDarkGrid == Color.Transparent | Colors.HistoryDarkGrid.A < 255)
        Colors.HistoryDarkGrid = defaultColors.HistoryDarkGrid;
    }
    public static AppSettings.AppColors GetDefaultColors(localRestrictionTracker.SatHostTypes useStyle)
    {
      AppSettings.AppColors outColors = new AppSettings.AppColors();
      switch (useStyle)
      {
        case localRestrictionTracker.SatHostTypes.WildBlue_LEGACY:
        case localRestrictionTracker.SatHostTypes.RuralPortal_LEGACY:
          outColors.MainDownA = Color.DarkBlue;
          outColors.MainDownB = Color.Blue;
          outColors.MainDownC = Color.Aqua;
          outColors.MainUpA = Color.DarkBlue;
          outColors.MainUpB = Color.Blue;
          outColors.MainUpC = Color.Aqua;
          outColors.MainText = Color.White;
          outColors.MainBackground = Color.Black;

          outColors.TrayDownA = Color.DarkBlue;
          outColors.TrayDownB = Color.Blue;
          outColors.TrayDownC = Color.Aqua;
          outColors.TrayUpA = Color.DarkBlue;
          outColors.TrayUpB = Color.Blue;
          outColors.TrayUpC = Color.Aqua;

          outColors.HistoryDownLine = Color.DarkBlue;
          outColors.HistoryDownA = Color.DarkBlue;
          outColors.HistoryDownB = Color.Blue;
          outColors.HistoryDownC = Color.Aqua;
          outColors.HistoryDownMax = Color.Yellow;
          outColors.HistoryUpLine = Color.DarkBlue;
          outColors.HistoryUpA = Color.DarkBlue;
          outColors.HistoryUpB = Color.Blue;
          outColors.HistoryUpC = Color.Aqua;
          outColors.HistoryUpMax = Color.Yellow;
          outColors.HistoryText = Color.Black;
          outColors.HistoryBackground = Color.White;
          outColors.HistoryLightGrid = Color.LightGray;
          outColors.HistoryDarkGrid = Color.DarkGray;
          break;
        case localRestrictionTracker.SatHostTypes.RuralPortal_EXEDE:
        case localRestrictionTracker.SatHostTypes.WildBlue_EXEDE:
          outColors.MainDownA = Color.DarkBlue;
          outColors.MainDownB = Color.Blue;
          outColors.MainDownC = Color.Aqua;
          outColors.MainUpA = Color.Transparent;
          outColors.MainUpB = Color.Transparent;
          outColors.MainUpC = Color.Transparent;
          outColors.MainText = Color.White;
          outColors.MainBackground = Color.Black;

          outColors.TrayDownA = Color.DarkBlue;
          outColors.TrayDownB = Color.Blue;
          outColors.TrayDownC = Color.Aqua;
          outColors.TrayUpA = Color.Transparent;
          outColors.TrayUpB = Color.Transparent;
          outColors.TrayUpC = Color.Transparent;

          outColors.HistoryDownLine = Color.DarkBlue;
          outColors.HistoryDownA = Color.DarkBlue;
          outColors.HistoryDownB = Color.Blue;
          outColors.HistoryDownC = Color.Aqua;
          outColors.HistoryDownMax = Color.Yellow;
          outColors.HistoryUpLine = Color.DarkBlue;
          outColors.HistoryUpA = Color.Transparent;
          outColors.HistoryUpB = Color.Transparent;
          outColors.HistoryUpC = Color.Transparent;
          outColors.HistoryUpMax = Color.Transparent;
          outColors.HistoryText = Color.Black;
          outColors.HistoryBackground = Color.White;
          outColors.HistoryLightGrid = Color.LightGray;
          outColors.HistoryDarkGrid = Color.DarkGray;
          break;

        case localRestrictionTracker.SatHostTypes.DishNet_EXEDE:
          outColors.MainDownA = Color.DarkBlue;
          outColors.MainDownB = Color.Blue;
          outColors.MainDownC = Color.Aqua;
          outColors.MainUpA = Color.DarkBlue;
          outColors.MainUpB = Color.Blue;
          outColors.MainUpC = Color.Aqua;
          outColors.MainText = Color.White;
          outColors.MainBackground = Color.Black;

          outColors.TrayDownA = Color.DarkBlue;
          outColors.TrayDownB = Color.Blue;
          outColors.TrayDownC = Color.Aqua;
          outColors.TrayUpA = Color.DarkBlue;
          outColors.TrayUpB = Color.Blue;
          outColors.TrayUpC = Color.Aqua;

          outColors.HistoryDownLine = Color.DarkBlue;
          outColors.HistoryDownA = Color.DarkBlue;
          outColors.HistoryDownB = Color.Blue;
          outColors.HistoryDownC = Color.Aqua;
          outColors.HistoryDownMax = Color.Yellow;
          outColors.HistoryUpLine = Color.DarkBlue;
          outColors.HistoryUpA = Color.DarkBlue;
          outColors.HistoryUpB = Color.Blue;
          outColors.HistoryUpC = Color.Aqua;
          outColors.HistoryUpMax = Color.Yellow;
          outColors.HistoryText = Color.Black;
          outColors.HistoryBackground = Color.White;
          outColors.HistoryLightGrid = Color.LightGray;
          outColors.HistoryDarkGrid = Color.DarkGray;
          break;

        default:
          outColors.MainDownA = Color.DarkBlue;
          outColors.MainDownB = Color.Blue;
          outColors.MainDownC = Color.Aqua;
          outColors.MainUpA = Color.DarkBlue;
          outColors.MainUpB = Color.Blue;
          outColors.MainUpC = Color.Aqua;
          outColors.MainText = Color.White;
          outColors.MainBackground = Color.Black;

          outColors.TrayDownA = Color.DarkBlue;
          outColors.TrayDownB = Color.Blue;
          outColors.TrayDownC = Color.Aqua;
          outColors.TrayUpA = Color.DarkBlue;
          outColors.TrayUpB = Color.Blue;
          outColors.TrayUpC = Color.Aqua;

          outColors.HistoryDownLine = Color.DarkBlue;
          outColors.HistoryDownA = Color.DarkBlue;
          outColors.HistoryDownB = Color.Blue;
          outColors.HistoryDownC = Color.Aqua;
          outColors.HistoryDownMax = Color.Yellow;
          outColors.HistoryUpLine = Color.DarkBlue;
          outColors.HistoryUpA = Color.DarkBlue;
          outColors.HistoryUpB = Color.Blue;
          outColors.HistoryUpC = Color.Aqua;
          outColors.HistoryUpMax = Color.Yellow;
          outColors.HistoryText = Color.Black;
          outColors.HistoryBackground = Color.White;
          outColors.HistoryLightGrid = Color.LightGray;
          outColors.HistoryDarkGrid = Color.DarkGray;
          break;
      }
      return(outColors);
    }
    #endregion
    /// <summary>
    /// Attempts to see if a file is in use, waiting up to five seconds for it to be freed.
    /// </summary>
    /// <param name="Filename">The exact path to the file which needs to be checked.</param>
    /// <param name="access">Write permissions required for checking.</param>
    /// <returns>True on available, false on in use.</returns>
    /// <remarks></remarks>
    public static bool InUseChecker(string Filename, FileAccess access)
    {
      if (!File.Exists(Filename))
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
              using (FileStream fs = File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
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
              using (FileStream fs = File.Open(Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
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
      ret = ret.Replace("-", "");
      return ret.Length == 0;
    }
    public static long TickCount()
    {
      return (long)((System.Diagnostics.Stopwatch.GetTimestamp() / (double)System.Diagnostics.Stopwatch.Frequency) * 1000);
    }
    public static string ProductVersion
    {
      get
      {
        System.Reflection.Assembly srt = System.Reflection.Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(srt.Location);
        return fInfo.FileVersion;
      }
    }
    public static string CompanyName
    {
      get
      {
        System.Reflection.Assembly srt = System.Reflection.Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(srt.Location);
        return fInfo.CompanyName;
      }
    }
    public static string ProductName
    {
      get
      {
        System.Reflection.Assembly srt = System.Reflection.Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(srt.Location);
        return fInfo.ProductName;
      }
    }
    #region "MsgBox"
    public static Gtk.ResponseType ShowMessageBox(Gtk.Window parent, string text, string title, Gtk.DialogFlags flags, Gtk.MessageType icon, Gtk.ButtonsType buttons)
    {
      Gtk.MessageDialog dlg = new Gtk.MessageDialog(parent, flags, icon, buttons, text);
      if (String.IsNullOrEmpty(title))
      {
        dlg.Title = modFunctions.ProductName;
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
        dlg.Title = modFunctions.ProductName;
      }
      else
      {
        dlg.Title = title;
      }
      Gtk.ResponseType ret = (Gtk.ResponseType)dlg.Run();
      dlg.Destroy();
      return ret;
    }
    #endregion
    #region "Image Stuff"
    public static Bitmap GetScreenRect(Rectangle rectIn)
    {
      if (CurrentOS.IsMac)
      {
        if (File.Exists("/usr/sbin/screencapture"))
        {
          string screenPath = Path.Combine(AppData, "srt_alert_clip.png");
          if (File.Exists(screenPath))
            File.Delete(screenPath);
          System.Diagnostics.ProcessStartInfo cap = new System.Diagnostics.ProcessStartInfo("/usr/sbin/screencapture", String.Format("-R{0},{1},{2},{3} \"{4}\"", rectIn.X, rectIn.Y, rectIn.Width, rectIn.Height, screenPath));
          cap.UseShellExecute = true;
          System.Diagnostics.Process.Start(cap);
          if (!File.Exists(screenPath))
          {
            long lStart = TickCount();
            do
            {
              System.Threading.Thread.Sleep(1);
            } while ((!File.Exists(screenPath)) & (TickCount() - lStart < 7000));
          }
          if (File.Exists(screenPath))
          {
            Gdk.Pixbuf pbImage = new Gdk.Pixbuf(screenPath);
            Bitmap bImg = (Bitmap)PixbufToImage(pbImage);
            pbImage.Dispose();
            pbImage = null;
            if (File.Exists(screenPath))
              File.Delete(screenPath);
            return bImg;
          }
        }
      }
      try
      {
        Gdk.Window window = Gdk.Global.DefaultRootWindow;
        if (window != null)
        {           
          Gdk.Pixbuf scrnBuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, rectIn.Width, rectIn.Height);          
          scrnBuf.GetFromDrawable(window, Gdk.Colormap.System, rectIn.X, rectIn.Y, 0, 0, rectIn.Width, rectIn.Height);  
          return (Bitmap)PixbufToImage(scrnBuf);
        }
        return null;
      }
      catch (Exception)
      {
        return null;
      }
    }
    public static Bitmap ReplaceColors(Bitmap bitIn, Color TransparencyKey, Bitmap bitBG)
    {
      Bitmap bitOut = new Bitmap(bitIn.Width, bitIn.Height, PixelFormat.Format32bppArgb);
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
      Bitmap bitOut = new Bitmap(bitIn.Width, bitIn.Height, PixelFormat.Format32bppPArgb);
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
    #region "Colors Conversions"
    public static bool CompareColors(Color a, Color b, bool IgnoreAlpha)
    {
      bool ret = true;
      if (IgnoreAlpha)
      {
        if (a.R != b.R)
          ret = false;
        else if (a.G != b.G)
          ret = false;
        else if (a.B != b.B)
          ret = false;
      }
      else
      {
        if (a.R != b.R)
          ret = false;
        else if (a.G != b.G)
          ret = false;
        else if (a.B != b.B)
          ret = false;
        else if (a.A != b.A)
          ret = false;
      }
      return(ret);
    }
    public static bool CompareColors(Color a, Gdk.Color b)
    {
      bool ret = true;
      Color b2 = GdkColorToDrawingColor(b);
      if (a.R != b2.R)
        ret = false;
      else if (a.G != b2.G)
        ret = false;
      else if (a.B != b2.B)
        ret = false;
      return(ret);
    }
    public static bool CompareColors(Gdk.Color a, Gdk.Color b)
    {
      bool ret = true;
      if (a.Red != b.Red)
        ret = false;
      else if (a.Green != b.Green)
        ret = false;
      else if (a.Blue != b.Blue)
        ret = false;
      return(ret);
    }
    #endregion
    public static Gdk.Pixbuf ImageToPixbuf(Image img)
    {
      try
      {
        using (MemoryStream ms = new MemoryStream())
        {
          img.Save(ms, ImageFormat.Png);
          ms.Position = 0;
          Gdk.Pixbuf ret = new Gdk.Pixbuf(ms);
          return ret;
        }
      }
      catch (Exception)
      {
        return Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.error.png");
      }
    }
    public static Image PixbufToImage(Gdk.Pixbuf pbf)
    {
      try
      {
        byte[] img = pbf.SaveToBuffer("png");
        using (MemoryStream ms = new MemoryStream(img))
        {
          Image ret = Image.FromStream(ms);
          return ret;
        }
      }
      catch (Exception)
      {
        return ((Image)new System.Drawing.Bitmap(MainClass.fMain.GetType(), "Resources.error.png"));
      }
    }
    #endregion
    #region "Startup"
    private static string LinStartup = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config", "autostart");
    private static string OSXStartup = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "LaunchAgents");
    private static string LinShortcut = "restriction-tracker.desktop";
    private static string OSXShortcut = "com.realityripple.restrictiontracker.agent.plist";
    public static bool RunOnStartup
    {
      get
      {
        if (CurrentOS.IsMac)
        {
          if (Directory.Exists(OSXStartup))
            return File.Exists(Path.Combine(OSXStartup, OSXShortcut));
        }
        else
        {
          if (Directory.Exists(LinStartup))
            return File.Exists(Path.Combine(LinStartup, LinShortcut));
        }
        return false;
      }
      set
      {
        if (CurrentOS.IsMac)
        {
          if (value)
          {
            GenerateOSXStartupFile();
          }
          else
          {
            if (Directory.Exists(OSXStartup))
            if (File.Exists(Path.Combine(OSXStartup, OSXShortcut)))
              File.Delete(Path.Combine(OSXStartup, OSXShortcut));
          }
        }
        else
        {
          if (value)
          {
            GenerateLinuxStartupFile();
          }
          else
          {
            try
            {
              if (Directory.Exists(LinStartup))
              if (File.Exists(Path.Combine(LinStartup, LinShortcut)))
                File.Delete(Path.Combine(LinStartup, LinShortcut));
            }
            catch (Exception)
            {
            }
          }
        }
      }
    }
    private static void GenerateLinuxStartupFile()
    {
      if (!Directory.Exists(LinStartup))
        Directory.CreateDirectory(LinStartup);
      if (!File.Exists(Path.Combine(LinStartup, LinShortcut)))
      {
        try
        {
          string sIcon;
          if (File.Exists("/usr/lib/restrictiontracker/RestrictionTracker-32.png"))
            sIcon = "Icon=/usr/lib/restrictiontracker/RestrictionTracker-32.png\n";
          else if (File.Exists("/usr/lib/restrictiontracker/RestrictionTracker-256.png"))
            sIcon = "Icon=/usr/lib/restrictiontracker/RestrictionTracker-256.png\n";
          else
            sIcon = "";
          string sLink = "[Desktop Entry]\n" + 
            "Name=" + ProductName + "\n" + 
            "GenericName=Bandwidth Monitor\n" +
            "Comment=Monitor and record your ViaSat Satellite network usage\n" +
            sIcon +
            "Encoding=UTF-8\n" +
            "Type=Application\n" +
            "Version=" + DisplayVersion(ProductVersion) + "\n" +
            "Exec=mono /usr/lib/restrictiontracker/RestrictionTracker.exe\n" +
            "Categories=Network;System;Monitor\n" +
            "StartupNotify=true\n" +
            "Terminal=0\n" +
            "TerminalOptions=";
          File.WriteAllText(Path.Combine(LinStartup, LinShortcut), sLink, System.Text.Encoding.GetEncoding(28591));
        }
        catch (Exception)
        {
        }
      }
    }
    private static void GenerateOSXStartupFile()
    {
      if (!Directory.Exists(OSXStartup))
        Directory.CreateDirectory(OSXStartup);
      if (!File.Exists(Path.Combine(OSXStartup, OSXShortcut)))
      {
        try
        {
          string sLink = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n" +
            "<plist version=\"1.0\">\n" +
            " <dict>\n" +
            "  <key>Label</key>\n" +
            "  <string>com.realityripple.restrictiontracker</string>\n" +
            "  <key>ProgramArguments</key>\n" +
            "  <array>\n" +
            "   <string>open</string>\n" +
            "   <string>-a</string>\n" +
            "   <string>" + ProductName + "</string>\n" +
            "  </array>\n" +
            "  <key>RunAtLoad</key>\n" +
            "  <true/>\n" +
            "  <key>KeepAlive</key>\n" +
            "  <false/>\n" +
            " </dict>\n" +
            "</plist>\n";
          File.WriteAllText(Path.Combine(OSXStartup, OSXShortcut), sLink, System.Text.Encoding.UTF8);
        }
        catch (Exception)
        {
        }
      }
    }
    public static string StartupPath
    {
      get
      {
        if (CurrentOS.IsMac)
          return (Path.Combine(OSXStartup, OSXShortcut));
        else
          return (Path.Combine(LinStartup, LinShortcut));
      }
    }
    #endregion
    public static bool RunTerminal(string command)
    {
      try
      {
        if (CurrentOS.IsMac)
          System.Diagnostics.Process.Start(command);
        else if (CurrentOS.IsLinux)
        {
          string sConsolePath = "\"" + command + "\"";
          if (File.Exists("/usr/bin/xfce4-terminal"))
            System.Diagnostics.Process.Start("xfce4-terminal", "-e 'bash " + sConsolePath + "'");
          else if (File.Exists("/usr/bin/gnome-terminal"))
            System.Diagnostics.Process.Start("gnome-terminal", "-e 'bash " + sConsolePath + "'");
          else if (File.Exists("/usr/bin/konsole"))
            System.Diagnostics.Process.Start("konsole", "-e " + sConsolePath + "");
          else if (File.Exists("/usr/bin/mate-terminal"))
            System.Diagnostics.Process.Start("mate-terminal", "-e 'bash " + sConsolePath + "'");
          else if (File.Exists("/usr/bin/xterm"))
            System.Diagnostics.Process.Start("xterm", "-e 'bash " + sConsolePath + "'");
          else if (File.Exists("/usr/bin/x-terminal-emulator"))
            System.Diagnostics.Process.Start("x-terminal-emulator", "-e 'bash " + sConsolePath + "'");
          else
            System.Diagnostics.Process.Start("bash", sConsolePath);
        }
        else
          System.Diagnostics.Process.Start(command);
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }
    private static System.Net.Sockets.TcpListener sckOpen;
    public static bool RunningLock()
    {
      try
      {
        sckOpen = new System.Net.Sockets.TcpListener(new System.Net.IPAddress(0x100007F), 23208);
        sckOpen.Start();
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }
    public static bool IterativeEqualityCheck(byte[] inArray1, byte[] inArray2)
    {
      if (inArray1.Length == inArray2.Length)
      {
        for (int I = 0; I <= inArray1.Length - 1; I++)
        {
          if (!(inArray1[I] == inArray2[I]))
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
    public static bool DirectoryEqualityCheck(string dirA, string dirB)
    {
      if (string.IsNullOrEmpty(dirA))
      {
        if (string.IsNullOrEmpty(dirB))
        {
          return true; 
        }
        else
        {
          return false;
        }
      }
      else if (string.IsNullOrEmpty(dirB))
      {
        return false;
      }
      if (!dirA.EndsWith(Path.DirectorySeparatorChar.ToString()))
      {
        dirA += Path.DirectorySeparatorChar;
      }
      if (!dirB.EndsWith(Path.DirectorySeparatorChar.ToString()))
      {
        dirB += Path.DirectorySeparatorChar;
      }
      if (dirA == dirB)
      {
        return true;
      }
      else
      {
        return false;
      }
    }
  }
}
