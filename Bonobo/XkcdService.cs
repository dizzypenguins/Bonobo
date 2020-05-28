using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace Bonobo
{
  public static class XkcdService
  {
    private const string BLACK = "@";
    private const string CHARCOAL = "#";
    private const string DARKGRAY = "8";
    private const string MEDIUMGRAY = "&";
    private const string MEDIUM = "o";
    private const string GRAY = ":";
    private const string SLATEGRAY = "*";
    private const string LIGHTGRAY = ".";
    private const string WHITE = " ";

    public static XkcdPostDetail GetPostDetails(int imgNumber)
    {
      using HttpClient httpClient = new HttpClient();
      using Stream jsonStream = httpClient.GetStreamAsync($"https://xkcd.com/{imgNumber}/info.0.json").Result;

      DataContractJsonSerializer serializer =
      new DataContractJsonSerializer(typeof(XkcdPostDetail),
      new DataContractJsonSerializerSettings
      {
        UseSimpleDictionaryFormat = true,
        SerializeReadOnlyTypes = true,
      });

      return serializer.ReadObject(jsonStream) as XkcdPostDetail;
    }

    public static Bitmap DownloadImage(string imageUrl)
    {
      using HttpClient httpClient = new HttpClient();
      using Image bmap = Image.FromStream(httpClient.GetStreamAsync(imageUrl).Result);

      if (bmap.PropertyIdList.Contains(0x112))
      {
        System.Drawing.Imaging.PropertyItem propItem = bmap.GetPropertyItem(0x112);
        bmap.RotateFlip(GetRotation(propItem.Value[0]));
      }

      double scaleFactor = (double)Console.WindowWidth * 0.9 / bmap.Width;
      return new Bitmap(bmap, (int)(bmap.Width * scaleFactor), (int)(bmap.Height * scaleFactor));
    }

    public static RotateFlipType GetRotation(int orientation) => orientation switch
    {
      2 => RotateFlipType.RotateNoneFlipX,
      3 => RotateFlipType.Rotate180FlipNone,
      4 => RotateFlipType.Rotate180FlipX,
      5 => RotateFlipType.Rotate90FlipX,
      6 => RotateFlipType.Rotate90FlipNone,
      7 => RotateFlipType.Rotate270FlipX,
      8 => RotateFlipType.Rotate270FlipNone,
      _ => RotateFlipType.RotateNoneFlipNone,
    };

    public static void PrintImageAsAscii(Bitmap image)
    {
      int imageWidth = image.Width;
      int imageHeight = image.Height;

      for (int i = 0; i < imageHeight; i++)
      {
        for (int j = 0; j < imageWidth; j++)
        {
          Color oc = image.GetPixel(j, i);
          int grayScale = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
          Console.Write(GetShade(grayScale));
        }

        Console.WriteLine("");
      }
    }

    private static string GetShade(int redValue)
    {
      if (redValue >= 230)
        return WHITE;
      if (redValue >= 200)
        return LIGHTGRAY;
      if (redValue >= 180)
        return SLATEGRAY;
      if (redValue >= 160)
        return GRAY;
      if (redValue >= 130)
        return MEDIUM;
      if (redValue >= 100)
        return MEDIUMGRAY;
      if (redValue >= 70)
        return DARKGRAY;
      if (redValue >= 50)
        return CHARCOAL;
      return BLACK;
    }

    public static int GenerateRandomId(int latestEntryId)
    {
      Random rand = new Random();
      return rand.Next(latestEntryId) + 1;
    }

    public static int GetLatestEntryId()
    {
      const string baseUrl = "https://xkcd.com/atom.xml";
      const string xmlPath = "descendant::xkcd:id[2]";

      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.Load(baseUrl);

      XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
      nsmgr.AddNamespace("xkcd", "http://www.w3.org/2005/Atom");

      XmlNode rootNode = xmlDoc.DocumentElement;
      XmlNode latestPost = rootNode.SelectSingleNode(xmlPath, nsmgr);

      return int.Parse(latestPost.InnerText.Trim('/').Split('/').Last());
    }

    [DataContract]
    public sealed class XkcdPostDetail
    {
      [DataMember(Name = "safe_title", IsRequired = true)]
      public string Name { get; set; }

      [DataMember(Name = "img", IsRequired = true)]
      public string ImageUrl { get; set; }
    }
  }
}
