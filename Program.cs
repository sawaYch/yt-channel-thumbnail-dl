using System.Net;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Drawing;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using Google.Apis.YouTube.v3.Data;
using System.Linq;

namespace Google.Apis.YouTube.Samples
{
    /// <summary>
    /// YouTube Data API v3 sample: search by keyword.
    /// Relies on the Google APIs Client Library for .NET, v1.7.0 or higher.
    /// See https://developers.google.com/api-client-library/dotnet/get_started
    ///
    /// Set ApiKey to the API key value from the APIs & auth > Registered apps tab of
    ///   https://cloud.google.com/console
    /// Please ensure that you have enabled the YouTube Data API for your project.
    /// </summary>
    internal class Search
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("YouTube Data API: Search");
            Console.WriteLine("========================");

            try
            {
                new Search().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task Run()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "",
                ApplicationName = this.GetType().ToString()
            });

            var responses = new List<SearchListResponse>();

            string? nextPageToken = null;
            while(true)
            {
                var searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.ChannelId = "UCVDrzfo7NnOvNx8dU-Ebitg";
                searchListRequest.Type = "video";
                searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
                searchListRequest.MaxResults = 50;
                if (nextPageToken != null)
                {
                    searchListRequest.PageToken = nextPageToken;
                }

                // Call the search.list method to retrieve results matching the specified query term.
                var searchListResponse = await searchListRequest.ExecuteAsync();
                nextPageToken = searchListResponse.NextPageToken;
                responses.Add(searchListResponse);
                if (nextPageToken == null) break;
            }

            List<string> videos = new List<string>();
            var client = new HttpClient();
            string imageDir = Directory.GetCurrentDirectory() + "/downloadedImage";
            if (!Directory.Exists(imageDir)) Directory.CreateDirectory(imageDir);
            ISupportedImageFormat format = new JpegFormat { Quality = 100 };

            var flattenItems = responses.SelectMany(r => r.Items).ToList();
            foreach (var searchResult in flattenItems)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        videos.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
                        Console.WriteLine($"url {searchResult.Snippet.Thumbnails.High.Url} {searchResult.Snippet.Title}");
                        var response = await client.GetAsync(new Uri(searchResult.Snippet.Thumbnails.High.Url));
                        if (response.StatusCode != HttpStatusCode.OK) continue;
                        var buffer = await response.Content.ReadAsByteArrayAsync();
                        string savedImagePath = Path.Combine(imageDir, searchResult.ETag + ".jpg");

                        using (MemoryStream instream = new MemoryStream(buffer))
                        {
                            using (MemoryStream outstream = new MemoryStream())
                            {
                                using (ImageFactory factory = new ImageFactory())
                                {
                                    factory.Load(instream).Crop(new Rectangle(x: 0, y:45, width: 480, height:270)).Format(format).Save(outstream);

                                }
                                File.WriteAllBytes(savedImagePath, outstream.ToArray());

                            }

                        }
                        break;
                }
            }

            Console.WriteLine(String.Format("Videos:\n{0}\n", string.Join("\n", videos)));
        }
    }
}