using JaLoader.Common;
using NLayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static UIBasicSprite;

namespace JaLoader
{
    public class CustomRadioController : MonoBehaviour
    {
        #region Singleton
        public static CustomRadioController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if (JaLoaderSettings.UseCustomSongs)
                WarnAboutBadFormats();

            EventsManager.Instance.OnGameLoad += OnGameLoad;
            EventsManager.Instance.OnMenuLoad += OnMenuLoad;
        }
        #endregion

        private readonly string folderPath = $@"{Application.dataPath}\..\Songs";
        public List<AudioClip> loadedSongs = new List<AudioClip>();

        private GameObject RadioFreq;
        private MenuVolumeChanger menuVolumeChanger;

        void Update()
        {
            if(JaLoaderSettings.DebugMode)
                if (Input.GetKeyDown(KeyCode.F6))
                    FindObjectOfType<RadioFreqLogicC>().NextSong();
        }

        private void WarnAboutBadFormats()
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] Songs = dir.GetFiles("*.wav")
                .Concat(dir.GetFiles("*.ogg"))
                .Concat(dir.GetFiles("*.flac"))
                .Concat(dir.GetFiles("*.aac"))
                .Concat(dir.GetFiles("*.aiff"))
                .ToArray();

            foreach (FileInfo file in Songs)
                Console.LogError($"Song '{file.Name}' couldn't be loaded! Songs must be in .mp3 format!");

            StartCoroutine(LoadAudioClips());
        }

        private IEnumerator LoadAudioClips()
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);

            FileInfo[] MP3Songs = dir.GetFiles("*.mp3");

            UnityEngine.Debug.Log($"Found {MP3Songs.Length} .mp3 files, loading audio clips!");

            foreach (FileInfo file in MP3Songs) 
            {
                UnityEngine.Debug.Log($"Loading song {file.Name}!");

                try
                {
                    var mpegFile = new MpegFile(file.FullName);

                    AudioClip clip = AudioClip.Create(Path.GetFileNameWithoutExtension(file.FullName),
                                        (int)(mpegFile.Length / sizeof(float) / mpegFile.Channels),
                                        mpegFile.Channels,
                                        44100,
                                        true,
                                        data => { int actualReadCount = mpegFile.ReadSamples(data, 0, data.Length); },
                                        position => { mpegFile = new MpegFile(file.FullName); });

                    if (clip.frequency != 44100)
                    {
                        Console.LogError($"Song '{file.Name}' couldn't be loaded! Songs must have a sample rate of 44.1kHz!");

                        continue;
                    }

                    clip.name = file.Name;

                    loadedSongs.Add(clip);
                }
                catch (Exception ex)
                {
                    Console.LogError($"Failed to load song '{file.Name}'! A common cause is the song being too long.");
                    Console.LogError(ex);

                    UnityEngine.Debug.LogError($"Failed to load song {file.Name}!");
                    UnityEngine.Debug.LogError(ex.Message);

                    continue;
                }

                UnityEngine.Debug.Log($"Loaded song {file.Name}!");
            }

            UnityEngine.Debug.Log($"Song loading complete ({loadedSongs.Count} songs)!");

            while (SceneManager.GetActiveScene().buildIndex != 1)
                yield return null;

            AddSongsToRadio();

            Console.Log("JaLoader", $"{loadedSongs.Count} custom songs loaded!");
        }

        private void OnGameLoad()
        {
            AddSongsToRadio();
        }

        private void OnMenuLoad()
        {
            RadioFreq = GameObject.Find("RadioFreq");
            menuVolumeChanger = RadioFreq.AddComponent<MenuVolumeChanger>();
            menuVolumeChanger.muted = true;

            UpdateMenuMusic(!JaLoaderSettings.DisableMenuMusic, (float)JaLoaderSettings.MenuMusicVolume / 100);

            StartCoroutine(AddSongsToRadioWithDelay());
        }

        internal void UpdateMenuMusic(bool enable, float volume)
        {
            if (SceneManager.GetActiveScene().buildIndex != 1)
                return;

            RadioFreq.SetActive(enabled);
            menuVolumeChanger.volume = volume;
        }

        private void AddSongsToRadio()
        {
            if (loadedSongs.Count == 0)
                return;

            var radio = FindObjectOfType<RadioFreqLogicC>();

            radio.enabled = false;

            if (JaLoaderSettings.CustomSongsBehaviour == CustomSongsBehaviour.Add)
            {
                var songListings = radio.songListings.ToList();

                foreach (var song in loadedSongs)
                    songListings.Add(song);

                radio.songListings = songListings.ToArray();
                radio.songNumber = radio.songListings.Length;
            }
            else
            {
                List<AudioClip> loadedSongsRepeated = new List<AudioClip>(loadedSongs);

                while (loadedSongsRepeated.Count <= 15)
                    loadedSongsRepeated.AddRange(loadedSongsRepeated);

                radio.songListings = loadedSongsRepeated.ToArray();
                radio.songNumber = loadedSongsRepeated.Count;
            } 

            radio.songShuffle = new List<AudioClip>();

            radio.ArrangeShuffle();

            radio.enabled = true;
        }

        private IEnumerator AddSongsToRadioWithDelay()
        {
            AddSongsToRadio();

            yield return new WaitForEndOfFrame();

            FindObjectOfType<MenuVolumeChanger>().muted = false;
        }
    }
}
