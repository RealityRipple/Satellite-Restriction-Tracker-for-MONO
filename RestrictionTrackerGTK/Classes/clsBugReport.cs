using System;
using RestrictionTrackerGTK;
using RestrictionLibrary;
namespace MantisBugTracker
{
  internal class MantisReporter
  {
    public const int ProjectID = 2;
    private static System.Net.CookieContainer cJar;
    private static string GetToken(int Project_ID)
    {
      System.Collections.Specialized.NameValueCollection pD1 = new System.Collections.Specialized.NameValueCollection();
      pD1.Add("ref", "bug_report_page.php");
      pD1.Add("project_id", Project_ID.ToString().Trim());
      pD1.Add("make_default", string.Empty);
      WebClientEx httpToken = new WebClientEx();
      cJar = new System.Net.CookieContainer();
      httpToken.KeepAlive = false;
      httpToken.CookieJar = cJar;
      httpToken.SendHeaders = new System.Net.WebHeaderCollection();
      httpToken.SendHeaders.Add(System.Net.HttpRequestHeader.Referer, "http://bugs.realityripple.com/login_select_proj_page.php?bug_report_page.php");
      string sTok = httpToken.UploadValues("http://bugs.realityripple.com/set_project.php", "POST", pD1);
      if (sTok.StartsWith("Error: "))
        return null;
      if (sTok.Contains("bug_report_token"))
      {
        sTok = sTok.Substring(sTok.IndexOf("bug_report_token") + 25);
        sTok = sTok.Substring(0, sTok.IndexOf("\"/"));
        return sTok;
      }
      else
      {
        return null;
      }
    }
    private static string ReportBug(string Token, int Project_ID, Mantis_Category Category, Mantis_Reproducibility Reproducable, Mantis_Severity Severity, Mantis_Priority Priority, string Platform, string OS, string OS_Build, string Product_Version, string Summary, string Description, string Steps, string Info, bool Public)
    {
      System.Collections.Specialized.NameValueCollection pData = new System.Collections.Specialized.NameValueCollection();
      pData.Add("bug_report_token", Token);
      pData.Add("m_id", "0");
      pData.Add("project_id", Project_ID.ToString());
      pData.Add("category_id", Category.ToString("d"));
      pData.Add("reproducibility", Reproducable.ToString("d"));
      pData.Add("severity", Severity.ToString("d"));
      pData.Add("priority", Priority.ToString("d"));
      pData.Add("platform", Platform);
      pData.Add("os", OS);
      pData.Add("os_build", OS_Build);
      pData.Add("product_version", Product_Version);
      pData.Add("summary", Summary);
      pData.Add("description", Description);
      pData.Add("steps_to_reproduce", Steps);
      pData.Add("additional_info", Info);
      if (Public)
      {
        pData.Add("view_state", "10");
      }
      else
      {
        pData.Add("view_state", "50");
      }
      pData.Add("report_stay", string.Empty);
      string sRet;
      WebClientEx httpReport = new WebClientEx();
      httpReport.KeepAlive = false;
      httpReport.CookieJar = cJar;
      sRet = httpReport.UploadValues("http://bugs.realityripple.com/bug_report.php", "POST", pData);
      if (sRet.StartsWith("Error: "))
        return null;
      if (sRet.Contains("Operation successful."))
      {
        return "OK";
      }
      else
      {
        sRet = sRet.Substring(sRet.IndexOf("width50") - 14);
        sRet = sRet.Substring(0, sRet.IndexOf("</table>") + 8);
        return sRet;
      }
    }
    static internal string ReportIssue(Exception e)
    {
      string sTok = GetToken(ProjectID);
      if (string.IsNullOrEmpty(sTok))
      {
        return "No token was supplied by the server.";
      }
      string sPlat = null;
      if (CurrentOS.Is64BitProcess)
        sPlat = "x64";
      else
      {
        if (CurrentOS.Is64bit)
          sPlat = "x86-64";
        else
          sPlat = "x86";
      }
      string sSum = e.Message;
      if (sSum.Length > 80)
      {
        sSum = sSum.Substring(0, 77) + "...";
      }
      string sDesc = e.ToString();
      string MyOS = CurrentOS.Name;
      string MyOSVer = Environment.OSVersion.VersionString;
      string sVer = modFunctions.ProductVersion;
      int iParts = (sVer.Split('.')).Length;
      if (iParts > 3)
        sVer = sVer.Substring(0, sVer.LastIndexOf('.'));
      return ReportBug(sTok, ProjectID, Mantis_Category.General, Mantis_Reproducibility.Have_Not_Tried, Mantis_Severity.Minor, Mantis_Priority.Normal, sPlat, MyOS, MyOSVer, sVer, sSum, sDesc, string.Empty, string.Empty, true);
    }
  }
}
