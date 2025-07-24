using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Discord;
using UnityEngine.SceneManagement;

namespace JaLoader
{
    public class DiscordController : MonoBehaviour
    {
        private Discord.Discord discord;
        private long time;

        private string State;
        private string Details;
        private string LargeText;
        
        void Start()
        {
            if (!SettingsManager.UseDiscordRichPresence)
            {
                Destroy(this);
                return;
            }

            discord = new Discord.Discord(1104362708134006827, (ulong)CreateFlags.NoRequireDiscord);

            time = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            State = LargeText = "In Menu";
            Details = "";

            EventsManager.Instance.OnGameLoad += OnGameLoad;
            EventsManager.Instance.OnMenuLoad += OnMenuLoad;
            EventsManager.Instance.OnRouteGenerated += OnRouteGenerated;

            UpdateActivity(true);
        }

        void Update()
        {
            discord.RunCallbacks();
        }

        void LateUpdate()
        {
            UpdateActivity(false);

            if (SceneManager.GetActiveScene().buildIndex != 3)
                return;

            if (ModHelper.Instance.player.transform.parent == null)
                Details = "Walking";
            else if (UncleHelper.Instance.Uncle.isSat)
                Details = "Driving with Uncle";
            else
                Details = "Driving";
        }

        private void OnGameLoad()
        {
            State = LargeText = "No Route Selected";
        }

        private void OnRouteGenerated(string start, string destination, int distance)
        {
            State = LargeText = $"Driving from {start} to {destination} ({distance}km)";
        }

        private void OnMenuLoad()
        {
            State = LargeText = "In Menu";
            Details = "";
        }

        private void UpdateActivity(bool updateTime)
        {
            try
            {
                var activityManager = discord.GetActivityManager();

                var activity = new Activity
                {
                    State = State,
                    Details = Details,
                    Assets =
                    {
                        LargeImage = "jalopy",
                        LargeText = LargeText,
                        SmallImage = "wrenchright",
                        SmallText = $"JaLoader {SettingsManager.GetVersionString()} - {ModLoader.Instance.modsNumber} mods loaded"
                    },
                    Timestamps =
                    {
                        Start = time
                    }
                };

                activityManager.UpdateActivity(activity, (res) =>
                {
                    if (res != Result.Ok && SettingsManager.DebugMode)
                        Console.LogError("Failed connecting to Discord!");
                });
            }
            catch (Exception)
            {
                Destroy(this);
                return;
                throw;
            }
        }
    }
}
