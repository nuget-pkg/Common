namespace Common
{
    using Global;
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using static Global.EasyObject;
    public static class VideoCommon
    {

        const string mediaType = "mp4";
        const string extension = "." + mediaType;
        public static void DownloadVideos(
            int resolution
            )
        {
            //string prefix = $"【{resolution}p】";
            string prefix = $"[{resolution}p] ";

            try
            {
                ShowDetail = true;
                string allJson = File.ReadAllText(@"C:\Users\user\Downloads\_all_playlist_info.json");
                var eo = FromJson(allJson);
                Echo(eo);
                var keys = eo.Keys;
                foreach (var key in keys)
                {
                    Echo(key);
                    RunYtDlp(eo[key], prefix: prefix, extension: extension, resolution: resolution);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                Environment.Exit(1);
            }
        }

        private static void RunYtDlp(
            EasyObject playlistObject,
            string prefix,
            string extension,
            int resolution
            )
        {
            var playlistDynamic = playlistObject.Dynamic;
            string playlistId = playlistDynamic.playlistId;
            string playlistTitle = playlistDynamic.playlistTitle;
            Echo(new { playlistId, playlistTitle });
            string m3uPath = $"#{YtDlpCommon.AdjustFileName(playlistTitle, prefix)}.m3u";
            string m3uText = """
                    #EXTM3U

                    """;
            var videos = playlistDynamic.videos;
            int count = 0;
            for (int i = 0; i < videos.Count; i++)
            {
                var video = videos[i];
                Echo(video);
                string videoId = video.videoId;
                string videoTitle = video.videoTitle;
                if (videoTitle == "Deleted video") continue;
                if (videoTitle == "Private video") continue;
                if (videoTitle.StartsWith("<video-not-found>")) continue;
                string videoOwnerChannelTitle = video.videoOwnerChannelTitle;
                string videoUrl = $"https://youtu.be/{videoId}";
                string artist = videoOwnerChannelTitle;
                Echo(new { playlistTitle, videoId, videoTitle, videoUrl, videoOwnerChannelTitle });
                string origName = $"『{videoTitle}』 【ID:{videoId}】{extension}"; ;
                origName = $"{artist} {origName}";
                string fileName = YtDlpCommon.AdjustFileName(origName, prefix);
                if (File.Exists(fileName))
                {
                    Log(fileName, "Skipping");
                }
                else
                {
                    string datetimeString = DateTime.Now.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
                    Echo(datetimeString, "datetimeString");
                    Sys.RunCommand(
                        "yt-dlp",
                        "-S", $"vcodec:h264,res:{resolution},acodec:aac", // https://github.com/yt-dlp/yt-dlp/issues/11362
                        "--add-metadata",
                        $"--postprocessor-args", $"-metadata album={videoUrl} -metadata title='{YtDlpCommon.AdjustMetaData(videoTitle, removeSurrogate: true)}' -metadata artist='{YtDlpCommon.AdjustMetaData(artist, removeSurrogate: true)}' -metadata comment='{datetimeString}'",
                        "--embed-thumbnail",
                        "--merge-output-format", mediaType,
                        $"{videoUrl}",
                        "-o", fileName
                    );
                }
                if (!File.Exists(fileName))
                {
                    string? found = YtDlpCommon.findFileByVideoId(videoId, extension);
                    if (found != null) fileName = found;
                }

                if (File.Exists(fileName))
                {
                    m3uText += $"#EXTINF:{++count},{fileName}\n";
                    m3uText += $"{fileName}\n";
                }
            }
            if (count == 0)
            {
                try
                {
                    File.Delete(m3uPath!);
                }
                catch
                {
                    ;
                }

            }
            else
            {
                File.WriteAllText(m3uPath!, m3uText.Replace("\r\n", "\n"));
            }
            return;
        }
    }
}

