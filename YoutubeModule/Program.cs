using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Logging;
using PLang.Attributes;
using PLang.Errors;
using PLang.Errors.Runtime;
using PLang.Interfaces;
using PLang.Modules;
using System.ComponentModel;
using System.Threading.Tasks;


namespace YoutubeModule
{
    [Description("Upload a video to youtube(set name, description, playlist, public/private, etc), get list of my videos, remove a video. Also monitor for change in the upload on a variable")]
    public class Program : BaseProgram
    {
        private readonly YouTubeService _youtubeService;

        public Program(ISettings settings, ILogger logger) : base()
        {
            var credential = GoogleCredential.FromJson(settings.Get<string>(typeof(string), "YoutubeCredentials", "", "The json for YouTube API credentials"))
                .CreateScoped(YouTubeService.Scope.Youtube);

            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "YoutubeModule"
            });
        }

		/*
         * plang: - upload file.mp4, "My first video", "I am doing my first video.....", put on "PLbm1UMZKMaqdOIjjdmz94x5BrGj2z9jQR" playlist, make it public and update %status% on the progress
         */
		[Description("privacyStatus=public|private|unlisted")]
        public async Task<IError?> UploadVideo(string filePath, string title, string categoryId, string? description = null, 
                string? playlistId = null, string? privacyStatus = null, string[]? tags = null,
                [HandlesVariable] string? statusVariable = null)
        {
            var absoluteFilePath = GetPath(filePath);

            var video = new Video
            {
                Snippet = new VideoSnippet
                {
                    Title = title,
                    Description = description,
                    Tags = tags,
                    CategoryId = categoryId // See https://developers.google.com/youtube/v3/docs/videoCategories/list
				},
                Status = new VideoStatus
                {
                    PrivacyStatus = privacyStatus // "public", "private" or "unlisted"
                }
            };

            using (var fileStream = new FileStream(absoluteFilePath, FileMode.Open, FileAccess.Read))
            {
				ProgramError? pe = null;
				var videosInsertRequest = _youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                if (!string.IsNullOrEmpty(statusVariable))
                {                    
                    videosInsertRequest.ProgressChanged += (progress) =>
                    {                       
                        memoryStack.Put(statusVariable, progress);
                    };
                }
                var progress = await videosInsertRequest.UploadAsync();
				if (progress.Exception != null)
				{
					return new ProgramError(progress.Exception.Message, goalStep, function, Exception: progress.Exception);
				}
            }

            if (!string.IsNullOrEmpty(playlistId))
            {
                var newPlaylistItem = new PlaylistItem
                {
                    Snippet = new PlaylistItemSnippet
                    {
                        PlaylistId = playlistId,
                        ResourceId = new ResourceId
                        {
                            Kind = "youtube#video",
                            VideoId = video.Id
                        }
                    }
                };
                await _youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet").ExecuteAsync();
            }
            return null;
        }

        /*
         * plang: - get all videos, write to %videos%
         */
        public async Task<string> GetAllMyVideos()
        {
            var searchListRequest = _youtubeService.Search.List("snippet");
            searchListRequest.MaxResults = 50;
            searchListRequest.ForMine = true;
            searchListRequest.Type = "video";

            var searchListResponse = await searchListRequest.ExecuteAsync();

            var videos = searchListResponse.Items;
            return string.Join(", ", videos.Select(v => v.Snippet.Title));
        }

        /*
         * plang: - get all videos in my "default" playlist
         */
        public async Task<string> GetVideosInPlaylist(string playlistId)
        {
            var playlistItemsListRequest = _youtubeService.PlaylistItems.List("snippet");
            playlistItemsListRequest.PlaylistId = playlistId;
            playlistItemsListRequest.MaxResults = 50;

            var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

            var videos = playlistItemsListResponse.Items;
            return string.Join(", ", videos.Select(v => v.Snippet.Title));
        }
    }
}
