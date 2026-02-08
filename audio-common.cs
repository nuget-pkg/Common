//css_inc ../common/YtDlpCommon.cs
using Global;
using System;
using System.IO;
using System.Text.RegularExpressions;
using static Global.EasyObject;

public static class AudioCommon
{
    //const string mediaType = "mp4";
    //const string extension = "." + mediaType;
    public static void DownloadVideos(
        string mediaType
        )
    {
        string prefix = $"[{mediaType}] ";
        string extension = "." + mediaType;

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
                RunYtDlp(eo[key], prefix: prefix, mediaType: mediaType, extension: extension);
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
        string mediaType,
        string extension
        //int resolution
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
                //Environment.Exit(0);
                if (mediaType == "mp3")
                {
                    Sys.RunCommand(
                        "yt-dlp",
                        "-c",
                        "--extract-audio",
                        "--audio-quality", "0",
                        "--audio-format", mediaType,
                        "--add-metadata",
                        $"--postprocessor-args", $"-metadata album={videoUrl} -metadata title='{YtDlpCommon.AdjustMetaData(videoTitle, removeSurrogate: true)}' -metadata artist='{YtDlpCommon.AdjustMetaData(artist, removeSurrogate: true)}' -metadata comment='{datetimeString}'",
                        "--embed-thumbnail",
                        $"{videoUrl}",
                        "-o", fileName
                    );
                }
                else if (mediaType == "m4a")
                {
                    Sys.RunCommand(
                        "yt-dlp",
                        "-c",
                        "--extract-audio",
                        "--audio-quality", "0",
                        "--audio-format", mediaType,
                        "-S", "+aext:m4a:aac",
                        "--add-metadata",
                        $"--postprocessor-args", $"-metadata album={videoUrl} -metadata title='{YtDlpCommon.AdjustMetaData(videoTitle, removeSurrogate: true)}' -metadata artist='{YtDlpCommon.AdjustMetaData(artist, removeSurrogate: true)}' -metadata comment='{datetimeString}'",
                        "--embed-thumbnail",
                        $"{videoUrl}",
                        "-o", fileName
                    );
                }
            }
            if (!File.Exists(fileName))
            {
                string found = YtDlpCommon.findFileByVideoId(videoId, extension);
                if (found != null) fileName = found;
            }

            if (File.Exists(fileName))
            {
                m3uText += $"#EXTINF:{++count},{fileName}\n";
                m3uText += $"{fileName}\n";
            }
        }
        File.WriteAllText(m3uPath!, m3uText.Replace("\r\n", "\n"));
        return;
    }
}

