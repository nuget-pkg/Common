#if false
// //+#nuget DotNetEnv@3.1.1;
// //+#nuget ProcessX@1.5.6;
// //+#nuget Kokuban@0.2.0;
#nullable enable
namespace Common
{
    using System.IO;
    using Zx;
    using Kokuban;
    using static Global.EasyObject;
    using System;
    using System.Threading.Tasks;

    public static class Shell
    {

        public static void header(string s)
        {
            Console.WriteLine(Chalk.Green[s]);
        }

        public static void echo(object x, string? title = null)
        {
            Echo(x, title);
        }


        public static void log(object x, string? title = null)
        {
            Log(x, title);
        }

        public static async Task runAsync(string s, bool ignoreErrors = false)
        {
            if (!ignoreErrors)
            {
                await s;
                return;
            }
            try
            {
                await s;
            }
            catch (Exception)
            {
            }
        }

        public static void run(string s, bool ignoreErrors = false)
        {
            var task = runAsync(s, ignoreErrors);
            task.Wait();
        }

        public static async Task<string> fetchAsync(string s)
        {
            return await s;
        }

        public static string fetch(string s)
        {
            var task = fetchAsync(s);
            task.Wait();
            return task.Result;
        }

        public static void load(string? path = null)
        {
            if (path == null)
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            }
            if (!File.Exists(path))
            {
                return;
            }
            DotNetEnv.Env.Load(path);
        }

        public static string @string(string name, string? fallback = default(string))
        {
            return DotNetEnv.Env.GetString(name, fallback);
        }

        public static bool @bool(string name, bool fallback = default(bool))
        {
            return DotNetEnv.Env.GetBool(name, fallback);
        }

        public static int @int(string name, int fallback = default(int))
        {
            return DotNetEnv.Env.GetInt(name, fallback);
        }

        public static double @double(string name, double fallback = default(double))
        {
            return DotNetEnv.Env.GetDouble(name, fallback);
        }
    }

}
#endif
