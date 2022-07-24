using System;
using RestrictionTrackerGTK;
using RestrictionLibrary;
internal class GitHubReporter
{
  public const string ProjectID = "Satellite-Restriction-Tracker-for-MONO";
  private const string Token = "";
  private static string JSONEscape(string sInput)
  {
    if (sInput.Contains("\\"))
      sInput = sInput.Replace("\\", "\\\\");
    if (sInput.Contains("\""))
      sInput = sInput.Replace("\"", "\\\"");
    if (sInput.Contains("/"))
      sInput = sInput.Replace("/", "\\/");
    if (sInput.Contains("\n"))
      sInput = sInput.Replace("\n", "\\n");
    if (sInput.Contains("\r"))
      sInput = sInput.Replace("\r", "\\r");
    for (ushort i = 1; i < 32; i++)
    {
      if (sInput.Contains(Convert.ToChar(i).ToString()))
        sInput = sInput.Replace(Convert.ToChar(i).ToString(), "\\u" + srlFunctions.PadHex(i, 4));
    }
    for (ushort i = 127; i < 65535; i++)
    {
      if (sInput.Contains(Convert.ToChar(i).ToString()))
        sInput = sInput.Replace(Convert.ToChar(i).ToString(), "\\u" + srlFunctions.PadHex(i, 4));
    }
    return sInput;
  }
  static internal string MakeIssueTitle(Exception e)
  {
    string sSum = e.Message;
    if (sSum.Contains("\r\n"))
      sSum = sSum.Substring(0, sSum.IndexOf("\r\n"));
    else if (sSum.Contains("\r"))
      sSum = sSum.Substring(0, sSum.IndexOf("\r"));
    else if (sSum.Contains("\n"))
      sSum = sSum.Substring(0, sSum.IndexOf("\n"));
    if (sSum.Length > 80)
      sSum = sSum.Substring(0, 77) + "...";
    return sSum;
  }
  static internal string MakeIssueBody(Exception e)
  {
    string sPlat = Environment.Is64BitProcess ? "x64" : Environment.Is64BitOperatingSystem ? "x86-64" : "x86";
    string sVer = modFunctions.ProductVersion;
    int iParts = (sVer.Split('.')).Length;
    if (iParts > 3)
      sVer = sVer.Substring(0, sVer.LastIndexOf('.'));
    string sRet = "Error in " + modFunctions.ProductName + " v" + sVer + ":\r\n";
    sRet += "```\r\n";
    sRet += e.ToString() + "\r\n";
    sRet += "```\r\n\r\n";
    sRet += "OS: " + CurrentOS.Name + " (" + sPlat + ") v" + Environment.OSVersion.VersionString + "\r\n";
    sRet += "CLR: " + srlFunctions.GetCLRCleanVersion();
    return sRet;
  }
  private static string ReportBug(string Title, string Body)
  {
    string sSend = "{";
    sSend += "\"title\": \"" + JSONEscape(Title) + "\",";
    sSend += "\"body\": \"" + JSONEscape(Body) + "\"";
    sSend += "}";
    WebClientEx httpReport = new WebClientEx();
    httpReport.KeepAlive = false;
    httpReport.ErrorBypass = true;
    httpReport.SendHeaders = new System.Net.WebHeaderCollection();
    httpReport.SendHeaders.Add("Accept", "application/vnd.github+json");
    httpReport.SendHeaders.Add("Authorization", "token " + Token);
    string sRet = httpReport.UploadString("https://api.github.com/repos/RealityRipple/" + ProjectID + "/issues", "POST", sSend);
    if (httpReport.ResponseCode == System.Net.HttpStatusCode.Created)
    {
      try
      {
        JSONReader jRet = new JSONReader(new System.IO.MemoryStream(httpReport.Encoding.GetBytes(sRet), false), false);
        foreach (JSONReader.JSElement jNode in jRet.Serial)
        {
          if (jNode.Type != JSONReader.ElementType.Group)
            continue;
          foreach (JSONReader.JSElement jEl in jNode.SubElements)
          {
            if (jEl.Type != JSONReader.ElementType.KeyValue)
              continue;
            if (jEl.Key == "html_url")
              return jEl.Value;
          }
        }
      }
      catch (Exception)
      {
      }
      return "https://github.com/RealityRipple/" + ProjectID + "/issues";
    }
    switch (httpReport.ResponseCode)
    {
      case System.Net.HttpStatusCode.Forbidden:
        return "HTTP Error 403: Forbidden\r\nYou are forbidden from reporting this error.";
      case System.Net.HttpStatusCode.NotFound:
        return "HTTP Error 404: Not Found\r\nThe " + ProjectID + " GitHub repository is not available.";
      case System.Net.HttpStatusCode.Gone:
        return "HTTP Error 410: Gone\r\nThe " + ProjectID + " GitHub repository does not have issues enabled at this time.";
      case (System.Net.HttpStatusCode)422:
        return "HTTP Error 422: Validation Failed\r\nThe error you attempted to report was invalid.";
      case System.Net.HttpStatusCode.ServiceUnavailable:
        return "HTTP Error 503: Service Unavailable\r\nThe GitHub service is not available at this time.";
    }
    string msg = "Unknown Error";
    try
    {
      JSONReader jRet = new JSONReader(new System.IO.MemoryStream(httpReport.Encoding.GetBytes(sRet), false), false);
      foreach (JSONReader.JSElement jNode in jRet.Serial)
      {
        if (jNode.Type != JSONReader.ElementType.Group)
          continue;
        foreach (JSONReader.JSElement jEl in jNode.SubElements)
        {
          if (jEl.Type != JSONReader.ElementType.KeyValue)
            continue;
          if (jEl.Key == "message")
          {
            msg = jEl.Value;
            break;
          }
        }
        if (msg != "Unknown Error")
          break;
      }
    }
    catch (Exception)
    {
      msg = "Unknown Error (Invalid JSON)";
    }
    if (Enum.IsDefined(httpReport.ResponseCode.GetType(), httpReport.ResponseCode))
      return "HTTP Error " + ((int)httpReport.ResponseCode) + ": " + Enum.GetName(httpReport.ResponseCode.GetType(), httpReport.ResponseCode) + "\n" + msg;
    return "HTTP Error " + ((int)httpReport.ResponseCode) + "\n" + msg;
  }
  static internal string ReportIssue(Exception e)
  {
    if (Token == "")
      return "Unable to Report: GitHub Account Token Not Provided";
    string sSum = MakeIssueTitle(e);
    string sBod = MakeIssueBody(e);
    return ReportBug(sSum, sBod);
  }
}
