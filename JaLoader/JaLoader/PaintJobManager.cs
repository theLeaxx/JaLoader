using JaLoader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class PaintJobManager : MonoBehaviour
    {
        public static PaintJobManager Instance;

        private void Awake()
        {
            Instance = this;

            EventsManager.Instance.OnSave += OnSave;
            EventsManager.Instance.OnModsInitialized += CreateObjects;
            EventsManager.Instance.OnModsInitialized += ApplySavedPaintjob;
        }

        public readonly string paintjobsLocation = $@"{Application.dataPath}\..\Paintjobs";
        public List<PaintJob> PaintJobs = new List<PaintJob>();

        public Texture2D DefaultPreviewIcon;
        private Material emptyPaintjobMaterial;

        private void Start()
        {
            if (!Directory.Exists(paintjobsLocation))
                Directory.CreateDirectory(paintjobsLocation);

            LoadDefaultTexture();

            var dirInfo = new DirectoryInfo(paintjobsLocation);
            var imageFiles = dirInfo.GetFiles("*_texture.png");
            var dataFiles = dirInfo.GetFiles("*_data.txt");
            var previewFiles = dirInfo.GetFiles("*_preview.png");

            foreach(var dataFile in dataFiles)
            {
                var paintJob = LoadPaintjob(dataFile.FullName);
                if (paintJob == null)
                    continue;

                PaintJobs.Add(paintJob);

                Console.LogDebug($"Loaded paintjob: {paintJob.Name}");
            }

            Console.Log("JaLoader", $"Loaded {PaintJobs.Count} paintjobs!");

            emptyPaintjobMaterial = CreateEmptyMaterial();
        }

        public void LoadDefaultTexture()
        {
            byte[] bytes = File.ReadAllBytes(Path.Combine(JaLoaderSettings.ModFolderLocation, @"Required\defaultPaintjobTexture.png"));

            Texture2D texture = new Texture2D(128, 128, TextureFormat.ARGB32, false);
            texture.LoadImage(bytes);

            DefaultPreviewIcon = texture;
        }

        public void ApplyPaintjob(PaintJob paintJob)
        {
            paintJob = PaintJobs[0];

            ChangeIndex1OfMaterialsArray(ModHelper.Instance.carFrame.GetComponent<MeshRenderer>(), paintJob.Material);
            ChangeIndex1OfMaterialsArray(ModHelper.Instance.carHood.GetComponent<MeshRenderer>(), paintJob.Material);
            ChangeIndex1OfMaterialsArray(ModHelper.Instance.carTrunk.GetComponent<MeshRenderer>(), paintJob.Material);
            ChangeIndex1OfMaterialsArray(ModHelper.Instance.carLeftDoor.GetComponent<MeshRenderer>(), paintJob.Material);
            ChangeIndex1OfMaterialsArray(ModHelper.Instance.carRightDoor.GetComponent<MeshRenderer>(), paintJob.Material);
            ChangeIndex1OfMaterialsArray(ModHelper.Instance.carRoof.GetComponent<MeshRenderer>(), paintJob.Material);
        }

        public void ClearPaintjob()
        {
            ChangeIndex1OfMaterialsArray(ModHelper.Instance.carFrame.GetComponent<MeshRenderer>(), emptyPaintjobMaterial);
            ChangeIndex1OfMaterialsArray(ModHelper.Instance.carHood.GetComponent<MeshRenderer>(), emptyPaintjobMaterial);
            ChangeIndex1OfMaterialsArray(ModHelper.Instance.carTrunk.GetComponent<MeshRenderer>(), emptyPaintjobMaterial);
            ChangeIndex1OfMaterialsArray(ModHelper.Instance.carLeftDoor.GetComponent<MeshRenderer>(), emptyPaintjobMaterial);
            ChangeIndex1OfMaterialsArray(ModHelper.Instance.carRightDoor.GetComponent<MeshRenderer>(), emptyPaintjobMaterial);
            ChangeIndex1OfMaterialsArray(ModHelper.Instance.carRoof.GetComponent<MeshRenderer>(), emptyPaintjobMaterial);
        }

        private void ChangeIndex1OfMaterialsArray(MeshRenderer renderer, Material material)
        {
            var mats = renderer.materials;
            mats[1] = material;
            renderer.materials = mats;
        }

        private void OnSave()
        {
            var appliedPaintJobMaterial = ModHelper.Instance.carFrame.GetComponent<MeshRenderer>().materials[1];

            foreach(var pj in PaintJobs)
            {
                if (pj.Material.name == appliedPaintJobMaterial.name.Replace(" (Instance)", ""))
                {
                    JaLoaderSettings.AppliedPaintJobName = pj.Name;
                    SettingsManager.SaveSettings();
                    return;
                }
            }
        }

        public void ReloadCurrentPaintjob()
        {
            var appliedPaintJobMaterial = ModHelper.Instance.carFrame.GetComponent<MeshRenderer>().materials[1];

            var paintJob = PaintJobs.First(x => x.Material.name == appliedPaintJobMaterial.name.Replace(" (Instance)", ""));

            if (paintJob == null)
                return;

            ReloadPaintjob(paintJob);

            ApplyPaintjob(paintJob);
        }

        public void ApplySavedPaintjob()
        {
            var savedPaintJob = JaLoaderSettings.AppliedPaintJobName;

            if (string.IsNullOrEmpty(savedPaintJob))
                return;

            foreach(var paintJob in PaintJobs)
            {
                if (paintJob.Name == savedPaintJob)
                {
                    ApplyPaintjob(paintJob);
                    return;
                }
            }
        }

        private void CreateTemplateItem()
        {
            var paintJob = new PaintJob("Name", "Description", "Author", 20, false);
            var json = JsonUtility.ToJson(paintJob, true);
            File.WriteAllText(Path.Combine(paintjobsLocation, "example.json"), json);
        }

        private PaintJob LoadPaintjob(string dataFile)
        {
            var fileData = File.ReadAllText(dataFile);

            PaintJob toReturn;

            try
            {
                toReturn = JsonUtility.FromJson<PaintJob>(fileData);
                toReturn.FileName = dataFile;
            }
            catch (Exception)
            {
                Console.LogError($"Error loading paintjob: {dataFile}. Invalid data!");
                return null;
            }

            var imageFile = dataFile.Replace("_data.txt", "_texture.png");

            if(!File.Exists(imageFile))
            {
                Console.LogError($"Error loading paintjob: {dataFile}. Texture file not found!");
                return null;
            }

            toReturn.Texture = LoadMainTexture(imageFile, toReturn.Name + "_Texture");

            if (toReturn.UsesPreviewIcon)
            {
                var previewFile = dataFile.Replace("_data.txt", "_preview.png");

                if (!File.Exists(previewFile))
                {
                    Console.LogError($"Error loading paintjob: {dataFile}. Preview file not found!");
                    return null;
                }

                toReturn.PreviewIcon = LoadPreviewTexture(previewFile, toReturn.Name + "_PreviewIcon");
            }
            else
                toReturn.PreviewIcon = DefaultPreviewIcon;

            toReturn.Material = CreateMaterial(toReturn);

            return toReturn;
        }

        private void CreateObjects()
        {
            foreach (var paintJob in PaintJobs)
            {
                var inGameObj = ModHelper.Instance.CreatePaintJobBox(paintJob.Name, paintJob.Author, paintJob.Description, paintJob.Price, paintJob.Material);
                CustomObjectsManager.Instance.RegisterObject(inGameObj, paintJob.Name, true);
            }
        }

        private void ReloadPaintjob(PaintJob paintJob)
        {
            var loadedPaintJob = LoadPaintjob(paintJob.FileName);

            if (loadedPaintJob == null)
                return;

            PaintJobs.Remove(paintJob);
            PaintJobs.Add(loadedPaintJob);

            Console.LogDebug($"Reloaded paintjob: {loadedPaintJob.Name}");
        }

        private Material CreateMaterial(PaintJob paintJob)
        {
            Shader shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            Material material = new Material(shader);
            material.mainTexture = paintJob.Texture;
            material.name = paintJob.Name + "_Material";

            return material;
        }

        private Material CreateEmptyMaterial()
        {
            Shader shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            Material material = new Material(shader);
            material.color = new Color(0, 0, 0, 0);
            material.name = "EmptyPaintjobMaterial";

            return material;
        }

        private Texture2D LoadMainTexture(string path, string textureName)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
            texture.name = textureName;
            texture.LoadImage(bytes);

            return texture;
        }

        private Texture2D LoadPreviewTexture(string path, string textureName)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(512, 512, TextureFormat.ARGB32, false);
            texture.name = textureName;
            texture.LoadImage(bytes);

            return texture;
        }

        public PaintJob GetPaintJobByMaterial(Material mat)
        {
            foreach(var paintJob in PaintJobs)
            {
                if (paintJob.Material.name == mat.name)
                    return paintJob;
            }

            return null;
        }

        public PaintJob GetPaintJobByMaterialName(string name)
        {
            foreach (var paintJob in PaintJobs)
            {
                if (paintJob.Material.name == name)
                    return paintJob;
            }

            return null;
        }
    }

    [Serializable]
    public class PaintJob
    {
        [SerializeField] public string Name;
        [SerializeField] public string Description;
        [SerializeField] public string Author;
        [SerializeField] public int Price;

        [SerializeField] public bool UsesPreviewIcon;

        [NonSerialized] public Texture2D Texture;
        [NonSerialized] public Texture2D PreviewIcon;
        [NonSerialized] public string FileName;
        [NonSerialized] public Material Material;

        public PaintJob(string name, string description, string author, int price, bool usesPreviewIcon)
        {
            Name = name;
            Description = description;
            Author = author;
            Price = price;
            UsesPreviewIcon = usesPreviewIcon;
        }
    }
}
