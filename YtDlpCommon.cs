//css_nuget EasyObject
//css_dir   $(HOME)/nuget.org/Internals
//xxx
namespace Common
{
    using Global;
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using static Global.EasyObject;

    public static class YtDlpCommon
    {
        public static void MoveQuiet(string from, string to)
        {
            try
            {
                File.Move(from, to);

            }
            catch (Exception /*e*/)
            {
                File.Delete(from);
            }
        }

        public static string サロゲート文字を削除(string s)
        {
            s = Regex.Replace(s, @"[\uD800-\uDFFF]", "★"); // https://teratail.com/questions/53520 絵文字の判別方法
            s = s.Replace("★★", "★");
            return s;
        }
        public static string AdjustFileName(string filename, string prefix)
        {
            filename = サロゲート文字を削除(filename);
            string result = filename
                .Replace("'", "’")
                .Replace("\"", "”")
                .Replace(":", "：")
                .Replace("/", "／")
                .Replace("　", " ")
                .Replace("|", "｜")
                .Replace("#", "＃")
                ;
            if (!result.StartsWith(prefix))
            {
                result = $"{prefix}{result}";
            }
            return result;
        }

        public static string AdjustMetaData(string metadata, bool removeSurrogate)
        {
            metadata = metadata
                .Replace("'", "’")
                .Replace("\"", "”")
                //.Replace("|", "｜")
                ;
            if (removeSurrogate)
            {
                metadata = サロゲート文字を削除(metadata);
            }
            return metadata;
        }

        public static string findFileByVideoId(string videoId, string extension)
        {
            string[] fileEntries = Directory.GetFiles(".");
            foreach (string fileEntry in fileEntries)
            {
                if (!fileEntry.EndsWith(extension)) continue;
                if (fileEntry.Contains($"[{videoId}]")) return fileEntry.Replace(@".\", "");
                if (fileEntry.Contains($"【{videoId}】")) return fileEntry.Replace(@".\", "");
            }
            return null;
        }
    }
}
