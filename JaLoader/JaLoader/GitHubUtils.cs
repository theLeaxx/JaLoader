using JaLoader.Common;
using UnityEngine;
using UnityEngine.Networking;

namespace JaLoader
{
    public class GitHubUtils : IGitHubReleaseUtils
    {
        private static GitHubUtils _instance;

        public static GitHubUtils Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GitHubUtils();
                }
                return _instance;
            }
        }

        private GitHubUtils() { }

        public string GetLatestTagFromAPIURL(string URL, string modName = null)
        {
            string messageSender;
            if (modName == null)
                messageSender = "JaLoader";
            else
                messageSender = modName;    

            UnityWebRequest request = UnityWebRequest.Get(URL);
            request.SetRequestHeader("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)");
            request.SetRequestHeader("Accept", "application/vnd.github.v3+json");

            request.SendWebRequest();

            while (!request.isDone)
            {
                // wait for the request to complete
            }

            if (request.isHttpError || request.error == "Generic/unknown HTTP error")
                return "0";

            string tagName = null;

            if (!request.isNetworkError)
            {
                string json = request.downloadHandler.text;
                Release release = JsonUtility.FromJson<Release>(json);
                tagName = release.tag_name;
            }
            else if (request.isNetworkError)
                return "-1";
            else
            {
                Console.LogError(messageSender, $"Error getting response for URL \"{URL}\": {request.error}");
                return "-1";
            }

            return tagName;
        }
    }
}
