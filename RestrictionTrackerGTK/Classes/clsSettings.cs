using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using RestrictionLibrary;
namespace RestrictionTrackerGTK
{
  class AppSettings
  {
    private string m_Account;
    private localRestrictionTracker.SatHostTypes m_AccountType;
    private bool m_AccountTypeF;
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
    private int m_Timeout;
    private int m_Overuse;
    private int m_Overtime;
    private string m_AlertStyle;
    private TrayStyles m_TrayIcon;
    private bool m_AutoHide;
    private string m_ProxySetting;
    private SecurityProtocolType m_Protocol;
    public bool Loaded;
    public AppColors Colors;
    public enum UpdateTypes{Auto = 1,Ask,None};
    public enum TrayStyles{Always, Minimized, Never};
    private string ConfigFile
    {
      get
      {
        return modFunctions.AppData + "user.config";
      }
    }

    private string ConfigFileBackup
    {
      get
      {
        return modFunctions.AppData + "backup.config";
      }
    }

    public AppSettings()
    {
      Loaded = false;
      BackupCheckup();
      if (File.Exists(ConfigFile))
      {
        XmlDocument m_xmld = new XmlDocument();
        m_xmld.Load(ConfigFile);
        if (m_xmld.HasChildNodes)
        {
          if (m_xmld.ChildNodes.Count > 1)
          {
            XmlNode xConfig = m_xmld.ChildNodes[1];
            if (xConfig.HasChildNodes)
            {
              XmlNode xUserSettings = xConfig.ChildNodes[0];
              if (xUserSettings.HasChildNodes)
              {
                XmlNode xMySettings = xUserSettings.ChildNodes[0];
                if (xMySettings.HasChildNodes)
                {
                  XmlNodeList xNodeList = xMySettings.ChildNodes;
                  Reset();
                  foreach (XmlNode m_node in xNodeList)
                  {
                    string xName = m_node.Attributes[0].InnerText;
                    string xValue = m_node.FirstChild.InnerText;
                    if (xName.CompareTo("Account") == 0)
                    {
                      m_Account = xValue;
                      if (m_node.Attributes.Count > 1)
                      {
                        m_AccountType = localRestrictionTracker.SatHostTypes.Other;
                        m_AccountTypeF = false;
                        foreach (XmlAttribute m_attrib in m_node.Attributes)
                        {
                          if (m_attrib.Name.CompareTo("type") == 0)
                            m_AccountType = modFunctions.StringToHostType(m_attrib.InnerText);
                          else if (m_attrib.Name.CompareTo("forceType") == 0)
                            m_AccountTypeF = (m_attrib.InnerText == "True");
                        }
                      }
                      else
                      {
                        m_AccountType = localRestrictionTracker.SatHostTypes.Other;
                        m_AccountTypeF = false;
                      }
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
                    }
                    else if (xName.CompareTo("Timeout") == 0)
                    {
                      if (!int.TryParse(xValue, out m_Timeout))
                      {
                        m_Timeout = 60;
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
                    else if (xName.CompareTo("AutoHide") == 0)
                    {
                      m_AutoHide = (xValue.CompareTo("True") == 0);
                    }
                    else if (xName.CompareTo("Proxy") == 0)
                    {
                      m_ProxySetting = xValue;
                    }
                    else if (xName.CompareTo("Protocol") == 0)
                    {
                      if (xValue == "TLS")
                      {
                        m_Protocol = SecurityProtocolType.Tls;
                      }
                      else
                      {
                        m_Protocol = SecurityProtocolType.Ssl3;
                      }
                    }
                  }
                  Loaded = true;
                }
                else
                {
                  Reset();
                  return;
                }
              }
              else
              {
                Reset();
                return;
              }

              Colors = new AppColors();
              if (xConfig.ChildNodes.Count > 1)
              {
                XmlNode xColorSettings = xConfig.ChildNodes[1];
                if (xColorSettings.HasChildNodes)
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
                              if (xName.CompareTo("Start") == 0)
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
                        else if (nodeName.CompareTo("Upload") == 0)
                        {
                          //Upload Section
                          if (m_node.HasChildNodes)
                          {
                            foreach (XmlNode xSetting in m_node.ChildNodes)
                            {
                              string xName = xSetting.Attributes[0].InnerText;
                              string xValue = xSetting.FirstChild.InnerText;
                              if (xName.CompareTo("Start") == 0)
                              {
                                if (graphName.CompareTo("Main") == 0)
                                {
                                  Colors.MainUpA = StrToColor(xValue);
                                }
                                else if (graphName.CompareTo("Tray") == 0)
                                {
                                  Colors.TrayUpA = StrToColor(xValue);
                                }
                                else if (graphName.CompareTo("History") == 0)
                                {
                                  Colors.HistoryUpA = StrToColor(xValue);
                                }
                              }
                              else if (xName.CompareTo("Mid") == 0)
                              {
                                if (graphName.CompareTo("Main") == 0)
                                {
                                  Colors.MainUpB = StrToColor(xValue);
                                }
                                else if (graphName.CompareTo("Tray") == 0)
                                {
                                  Colors.TrayUpB = StrToColor(xValue);
                                }
                                else if (graphName.CompareTo("History") == 0)
                                {
                                  Colors.HistoryUpB = StrToColor(xValue);
                                }
                              }
                              else if (xName.CompareTo("End") == 0)
                              {
                                if (graphName.CompareTo("Main") == 0)
                                {
                                  Colors.MainUpC = StrToColor(xValue);
                                }
                                else if (graphName.CompareTo("Tray") == 0)
                                {
                                  Colors.TrayUpC = StrToColor(xValue);
                                }
                                else if (graphName.CompareTo("History") == 0)
                                {
                                  Colors.HistoryUpC = StrToColor(xValue);
                                }
                              }
                              else if (xName.CompareTo("Maximum") == 0)
                              {
                                if (graphName.CompareTo("History") == 0)
                                {
                                  Colors.HistoryUpMax = StrToColor(xValue);
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
                      }
                    }
                  }
                }
                else
                {
                  ResetColors();
                  return;
                }
              }
              else
              {
                ResetColors();
                return;
              }
            }
            else
            {
              Reset();
              return;
            }
          }
          else
          {
            Reset();
            return;
          }
        }
        else
        {
          Reset();
          return;
        }
      }
      else
      {
        Reset();
      }
    }

    private void Reset()
    {
      m_Account = null;
      m_AccountType = localRestrictionTracker.SatHostTypes.Other;
      m_AccountTypeF = false;
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
      m_Timeout = 60;
      m_Overuse = 0;
      m_Overtime = 60;
      m_AlertStyle = "Default";
      m_TrayIcon = TrayStyles.Always;
      m_AutoHide = true;
      m_ProxySetting = "System";
      m_Protocol = SecurityProtocolType.Tls;
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
      Colors.MainUpA = Color.Transparent;
      Colors.MainUpB = Color.Transparent;
      Colors.MainUpC = Color.Transparent;
      Colors.MainText = Color.Transparent;
      Colors.MainBackground = Color.Transparent;
    }

    private void ResetTray()
    {
      Colors.TrayDownA = Color.Transparent;
      Colors.TrayDownB = Color.Transparent;
      Colors.TrayDownC = Color.Transparent;
      Colors.TrayUpA = Color.Transparent;
      Colors.TrayUpB = Color.Transparent;
      Colors.TrayUpC = Color.Transparent;
    }

    private void ResetHistory()
    {
      Colors.HistoryDownA = Color.Transparent;
      Colors.HistoryDownB = Color.Transparent;
      Colors.HistoryDownC = Color.Transparent;
      Colors.HistoryDownMax = Color.Transparent;
      Colors.HistoryUpA = Color.Transparent;
      Colors.HistoryUpB = Color.Transparent;
      Colors.HistoryUpC = Color.Transparent;
      Colors.HistoryUpMax = Color.Transparent;
      Colors.HistoryText = Color.Transparent;
      Colors.HistoryBackground = Color.Transparent;
    }

    public void Save()
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
      string sAccountTypeF = "False";
      if (m_AccountTypeF)
      {
        sAccountTypeF = "True";
      }
      string sScaleScreen = "False";
      if (m_ScaleScreen)
      {
        sScaleScreen = "True";
      }
      string sProtocol = "TLS";
      if (m_Protocol == SecurityProtocolType.Ssl3)
      {
        sProtocol = "SSL";
      }
      string sAccountType = "Other";
      sAccountType = modFunctions.HostTypeToString(m_AccountType);
      string sTrayIcon = "Always";
      if (m_TrayIcon == TrayStyles.Never)
        sTrayIcon = "Never";
      else if (m_TrayIcon == TrayStyles.Minimized)
        sTrayIcon = "Minimized";
      string sAutoHide = "True";
      if (!m_AutoHide)
        sAutoHide = "False";
      string sRet = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
        "<configuration>" + Environment.NewLine +
        "  <userSettings>" + Environment.NewLine +
        "    <RestrictionTracker.My.MySettings>" + Environment.NewLine +
        "      <setting name=\"Account\" type=\"" + sAccountType + "\" forceType=\"" + sAccountTypeF + "\">" + Environment.NewLine +
        "        <value>" + m_Account + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"PassCrypt\">" + Environment.NewLine +
        "        <value>" + m_PassCrypt + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"StartWait\">" + Environment.NewLine + 
        "        <value>" + m_StartWait.ToString() + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"Interval\">" + Environment.NewLine + 
        "        <value>" + m_Interval.ToString() + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"Gr\">" + Environment.NewLine +
        "        <value>" + m_Gr + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"LastUpdate\">" + Environment.NewLine +
        "        <value>" + m_LastUpdate.ToBinary() + "</value>" + Environment.NewLine + 
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"LastSyncTime\">" + Environment.NewLine +
        "        <value>" + m_LastSyncTime.ToBinary() + "</value>" + Environment.NewLine + 
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"Accuracy\">" + Environment.NewLine +
        "        <value>" + m_Accuracy.ToString() + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"Ago\">" + Environment.NewLine +
        "        <value>" + m_Ago.ToString() + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"HistoryDir\">" + Environment.NewLine + 
        "        <value>" + m_HistoryDir + "</value>" + Environment.NewLine + 
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"UpdateBETA\">" + Environment.NewLine + 
        "        <value>" + sBeta + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"UpdateType\">" + Environment.NewLine +
        "        <value>" + sUpdateType + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"UpdateTime\">" + Environment.NewLine +
        "        <value>" + m_UpdateTime.ToString() + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"ScaleScreen\">" + Environment.NewLine +
        "        <value>" + sScaleScreen + "</value>" + Environment.NewLine + 
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"MainSize\">" + Environment.NewLine + 
        "        <value>" + m_MainSize.Width.ToString() + "," + m_MainSize.Height.ToString() + "</value>" + Environment.NewLine + 
        "      </setting>" + Environment.NewLine + 
        "      <setting name=\"RemoteKey\">" + Environment.NewLine +
        "        <value>" + m_RemoteKey + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine + 
        "      <setting name=\"Timeout\">" + Environment.NewLine +
        "        <value>" + m_Timeout.ToString() + "</value>" + Environment.NewLine + 
        "      </setting>" + Environment.NewLine + 
        "      <setting name=\"Overuse\">" + Environment.NewLine + 
        "        <value>" + m_Overuse.ToString() + "</value>" + Environment.NewLine + 
        "      </setting>" + Environment.NewLine + 
        "      <setting name=\"Overtime\">" + Environment.NewLine + 
        "        <value>" + m_Overtime.ToString() + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"AlertStyle\">" + Environment.NewLine + 
        "        <value>" + m_AlertStyle + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"TrayIcon\">" + Environment.NewLine +
        "        <value>" + sTrayIcon + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"AutoHide\">" + Environment.NewLine +
        "        <value>" + sAutoHide + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"Proxy\">" + Environment.NewLine + 
        "        <value>" + m_ProxySetting + "</value>" + Environment.NewLine + 
        "      </setting>" + Environment.NewLine + 
        "      <setting name=\"Protocol\">" + Environment.NewLine + 
        "        <value>" + sProtocol + "</value>" + Environment.NewLine + 
        "      </setting>" + Environment.NewLine + 
        "    </RestrictionTracker.My.MySettings>" + Environment.NewLine + 
        "  </userSettings>" + Environment.NewLine +
        "  <colorSettings>" + Environment.NewLine +
        "    <graph name=\"Main\">" + Environment.NewLine + 
        "      <section name=\"Download\">" + Environment.NewLine +
        "        <setting name=\"Start\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.MainDownA) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"Mid\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.MainDownB) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"End\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.MainDownC) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "      </section>" + Environment.NewLine +
        "      <section name=\"Upload\">" + Environment.NewLine +
        "        <setting name=\"Start\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.MainUpA) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"Mid\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.MainUpB) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"End\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.MainUpC) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "      </section>" + Environment.NewLine +
        "      <setting name=\"Text\">" + Environment.NewLine +
        "        <value>" + ColorToStr(Colors.MainText) + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine + 
        "      <setting name=\"Background\">" + Environment.NewLine +
        "        <value>" + ColorToStr(Colors.MainBackground) + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine + 
        "    </graph>" + Environment.NewLine +
        "    <graph name=\"Tray\">" + Environment.NewLine + 
        "      <section name=\"Download\">" + Environment.NewLine +
        "        <setting name=\"Start\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.TrayDownA) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"Mid\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.TrayDownB) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"End\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.TrayDownC) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "      </section>" + Environment.NewLine +
        "      <section name=\"Upload\">" + Environment.NewLine +
        "        <setting name=\"Start\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.TrayUpA) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"Mid\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.TrayUpB) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"End\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.TrayUpC) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "      </section>" + Environment.NewLine +
        "    </graph>" + Environment.NewLine +
        "    <graph name=\"History\">" + Environment.NewLine + 
        "      <section name=\"Download\">" + Environment.NewLine +
        "        <setting name=\"Start\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.HistoryDownA) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"Mid\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.HistoryDownB) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"End\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.HistoryDownC) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"Maximum\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.HistoryDownMax) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "      </section>" + Environment.NewLine +
        "      <section name=\"Upload\">" + Environment.NewLine +
        "        <setting name=\"Start\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.HistoryUpA) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"Mid\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.HistoryUpB) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"End\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.HistoryUpC) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "        <setting name=\"Maximum\">" + Environment.NewLine +
        "          <value>" + ColorToStr(Colors.HistoryUpMax) + "</value>" + Environment.NewLine +
        "        </setting>" + Environment.NewLine +
        "      </section>" + Environment.NewLine +
        "      <setting name=\"Text\">" + Environment.NewLine +
        "        <value>" + ColorToStr(Colors.HistoryText) + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine + 
        "      <setting name=\"Background\">" + Environment.NewLine +
        "        <value>" + ColorToStr(Colors.HistoryBackground) + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine + 
        "    </graph>" + Environment.NewLine +
        "  </colorSettings>" + Environment.NewLine +
        "</configuration>";
      MakeBackup();
      using (FileStream ioWriter = new FileStream(ConfigFile, FileMode.Create, FileAccess.Write, FileShare.None))
      {
        using (StreamWriter nOut = new StreamWriter(ioWriter))
        {
          nOut.Write(sRet);
        }
      }
    }

    private string ColorToStr(Color c)
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

    private Color StrToColor(string s)
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

    public void MakeBackup()
    {
      if (File.Exists(ConfigFile))
      {
        File.Copy(ConfigFile, ConfigFileBackup, true);
      }
    }

    public void BackupCheckup()
    {
      if (File.Exists(ConfigFile))
      {
        if (File.Exists(ConfigFileBackup))
        {
          try
          {
            XmlDocument m_xmld = new XmlDocument();
            m_xmld.Load(ConfigFile);
          }
          catch (Exception)
          {
            File.Copy(ConfigFileBackup, ConfigFile, true);
          }
          finally
          {
            File.Delete(ConfigFileBackup);
          }
        }
      }
      else
      {
        if (File.Exists(ConfigFileBackup))
        {
          File.Copy(ConfigFileBackup, ConfigFile, true);
          File.Delete(ConfigFileBackup);
        }
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

    public localRestrictionTracker.SatHostTypes AccountType
    {
      get
      {
        return m_AccountType;
      }
      set
      {
        m_AccountType = value;
      }
    }

    public bool AccountTypeForced
    {
      get
      {
        return m_AccountTypeF;
      }
      set
      {
        m_AccountTypeF = value;
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
          if (string.Compare(pType.ToLower(), "ip") == 0)
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
          else if (String.Compare(pType.ToLower(), "url") == 0)
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
          if (String.Compare(m_ProxySetting.ToLower(), "none") == 0)
          {
            return null;
          }
          else if (String.Compare(m_ProxySetting.ToLower(), "system") == 0)
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
              if (string.IsNullOrEmpty(mCreds.Domain))
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

    public SecurityProtocolType Protocol
    {
      get
      {
        return m_Protocol;
      }
      set
      {
        m_Protocol = value;
      }
    }

    public class AppColors
    {
      private Color c_MainDA;
      private Color c_MainDB;
      private Color c_MainDC;
      private Color c_MainUA;
      private Color c_MainUB;
      private Color c_MainUC;
      private Color c_MainText;
      private Color c_MainBG;
      private Color c_TrayDA;
      private Color c_TrayDB;
      private Color c_TrayDC;
      private Color c_TrayUA;
      private Color c_TrayUB;
      private Color c_TrayUC;
      private Color c_HistoryDA;
      private Color c_HistoryDB;
      private Color c_HistoryDC;
      private Color c_HistoryDM;
      private Color c_HistoryUA;
      private Color c_HistoryUB;
      private Color c_HistoryUC;
      private Color c_HistoryUM;
      private Color c_HistoryText;
      private Color c_HistoryBG;

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

      public Color MainUpA
      {
        get
        {
          return c_MainUA;
        }
        set
        {
          c_MainUA = value;
        }
      }

      public Color MainUpB
      {
        get
        {
          return c_MainUB;
        }
        set
        {
          c_MainUB = value;
        }
      }

      public Color MainUpC
      {
        get
        {
          return c_MainUC;
        }
        set
        {
          c_MainUC = value;
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

      public Color TrayUpA
      {
        get
        {
          return c_TrayUA;
        }
        set
        {
          c_TrayUA = value;
        }
      }

      public Color TrayUpB
      {
        get
        {
          return c_TrayUB;
        }
        set
        {
          c_TrayUB = value;
        }
      }

      public Color TrayUpC
      {
        get
        {
          return c_TrayUC;
        }
        set
        {
          c_TrayUC = value;
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

      public Color HistoryUpA
      {
        get
        {
          return c_HistoryUA;
        }
        set
        {
          c_HistoryUA = value;
        }
      }

      public Color HistoryUpB
      {
        get
        {
          return c_HistoryUB;
        }
        set
        {
          c_HistoryUB = value;
        }
      }

      public Color HistoryUpC
      {
        get
        {
          return c_HistoryUC;
        }
        set
        {
          c_HistoryUC = value;
        }
      }

      public Color HistoryUpMax
      {
        get
        {
          return c_HistoryUM;
        }
        set
        {
          c_HistoryUM = value;
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
    }


  }
}

