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

            if (SettingsManager.Instance.UseCustomSongs)
                WarnAboutBadFormats();

            EventsManager.Instance.OnGameLoad += OnGameLoad;
            EventsManager.Instance.OnMenuLoad += OnMenuLoad;
            SceneManager.sceneLoaded += OnSceneChanged;
        }
        #endregion

        private void OnSceneChanged(Scene sceneToLoad, LoadSceneMode mode)
        {
            if (sceneToLoad.buildIndex == 1)
                FindObjectOfType<MenuVolumeChanger>().muted = true;
        }

        private readonly string folderPath = $@"{Application.dataPath}\..\Songs";
        public List<AudioClip> loadedSongs = new List<AudioClip>();

        void Update()
        {
            if(SettingsManager.Instance.DebugMode)
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

                var mpegFile = new MpegFile(file.FullName);

                AudioClip clip = AudioClip.Create(System.IO.Path.GetFileNameWithoutExtension(file.FullName),
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

                UnityEngine.Debug.Log($"Loaded song {file.Name}!");
            }

            UnityEngine.Debug.Log($"Song loading complete ({loadedSongs.Count} songs)!");

            while (SceneManager.GetActiveScene().buildIndex != 1)
                yield return null;

            AddSongsToRadio();
        }

        private void OnGameLoad()
        {
            AddSongsToRadio();
        }

        private void OnMenuLoad()
        {
            StartCoroutine(AddSongsToRadioWithDelay());
        }

        private void AddSongsToRadio()
        {
            if (loadedSongs.Count == 0)
                return;

            var radio = FindObjectOfType<RadioFreqLogicC>();

            radio.enabled = false;

            if (SettingsManager.Instance.CustomSongsBehaviour == CustomSongsBehaviour.Add)
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
            yield return new WaitForEndOfFrame();

            FindObjectOfType<MenuVolumeChanger>().muted = false;
        }
    }
}
