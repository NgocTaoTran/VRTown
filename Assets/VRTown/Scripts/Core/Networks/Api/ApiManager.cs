using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using VRTown.Service;
using Proyecto26;
using VRTown.Model;
using System.Threading.Tasks;
using Zenject;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;

namespace VRTown.Network
{
    public partial class ApiManager : BaseApiManager
    {
        public override async UniTask<Dictionary<string, Dictionary<string, System.Collections.Generic.Dictionary<string, CharacterItem[]>>>> GetCharacterConfigs()
        {
            var environment = EnvironmentManager.GetEnvironment();

            var webRequest = UnityWebRequest.Get(new System.Uri(environment.ApiUrl + "/lands/CustomizeAssets3/character.json"));
            await webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                _logger.Error(webRequest.error);
                return default;
            }
            else
            {
                // Show results as text
                var data = webRequest.downloadHandler.data;
                var str = Encoding.UTF8.GetString(data);

                var raw = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, System.Collections.Generic.Dictionary<string, CharacterItem[]>>>(str);
                var resultConfig = new Dictionary<string, Dictionary<string, System.Collections.Generic.Dictionary<string, CharacterItem[]>>>();

                foreach (var genderEntry in raw)
                {
                    var gender = genderEntry.Key;
                    resultConfig.Add(gender, new Dictionary<string, System.Collections.Generic.Dictionary<string, CharacterItem[]>>());

                    foreach (var entry in genderEntry.Value)
                    {
                        var key = entry.Key;
                        var pair = key.Split('.');
                        var category = pair[0];
                        var type = pair[1];
                        Dictionary<string, CharacterItem[]> Category;
                        if (resultConfig[gender].ContainsKey(category))
                        {
                            Category = resultConfig[gender][category];
                        }
                        else
                        {
                            Category = new Dictionary<string, CharacterItem[]>();
                            resultConfig[gender].Add(category, Category);
                        }

                        foreach (var subEntry in entry.Value)
                        {
                            subEntry.category = category;
                            subEntry.type = type;
                        }

                        Category[type] = entry.Value;
                    }
                }

                return resultConfig;
            };
        }

        public override async UniTask<(AgoraData, System.Exception)> GetAgoraConfiguration()
        {
            var environment = EnvironmentManager.GetEnvironment();

            var webRequest = UnityWebRequest.Get(new System.Uri(environment.ApiUrl + "/configs/agora.json"));
            await webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                _logger.Error(webRequest.error);
                return (null, new System.Exception(webRequest.error));
            }
            else
            {
                return (Newtonsoft.Json.JsonConvert.DeserializeObject<AgoraData>(Encoding.UTF8.GetString(webRequest.downloadHandler.data)), null);
            };
        }

        public override async UniTask<(string, System.Exception)> LoadPropDatas()
        {
            var environment = EnvironmentManager.GetEnvironment();

            var webRequest = UnityWebRequest.Get(new System.Uri(environment.ApiUrl + "/props/character_props.json"));
            await webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                _logger.Error(webRequest.error);
                return ("", new System.Exception(webRequest.error));
            }
            else
            {
                return (Encoding.UTF8.GetString(webRequest.downloadHandler.data), null);
            };
        }

        public override async UniTask<(TResponse, System.Exception)> GetNonce<TResponse>(string address)
        {
            var currentRequest = new RequestHelper
            {
                Uri = EnvironmentManager.GetEnvironment().ApiUrl + ApiUrl.GetNonceUrl,
                Params = new ApiRequest.GetNonceParam { address = address }.ToDictionary(),
                Headers = new Dictionary<string, string> { { "Accept", "application/json" } }
            };
            var promise = RestClient.Get<TResponse>(currentRequest);
            return await promise.GetPromiseAwaiter().GetResult();
        }


        public override async UniTask<(Sprite, System.Exception)> LoadSpriteAsync(string spritePath)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(spritePath);
            await request.SendWebRequest();

            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    return (null, new System.Exception($"{request.result.ToString()}, Message: {request.error}"));

                case UnityWebRequest.Result.Success:
                    {
                        var tex = DownloadHandlerTexture.GetContent(request);
                        var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                        return (sprite, null);
                    }

                default:
                    {
                        return (null, new System.Exception($"Load Sprite: In Progress"));
                    }
            }
        }
    }
}