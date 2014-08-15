using System;
namespace RestrictionTrackerGTK
{
  public class clsUpdate : IDisposable
  {
    private const string VersionURL = "http://update.realityripple.com/Satellite_Restriction_Tracker/ver";
    public class ProgressEventArgs : EventArgs
    {
      public long BytesReceived;
      public int ProgressPercentage;
      public long TotalBytesToReceive;
      internal ProgressEventArgs(long bReceived, long bToReceive, int iPercentage)
      {
        BytesReceived = bReceived;
        TotalBytesToReceive = bToReceive;
        ProgressPercentage = iPercentage;
      }
    }
    public class CheckEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {
      public enum ResultType
      {
        NoUpdate,
        NewUpdate,
        NewBeta
      }
      public ResultType Result;
      public string Version;
      internal CheckEventArgs(ResultType rtResult, string sVersion, Exception ex, bool bCancelled, object state) : base(ex, bCancelled, state)
      {
        Version = sVersion;
        Result = rtResult;
      }
    }
    public event CheckingVersionEventHandler CheckingVersion;
    public delegate void CheckingVersionEventHandler(object sender,EventArgs e);
    public event CheckProgressChangedEventHandler CheckProgressChanged;
    public delegate void CheckProgressChangedEventHandler(object sender,ProgressEventArgs e);
    public event CheckResultEventHandler CheckResult;
    public delegate void CheckResultEventHandler(object sender,CheckEventArgs e);
    public event DownloadingUpdateEventHandler DownloadingUpdate;
    public delegate void DownloadingUpdateEventHandler(object sender,EventArgs e);
    public event UpdateProgressChangedEventHandler UpdateProgressChanged;
    public delegate void UpdateProgressChangedEventHandler(object sender,ProgressEventArgs e);
    public event DownloadResultEventHandler DownloadResult;
    public delegate void DownloadResultEventHandler(object sender,System.ComponentModel.AsyncCompletedEventArgs e);
    private RestrictionLibrary.CookieAwareWebClient wsVer;
    private string DownloadURL;
    public void CheckVersion()
    {
      AppSettings myS = new AppSettings();
      wsVer = new RestrictionLibrary.CookieAwareWebClient();
      wsVer.DownloadProgressChanged += wsVer_DownloadProgressChanged;
      wsVer.DownloadStringCompleted += wsVer_DownloadStringCompleted;
      wsVer.DownloadFileCompleted += wsVer_DownloadFileCompleted;
      wsVer.Proxy = myS.Proxy;
      wsVer.Timeout = myS.Timeout;
      myS = null;
      wsVer.DownloadStringAsync(new Uri(VersionURL), "INFO");
      if (CheckingVersion != null)
      {
        CheckingVersion(this, new EventArgs());
      }
    }
    public static CheckEventArgs.ResultType QuickCheckVersion()
    {
      string sVerStr = null;
      AppSettings mySettings = new AppSettings();
      using (RestrictionLibrary.CookieAwareWebClient wsCheck = new RestrictionLibrary.CookieAwareWebClient())
      {
        wsCheck.Proxy = mySettings.Proxy;
        wsCheck.Timeout = mySettings.Timeout;
        try
        {
          sVerStr = wsCheck.DownloadString(new Uri(VersionURL));
        }
        catch (Exception)
        {
          mySettings = null;
          return CheckEventArgs.ResultType.NoUpdate;
        }
      }
      char[] sSplit = { ' ' };
      if (sVerStr.Contains("\r\n"))
      {
        sVerStr = sVerStr.Replace("\r\n", "\n");
        sSplit[0] = '\n';
      }
      else if (sVerStr.Contains("\n"))
        {
          sSplit[0] = '\n';
        }
        else if (sVerStr.Contains("\r"))
          {
            sSplit[0] = '\r';
          }
      char[] verSplit = { '|' };
      if (sSplit[0] == ' ')
      {
        string[] sVU = sVerStr.Split(verSplit);
        if (modFunctions.CompareVersions(sVU[0]))
        {
          mySettings = null;
          return CheckEventArgs.ResultType.NewUpdate;
        }
      }
      else
      {
        string[] sVL = sVerStr.Split(sSplit, 2);
        if (!string.IsNullOrEmpty(sVL[0]))
        {
          string[] sVMU = sVL[0].Split(verSplit);
          if (modFunctions.CompareVersions(sVMU[0]))
          {
            mySettings = null;
            return CheckEventArgs.ResultType.NewUpdate;
          }
          else if (mySettings.BetaCheck & !string.IsNullOrEmpty(sVL[1]))
            {
              string[] sVBU = sVL[1].Split(verSplit);
              if (modFunctions.CompareVersions(sVBU[0]))
              {
                mySettings = null;
                return CheckEventArgs.ResultType.NewBeta;
              }
            }
        }
      }
      mySettings = null;
      return CheckEventArgs.ResultType.NoUpdate;
    }
    public void DownloadUpdate(string toLocation)
    {
      if (!string.IsNullOrEmpty(DownloadURL))
      {
        wsVer.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
        wsVer.DownloadFileAsync(new Uri(DownloadURL), toLocation, "FILE");
        if (DownloadingUpdate != null)
          DownloadingUpdate(this, new EventArgs());
      }
      else
      {
        if (DownloadResult != null)
          DownloadResult(this, new System.ComponentModel.AsyncCompletedEventArgs(new Exception("Version Check was not run."), true, null));
      }
    }
    private void wsVer_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
    {
      if (e.UserState.ToString().CompareTo("INFO") == 0)
      {
        if (CheckProgressChanged != null)
        {
          CheckProgressChanged(sender, new ProgressEventArgs(e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
        }
      }
      else if (e.UserState.ToString().CompareTo("FILE") == 0)
        {
          if (UpdateProgressChanged != null)
            UpdateProgressChanged(sender, new ProgressEventArgs(e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
        }
    }
    private void wsVer_DownloadStringCompleted(object sender, System.Net.DownloadStringCompletedEventArgs e)
    {
      CheckEventArgs.ResultType rRet = CheckEventArgs.ResultType.NoUpdate;
      string sVer = null;
      DownloadURL = null;
      if (e.Error == null)
      {
        try
        {
          string sVerStr = e.Result;
          char[] sSplit = { ' ' };
          if (sVerStr.Contains("\r\n"))
          {
            sSplit[0] = '\n';
            sVerStr = sVerStr.Replace("\r\n", "\n");
          }
          else if (sVerStr.Contains("\n"))
            {
              sSplit[0] = '\n';
            }
            else if (sVerStr.Contains("\r"))
              {
                sSplit[0] = '\r';
              }
          char verSplit = '|';
          if (sSplit[0] == ' ')
          {
            string[] sVU = sVerStr.Split(verSplit);
            if (modFunctions.CompareVersions(sVU[0]))
            {
              rRet = CheckEventArgs.ResultType.NewUpdate;
              DownloadURL = sVU[1];
              sVer = sVU[0];
            }
          }
          else
          {
            string[] sVL = sVerStr.Split(sSplit, 2);
            if (!string.IsNullOrEmpty(sVL[0]))
            {
              string[] sVMU = sVL[0].Split(verSplit);
              AppSettings mySettings = new AppSettings();
              if (modFunctions.CompareVersions(sVMU[0]))
              {
                rRet = CheckEventArgs.ResultType.NewUpdate;
                DownloadURL = sVMU[1];
                sVer = sVMU[0];
              }
              else if (mySettings.BetaCheck & !string.IsNullOrEmpty(sVL[1]))
                {
                  string[] sVBU = sVL[1].Split(verSplit);
                  if (modFunctions.CompareVersions(sVBU[0]))
                  {
                    rRet = CheckEventArgs.ResultType.NewBeta;
                  DownloadURL = sVBU [1];
                    sVer = sVBU[0];
                  }
                }
              mySettings = null;
            }
            else
            {
              if (CheckResult != null)
              {
                CheckResult(sender, new CheckEventArgs(CheckEventArgs.ResultType.NoUpdate, sVer, new Exception("Version Reading Error", new Exception("Empty Version String")), e.Cancelled, e.UserState));
              }
              return;
            }
          }
        }
        catch (Exception ex)
        {
          if (CheckResult != null)
          {
            CheckResult(sender, new CheckEventArgs(CheckEventArgs.ResultType.NoUpdate, sVer, new Exception("Version Parsing Error", ex), e.Cancelled, e.UserState));
          }
        }
      }
      if (CheckResult != null)
      {
        CheckResult(sender, new CheckEventArgs(rRet, sVer, e.Error, e.Cancelled, e.UserState));
      }
    }
    private void wsVer_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      if (DownloadResult != null)
        DownloadResult(sender, e);
    }
    #region "IDisposable Support"
    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposedValue)
      {
        if (disposing)
        {
          if (wsVer != null)
          {
            if (wsVer.IsBusy)
            {
              wsVer.CancelAsync();
            }
            wsVer.Dispose();
            wsVer = null;
          }
        }

      }
      this.disposedValue = true;
    }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}

