using System;

namespace RestrictionTrackerGTK
{
	internal class MantisReporter
	{
		private static RestrictionLibrary.CookieAwareWebClient httpSend;
		internal enum Mantis_Category
		{
			Select = 0,
			General = 1,
			Interface = 3,
			Setup = 4
    }
		private enum Mantis_Reproducibility
		{
			Always = 10,
			Sometimes = 30,
			Random = 50,
			Have_Not_Tried = 70,
			Unable_to_Reproduce = 90,
			NotApplicable = 100
    }
		private enum Mantis_Severity
		{
			Feature = 10,
			Trivial = 20,
			Text = 30,
			Tweak = 40,
			Minor = 50,
			Major = 60,
			Crash = 70,
			Block = 80
    }
		private enum Mantis_Priority
		{
			None = 10,
			Low = 20,
			Normal = 30,
			High = 40,
			Urgent = 50,
			Immediate = 60
    }

		private static string GetToken(int Project_ID)
    {
      System.Collections.Specialized.NameValueCollection pD1 = new System.Collections.Specialized.NameValueCollection();
      pD1.Add("ref", "bug_report_page.php");
      pD1.Add("project_id", Project_ID.ToString().Trim());
      pD1.Add("make_default", null);
      httpSend.Headers.Add(System.Net.HttpRequestHeader.Referer, "http://bugs.realityripple.com/login_select_proj_page.php?bug_report_page.php");
      byte[] bTok = httpSend.UploadValues("http://bugs.realityripple.com/set_project.php", "POST", pD1);
      string sTok = System.Text.Encoding.GetEncoding(28591).GetString(bTok);
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

		private static string ReportBug(string Token, int Project_ID, Mantis_Category Category, Mantis_Reproducibility Reproducable, Mantis_Severity Severity, Mantis_Priority Priority, string Platform, string OS, string OS_Build, string Summary,
		string Description, string Steps, string Info, bool Public)
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
      byte[] bRet = null;
      bRet = httpSend.UploadValues("http://bugs.realityripple.com/bug_report.php", "POST", pData);
      string sRet = System.Text.Encoding.GetEncoding(28591).GetString(bRet);
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
      httpSend = new RestrictionLibrary.CookieAwareWebClient();
      string sTok = GetToken(1);
      if (string.IsNullOrEmpty(sTok))
      {
        return "No token was supplied by the server.";
      }
      string sPlat = Environment.OSVersion.Platform.ToString();
      string sSum = e.Message;
      if (sSum.Length > 80)
      {
        sSum = sSum.Substring(0, 77) + "...";
      }
      string sDesc = e.Message;
      if (!string.IsNullOrEmpty(e.StackTrace))
      {
        sDesc += "\r\n" + e.StackTrace;
      }
      else
      {
        if (!string.IsNullOrEmpty(e.Source))
        {
          sDesc += "\r\n @ " + e.Source;
          if (e.TargetSite != null)
          {
            sDesc += "." + e.TargetSite.Name;
          }
        }
        else
        {
          if (e.TargetSite != null)
          {
            sDesc += "\r\n @ " + e.TargetSite.Name;
          }
        }
      }
      sDesc += "\r\nVersion " + modFunctions.ProductVersion();
      string sSteps = null;
      string sInfo = null;
      if (e.InnerException != null)
      {
        sInfo = e.InnerException.Message;
        if (e.InnerException.TargetSite != null)
        {
          sInfo += "\r\nTrace - " + e.InnerException.TargetSite.Name;
        }
        if (!string.IsNullOrEmpty(e.InnerException.Source))
        {
          sInfo += " > " + e.InnerException.Source;
        }
        if (!string.IsNullOrEmpty(e.InnerException.StackTrace))
        {
          sInfo += "\r\n" + e.InnerException.StackTrace;
        }
      }
      string MyOS = CurrentOS.Name;
      string MyOSVer = Environment.OSVersion.VersionString;
      return ReportBug(sTok, 2, Mantis_Category.General, Mantis_Reproducibility.Have_Not_Tried, Mantis_Severity.Minor, Mantis_Priority.Normal, sPlat, MyOS, MyOSVer, sSum,
			sDesc, sSteps, sInfo, true);
    }
	}
}

