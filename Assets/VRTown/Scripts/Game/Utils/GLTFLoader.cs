using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GLTFast;
using GLTFast.Logging;
using VRTown.Provider;
using VRTown.Model;
using System.IO.Compression;
using System.Threading.Tasks;
using GLTFast.Loading;
using UnityEngine.Networking;
using System.IO;
using System;

namespace VRTown.Game
{
    public class ZipDownloadProvider : GLTFast.Loading.IDownloadProvider
    {
        static Dictionary<string, ZipArchive> zips = new Dictionary<string, ZipArchive>();
        static Dictionary<string, ZipTextureDownload> textures = new Dictionary<string, ZipTextureDownload>();
        static UrlTransformAll urlTransformAll = new UrlTransformAll();
        public async Task<IDownload> Request(System.Uri url)
        {
            var urlString = url.ToString();
            var index = urlString.IndexOf(".zip/");
            try
            {
                if (index >= 0)
                {
                    var zipUrl = urlString.Substring(0, index + 4);
                    var zipUrlOrigin = urlTransformAll.ReverseTransform(zipUrl);
                    var relativeUrl = urlString.Substring(index + "zip/".Length + 1);
                    if (zips.ContainsKey(zipUrl))
                    {
                        //Debug.Log($"Already has zip file: {zipUrl}, {relativeUrl}");
                        return new ZipDownload(zips[zipUrl], relativeUrl);
                    }
                    else
                    {
                        //Debug.LogError($"Load zip file: {zipUrlOrigin}, {zipUrl}, {relativeUrl}");
                        var www = UnityWebRequest.Get(new System.Uri(zipUrlOrigin));
                        www.SetRequestHeader("Accept-Encoding", "gzip;q=0,deflate;q=0,sdch;q=0");
                        www.downloadHandler = new DownloadHandlerBuffer();
                        await www.SendWebRequest();
                        
                        //Debug.LogError($"Load zip file {zipUrlOrigin}, byte downloaded: {www.downloadedBytes}, result={www.result}, progress={www.downloadProgress}");
                        zips[zipUrl] = new ZipArchive(new MemoryStream(www.downloadHandler.data));
                        return new ZipDownload(zips[zipUrl], relativeUrl);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("[request] error, urlString={0}, error={1}", urlString, e.ToString());
                return null;
            }

            Debug.LogErrorFormat("========={0}", url.ToString());
            throw new System.NotImplementedException();
        }

        public async Task<ITextureDownload> RequestTexture(System.Uri url, bool nonReadable)
        {
            try
            {
                var urlString = url.ToString();
                var index = urlString.IndexOf(".zip/");
                if (index >= 0)
                {
                    var zipUrl = urlString.Substring(0, index + 4);
                    var relativeUrl = urlString.Substring(index + "zip/".Length + 1);
                    if (textures.ContainsKey(urlString)) return textures[urlString];

                    if (zips.ContainsKey(zipUrl))
                    {
                        return textures[urlString] = new ZipTextureDownload(zips[zipUrl], relativeUrl);
                    }
                    else
                    {
                        var www = UnityWebRequest.Get(new System.Uri(zipUrl));
                        await www.SendWebRequest();
                        zips[zipUrl] = new ZipArchive(new MemoryStream(www.downloadHandler.data));
                        return textures[urlString] = new ZipTextureDownload(zips[zipUrl], relativeUrl);
                    }
                }
                else
                {
                    var req = new AwaitableTextureDownload(url, nonReadable);
                    await req.WaitAsync();
                    return req;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ZipDownloadProvider][requestTexture] error, url={url.ToString()}, error={e.ToString()}");
            }


            throw new System.NotImplementedException();
        }
    }

    public class GLTFLoader : MonoBehaviour
    {
        [SerializeField] private string url;

        void Start()
        {
            Load();
        }

        public async void Load()
        {
            var importer = new GLTFast.GltfImport(logger: new ConsoleLogger(), downloadProvider: new ZipDownloadProvider());
            var settings = new ImportSettings() { GenerateMipMaps = false, AnisotropicFilterLevel = 3, NodeNameMethod = GLTFast.NameImportMethod.OriginalUnique };
            var result = await importer.Load(new System.Uri(url), settings);
            if (result)
            {
                var tfAsset = new GameObject(string.Format($"Preload_Demo"));
                tfAsset.transform.SetParent(this.transform);
                try
                {
                    if (await importer.InstantiateSceneAsync(tfAsset.transform))
                    {
                        // tfAsset.SetActive(false);
                        // _poolModels.Add(asset.Key, new ObjectPool<Transform>(tfAsset.transform));
                        // _cacheAssets.Remove(asset.Key);
                    }
                    else
                    {
                        Debug.LogError($"[ModelController] Instantiate Model failed!");
                    }
                }
                catch
                {
                    Debug.LogError($"[ModelController] Exception, [tfAsset = {tfAsset.transform == null}] failed!");
                }
            }
            else
            {
                Debug.LogError($"[ModelController] Load Model, failed!");
            }
        }
    }
}