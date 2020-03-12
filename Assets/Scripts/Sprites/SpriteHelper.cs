using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.World;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Scripts.Sprites
{
    /// <summary>
    /// Loads Sprites from Asset Bundles (AddressableAssets), or the persistentData folder for custom user textures
    /// </summary>
    public class SpriteHelper : MonoBehaviour
    {
        /// <summary>
        /// Dictionary of Sprites
        /// </summary>
        public static Dictionary<string, Sprite> Sprites;

        /// <summary>
        /// Stores the loaded custom textures, sorted by their Asset Addressable Address (See spriteSheets array)
        /// </summary>
        private Dictionary<string, Texture2D> customTextures;
        
        /// <summary>
        /// Status of custom texture loading threads
        /// <key type="string">SpriteSheet Address</key>
        /// <value type="bool">True when thread is finished, and the texture has been loaded into customTextures</value>
        /// </summary>
        private Dictionary<string, bool> activeCustomThreads;

        /// <summary>
        /// Amount of SpriteSheets still being loaded and processed
        /// </summary>
        public int ActiveThreads;

        /// <summary>
        /// List of Addressable Addresses for all sprite sheets.
        /// 
        /// </summary>
        public readonly string[] SpriteSheets =
        {
            "spritesheet_terrain",
            "spritesheet_villages"
        };

        /// <summary>
        /// Loads sprites from asset bundles. Overwrites atlas with custom textures if they exist in persistentData
        /// </summary>
        public void LoadSprites()
        {
            customTextures = new Dictionary<string, Texture2D>(SpriteSheets.Length);
            activeCustomThreads = new Dictionary<string, bool>(SpriteSheets.Length);
            Sprites = new Dictionary<string, Sprite>();
            string userDataPath = Application.persistentDataPath;
            

            //Load textures. If custom textures exists, load those as well
            foreach (string spriteSheet in SpriteSheets)
            {
                ActiveThreads++;
                //Pulls the folder and file name from the asset address
                // ReSharper disable once StringIndexOfIsCultureSpecific.1
                int delimiter = spriteSheet.IndexOf("_");
                string folder = "/" + spriteSheet.Substring(0, delimiter);
                string customFileName = "/" + spriteSheet.Substring(delimiter+1) + ".png";
                string totalPath = userDataPath + folder + customFileName;

                if (File.Exists(totalPath))
                {
                    activeCustomThreads.Add(spriteSheet, false);
                    StartCoroutine(LoadCustomTexture(totalPath, spriteSheet));
                }
                Addressables.LoadAssetAsync<IList<Sprite>>(spriteSheet).Completed += delegate (AsyncOperationHandle<IList<Sprite>> sheet) { SpriteSheetLoaded(sheet, spriteSheet); };
            }
        }

        /// <summary>
        /// Loads custom textures from persistentData
        /// </summary>
        /// <param name="path">Full system file path to the texture</param>
        /// <param name="address">The asset bundle address this texture will overwrite</param>
        /// <returns></returns>
        IEnumerator<UnityWebRequestAsyncOperation> LoadCustomTexture(string path, string address)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file:///" + path))
            {
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    // Get custom texture and add it to our dictionary of custom textures
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    texture.filterMode = FilterMode.Point;
                    customTextures.Add(address,texture);
                }
            }
            //This thread is done
            activeCustomThreads[address] = true;
        }

        /// <summary>
        /// Called by Addressables.LoadAssetAsync().Completed once the asset bundle is loaded
        /// </summary>
        /// <param name="spritesheet">The output spritesheet from LoadSprites()</param>
        /// <param name="address">The asset bundle address</param>
        private void SpriteSheetLoaded(AsyncOperationHandle<IList<Sprite>> spritesheet, string address)
        {
            StartCoroutine(CreateSpriteSheet(spritesheet.Result, address));
        }

        /// <summary>
        /// Creates a spritesheet, using the asset bundle as a base and overwriting with custom textures, if applicable
        /// </summary>
        /// <param name="spritesheet">The spritesheet to create, provided by Addressables.LoadAssetAsync()</param>
        /// <param name="address">The asset bundle address</param>
        /// <returns></returns>
        private IEnumerator<Coroutine> CreateSpriteSheet(IList<Sprite> spritesheet, string address)
        {
            //If there are custom textures related to this asset bundle, wait until they are loaded
            if (activeCustomThreads.ContainsKey(address))
            {
                while (!activeCustomThreads[address]) yield return null;
            }

            //Create our sprite reference dictionary, containing ALL sprites used in the game
            foreach (Sprite sprite in spritesheet)
            {
                Sprite spriteToAdd = sprite;

                //If there is a custom texture to use, replace the asset bundle sprite with a sprite created from the custom texture.
                if (customTextures.ContainsKey(address))
                {
                    spriteToAdd = Sprite.Create(customTextures[address],sprite.rect,Vector2.one * 0.5f,sprite.pixelsPerUnit,0,SpriteMeshType.FullRect);
                }

                //Add sprite to dictionary
                Sprites.Add(sprite.name.ToLower(), spriteToAdd);
            }
            
            //This thread is done
            ActiveThreads--;
        }

        public Sprite GetTerrainSprite(HeightType heightType)
        {
            string heightString = heightType.ToString().ToLower();
            if (!Sprites.ContainsKey(heightString))
            {
                throw new ArgumentNullException(nameof(heightType));
            }
            return Sprites[heightString];
        }

        public Sprite GetVillageSprite()
        {
            return Sprites["villages_165"];
        }

    }
}
