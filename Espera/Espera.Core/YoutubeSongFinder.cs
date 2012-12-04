﻿using Espera.Core.Audio;
using Google.GData.Client;
using Google.GData.YouTube;
using Google.YouTube;
using Rareform.Validation;
using System;

namespace Espera.Core
{
    public sealed class YoutubeSongFinder : SongFinder<YoutubeSong>
    {
        private const string ApiKey =
            "AI39si5_zcffmO_ErRSZ9xUkfy_XxPZLWuxTOzI_1RH9HhXDI-GaaQ-j6MONkl2JiF01yBDgBFPbC8-mn6U9Qo4Ek50nKcqH5g";

        private readonly string searchString;

        public YoutubeSongFinder(string searchString)
        {
            if (searchString == null)
                Throw.ArgumentNullException(() => searchString);

            this.searchString = searchString;
        }

        public override void Execute()
        {
            var query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri)
            {
                OrderBy = "relevance",
                Query = searchString,
                SafeSearch = YouTubeQuery.SafeSearchValues.None
            };

            var settings = new YouTubeRequestSettings("Espera", ApiKey);
            var request = new YouTubeRequest(settings);
            Feed<Video> feed = request.Get<Video>(query);

            foreach (Video video in feed.Entries)
            {
                var duration = TimeSpan.FromSeconds(Int32.Parse(video.YouTubeEntry.Duration.Seconds));
                string url = video.WatchPage.OriginalString
                    .Replace("&feature=youtube_gdata_player", String.Empty) /* Unnecessary long url */
                    .Replace("https://", "http://"); /* Secure connections are not always easy to handle when streaming */

                var song = new YoutubeSong(url, AudioType.Mp3, duration, CoreSettings.Default.StreamYoutube)
                {
                    Title = video.Title,
                    Description = video.Description,
                    Rating = video.RatingAverage >= 1 ? video.RatingAverage : (double?)null,
                    ThumbnailSource = new Uri(video.Thumbnails[0].Url),
                    Views = video.ViewCount
                };

                this.OnSongFound(song);
            }

            this.OnCompleted();
        }
    }
}