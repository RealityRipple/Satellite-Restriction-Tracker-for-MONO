using System;
using RestrictionLibrary;
namespace RestrictionTrackerGTK
{
  public class clsUpdate:
    IDisposable
  {
    private const string VersionURL = "http://update.realityripple.com/Satellite_Restriction_Tracker/ver";
    public class ProgressEventArgs: EventArgs
    {
      private long mBytesReceived;
      public long BytesReceived
      {
        get
        {
          return mBytesReceived;
        }
      }
      private int mProgressPercentage;
      public int ProgressPercentage
      {
        get
        {
          return mProgressPercentage;
        }
      }
      private long mTotalBytesToReceive;
      public long TotalBytesToReceive
      {
        get
        {
          return mTotalBytesToReceive;
        }
      }
      internal ProgressEventArgs(long bReceived, long bToReceive, int iPercentage)
      {
        mBytesReceived = bReceived;
        mTotalBytesToReceive = bToReceive;
        mProgressPercentage = iPercentage;
      }
    }
    public class CheckEventArgs: System.ComponentModel.AsyncCompletedEventArgs
    {
      private ResultType mResult;
      public ResultType Result
      {
        get
        {
          return mResult;
        }
      }
      private string mVersion;
      public string Version
      {
        get
        {
          return mVersion;
        }
      }
      internal CheckEventArgs(ResultType rtResult, string sVersion, Exception ex, bool bCancelled, object state) :
        base(ex, bCancelled, state)
      {
        mVersion = sVersion;
        mResult = rtResult;
      }
    }
    public event EventHandler CheckingVersion;
    public event EventHandler<ProgressEventArgs> CheckProgressChanged;
    public event EventHandler<CheckEventArgs> CheckResult;
    public event EventHandler DownloadingUpdate;
    public event EventHandler<ProgressEventArgs> UpdateProgressChanged;
    public event EventHandler<System.ComponentModel.AsyncCompletedEventArgs> DownloadResult;
    private RestrictionLibrary.WebClientCore wsVer;
    private string DownloadURL;
    private string VerNumber;
    public void CheckVersion()
    {
      AppSettings myS = new AppSettings();
      wsVer = new RestrictionLibrary.WebClientCore();
      wsVer.DownloadProgressChanged += wsVer_DownloadProgressChanged;
      wsVer.DownloadStringCompleted += wsVer_DownloadStringCompleted;
      wsVer.DownloadFileCompleted += wsVer_DownloadFileCompleted;
      wsVer.KeepAlive = false;
      wsVer.Proxy = myS.Proxy;
      wsVer.Timeout = myS.Timeout;
      myS = null;
      wsVer.DownloadStringAsync(new Uri(VersionURL), "INFO");
      if (CheckingVersion != null)
      {
        CheckingVersion(this, new EventArgs());
      }
    }
    public static ResultType QuickCheckVersion()
    {
      string sVerStr = null;
      AppSettings mySettings = new AppSettings();
      WebClientEx wsCheck = new WebClientEx();
      wsCheck.KeepAlive = false;
      wsCheck.Proxy = mySettings.Proxy;
      wsCheck.Timeout = mySettings.Timeout;
      sVerStr = wsCheck.DownloadString(VersionURL);
      string sHash = null;
      foreach(string sKey in wsCheck.ResponseHeaders)
      {
        if (sKey.ToLower() == "x-update-signature")
        {
          sHash = wsCheck.ResponseHeaders[sKey];
          break;
        }
      }
      if (!VerifySignature(sVerStr, sHash))
      {
        mySettings = null;
        return ResultType.NoUpdate;
      }
      if (sVerStr.StartsWith("Error: "))
      {
        mySettings = null;
        return ResultType.NoUpdate;
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
          return ResultType.NewUpdate;
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
            return ResultType.NewUpdate;
          }
          else if (mySettings.UpdateBETA & !string.IsNullOrEmpty(sVL[1]))
          {
            string[] sVBU = sVL[1].Split(verSplit);
            if (modFunctions.CompareVersions(sVBU[0]))
            {
              mySettings = null;
              return ResultType.NewBeta;
            }
          }
        }
      }
      mySettings = null;
      return ResultType.NoUpdate;
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
      ResultType rRet = ResultType.NoUpdate;
      DownloadURL = null;
      VerNumber = null;
      if (e.Error == null)
      {
        try
        {
          string sVerStr = e.Result;
          string sHash = null;
          foreach(string sKey in wsVer.ResponseHeaders )
          {
            if (sKey.ToLower() == "x-update-signature")
            {
              sHash = wsVer.ResponseHeaders[sKey];
              break;
            }
          }
          if (!VerifySignature(sVerStr ,sHash ))
          {
            CheckResult(sender, new CheckEventArgs(ResultType.NoUpdate, VerNumber, new Exception("Invalid Server Response"), e.Cancelled, e.UserState));
            return;
          }
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
              rRet = ResultType.NewUpdate;
              DownloadURL = sVU[1];
              VerNumber = sVU[0];
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
                rRet = ResultType.NewUpdate;
                DownloadURL = sVMU[1];
                VerNumber = sVMU[0];
              }
              else if (mySettings.UpdateBETA & !string.IsNullOrEmpty(sVL[1]))
              {
                string[] sVBU = sVL[1].Split(verSplit);
                if (modFunctions.CompareVersions(sVBU[0]))
                {
                  rRet = ResultType.NewBeta;
                  DownloadURL = sVBU[1];
                  VerNumber = sVBU[0];
                }
              }
              mySettings = null;
            }
            else
            {
              if (CheckResult != null)
              {
                CheckResult(sender, new CheckEventArgs(ResultType.NoUpdate, VerNumber, new Exception("Version Reading Error", new Exception("Empty Version String")), e.Cancelled, e.UserState));
              }
              return;
            }
          }
        }
        catch (Exception ex)
        {
          if (CheckResult != null)
          {
            CheckResult(sender, new CheckEventArgs(ResultType.NoUpdate, VerNumber, new Exception("Version Parsing Error", ex), e.Cancelled, e.UserState));
          }
        }
      }
      if (CheckResult != null)
      {
        CheckResult(sender, new CheckEventArgs(rRet, VerNumber, e.Error, e.Cancelled, e.UserState));
      }
    }
    private void wsVer_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      if (DownloadResult != null)
        DownloadResult(sender, e);
    }
    private static bool VerifySignature(string Message, string Signature)
    {
      if (String.IsNullOrEmpty(Signature))
        return false;
      byte[] bMsg = System.Text.Encoding.GetEncoding(srlFunctions.LATIN_1).GetBytes(Message);
      byte[] bSig = null;
      try
      {
        bSig = System.Convert.FromBase64String(Signature);
      }
      catch(Exception)
      {
        return false;
      }
      string key = null;
      System.Security.Cryptography.RSACryptoServiceProvider rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
      System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
      using (System.IO.Stream s = asm.GetManifestResourceStream("RestrictionTrackerGTK.Resources.pubkey"))
      {
        using (System.IO.StreamReader r = new System.IO.StreamReader(s))
        {
          key = r.ReadToEnd();
        }
      }
      rsa.FromXmlString(key);
      return rsa.VerifyData(bMsg, System.Security.Cryptography.CryptoConfig.MapNameToOID("SHA1"), bSig);
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
