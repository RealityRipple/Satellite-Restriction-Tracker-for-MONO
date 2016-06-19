using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
public class NotifierStyle
{
  public Image Background;
  public Image CloseButton;
  public Color TransparencyKey;
  public Rectangle TitleLocation;
  public Rectangle ContentLocation;
  public Point CloseLocation;
  public Color TitleColor;
  public Color ContentColor;
  public Color ContentHoverColor;
  public NotifierStyle()
  {
    Background = new Bitmap(GetType(), "RestrictionTrackerGTK.Resources.default_alert.png");
    CloseButton = new Bitmap(GetType(), "RestrictionTrackerGTK.Resources.default_close.png");
    TransparencyKey = Color.Fuchsia;
    TitleLocation = new Rectangle(7, 3, 188, 24);
    ContentLocation = new Rectangle(9, 31, 227, 66);
    CloseLocation = new Point(190, 0);
    TitleColor = Color.White;
    ContentColor = Color.Black;
    ContentHoverColor = Color.Blue;
  }
  public NotifierStyle(string BGPath, string ClosePath, string LocPath)
  {
    using (Image iBG = Image.FromFile(BGPath))
    {
      using (Bitmap bg = new Bitmap(iBG.Width, iBG.Height))
      {
        using (Graphics g = Graphics.FromImage(bg))
        {
          g.DrawImage(iBG, 0, 0);
        }
        Background = (Image) bg.Clone();
      }
    }

    using (Image iClose = Image.FromFile(ClosePath))
    {
      using (Bitmap close = new Bitmap(iClose.Width, iClose.Height))
      {
        using (Graphics g = Graphics.FromImage(close))
        {
          g.DrawImage(iClose, 0, 0);
        }
        CloseButton = (Image) close.Clone();
      }
    }

    string locData = "";
    using (FileStream fs = new FileStream(LocPath, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
      byte[] inData = new byte[fs.Length];
      fs.Read(inData, 0, (int) fs.Length);
      locData = System.Text.Encoding.GetEncoding("latin1").GetString(inData);
    }
    locData = locData.Replace("\n\r", "\n").Replace("\r", "\n");
    while (locData.Contains("\n\n"))
    {
      locData = locData.Replace("\n\n", "\n");
    }
    string[] locLines = locData.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
    TransparencyKey = ColorIDToColor(locLines[0]);
    TitleLocation = RectIDToRect(locLines[1]);
    TitleColor = ColorIDToColor(locLines[2]);
    ContentLocation = RectIDToRect(locLines[3]);
    ContentColor = ColorIDToColor(locLines[4]);
    ContentHoverColor = ColorIDToColor(locLines[5]);
    CloseLocation = PointIDToPoint(locLines[6]);
  }
  private Color ColorIDToColor(string ID)
  {
    try
    {
      if (ID.StartsWith("#"))
      {
        ID = ID.Substring(1);
      }
      int iR = int.Parse(ID.Substring(0, 2), NumberStyles.HexNumber);
      int iG = int.Parse(ID.Substring(2, 2), NumberStyles.HexNumber);
      int iB = int.Parse(ID.Substring(4, 2), NumberStyles.HexNumber);
      return Color.FromArgb(iR, iG, iB);
    }
    catch (Exception)
    {
      return Color.Transparent;
    }
  }
  private Rectangle RectIDToRect(string ID)
  {
    try
    {
      ID = ID.Replace(" ", "");
      string[] IDs = ID.Split(',');
      int x = int.Parse(IDs[0]);
      int y = int.Parse(IDs[1]);
      int w = int.Parse(IDs[2]);
      int h = int.Parse(IDs[3]);
      return (new Rectangle(x, y, w, h));
    }
    catch (Exception)
    {
      return Rectangle.Empty;
    }
  }
  private Point PointIDToPoint(string ID)
  {
    try
    {
      ID = ID.Replace(" ", "");
      string[] IDs = ID.Split(',');
      int x = int.Parse(IDs[0]);
      int y = int.Parse(IDs[1]);
      return new Point(x, y);
    }
    catch (Exception)
    {
      return Point.Empty;
    }
  }
}
public class TarFileData
{
  public string FileName;
  public uint FileMode;
  public uint OwnerID;
  public uint GroupID;
  public ulong FileSize;
  public ulong LastMod;
  public uint Checksum;
  public byte LinkIndicator;
  public string LinkedFile;
  public byte[] FileData;
  public TarFileData(BinaryReader bIn)
  {
    long startAt = bIn.BaseStream.Position;
    FileName = ReadBString(bIn.ReadBytes(100));
    FileMode = ReadBInt(bIn.ReadBytes(8));
    OwnerID = ReadBInt(bIn.ReadBytes(8));
    GroupID = ReadBInt(bIn.ReadBytes(8));
    FileSize = ReadBOct(bIn.ReadBytes(12));
    LastMod = ReadBOct(bIn.ReadBytes(12));
    Checksum = ReadBInt(bIn.ReadBytes(8));
    LinkIndicator = ReadBByte(bIn.ReadBytes(1));
    LinkedFile = ReadBString(bIn.ReadBytes(100));
    bIn.BaseStream.Seek(startAt, SeekOrigin.Begin);
    bIn.BaseStream.Seek(512, SeekOrigin.Current);
    if (FileSize > 0)
    {
      FileData = bIn.ReadBytes((int) FileSize);
      int Leftovers = 0;
      Math.DivRem((int) FileSize, 512, out Leftovers);
      if (Leftovers > 0)
      {
        bIn.BaseStream.Seek(512 - Leftovers, SeekOrigin.Current);
      }
    }
  }
  private string ReadBString(byte[] inBytes)
  {
    string sRet = Encoding.ASCII.GetString(inBytes);
    if (sRet.Contains("\0"))
    {
      sRet = sRet.Replace('\0', ' ');
    }
    sRet = sRet.Trim();
    return sRet;
  }
  private byte ReadBByte(byte[] inBytes)
  {
    string sRet = ReadBString(inBytes);
    if (!string.IsNullOrEmpty(sRet))
    {
      return (Byte.Parse(sRet));
    }
    else
    {
      return 0;
    }
  }
  private uint ReadBInt(byte[] inBytes)
  {
    string sRet = ReadBString(inBytes);
    if (!string.IsNullOrEmpty(sRet))
    {
      return (UInt32.Parse(sRet));
    }
    else
    {
      return 0u;
    }
  }
  private ulong ReadBOct(byte[] inBytes)
  {
    string sRet = ReadBString(inBytes);
    if (!string.IsNullOrEmpty(sRet))
    {
      return (Convert.ToUInt64(sRet, 8));
    }
    else
    {
      return 0ul;
    }
  }
}
