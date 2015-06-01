using System;
using System.IO;
using System.Drawing;

namespace RestrictionTrackerGTK
{
  public class clsFavicon : IDisposable
  {
    private RestrictionLibrary.WebClientEx wsNetTest;
    public class DownloadIconCompletedEventArgs:EventArgs
    {
      public Exception Error;
      public Gdk.Pixbuf Icon32;
      public Gdk.Pixbuf Icon16;
      public DownloadIconCompletedEventArgs(Gdk.Pixbuf icon16, Gdk.Pixbuf icon32)
      {
        this.Icon16 = icon16;
        this.Icon32 = icon32;
        this.Error = null;
      }
      public DownloadIconCompletedEventArgs(Exception error)
      {
        this.Icon16 = null;
        this.Icon32 = null;
        this.Error = error;
      }
    }
    public event DownloadIconCompletedEventHandler DownloadIconCompleted;
    public delegate void DownloadIconCompletedEventHandler(object sender, DownloadIconCompletedEventArgs e);
    public clsFavicon(string URL)
    {
      if (string.IsNullOrEmpty(URL))
        return;
      if (!URL.Contains("://"))
        URL = "http://" + URL;
      ConnectToURL(new Uri(URL), URL);
    }
    private void ConnectToURL(Uri URL, object token)
    {
      try
      {
        wsNetTest = new RestrictionLibrary.WebClientEx();
        wsNetTest.DownloadStringCompleted += wsNetTest_DownloadStringCompleted;
        wsNetTest.ErrorBypass = true;
        wsNetTest.DownloadStringAsync(URL, token);
      }
      catch(Exception)
      {
        if (DownloadIconCompleted != null)
        {
          DownloadIconCompleted(this, new DownloadIconCompletedEventArgs(new Exception("Failed to initialize connection to \"" + URL.OriginalString + "\"!")));
        }
      }
    }

    private void ConnectToFile(Uri URL, string Filename)
    {
      ConnectToFile(URL, Filename, null);
    }
    
    private void ConnectToFile(Uri URL, string Filename, object token)
    {
      try
      {
        wsNetTest = new RestrictionLibrary.WebClientEx();
        wsNetTest.DownloadFileCompleted+= wsNetTest_DownloadFileCompleted;
        wsNetTest.ErrorBypass = true;
        wsNetTest.DownloadFileAsync(URL, Filename, token);
      }
      catch(Exception)
      {
        if (DownloadIconCompleted != null)
        {
          DownloadIconCompleted(this, new DownloadIconCompletedEventArgs(new Exception("Failed to initialize connection to \"" + URL.OriginalString + "\"!")));
        }
      }
    }

    private Bitmap GenerateCloneImage(Image fromImage,int width,int height)
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

    private Bitmap GenerateCloneImage(Icon fromIcon)
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
    
    void wsNetTest_DownloadFileCompleted (object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      if (e.Error != null)
      {
        if (e.UserState == null)
        {
          if (DownloadIconCompleted != null)
          {
            DownloadIconCompleted(this, new DownloadIconCompletedEventArgs(new Exception("Failed to get an icon.")));
          }
        }
        else
        {
          Uri pathURL = new Uri((string)e.UserState);
          ConnectToFile(new Uri(pathURL.Scheme + "://" + pathURL.Host + "/favicon.ico"), Path.Combine(Path.GetTempPath(), "srt_nettest_favicon.ico"), null);
        }
        return;
      }
      else if (e.Cancelled)
      {
        return;
      }
      string imgFile = Path.Combine(Path.GetTempPath(), "srt_nettest_favicon.ico");
      Bitmap pctPNG16, pctPNG32;
      bool didOK = true;
      pctPNG16 = new Bitmap(16, 16);
      pctPNG32 = new Bitmap(32, 32);
      using (Graphics gPNG = Graphics.FromImage(pctPNG16))
      {
        byte[] imgHeader = new byte[4];
        using (FileStream iStream = File.OpenRead(imgFile))
        {
          iStream.Read(imgHeader, 0, 4);
        }
        try
        {
          switch (BitConverter.ToUInt32(imgHeader,0))
          {
            case 0x00010000:
              using (Icon ico = new Icon(imgFile, 16, 16))
              {
                pctPNG16 = GenerateCloneImage(ico);
              }
              using (Icon ico = new Icon(imgFile, 32, 32))
              {
                pctPNG32 = GenerateCloneImage(ico);
              }
              break;
            case 0x474E5089:
            case 0x38464947:
              using (Image ico = Image.FromFile(imgFile))
              {
                pctPNG16 = GenerateCloneImage(ico, 16, 16);
                pctPNG32 = GenerateCloneImage(ico, 32, 32);
              }
              break;
            default:
              using (Image ico = Image.FromFile(imgFile))
              {
                pctPNG16 = GenerateCloneImage(ico, 16, 16);
                pctPNG32 = GenerateCloneImage(ico, 32, 32);
              }
              break;
          }
        }
        catch(Exception)
        {
          didOK = false;
        }
      }
      if (File.Exists(imgFile))
        File.Delete(imgFile);
      if (didOK)
      {
        if (DownloadIconCompleted != null)
          DownloadIconCompleted(this, new DownloadIconCompletedEventArgs(modFunctions.ImageToPixbuf((Image) pctPNG16.Clone()), modFunctions.ImageToPixbuf((Image) pctPNG32.Clone())));
      }
      else
      {
        if (e.UserState == null)
        {
          if (DownloadIconCompleted != null)
            DownloadIconCompleted(this, new DownloadIconCompletedEventArgs(new Exception("Failed to read the icon.")));
        }
        else
        {
          Uri pathURL = new Uri((string) e.UserState);
          ConnectToFile(new Uri(pathURL.Scheme + "://" + pathURL.Host + "/favicon.ico"), imgFile);
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

    void wsNetTest_DownloadStringCompleted (object sender, System.Net.DownloadStringCompletedEventArgs e)
    {
      if (e.Error != null)
      {
        Uri pathURL = new Uri((string) e.UserState);
        ConnectToFile(new Uri(pathURL.Scheme + "://" + pathURL.Host + "/favicon.ico"), Path.Combine(Path.GetTempPath(), "srt_nettest_favicon.ico"), null);
      }
      else if (e.Cancelled)
      {
        return;
      }
      else
      {
        try
        {
          string sHTML = e.Result;
          if (sHTML.ToLower().Contains("shortcut icon"))
          {
            int startsAt = sHTML.ToLower().IndexOf("shortcut icon");
            int endsAt = startsAt + 13;
            sHTML = sHTML.Substring(0,startsAt) + "icon" + sHTML.Substring(endsAt);
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
                        string oldURL = (string) e.UserState;
                        if (oldURL.Contains("://"))
                          oldURL = oldURL.Substring(0, oldURL.IndexOf("://") + 1);
                        URL = oldURL + URL;
                      }
                      else
                      {
                        string oldURL = (string) e.UserState;
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
                      ConnectToFile(new Uri(URL), Path.Combine(Path.GetTempPath(), "srt_nettest_favicon.ico"), URL);
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
        Uri pathURL = new Uri((string) e.UserState);
        ConnectToFile(new Uri(pathURL.Scheme + "://" + pathURL.Host + "/favicon.ico"), Path.Combine(Path.GetTempPath(), "srt_nettest_favicon.ico"), null);
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
          if (wsNetTest != null)
          {
            if (wsNetTest.IsBusy)
            {
              wsNetTest.CancelAsync();
            }
            wsNetTest.Dispose();
            wsNetTest = null;
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

