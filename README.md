# yt-channel-thumbnail-dl

### Prepare
Fill in Youtube Data API key

```csharp
var youtubeService = new YouTubeService(new BaseClientService.Initializer()
{
    ApiKey = "xxxxxxxxxx",
    ApplicationName = this.GetType().ToString()
});
```


### Build & run
```bash
dotnet restore
dotnet watch run
```
