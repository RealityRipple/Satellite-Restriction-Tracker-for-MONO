using System;
using System.IO;
using System.Drawing;
using System.Security.Policy;
using RestrictionLibrary;
using GLib;
using System.Threading;
using Gdk;
using Gtk;
namespace RestrictionTrackerGTK
{
  public class clsFavicon : IDisposable
  {
    private RestrictionLibrary.WebClientCore wsFile;
    public delegate void DownloadIconCompletedCallback(Gdk.Pixbuf icon16,Gdk.Pixbuf icon32,object token,Exception Error);
    private DownloadIconCompletedCallback c_callback;
    public clsFavicon(string URL, DownloadIconCompletedCallback callback, object token)
    {
      if (string.IsNullOrEmpty(URL))
        return;
      c_callback = callback;
      System.Threading.Thread connectThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(BeginConnect));
      object[] Params = { URL, token };
      connectThread.Start(Params);
    }
    private void BeginConnect(object o)
    {
      string URL = (string) ((object[]) o)[0];
      object token = ((object[]) o)[1];
      if (!URL.Contains("://"))
        URL = "http://" + URL;
      Uri URI;
      try
      {
        URI = new Uri(URL);
      }
      catch (Exception)
      {
        return;
      }
      ConnectToURL(URI, token);
    }
    private void ConnectToURL(Uri URI, object token)
    {
      if (URI.Host == "192.168.100.1")
      {
        c_callback.Invoke(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.modem16.png"), Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.modem32.png"), token, null);
        return;
      }
      try
      {
        System.Net.IPAddress[] urlRes = System.Net.Dns.GetHostAddresses(URI.Host);
        foreach (System.Net.IPAddress addr in urlRes)
        {
          if (addr.ToString() == "192.168.100.1")
          {
            c_callback.Invoke(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.modem16.png"), Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.modem32.png"), token, null);
            return;
          }
        }
      }
      catch (Exception)
      {
      }
      try
      {
        WebClientEx wsString = new WebClientEx();
        wsString.ErrorBypass = true;
        wsString.ManualRedirect = false;
        string sRet = wsString.DownloadString(URI.OriginalString);
        if (sRet.StartsWith("Error: "))
        {
          try
          {
            ConnectToFile(new Uri(URI.Scheme + "://" + URI.Host + "/favicon.ico"), Path.Combine(modFunctions.AppData, "srt_nettest_favicon.ico"), token, false);
          }
          catch (Exception)
          {
          }
          return;
        }
        try
        {
          string sHTML = sRet;
          if (sHTML.ToLower().Contains("shortcut icon"))
          {
            int startsAt = sHTML.ToLower().IndexOf("shortcut icon");
            int endsAt = startsAt + 13;
            sHTML = sHTML.Substring(0, startsAt) + "icon" + sHTML.Substring(endsAt);
          }
          if (sHTML.ToLower().Contains("rel=\"icon\""))
          {
            if (sHTML.Substring(0, sHTML.ToLower().IndexOf("rel=\"icon\"")).Contains("<"))
            {
              sHTML = sHTML.Substring(sHTML.Substring(0, sHTML.ToLower().IndexOf("rel=\"icon\"")).LastIndexOf("<"));
              if (sHTML.Contains(">"))
              {
                sHTML = sHTML.Substring(0, sHTML.IndexOf(">") + 1);
                if (sHTML.ToLower().Contains("href"))
                {
                  sHTML = sHTML.Substring(sHTML.IndexOf("href"));
                  if (sHTML.Contains("\""))
                  {
                    sHTML = sHTML.Substring(sHTML.IndexOf("\"") + 1);
                    if (sHTML.Contains("\""))
                    {
                      string URL = sHTML.Substring(0, sHTML.IndexOf("\""));
                      if (URL.Contains("://"))
                      {
                      }
                      else if (URL.Contains("//"))
                      {
                        string oldURL = (string) URI.OriginalString;
                        if (oldURL.Contains("://"))
                          oldURL = oldURL.Substring(0, oldURL.IndexOf("://") + 1);
                        URL = oldURL + URL;
                      }
                      else
                      {
                        string oldURL = (string) URI.OriginalString;
                        if (!oldURL.EndsWith("/") & oldURL.IndexOf("/", oldURL.IndexOf("//") + 2) > -1)
                          oldURL = oldURL.Substring(0, oldURL.LastIndexOf("/") + 1);
                        if (URL.StartsWith("/"))
                        {
                          if (oldURL.IndexOf("/", oldURL.IndexOf("//") + 2) > -1)
                            oldURL = oldURL.Substring(0, oldURL.IndexOf("/", oldURL.IndexOf("//") + 2));
                          URL = oldURL + URL;
                        }
                        else if (oldURL.EndsWith("/"))
                          URL = oldURL + URL;
                        else
                          URL = oldURL + "/" + URL;
                      }
                      try
                      {
                        ConnectToFile(new Uri(URL), Path.Combine(modFunctions.AppData, "srt_nettest_favicon.ico"), token, true);
                      }
                      catch (Exception)
                      {
                      }
                      return;
                    }
                  }
                }
              }
            }
          }
        }
        catch (Exception)
        {
        }
        try
        {
          ConnectToFile(new Uri(URI.Scheme + "://" + URI.Host + "/favicon.ico"), Path.Combine(modFunctions.AppData, "srt_nettest_favicon.ico"), token, true);
        }
        catch (Exception)
        {
        }
      }
      catch (Exception)
      {
        c_callback.Invoke(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.error.png"), Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_error.png"), token, new Exception(" Failed to initialize connection to \"" + URI.OriginalString + "\"!"));
      }
    }
    private void ConnectToFile(Uri URL, string Filename, object token, bool trySimpler)
    {
      try
      {
        wsFile = new WebClientCore();
        wsFile.ErrorBypass = true;
        wsFile.ManualRedirect = false;
        System.Threading.Timer tmrSocket = new System.Threading.Timer(new TimerCallback(DownloadFile), new object[] { URL, Filename, token, trySimpler }, 250, System.Threading.Timeout.Infinite);
        tmrSocket.GetType();
      }
      catch
      {
        c_callback.Invoke(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.error.png"), Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_error.png"), token, new Exception(" Failed to initialize connection to \"" + URL.OriginalString + "\"!"));
      }
    }
    private void DownloadFile(object state)
    {
      Uri URI = (Uri) ((object[]) state)[0];
      string Filename = (string) ((object[]) state)[1];
      object token = ((object[]) state)[2];
      wsFile.DownloadFileCompleted += wsFile_DownloadFileCompleted;
      bool trySimpler = (bool) ((object[]) state)[3];
      try
      {
        wsFile.DownloadFileAsync(URI, Filename, new object[] { token, URI, trySimpler });
      }
      catch (Exception)
      {
        c_callback.Invoke(Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.error.png"), Gdk.Pixbuf.LoadFromResource("RestrictionTrackerGTK.Resources.config.linux.advanced_nettest_error.png"), token, new Exception(" Failed to initialize connection to \"" + URI.OriginalString + "\"!"));
      }
    }
    private Bitmap GenerateCloneImage(System.Drawing.Image fromImage, int width, int height)
    {
      using (Bitmap newImage = new Bitmap(width, height))
      {
        using (Graphics g = Graphics.FromImage(newImage))
        {
          g.DrawImage(fromImage, 0, 0, width, height);
        }
        return (Bitmap) newImage.Clone();
      }
    }
    private Bitmap GenerateCloneImage(System.Drawing.Icon fromIcon)
    {
      using (Bitmap newImage = new Bitmap(fromIcon.Width, fromIcon.Height))
      {
        using (Graphics g = Graphics.FromImage(newImage))
        {
          g.DrawIcon(fromIcon, 0, 0);
        }
        return (Bitmap) newImage.Clone();
      }
    }
    void wsFile_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      object Token = ((object[]) e.UserState)[0];
      Uri URI = (Uri) ((object[]) e.UserState)[1];
      bool trySimpler = (bool) ((object[]) e.UserState)[2];
      if (e.Error != null)
      {
        if (!trySimpler)
        {
          c_callback.Invoke(null, null, Token, new Exception("Failed to get an icon."));
        }
        else
        {
          try
          {
            ConnectToFile(new Uri(URI.Scheme + "://" + URI.Host + "/favicon.ico"), Path.Combine(modFunctions.AppData, "srt_nettest_favicon.ico"), Token, false);
          }
          catch (Exception)
          {
          }
        }
        return;
      }
      else if (e.Cancelled)
      {
        return;
      }
      string imgFile = Path.Combine(modFunctions.AppData, "srt_nettest_favicon.ico");
      Bitmap pctPNG16, pctPNG32;
      bool didOK = true;
      pctPNG16 = new Bitmap(16, 16);
      pctPNG32 = new Bitmap(32, 32);
      byte[] imgHeader = new byte[4];
      using (FileStream iStream = File.OpenRead(imgFile))
      {
        iStream.Read(imgHeader, 0, 4);
      }
      try
      {
        switch (BitConverter.ToUInt32(imgHeader, 0))
        {
          case 0x00010000:
            using (System.Drawing.Icon ico = new System.Drawing.Icon(imgFile, 16, 16))
            {
              pctPNG16 = GenerateCloneImage(ico);
            }
            using (System.Drawing.Icon ico = new System.Drawing.Icon(imgFile, 32, 32))
            {
              pctPNG32 = GenerateCloneImage(ico);
            }
            break;
          case 0x474E5089:
          case 0x38464947:
            using (System.Drawing.Image ico = System.Drawing.Image.FromFile(imgFile))
            {
              pctPNG16 = GenerateCloneImage(ico, 16, 16);
              pctPNG32 = GenerateCloneImage(ico, 32, 32);
            }
            break;
          default:
            using (System.Drawing.Image ico = System.Drawing.Image.FromFile(imgFile))
            {
              pctPNG16 = GenerateCloneImage(ico, 16, 16);
              pctPNG32 = GenerateCloneImage(ico, 32, 32);
            }
            break;
        }
      }
      catch (Exception)
      {
        didOK = false;
      }
      if (File.Exists(imgFile))
        File.Delete(imgFile);
      if (didOK)
      {
        c_callback.Invoke(modFunctions.ImageToPixbuf((System.Drawing.Image) pctPNG16.Clone()), modFunctions.ImageToPixbuf((System.Drawing.Image) pctPNG32.Clone()), Token, null);
      }
      else
      {
        if (!trySimpler)
        {
          c_callback.Invoke(null, null, Token, new Exception("Failed to read the icon."));
        }
        else
        {
          try
          {
            ConnectToFile(new Uri(URI.Scheme + "://" + URI.Host + "/favicon.ico"), imgFile, Token, false);
          }
          catch (Exception)
          {
          }
        }
      }
      if (pctPNG16 != null)
      {
        pctPNG16.Dispose();
        pctPNG16 = null;
      }
      if (pctPNG32 != null)
      {
        pctPNG32.Dispose();
        pctPNG32 = null;
      }
    }
    #region "IDisposable Support"
    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposedValue)
      {
        if (disposing)
        {
          if (wsFile != null)
          {
            if (wsFile.IsBusy)
            {
              wsFile.CancelAsync();
            }
            wsFile.Dispose();
            wsFile = null;
          }
        }
      }
      this.disposedValue = true;
    }
    public void Dispose()
    {
      Dispose(true);
      System.GC.SuppressFinalize(this);
    }
    #endregion
  }
}
