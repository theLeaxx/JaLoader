using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Theraot.Collections;
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
                ConvertSongs();

            EventsManager.Instance.OnLoadStart += OnLoadStart;
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

            //Console.Instance?.Log(FindObjectOfType<RadioFreqLogicC>().songShuffle.Count);
        }

        private void ConvertSongs()
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] MP3Songs = dir.GetFiles("*.mp3");

            foreach (FileInfo file in MP3Songs)
            {
                string newName = file.FullName.Replace(".mp3", ".wav");

                Mp3FileReader reader = new Mp3FileReader(file.FullName);
                WaveStream stream = WaveFormatConversionStream.CreatePcmStream(reader);
                WaveFileWriter.CreateWaveFile(newName, stream);
            }

            StartCoroutine(LoadAudioClips());
        }

        private void DeleteMP3Files()
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] MP3Songs = dir.GetFiles("*.mp3");

            foreach (FileInfo file in MP3Songs)
            {
                File.Delete(file.FullName);
            }
        }

        private IEnumerator LoadAudioClips()
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);

            FileInfo[] WAVSongs = dir.GetFiles("*.wav");

            foreach (FileInfo file in WAVSongs) 
            {
                string path = $"file://{file.FullName}";

                UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV);

                yield return req.SendWebRequest();

                AudioClip clip = DownloadHandlerAudioClip.GetContent(req);

                clip.name = file.Name;

                loadedSongs.Add(clip);
            }

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

        private void OnLoadStart()
        {
            DeleteMP3Files();
        }

        private void AddSongsToRadio()
        {
            if (loadedSongs.Count == 0)
                return;

            var radio = FindObjectOfType<RadioFreqLogicC>();

            radio.enabled = false;

            var songListings = radio.songListings.ToList();

            foreach (var song in loadedSongs)
                songListings.Add(song);

            radio.songListings = songListings.ToArray();
            radio.songNumber = radio.songListings.Length;

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
