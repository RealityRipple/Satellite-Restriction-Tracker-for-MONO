using System;
using System.Drawing;
using System.Xml;
using RestrictionLibrary;
namespace RestrictionTrackerGTK
{
  class AppSettings
  {
    private string m_Account;
    private localRestrictionTracker.SatHostTypes m_AccountType;
    private int m_Interval;
    private string m_Gr;
    private System.DateTime m_LastSyncTime;
    private int m_Accuracy;
    private uint m_Ago;
    private string m_HistoryDir;
    private bool m_HistoryInvert;
    private bool m_BetaCheck;
    private bool m_ScaleScreen;
    private Gdk.Size m_MainSize;
    private string m_RemoteKey;
    private string m_PassCrypt;
    private int m_Timeout;
    private int m_Overuse;
    private int m_Overtime;
    private string m_AlertStyle;
    private string m_ProxySetting;
    public bool Loaded;
    public AppColors Colors;

    private string ConfigFile
    {
      get
      {
        return modFunctions.AppData + System.IO.Path.DirectorySeparatorChar.ToString() + "user.config";
      }
    }

    private string ConfigFileBackup
    {
      get
      {
        return modFunctions.AppData + System.IO.Path.DirectorySeparatorChar.ToString() + "backup.config";
      }
    }

    public AppSettings()
    {
      Loaded = false;
      BackupCheckup();
      if (System.IO.File.Exists(ConfigFile))
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
                        m_AccountType = modFunctions.StringToHostType(m_node.Attributes[1].InnerText);
                      }
                      else
                      {
                        m_AccountType = localRestrictionTracker.SatHostTypes.Other;
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
                    else if (xName.CompareTo("LastSyncTime") == 0)
                    {
                      m_LastSyncTime = System.DateTime.FromBinary(long.Parse(xValue));
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
                    else if (xName.CompareTo("HistoryInversion") == 0)
                    {
                      m_HistoryInvert = (xValue.CompareTo("True") == 0);
                    }
                    else if (xName.CompareTo("BetaCheck") == 0)
                    {
                      m_BetaCheck = (xValue.CompareTo("True") == 0);
                    }
                    else if (xName.CompareTo("ScaleScreen") == 0)
                    {
                      m_ScaleScreen = (xValue.CompareTo("True") == 0);
                    }
                    else if (xName.CompareTo("MainSize") == 0)
                    {
                      char[] comma = {','};
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
                    else if (xName.CompareTo("Proxy") == 0)
                    {
                      m_ProxySetting = xValue;
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
      m_Interval = 15;
      m_Gr = "aph";
      m_LastSyncTime = new System.DateTime(2000, 1, 1);
      m_Accuracy = 0;
      m_Ago = 30u;
      m_HistoryDir = null;
      m_HistoryInvert = false;
      m_BetaCheck = true;
      m_ScaleScreen = false;
      m_MainSize = new Gdk.Size(450, 200);
      m_RemoteKey = null;
      m_PassCrypt = null;
      m_Timeout = 60;
      m_Overuse = 0;
      m_Overtime = 60;
      m_AlertStyle = "Default";
      m_ProxySetting = "System";
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
      if (m_BetaCheck)
      {
        sBeta = "True";
      }
      string sScaleScreen = "False";
      if (m_ScaleScreen)
      {
        sScaleScreen = "True";
      }
      string sAccountType = "Other";
      string sInversion = "False";
      if (m_HistoryInvert)
      {
        sInversion = "True";
      }
      sAccountType = modFunctions.HostTypeToString(m_AccountType);
      string sRet = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
        "<configuration>" + Environment.NewLine +
        "  <userSettings>" + Environment.NewLine +
        "    <RestrictionTracker.My.MySettings>" + Environment.NewLine +
        "      <setting name=\"Account\" type=\"" + sAccountType + "\">" + Environment.NewLine +
        "        <value>" + m_Account + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"PassCrypt\">" + Environment.NewLine +
        "        <value>" + m_PassCrypt + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"Interval\">" + Environment.NewLine + 
        "        <value>" + m_Interval.ToString() + "</value>" + Environment.NewLine +
        "      </setting>" + Environment.NewLine +
        "      <setting name=\"Gr\">" + Environment.NewLine +
        "        <value>" + m_Gr + "</value>" + Environment.NewLine +
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
        "      <setting name=\"HistoryInversion\">" + Environment.NewLine +
        "        <value>" + sInversion + "</value>" + Environment.NewLine + 
        "      </setting>" + Environment.NewLine + 
        "      <setting name=\"BetaCheck\">" + Environment.NewLine + 
        "        <value>" + sBeta + "</value>" + Environment.NewLine +
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
        "      <setting name=\"Proxy\">" + Environment.NewLine + 
        "        <value>" + m_ProxySetting + "</value>" + Environment.NewLine + 
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
      using (System.IO.FileStream ioWriter = new System.IO.FileStream(ConfigFile, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
      {
        using (System.IO.StreamWriter nOut = new System.IO.StreamWriter(ioWriter))
        {
          nOut.Write(sRet);
        }
      }
    }

    private string ColorToStr(Color c)
    {
      string sA = null;
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
      if (int.TryParse(s, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out iColor))
      {
        return Color.FromArgb(iColor);
      }
      else
      {
        return Color.Transparent;
      }
    }

    public void MakeBackup()
    {
      if (System.IO.File.Exists(ConfigFile))
      {
        System.IO.File.Copy(ConfigFile, ConfigFileBackup, true);
      }
    }

    public void BackupCheckup()
    {
      if (System.IO.File.Exists(ConfigFile))
      {
        if (System.IO.File.Exists(ConfigFileBackup))
        {
          try
          {
            XmlDocument m_xmld = new XmlDocument();
            m_xmld.Load(ConfigFile);
          }
          catch (Exception)
          {
            System.IO.File.Copy(ConfigFileBackup, ConfigFile, true);
          }
          finally
          {
            System.IO.File.Delete(ConfigFileBackup);
          }
        }
      }
      else
      {
        if (System.IO.File.Exists(ConfigFileBackup))
        {
          System.IO.File.Copy(ConfigFileBackup, ConfigFile, true);
          System.IO.File.Delete(ConfigFileBackup);
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

    public System.DateTime LastSyncTime
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

    public bool HistoryInversion
    {
      get
      {
        return m_HistoryInvert;
      }
      set
      {
        m_HistoryInvert = value;
      }
    }

    public bool BetaCheck
    {
      get
      {
        return m_BetaCheck;
      }
      set
      {
        m_BetaCheck = value;
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

    public System.Net.IWebProxy Proxy
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
            int pPort = int.Parse(myProxySettings[2].Replace("/", ""));
            if (myProxySettings.Length > 3)
            {
              string pUser = myProxySettings[3];
              string pPass = myProxySettings[4];
              if (myProxySettings.Length > 5)
              {
                string pDomain = myProxySettings[5];
                return new System.Net.WebProxy(pIP, pPort) { Credentials = new System.Net.NetworkCredential(pUser, pPass, pDomain) };
              }
              else
              {
                return new System.Net.WebProxy(pIP, pPort) { Credentials = new System.Net.NetworkCredential(pUser, pPass) };
              }
            }
            else
            {
              return new System.Net.WebProxy(pIP, pPort);
            }
          }
          else if (String.Compare(pType.ToLower(), "url") == 0)
          {
            string pURL = myProxySettings[1];
            if (myProxySettings.Length > 2)
            {
              string pUser = myProxySettings[2];
              string pPass = myProxySettings[3];
              if (myProxySettings.Length > 4)
              {
                string pDomain = myProxySettings[4];
                return new System.Net.WebProxy(pURL, false, null, new System.Net.NetworkCredential(pUser, pPass, pDomain));
              }
              else
              {
                return new System.Net.WebProxy(pURL, false, null, new System.Net.NetworkCredential(pUser, pPass));
              }
            }
            else
            {
              return new System.Net.WebProxy(pURL);
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
            return System.Net.WebRequest.DefaultWebProxy;
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
        else if (value.Equals(System.Net.WebRequest.DefaultWebProxy))
        {
          m_ProxySetting = "System";
        }
        else
        {
          System.Net.WebProxy wValue = (System.Net.WebProxy)value;
          if (modFunctions.IsNumeric(wValue.Address.Host.Replace(".", string.Empty)))
          {
            if (value.Credentials != null)
            {
              System.Net.NetworkCredential mCreds = value.Credentials.GetCredential(null, string.Empty);
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
              System.Net.NetworkCredential mCreds = value.Credentials.GetCredential(null, string.Empty);
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

