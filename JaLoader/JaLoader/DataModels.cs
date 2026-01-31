using System;
using UnityEngine;
using UnityEngine.UI;
using JaLoader.Common;
using System.Collections.Generic;

namespace JaLoader
{
    internal class ModDataForManager
    {
        public Common.WhenToInit InitTime { get; set; }
        public bool IsEnabled { get; set; }
        public Text DisplayText { get; set; }

        public GenericModData GenericModData { get;}

        public ModDataForManager(Common.WhenToInit initTime, bool isEnabled, Text displayText, GenericModData data)
        {
            InitTime = initTime;
            IsEnabled = isEnabled;
            DisplayText = displayText;
            GenericModData = data;
        }
    }

    // bridger for new WhenToInit enum from Common namespace
    [Obsolete("Use JaLoader.Common.WhenToInit instead.")]
    public enum WhenToInit
    {
        InMenu,
        InGame,
        None
    }

    public class GenericModData
    {
        public string ModID { get; set; }
        public string ModName { get; set; }
        public string ModVersion { get; set; }
        public string ModDescription { get; set; }
        public string ModAuthor { get; set; }
        public string GitHubLink { get; set; }
        public string NexusModsLink { get; set; }
        public MonoBehaviour Mod { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsBepInExMod { get; set; }

        public GenericModData(string modID, string modName, string modVersion, string modDescription, string modAuthor, string gitHubLink, string nexusModsLink, MonoBehaviour mod, bool isEnabled = true, bool isBIXMod = false)
        {
            ModID = modID;
            ModName = modName;
            ModVersion = modVersion;
            ModDescription = modDescription;
            ModAuthor = modAuthor;
            GitHubLink = gitHubLink;
            NexusModsLink = nexusModsLink;
            Mod = mod;
            IsEnabled = isEnabled;
            IsBepInExMod = isBIXMod;
        }

        public override string ToString()
        {
            return $"[ModMetadata: ID={ModID}, ModName='{ModName}', ModAuthor='{ModAuthor}']";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals(obj as GenericModData);
        }

        public bool Equals(GenericModData other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return ModID == other.ModID &&
                   ModName.Equals(other.ModName, StringComparison.OrdinalIgnoreCase) &&
                   ModAuthor.Equals(other.ModAuthor, StringComparison.OrdinalIgnoreCase) &&
                   ReferenceEquals(Mod, other.Mod);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17; 
                hash = hash * 23 + ModID.GetHashCode();
                hash = hash * 23 + (ModName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(ModName) : 0);
                hash = hash * 23 + (ModAuthor != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(ModAuthor) : 0);
                hash = hash * 23 + (Mod != null ? Mod.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(GenericModData left, GenericModData right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }

        public static bool operator !=(GenericModData left, GenericModData right)
        {
            return !(left == right);
        }

    }

    [Obsolete("Use JLRouteGenerator.GetCountryFromCode(string countryCode) instead.")]
    public enum CountryCode
    {
        Germany,
        Czechoslovakia,
        Hungary,
        Yugoslavia,
        Bulgaria,
        Turkey
    }

    public class Country
    {
        public string GenericCountryCode;

        public Dictionary<int, (string, GameObject[])> RoadSegmentsWithRouteLevel;

        public Country(string countryCode)
        {
            GenericCountryCode = countryCode;
            RoadSegmentsWithRouteLevel = new Dictionary<int, (string, GameObject[])>();
        }

        public void AddRoadSegment(int routeLevel, string segmentName, GameObject[] roadPrefabs)
        {
            RoadSegmentsWithRouteLevel[routeLevel] = (segmentName, roadPrefabs);
        }
    }
}
