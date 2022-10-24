using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using RestrictionLibrary;
using Atk;
using System.Runtime.InteropServices;
namespace RestrictionTrackerGTK
{
  class AppSettings
  {
    private string m_Account;
    private int m_StartWait;
    private int m_Interval;
    private string m_Gr;
    private DateTime m_LastUpdate;
    private DateTime m_LastSyncTime;
    private int m_Accuracy;
    private uint m_Ago;
    private string m_HistoryDir;
    private bool m_UpdateBETA;
    private UpdateTypes m_UpdateType;
    private byte m_UpdateTime;
    private bool m_ScaleScreen;
    private Gdk.Size m_MainSize;
    private string m_RemoteKey;
    private string m_PassCrypt;
    private string m_PassKey;
    private string m_PassSalt;
    private int m_Timeout;
    private int m_Retries;
    private int m_Overuse;
    private int m_Overtime;
    private string m_AlertStyle;
    private TrayStyles m_TrayIcon;
    private bool m_TrayClose;
    private bool m_AutoHide;
    private bool m_TLSProxy;
    private string m_ProxySetting;
    private string m_NetTest;
    private SecurityProtocolType m_SecurProtocol;
    private bool m_SecurEnforced;
    public bool Loaded;
    public AppColors Colors;
    private static string ConfigFile
    {
      get
      {
        return System.IO.Path.Combine(modFunctions.AppData, "user.config");
      }
    }
    private static string ConfigFileBackup
    {
      get
      {
        return System.IO.Path.Combine(modFunctions.AppData, "backup.config");
      }
    }
    public AppSettings()
    {
      Load();
    }
    private void Load()
    {
      Loaded = false;
      if (!File.Exists(ConfigFile))
      {
        Reset();
        Loaded = true;
        return;
      }
      XmlDocument m_xmld = new XmlDocument();
      try
      {
        m_xmld.Load(ConfigFile);
      }
      catch (Exception)
      {
        if (File.Exists(ConfigFileBackup))
        {
          File.Delete(ConfigFile);
          File.Move(ConfigFileBackup, ConfigFile);
          Load();
          if (Loaded)
            return;
        }
        Reset();
        Loaded = true;
        return;
      }
      if (!m_xmld.HasChildNodes)
      {
        Reset();
        Loaded = true;
        return;
      }
      if (m_xmld.ChildNodes.Count < 2)
      {
        Reset();
        Loaded = true;
        return;
      }
      XmlNode xConfig = m_xmld.ChildNodes[1];
      if (!xConfig.HasChildNodes)
      {
        Reset();
        Loaded = true;
        return;
      }
      XmlNode xUserSettings = xConfig.ChildNodes[0];
      if (!xUserSettings.HasChildNodes)
      {
        Reset();
        Loaded = true;
        return;
      }
      XmlNode xMySettings = xUserSettings.ChildNodes[0];
      if (!xMySettings.HasChildNodes)
      {
        Reset();
        Loaded = true;
        return;
      }
      XmlNodeList xNodeList = xMySettings.ChildNodes;
      Reset();
      foreach (XmlNode m_node in xNodeList)
      {
        string xName = m_node.Attributes[0].InnerText;
        string xValue = m_node.FirstChild.InnerText;
        if (xName.CompareTo("Account") == 0)
        {
          m_Account = xValue;
        }
        else if (xName.CompareTo("StartWait") == 0)
        {
          if (!int.TryParse(xValue, out m_StartWait))
          {
            m_StartWait = 5;
          }
        }
        else if (xName.CompareTo("Interval") == 0)
        {
          if (!int.TryParse(xValue, out m_Interval))
          {
            m_Interval = 15;
          }
        }
        else if (xName.CompareTo("Gr") == 0)
        {
          m_Gr = xValue;
        }
        else if (xName.CompareTo("LastUpdate") == 0)
        {
          m_LastUpdate = DateTime.FromBinary(long.Parse(xValue));
        }
        else if (xName.CompareTo("LastSyncTime") == 0)
        {
          m_LastSyncTime = DateTime.FromBinary(long.Parse(xValue));
        }
        else if (xName.CompareTo("Accuracy") == 0)
        {
          if (!int.TryParse(xValue, out m_Accuracy))
          {
            m_Accuracy = 0;
          }
        }
        else if (xName.CompareTo("Ago") == 0)
        {
          if (!uint.TryParse(xValue, out m_Ago))
          {
            m_Ago = 30;
          }
        }
        else if (xName.CompareTo("HistoryDir") == 0)
        {
          m_HistoryDir = xValue;
        }
        else if (xName.CompareTo("UpdateBETA") == 0)
        {
          m_UpdateBETA = (xValue.CompareTo("True") == 0);
        }
        else if (xName.CompareTo("BetaCheck") == 0)
        {
          m_UpdateBETA = (xValue.CompareTo("True") == 0);
        }
        else if (xName.CompareTo("UpdateType") == 0)
        {
          switch (xValue)
          {
            case "BETA":
              m_UpdateBETA = true;
              m_UpdateType = UpdateTypes.Ask;
              break;
            case "Auto":
              m_UpdateType = UpdateTypes.Auto;
              break;
            case "None":
              m_UpdateType = UpdateTypes.None;
              break;
            default:
              m_UpdateType = UpdateTypes.Ask;
              break;
          }
        }
        else if (xName.CompareTo("UpdateTime") == 0)
        {
          if (!byte.TryParse(xValue, out m_UpdateTime))
          {
            m_UpdateTime = 15;
          }
        }
        else if (xName.CompareTo("ScaleScreen") == 0)
        {
          m_ScaleScreen = (xValue.CompareTo("True") == 0);
        }
        else if (xName.CompareTo("MainSize") == 0)
        {
          char[] comma = { ',' };
          string[] sSizes = xValue.Split(comma, 2);
          int iWidth = 450;
          if (!int.TryParse(sSizes[0], out iWidth))
          {
            iWidth = 450;
          }
          int iHeight = 200;
          if (!int.TryParse(sSizes[1], out iHeight))
          {
            iHeight = 200;
          }
          m_MainSize = new Gdk.Size(iWidth, iHeight);
        }
        else if (xName.CompareTo("RemoteKey") == 0)
        {
          m_RemoteKey = xValue;
        }
        else if (xName.CompareTo("PassCrypt") == 0)
        {
          m_PassCrypt = xValue;
          m_PassKey = "";
          m_PassSalt = "";
          if (m_node.Attributes.Count > 1)
          {
            foreach (XmlAttribute m_attrib in m_node.Attributes)
            {
              if (m_attrib.Name.CompareTo("key") == 0)
                m_PassKey = m_attrib.InnerText;
              else if (m_attrib.Name.CompareTo("salt") == 0)
                m_PassSalt = m_attrib.InnerText;
            }
          }
        }
        else if (xName.CompareTo("Timeout") == 0)
        {
          if (!int.TryParse(xValue, out m_Timeout))
          {
            m_Timeout = 120;
          }
        }
        else if (xName.CompareTo("Retries") == 0)
        {
          if (!int.TryParse(xValue, out m_Retries))
          {
            m_Retries = 2;
          }
        }
        else if (xName.CompareTo("Overuse") == 0)
        {
          if (!int.TryParse(xValue, out m_Overuse))
          {
            m_Overuse = 0;
          }
        }
        else if (xName.CompareTo("Overtime") == 0)
        {
          if (!int.TryParse(xValue, out m_Overtime))
          {
            m_Overtime = 60;
          }
        }
        else if (xName.CompareTo("AlertStyle") == 0)
        {
          m_AlertStyle = xValue;
        }
        else if (xName.CompareTo("TrayIcon") == 0)
        {
          switch (xValue)
          {
            case "Never":
              m_TrayIcon = TrayStyles.Never;
              break;
            case "Minimized":
              m_TrayIcon = TrayStyles.Minimized;
              break;
            default:
              m_TrayIcon = TrayStyles.Always;
              break;
          }
        }
        else if (xName.CompareTo("TrayClose") == 0)
        {
          m_TrayClose = (xValue.CompareTo("True") == 0);
        }
        else if (xName.CompareTo("AutoHide") == 0)
        {
          m_AutoHide = (xValue.CompareTo("True") == 0);
        }
        else if (xName.CompareTo("TLSProxy") == 0)
        {
          m_TLSProxy = (xValue.CompareTo("True") == 0);
        }
        else if (xName.CompareTo("Proxy") == 0)
        {
          m_ProxySetting = xValue;
        }
        else if (xName.CompareTo("Protocol") == 0)
        {
          m_SecurProtocol = (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.None;
          if (xValue.Contains("SSL"))
          {
            m_SecurProtocol |= (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.Ssl3;
          }
          if (xValue.Contains("TLS10"))
          {
            m_SecurProtocol |= (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.Tls10;
          }
          if (xValue.Contains("TLS11"))
          {
            m_SecurProtocol |= (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.Tls11;
          }
          if (xValue.Contains("TLS12"))
          {
            m_SecurProtocol |= (System.Net.SecurityProtocolType)SecurityProtocolTypeEx.Tls12;
          }
          if (xValue.Contains("TLS") & !xValue.Contains("TLS1"))
          {
            m_SecurProtocol |= (System.Net.SecurityProtocolType)(SecurityProtocolTypeEx.Tls11 | SecurityProtocolTypeEx.Tls12);
          }
        }
        else if (xName.CompareTo("EnforcedSecurity") == 0)
        {
          m_SecurEnforced = (xValue.CompareTo("True") == 0);
        }
        else if (xName.CompareTo("NetTestURL") == 0)
        {
          m_NetTest = xValue;
        }
      }
      Colors = new AppColors();
      if (xConfig.ChildNodes.Count < 2)
      {
        ResetColors();
      }
      else
      {
        XmlNode xColorSettings = xConfig.ChildNodes[1];
        if (!xColorSettings.HasChildNodes)
        {
          ResetColors();
        }
        else
        {
          foreach (XmlNode m_graph in xColorSettings.ChildNodes)
          {
            string graphName = m_graph.Attributes[0].InnerText;
            if (m_graph.HasChildNodes)
            {
              foreach (XmlNode m_node in m_graph.ChildNodes)
              {
                string nodeName = m_node.Attributes[0].InnerText;
                if (nodeName.CompareTo("Download") == 0)
                {
                  //Download Section
                  if (m_node.HasChildNodes)
                  {
                    foreach (XmlNode xSetting in m_node.ChildNodes)
                    {
                      string xName = xSetting.Attributes[0].InnerText;
                      string xValue = xSetting.FirstChild.InnerText;
                      if (xName.CompareTo("Line") == 0)
                      {
                        if (graphName.CompareTo("History") == 0)
                        {
                          Colors.HistoryDownLine = StrToColor(xValue);
                        }
                      }
                      else if (xName.CompareTo("Start") == 0)
                      {
                        if (graphName.CompareTo("Main") == 0)
                        {
                          Colors.MainDownA = StrToColor(xValue);
                        }
                        else if (graphName.CompareTo("Tray") == 0)
                        {
                          Colors.TrayDownA = StrToColor(xValue);
                        }
                        else if (graphName.CompareTo("History") == 0)
                        {
                          Colors.HistoryDownA = StrToColor(xValue);
                        }
                      }
                      else if (xName.CompareTo("Mid") == 0)
                      {
                        if (graphName.CompareTo("Main") == 0)
                        {
                          Colors.MainDownB = StrToColor(xValue);
                        }
                        else if (graphName.CompareTo("Tray") == 0)
                        {
                          Colors.TrayDownB = StrToColor(xValue);
                        }
                        else if (graphName.CompareTo("History") == 0)
                        {
                          Colors.HistoryDownB = StrToColor(xValue);
                        }
                      }
                      else if (xName.CompareTo("End") == 0)
                      {
                        if (graphName.CompareTo("Main") == 0)
                        {
                          Colors.MainDownC = StrToColor(xValue);
                        }
                        else if (graphName.CompareTo("Tray") == 0)
                        {
                          Colors.TrayDownC = StrToColor(xValue);
                        }
                        else if (graphName.CompareTo("History") == 0)
                        {
                          Colors.HistoryDownC = StrToColor(xValue);
                        }
                      }
                      else if (xName.CompareTo("Maximum") == 0)
                      {
                        if (graphName.CompareTo("History") == 0)
                        {
                          Colors.HistoryDownMax = StrToColor(xValue);
                        }
                      }
                    }
                  }
                }
                else if (nodeName.CompareTo("Text") == 0)
                {
                  //Text setting
                  string nodeVal = m_node.FirstChild.InnerText;
                  if (graphName.CompareTo("Main") == 0)
                  {
                    Colors.MainText = StrToColor(nodeVal);
                  }
                  else if (graphName.CompareTo("History") == 0)
                  {
                    Colors.HistoryText = StrToColor(nodeVal);
                  }
                }
                else if (nodeName.CompareTo("Background") == 0)
                {
                  //Background setting
                  string nodeVal = m_node.FirstChild.InnerText;
                  if (graphName.CompareTo("Main") == 0)
                  {
                    Colors.MainBackground = StrToColor(nodeVal);
                  }
                  else if (graphName.CompareTo("History") == 0)
                  {
                    Colors.HistoryBackground = StrToColor(nodeVal);
                  }
                }
                else if (nodeName.CompareTo("Grid") == 0)
                {
                  //Grid Section
                  if (m_node.HasChildNodes)
                  {
                    foreach (XmlNode xSetting in m_node.ChildNodes)
                    {
                      string xName = xSetting.Attributes[0].InnerText;
                      string xValue = xSetting.FirstChild.InnerText;
                      if (xName.CompareTo("Light") == 0)
                      {
                        if (graphName.CompareTo("History") == 0)
                        {
                          Colors.HistoryLightGrid = StrToColor(xValue);
                        }
                      }
                      else if (xName.CompareTo("Dark") == 0)
                      {
                        if (graphName.CompareTo("History") == 0)
                        {
                          Colors.HistoryDarkGrid = StrToColor(xValue);
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
      Loaded = true;
    }
    private void Reset()
    {
      m_Account = null;
      m_StartWait = 5;
      m_Interval = 15;
      m_Gr = "aph";
      m_LastUpdate = new DateTime(2000, 1, 1);
      m_LastSyncTime = new DateTime(2000, 1, 1);
      m_Accuracy = 0;
      m_Ago = 30u;
      m_HistoryDir = null;
      m_UpdateBETA = true;
      m_UpdateType = UpdateTypes.Ask;
      m_UpdateTime = 15;
      m_ScaleScreen = false;
      m_MainSize = new Gdk.Size(450, 200);
      m_RemoteKey = null;
      m_PassCrypt = null;
      m_PassKey = "";
      m_PassSalt = "";
      m_Timeout = 120;
      m_Retries = 2;
      m_Overuse = 0;
      m_Overtime = 60;
      m_AlertStyle = "Default";
      m_TrayIcon = TrayStyles.Always;
      m_TrayClose = false;
      m_AutoHide = true;
      m_TLSProxy = false;
      m_ProxySetting = "System";
      m_SecurProtocol = (System.Net.SecurityProtocolType)(SecurityProtocolTypeEx.Tls11 | SecurityProtocolTypeEx.Tls12);
      m_SecurEnforced = false;
      m_NetTest = null;
      Colors = new AppColors();
      ResetColors();
    }
    private void ResetColors()
    {
      ResetMain();
      ResetTray();
      ResetHistory();
    }
    private void ResetMain()
    {
      Colors.MainDownA = Color.Transparent;
      Colors.MainDownB = Color.Transparent;
      Colors.MainDownC = Color.Transparent;
      Colors.MainText = Color.Transparent;
      Colors.MainBackground = Color.Transparent;
    }
    private void ResetTray()
    {
      Colors.TrayDownA = Color.Transparent;
      Colors.TrayDownB = Color.Transparent;
      Colors.TrayDownC = Color.Transparent;
    }
    private void ResetHistory()
    {
      Colors.HistoryDownA = Color.Transparent;
      Colors.HistoryDownB = Color.Transparent;
      Colors.HistoryDownC = Color.Transparent;
      Colors.HistoryDownMax = Color.Transparent;
      Colors.HistoryText = Color.Transparent;
      Colors.HistoryBackground = Color.Transparent;
      Colors.HistoryLightGrid = Color.Transparent;
      Colors.HistoryDarkGrid = Color.Transparent;
    }
    public bool Save()
    {
      string sBeta = "False";
      if (m_UpdateBETA)
      {
        sBeta = "True";
      }
      string sUpdateType = "Ask";
      switch (m_UpdateType)
      {
        case UpdateTypes.Auto:
          sUpdateType = "Auto";
          break;
        case UpdateTypes.Ask:
          sUpdateType = "Ask";
          break;
        case UpdateTypes.None:
          sUpdateType = "None";
          break;
      }
      string sScaleScreen = "False";
      if (m_ScaleScreen)
      {
        sScaleScreen = "True";
      }
      string sProtocol = "";
      foreach (SecurityProtocolTypeEx protocolTest in Enum.GetValues(typeof(SecurityProtocolTypeEx)))
      {
        if ((((SecurityProtocolTypeEx)m_SecurProtocol) & protocolTest) == protocolTest)
        {
          sProtocol += Enum.GetName(typeof(SecurityProtocolTypeEx), protocolTest).ToUpperInvariant() + ", ";
        }
      }
      if (!string.IsNullOrEmpty(sProtocol) && sProtocol.EndsWith(", "))
      {
        sProtocol = sProtocol.Substring(0, sProtocol.Length - 2);
      }
      string sSecurEnforced = "False";
      if (m_SecurEnforced)
        sSecurEnforced = "True";
      string sTrayIcon = "Always";
      if (m_TrayIcon == TrayStyles.Never)
        sTrayIcon = "Never";
      else if (m_TrayIcon == TrayStyles.Minimized)
        sTrayIcon = "Minimized";
      string sAutoHide = "True";
      if (!m_AutoHide)
        sAutoHide = "False";
      string sTLSProxy = "True";
      if (!m_TLSProxy)
        sTLSProxy = "False";
      string sTrayClose = "True";
      if (!m_TrayClose)
        sTrayClose = "False";
      string sRet = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
        "<configuration>\n" +
        "  <userSettings>\n" +
        "    <RestrictionTracker.My.MySettings>\n" +
        "      <setting name=\"Account\">\n" +
        "        <value>" + m_Account + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"PassCrypt\" key=\"" + m_PassKey + "\" salt=\"" + m_PassSalt + "\">\n" +
        "        <value>" + m_PassCrypt + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"StartWait\">\n" +
        "        <value>" + m_StartWait.ToString() + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"Interval\">\n" +
        "        <value>" + m_Interval.ToString() + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"Gr\">\n" +
        "        <value>" + m_Gr + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"LastUpdate\">\n" +
        "        <value>" + m_LastUpdate.ToBinary() + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"LastSyncTime\">\n" +
        "        <value>" + m_LastSyncTime.ToBinary() + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"Accuracy\">\n" +
        "        <value>" + m_Accuracy.ToString() + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"Ago\">\n" +
        "        <value>" + m_Ago.ToString() + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"HistoryDir\">\n" +
        "        <value>" + m_HistoryDir + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"UpdateBETA\">\n" +
        "        <value>" + sBeta + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"UpdateType\">\n" +
        "        <value>" + sUpdateType + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"UpdateTime\">\n" +
        "        <value>" + m_UpdateTime.ToString() + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"ScaleScreen\">\n" +
        "        <value>" + sScaleScreen + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"MainSize\">\n" +
        "        <value>" + m_MainSize.Width.ToString() + "," + m_MainSize.Height.ToString() + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"RemoteKey\">\n" +
        "        <value>" + m_RemoteKey + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"Timeout\">\n" +
        "        <value>" + m_Timeout.ToString() + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"Retries\">\n" +
        "        <value>" + m_Retries.ToString() + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"Overuse\">\n" +
        "        <value>" + m_Overuse.ToString() + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"Overtime\">\n" +
        "        <value>" + m_Overtime.ToString() + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"AlertStyle\">\n" +
        "        <value>" + m_AlertStyle + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"TrayIcon\">\n" +
        "        <value>" + sTrayIcon + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"TrayClose\">\n" +
        "        <value>" + sTrayClose + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"AutoHide\">\n" +
        "        <value>" + sAutoHide + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"TLSProxy\">\n" +
        "        <value>" + sTLSProxy + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"Proxy\">\n" +
        "        <value>" + m_ProxySetting + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"Protocol\">\n" +
        "        <value>" + sProtocol + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"EnforcedSecurity\">\n" +
        "        <value>" + sSecurEnforced + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"NetTestURL\">\n" +
        "        <value>" + m_NetTest + "</value>\n" +
        "      </setting>\n" +
        "    </RestrictionTracker.My.MySettings>\n" +
        "  </userSettings>\n" +
        "  <colorSettings>\n" +
        "    <graph name=\"Main\">\n" +
        "      <section name=\"Download\">\n" +
        "        <setting name=\"Start\">\n" +
        "          <value>" + ColorToStr(Colors.MainDownA) + "</value>\n" +
        "        </setting>\n" +
        "        <setting name=\"Mid\">\n" +
        "          <value>" + ColorToStr(Colors.MainDownB) + "</value>\n" +
        "        </setting>\n" +
        "        <setting name=\"End\">\n" +
        "          <value>" + ColorToStr(Colors.MainDownC) + "</value>\n" +
        "        </setting>\n" +
        "      </section>\n" +
        "      <setting name=\"Text\">\n" +
        "        <value>" + ColorToStr(Colors.MainText) + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"Background\">\n" +
        "        <value>" + ColorToStr(Colors.MainBackground) + "</value>\n" +
        "      </setting>\n" +
        "    </graph>\n" +
        "    <graph name=\"Tray\">\n" +
        "      <section name=\"Download\">\n" +
        "        <setting name=\"Start\">\n" +
        "          <value>" + ColorToStr(Colors.TrayDownA) + "</value>\n" +
        "        </setting>\n" +
        "        <setting name=\"Mid\">\n" +
        "          <value>" + ColorToStr(Colors.TrayDownB) + "</value>\n" +
        "        </setting>\n" +
        "        <setting name=\"End\">\n" +
        "          <value>" + ColorToStr(Colors.TrayDownC) + "</value>\n" +
        "        </setting>\n" +
        "      </section>\n" +
        "    </graph>\n" +
        "    <graph name=\"History\">\n" +
        "      <section name=\"Download\">\n" +
        "        <setting name=\"Line\">\n" +
        "          <value>" + ColorToStr(Colors.HistoryDownLine) + "</value>\n" +
        "        </setting>\n" +
        "        <setting name=\"Start\">\n" +
        "          <value>" + ColorToStr(Colors.HistoryDownA) + "</value>\n" +
        "        </setting>\n" +
        "        <setting name=\"Mid\">\n" +
        "          <value>" + ColorToStr(Colors.HistoryDownB) + "</value>\n" +
        "        </setting>\n" +
        "        <setting name=\"End\">\n" +
        "          <value>" + ColorToStr(Colors.HistoryDownC) + "</value>\n" +
        "        </setting>\n" +
        "        <setting name=\"Maximum\">\n" +
        "          <value>" + ColorToStr(Colors.HistoryDownMax) + "</value>\n" +
        "        </setting>\n" +
        "      </section>\n" +
        "      <setting name=\"Text\">\n" +
        "        <value>" + ColorToStr(Colors.HistoryText) + "</value>\n" +
        "      </setting>\n" +
        "      <setting name=\"Background\">\n" +
        "        <value>" + ColorToStr(Colors.HistoryBackground) + "</value>\n" +
        "      </setting>\n" +
        "      <section name=\"Grid\">\n" +
        "        <setting name=\"Light\">\n" +
        "          <value>" + ColorToStr(Colors.HistoryLightGrid) + "</value>\n" +
        "        </setting>\n" +
        "        <setting name=\"Dark\">\n" +
        "          <value>" + ColorToStr(Colors.HistoryDarkGrid) + "</value>\n" +
        "        </setting>\n" +
        "      </section>\n" +
        "    </graph>\n" +
        "  </colorSettings>\n" +
        "</configuration>";
      string saveRet = SettingsFunctions.SafeSave(ConfigFile, ConfigFileBackup, sRet);
      if (saveRet == "SAVED")
        return true;
      SettingsFunctions.SaveErrDlg(saveRet);
      return false;
    }
    private static string ColorToStr(Color c)
    {
      string sA = null;
      if (c == Color.Transparent)
        sA = "00";
      else
      {
        if (c.A > 0)
        {
          sA = c.A.ToString("X");
          if (sA.Length == 0)
            sA = "FF";
          else if (sA.Length == 1)
            sA = 0 + "sA";
        }
        else
          sA = "FF";
      }
      if (c.A > 0)
      {
        sA = c.A.ToString("X");
        if (sA.Length == 0)
        {
          sA = "00";
        }
        else if (sA.Length == 1)
        {
          sA = "0" + sA;
        }
      }
      else
      {
        sA = "00";
      }
      string sR = null;
      if (c.R > 0)
      {
        sR = c.R.ToString("X");
        if (sR.Length == 0)
        {
          sR = "00";
        }
        else if (sR.Length == 1)
        {
          sR = "0" + sR;
        }
      }
      else
      {
        sR = "00";
      }
      string sG = null;
      if (c.G > 0)
      {
        sG = c.G.ToString("X");
        if (sG.Length == 0)
        {
          sG = "00";
        }
        else if (sG.Length == 1)
        {
          sG = "0" + sG;
        }
      }
      else
      {
        sG = "00";
      }
      string sB = null;
      if (c.B > 0)
      {
        sB = c.B.ToString("X");
        if (sB.Length == 0)
        {
          sB = "00";
        }
        else if (sB.Length == 1)
        {
          sB = "0" + sB;
        }
      }
      else
      {
        sB = "00";
      }
      return sA + sR + sG + sB;
    }
    private static Color StrToColor(string s)
    {
      int iColor = 0;
      if (int.TryParse(s, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out iColor))
      {
        if (!((iColor & 0xFF000000) == 0xFF000000))
          return Color.Transparent;
        return Color.FromArgb(iColor);
      }
      else
      {
        return Color.Transparent;
      }
    }
    public string Account
    {
      get
      {
        return m_Account;
      }
      set
      {
        m_Account = value;
      }
    }
    public string PassCrypt
    {
      get
      {
        return m_PassCrypt;
      }
      set
      {
        m_PassCrypt = value;
      }
    }
    public string PassKey
    {
      get
      {
        return m_PassKey;
      }
      set
      {
        m_PassKey = value;
      }
    }
    public string PassSalt
    {
      get
      {
        return m_PassSalt;
      }
      set
      {
        m_PassSalt = value;
      }
    }
    public int StartWait
    {
      get
      {
        return m_StartWait;
      }
      set
      {
        m_StartWait = value;
      }
    }
    public int Interval
    {
      get
      {
        return m_Interval;
      }
      set
      {
        m_Interval = value;
      }
    }
    public string Gr
    {
      get
      {
        return m_Gr;
      }
      set
      {
        m_Gr = value;
      }
    }
    public DateTime LastUpdate
    {
      get
      {
        return m_LastUpdate;
      }
      set
      {
        m_LastUpdate = value;
      }
    }
    public DateTime LastSyncTime
    {
      get
      {
        return m_LastSyncTime;
      }
      set
      {
        m_LastSyncTime = value;
      }
    }
    public int Accuracy
    {
      get
      {
        return m_Accuracy;
      }
      set
      {
        m_Accuracy = value;
      }
    }
    public uint Ago
    {
      get
      {
        return m_Ago;
      }
      set
      {
        m_Ago = value;
      }
    }
    public string HistoryDir
    {
      get
      {
        return m_HistoryDir;
      }
      set
      {
        m_HistoryDir = value;
      }
    }
    public bool UpdateBETA
    {
      get
      {
        return m_UpdateBETA;
      }
      set
      {
        m_UpdateBETA = value;
      }
    }
    public UpdateTypes UpdateType
    {
      get
      {
        return m_UpdateType;
      }
      set
      {
        m_UpdateType = value;
      }
    }
    public byte UpdateTime
    {
      get
      {
        return m_UpdateTime;
      }
      set
      {
        m_UpdateTime = value;
      }
    }
    public bool ScaleScreen
    {
      get
      {
        return m_ScaleScreen;
      }
      set
      {
        m_ScaleScreen = value;
      }
    }
    public Gdk.Size MainSize
    {
      get
      {
        return m_MainSize;
      }
      set
      {
        m_MainSize = value;
      }
    }
    public string RemoteKey
    {
      get
      {
        return m_RemoteKey;
      }
      set
      {
        m_RemoteKey = value;
      }
    }
    public int Timeout
    {
      get
      {
        return m_Timeout;
      }
      set
      {
        m_Timeout = value;
      }
    }
    public int Retries
    {
      get
      {
        return m_Retries;
      }
      set
      {
        m_Retries = value;
      }
    }
    public int Overuse
    {
      get
      {
        return m_Overuse;
      }
      set
      {
        m_Overuse = value;
      }
    }
    public int Overtime
    {
      get
      {
        return m_Overtime;
      }
      set
      {
        m_Overtime = value;
      }
    }
    public string AlertStyle
    {
      get
      {
        return m_AlertStyle;
      }
      set
      {
        m_AlertStyle = value;
      }
    }
    public TrayStyles TrayIconStyle
    {
      get
      {
        return m_TrayIcon;
      }
      set
      {
        m_TrayIcon = value;
      }
    }
    public bool TrayIconOnClose
    {
      get
      {
        return m_TrayClose;
      }
      set
      {
        m_TrayClose = value;
      }
    }
    public bool AutoHide
    {
      get
      {
        return m_AutoHide;
      }
      set
      {
        m_AutoHide = value;
      }
    }
    public bool TLSProxy
    {
      get
      {
        return m_TLSProxy;
      }
      set
      {
        m_TLSProxy = value;
      }
    }
    public IWebProxy Proxy
    {
      get
      {
        if (m_ProxySetting.Contains("http://"))
        {
          m_ProxySetting = m_ProxySetting.Replace("http://", "");
        }
        if (m_ProxySetting.Contains(":"))
        {
          string[] myProxySettings = m_ProxySetting.Split(':');
          string pType = myProxySettings[0];
          if (string.Compare(pType, "ip", StringComparison.OrdinalIgnoreCase) == 0)
          {
            string pIP = myProxySettings[1];
            int pPort = 80;
            if (myProxySettings.Length > 2)
              pPort = int.Parse(myProxySettings[2].Replace("/", ""));
            if (myProxySettings.Length > 3)
            {
              string pUser = myProxySettings[3];
              string pPass = myProxySettings[4];
              if (myProxySettings.Length > 5)
              {
                string pDomain = myProxySettings[5];
                return new WebProxy(pIP, pPort) { Credentials = new NetworkCredential(pUser, pPass, pDomain) };
              }
              else
              {
                return new WebProxy(pIP, pPort) { Credentials = new NetworkCredential(pUser, pPass) };
              }
            }
            else
            {
              return new WebProxy(pIP, pPort);
            }
          }
          else if (string.Compare(pType, "url", StringComparison.OrdinalIgnoreCase) == 0)
          {
            string pURL = myProxySettings[1];
            if (myProxySettings.Length > 2)
            {
              if (myProxySettings.Length > 3)
              {
                string pUser = myProxySettings[2];
                string pPass = myProxySettings[3];
                if (myProxySettings.Length > 4)
                {
                  string pDomain = myProxySettings[4];
                  return new WebProxy(pURL, false, null, new NetworkCredential(pUser, pPass, pDomain));
                }
                else
                {
                  return new WebProxy(pURL, false, null, new NetworkCredential(pUser, pPass));
                }
              }
              else
              {
                int pPort = int.Parse(myProxySettings[2].Replace("/", ""));
                return new WebProxy(pURL, pPort);
              }
            }
            else
            {
              return new WebProxy(pURL);
            }
          }
          else
          {
            return null;
          }
        }
        else
        {
          if (string.Compare(m_ProxySetting, "none", StringComparison.OrdinalIgnoreCase) == 0)
          {
            return null;
          }
          else if (string.Compare(m_ProxySetting, "system", StringComparison.OrdinalIgnoreCase) == 0)
          {
            return WebRequest.DefaultWebProxy;
          }
          else
          {
            return null;
          }
        }
      }
      set
      {
        if (value == null)
        {
          m_ProxySetting = "None";
        }
        else if (value.Equals(WebRequest.DefaultWebProxy))
        {
          m_ProxySetting = "System";
        }
        else
        {
          WebProxy wValue = (WebProxy)value;
          if (modFunctions.IsNumeric(wValue.Address.Host.Replace(".", string.Empty)))
          {
            if (value.Credentials != null)
            {
              NetworkCredential mCreds = value.Credentials.GetCredential(null, string.Empty);
              if (string.IsNullOrEmpty(mCreds.Domain))
              {
                m_ProxySetting = "IP:" + wValue.Address.ToString() + ":" + ":" + mCreds.UserName + ":" + mCreds.Password;
              }
              else
              {
                m_ProxySetting = "IP:" + wValue.Address.ToString() + ":" + ":" + mCreds.UserName + ":" + mCreds.Password + ":" + mCreds.Domain;
              }
            }
            else
            {
              m_ProxySetting = "IP:" + wValue.Address.ToString();
            }
          }
          else
          {
            if (value.Credentials != null)
            {
              NetworkCredential mCreds = value.Credentials.GetCredential(null, string.Empty);
              if (String.IsNullOrEmpty(mCreds.Domain))
              {
                m_ProxySetting = "URL:" + wValue.Address.ToString() + ":" + ":" + mCreds.UserName + ":" + mCreds.Password;
              }
              else
              {
                m_ProxySetting = "URL:" + wValue.Address.ToString() + ":" + ":" + mCreds.UserName + ":" + mCreds.Password + ":" + mCreds.Domain;
              }
            }
            else
            {
              m_ProxySetting = "URL:" + wValue.Address.ToString();
            }
          }
        }
      }
    }
    public SecurityProtocolType SecurityProtocol
    {
      get
      {
        return m_SecurProtocol;
      }
      set
      {
        m_SecurProtocol = value;
      }
    }
    public bool SecurityEnforced
    {
      get
      {
        return m_SecurEnforced;
      }
      set
      {
        m_SecurEnforced = value;
      }
    }
    public string NetTestURL
    {
      get
      {
        return m_NetTest;
      }
      set
      {
        m_NetTest = value;
      }
    }
    public class AppColors
    {
      private Color c_MainDA;
      private Color c_MainDB;
      private Color c_MainDC;
      private Color c_MainText;
      private Color c_MainBG;
      private Color c_TrayDA;
      private Color c_TrayDB;
      private Color c_TrayDC;
      private Color c_HistoryDL;
      private Color c_HistoryDA;
      private Color c_HistoryDB;
      private Color c_HistoryDC;
      private Color c_HistoryDM;
      private Color c_HistoryText;
      private Color c_HistoryBG;
      private Color c_HistoryGridL;
      private Color c_HistoryGridD;
      public AppColors()
      {
      }
      public Color MainDownA
      {
        get
        {
          return c_MainDA;
        }
        set
        {
          c_MainDA = value;
        }
      }
      public Color MainDownB
      {
        get
        {
          return c_MainDB;
        }
        set
        {
          c_MainDB = value;
        }
      }
      public Color MainDownC
      {
        get
        {
          return c_MainDC;
        }
        set
        {
          c_MainDC = value;
        }
      }
      public Color MainText
      {
        get
        {
          return c_MainText;
        }
        set
        {
          c_MainText = value;
        }
      }
      public Color MainBackground
      {
        get
        {
          return c_MainBG;
        }
        set
        {
          c_MainBG = value;
        }
      }
      public Color TrayDownA
      {
        get
        {
          return c_TrayDA;
        }
        set
        {
          c_TrayDA = value;
        }
      }
      public Color TrayDownB
      {
        get
        {
          return c_TrayDB;
        }
        set
        {
          c_TrayDB = value;
        }
      }
      public Color TrayDownC
      {
        get
        {
          return c_TrayDC;
        }
        set
        {
          c_TrayDC = value;
        }
      }
      public Color HistoryDownLine
      {
        get
        {
          return c_HistoryDL;
        }
        set
        {
          c_HistoryDL = value;
        }
      }
      public Color HistoryDownA
      {
        get
        {
          return c_HistoryDA;
        }
        set
        {
          c_HistoryDA = value;
        }
      }
      public Color HistoryDownB
      {
        get
        {
          return c_HistoryDB;
        }
        set
        {
          c_HistoryDB = value;
        }
      }
      public Color HistoryDownC
      {
        get
        {
          return c_HistoryDC;
        }
        set
        {
          c_HistoryDC = value;
        }
      }
      public Color HistoryDownMax
      {
        get
        {
          return c_HistoryDM;
        }
        set
        {
          c_HistoryDM = value;
        }
      }
      public Color HistoryText
      {
        get
        {
          return c_HistoryText;
        }
        set
        {
          c_HistoryText = value;
        }
      }
      public Color HistoryBackground
      {
        get
        {
          return c_HistoryBG;
        }
        set
        {
          c_HistoryBG = value;
        }
      }
      public Color HistoryLightGrid
      {
        get
        {
          return c_HistoryGridL;
        }
        set
        {
          c_HistoryGridL = value;
        }
      }
      public Color HistoryDarkGrid
      {
        get
        {
          return c_HistoryGridD;
        }
        set
        {
          c_HistoryGridD = value;
        }
      }
    }
  }
  static class SettingsFunctions
  {
    public static string SafeSave(string sPath, string sBackup, string sConfig)
    {
      if (!File.Exists(sPath))
      {
        try
        {
          File.WriteAllText(sPath, sConfig, System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
          return "PERMISSION: was unable to write to settings file \"" + sPath + "\". " + ex.Message;
        }
        string firstFileStr = File.ReadAllText(sPath, System.Text.Encoding.UTF8);
        if (firstFileStr == sConfig)
          return "SAVED";
        return "WRITE: could not verify the settings file \"" + sPath + "\".";
      }
      string sNew = sPath + ".out";
      string sOld = sBackup;
      try
      {
        if (File.Exists(sOld))
          File.Delete(sOld);
      }
      catch (Exception ex)
      {
        return "WRITE: failed to erase backup file \"" + sOld + "\". " + ex.Message;
      }
      try
      {
        if (File.Exists(sNew))
          File.Delete(sNew);
      }
      catch (Exception ex)
      {
        return "WRITE: failed to erase temp file \"" + sNew + "\". " + ex.Message;
      }
      try
      {
        File.WriteAllText(sNew, sConfig, System.Text.Encoding.UTF8);
      }
      catch (Exception ex)
      {
        return "PERMISSION: was unable to write to settings file \"" + sNew + "\". " + ex.Message;
      }
      try
      {
        File.Replace(sNew, sPath, sOld, true);
      }
      catch (Exception ex)
      {
        return "PERMISSION: was unable to move new settings file \"" + sNew + "\" to settings location \"" + sPath + "\". " + ex.Message;
      }
      string fileStr = File.ReadAllText(sPath, System.Text.Encoding.UTF8);
      if (fileStr != sConfig)
      {
        File.Delete(sPath);
        File.Move(sOld, sPath);
        return "WRITE: could not verify the settings file \"" + sPath + "\".";
      }
      try
      {
        File.Delete(sOld);
      }
      catch
      {
        //I really don't care
      }
      return "SAVED";
    }
    public static void SaveErrDlg(string saveRet)
    {
      if (!saveRet.Contains(": "))
        return;
      string sErrType = saveRet.Substring(0, saveRet.IndexOf(": "));
      string sErrMsg = saveRet.Substring(saveRet.IndexOf(": ") + 2);
      string sRealErr = null;
      if (sErrMsg.Contains(". "))
      {
        sRealErr = sErrMsg.Substring(sErrMsg.IndexOf(". ") + 2);
        sErrMsg = sErrMsg.Substring(0, sErrMsg.IndexOf(". ") + 1);
      }
      sErrMsg = modFunctions.ProductName + " " + sErrMsg;
      string sCaption = "Your settings were not saved.";
      string sHeader = "Program Settings Error";
      switch (sErrType)
      {
        case "WRITE":
          sCaption = "Your Program settings were not saved.";
          break;
        case "PERMISSION":
          sCaption = "Your Program settings could not be saved.";
          break;
        default:
          sCaption = "There was an error saving your Program settings.";
          break;
      }
      if (String.IsNullOrEmpty(sRealErr))
        modFunctions.ShowMessageBox(null, sCaption + Environment.NewLine + sErrMsg, sHeader, Gtk.DialogFlags.Modal, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok);
      else
        modFunctions.ShowMessageBox(null, sCaption + Environment.NewLine + sErrMsg + Environment.NewLine + Environment.NewLine + sRealErr, sHeader, Gtk.DialogFlags.Modal, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok);
    }
  }
}
