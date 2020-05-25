using System;
using System.Drawing;

namespace Bonobo
{
  internal static class Program
  {
    private static void Main()
    {
      int latestEntry = XkcdService.GetLatestEntryId();
      int entryId = XkcdService.GenerateRandomId(latestEntry);

      XkcdService.XkcdPostDetail postDetails = XkcdService.GetPostDetails(entryId);

      Console.WriteLine(postDetails.Name);
      Console.WriteLine(postDetails.ImageUrl);

      Bitmap image = XkcdService.DownloadImage(postDetails.ImageUrl);
      XkcdService.PrintImageAsAscii(image);
    }
  }
}