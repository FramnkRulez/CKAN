using System;
using System.Net;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

// We could use OctoKit for this, but since we're only pinging the
// release API, I'm happy enough without yet another dependency.

namespace CKAN.NetKAN
{
    public static class GithubAPI
    {

        private static readonly Uri api_base = new Uri("https://api.github.com/");
        private static readonly ILog log = LogManager.GetLogger(typeof (KSAPI));
        private static readonly WebClient web = new WebClient();
        private static bool done_init = false;

        internal static void Init() {
            if (done_init)
            {
                return;
            }

            // Github requires a user-agent. How about that?
            web.Headers.Add("user-agent", "CKAN Github2CKAN ( https://github.com/KSP-CKAN/CKAN )");

            done_init = true;
            return;
        }

        public static void SetCredentials(string oauth_token)
        {
            web.Headers.Add("Authorization", String.Format("token {0}", oauth_token));
        }

        public static string Call(string path)
        {
            Init();

            Uri url = new Uri (api_base, path);
            log.DebugFormat("Calling {0}", url);

            return web.DownloadString(url);
        }

        /// <summary>
        /// Repository is in the form "Author/Repo". Eg: "pjf/DogeCoinFlag".
        /// </summary>
        public static GithubRelease GetLatestRelease( string repository )
        {
            string json = Call ("repos/" + repository + "/releases");
            log.Debug("Parsing JSON...");
            JArray releases = JArray.Parse(json);
            log.Debug("Parsed, finding most recent stable release");

            // Finding the most recent *stable* release means filtering
            // out on pre-releases.

            JObject release = (JObject) releases.Where(x => (bool) x["prerelease"] == false).First();

            return new GithubRelease(release);
        }

    }
}

