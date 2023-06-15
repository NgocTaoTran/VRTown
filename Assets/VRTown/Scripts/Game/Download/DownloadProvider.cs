using Cysharp.Threading.Tasks;
using GLTFast.Loading;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using VRTown.Model;
using VRTown.Service;
using Zenject;

namespace VRTown.Provider
{
    public class DownloadProvider : GLTFast.Loading.IDownloadProvider
    {
        static UrlTransformAll urlTransformAll = new UrlTransformAll();
        static Dictionary<string, ZipTextureDownload> textures = new Dictionary<string, ZipTextureDownload>();
        IBundleLoader _zipLoader;

        public DownloadProvider(IBundleLoader zipLoader)
        {
            _zipLoader = zipLoader;
        }

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

                    // Debug.LogError("[DNguyen] Download Zip: " + zipUrlOrigin);
                    var zipArchive = await _zipLoader.LoadAssetAsync<ZipArchive>(zipUrlOrigin);
                    // Debug.LogError("[Provider] Download Complete: " + zipUrlOrigin + " with value: " + relativeUrl);
                    return new ZipDownload(zipArchive, relativeUrl);
                }
            }
            catch (System.Exception e)
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

                    var zipArchive = await _zipLoader.LoadAssetAsync<ZipArchive>(zipUrl);
                    return textures[urlString] = new ZipTextureDownload(zipArchive, relativeUrl);
                }
                else
                {
                    var req = new AwaitableTextureDownload(url, nonReadable);
                    Debug.LogError("[Provider] Request URI: " + url.ToString());
                    await req.WaitAsync();
                    return req;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Provider] Request Texture, Error, url={url.ToString()}, Message={e.ToString()}");
            }

            throw new System.NotImplementedException();
        }
    }
}
