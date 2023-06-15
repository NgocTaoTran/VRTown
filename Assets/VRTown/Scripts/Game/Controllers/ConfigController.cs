using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Network;
using Zenject;
using VRTown.Model;
using VRTown.Utilities;
using VRTown.Service;
using Cysharp.Threading.Tasks;
using GLTFast;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace VRTown.Game
{
    public partial class ConfigController
    {
        #region Zenject_Binding
        [Inject] VRTown.Service.ILogger _logger;
        [Inject] readonly IApiManager _apiManager;
        #endregion Zenject_Binding

        #region Properties
        public Dictionary<string, Dictionary<string, System.Collections.Generic.Dictionary<string, CharacterItem[]>>> CharacterConfig { get { return _characterConfig; } }
        public List<PropData> PropDatas { get { return _propDatas; } }
        public List<PropType> PropNoColor { get { return _propNoColors; } }
        public List<PropType> DefaultProps { get { return _defaultProps; } }
        #endregion Properties

        #region Local
        Dictionary<string, Dictionary<string, System.Collections.Generic.Dictionary<string, CharacterItem[]>>> _characterConfig;
        List<PropData> _propDatas;
        List<PropType> _propNoColors = new List<PropType>();
        List<PropType> _defaultProps = new List<PropType>();
        #endregion Local

        #region Object_Pool
        Dictionary<string, Sprite> _cacheThumbnails = new Dictionary<string, Sprite>();
        List<string> _thumbnailLoadings = new List<string>();
        private Dictionary<string, PartMesh> _cacheParts = new Dictionary<string, PartMesh>();
        #endregion Object_Pool

        public async UniTask Initialize()
        {
            _logger.Log("[ConfigController] Initialize");
            await LoadConfig();
            GHelper.Config = this;
        }

        public async UniTask LoadConfig()
        {
            await UniTask.WhenAll(LoadCharacterConfig(), LoadPropData());
        }

        async UniTask LoadCharacterConfig()
        {
            _characterConfig = await _apiManager.GetCharacterConfigs();

            _propNoColors = new List<PropType>() { PropType.shirt, PropType.pants, PropType.glasses,
                PropType.hat, PropType.shoes, PropType.bag };

            _defaultProps = new List<PropType>() { PropType.hair, PropType.shirt, PropType.pants, PropType.shoes };
        }

        async UniTask LoadPropData()
        {
            var (response, ex) = await _apiManager.LoadPropDatas();
            if (ex != null)
            {
                _logger.Error("[Config] Load Prop Data, error = " + ex.Message);
            }
            else
            {
                _propDatas = JsonConvert.DeserializeObject<List<PropData>>(response);
            }
        }

        // public CharacterItem GetCharacterConfig(string gender, string cateItem, string typeItem, int id)
        // {
        //     PropCategory cate = PropCategory.top;
        //     System.Enum.TryParse<PropCategory>(cateItem, out cate);

        //     PropType type = PropType.top;
        //     System.Enum.TryParse<PropType>(typeItem, out type);

        //     CharacterItem item = _propDatas.Find(val => val.category == "")
        // }

        public async UniTask<Sprite> LoadTextureAsync(string path)
        {
            var thumbnailPath = path.Replace(".glb", "_thumb.png");
            var keyName = System.IO.Path.GetFileName(path);

            if (_cacheThumbnails.ContainsKey(keyName))
            {
                return _cacheThumbnails[keyName];
            }
            else if (_thumbnailLoadings.Contains(keyName))
            {
                while (!_cacheThumbnails.ContainsKey(keyName))
                    await UniTask.Yield();
                return _cacheThumbnails[keyName];
            }
            else
            {
                _thumbnailLoadings.Add(keyName);
                var urlSprite = $"https://s3.ap-southeast-1.amazonaws.com/cdn.vrtown.io/lands/CustomizeAssets3/{thumbnailPath}";

                var (response, ex) = await _apiManager.LoadSpriteAsync(urlSprite);
                if (ex != null)
                {
                    _cacheThumbnails.Add(keyName, null);
                    _logger.Error("[API] Load Texture, Error = " + ex.Message);
                }
                else
                {
                    _cacheThumbnails.Add(keyName, response);
                }
                return _cacheThumbnails[keyName];
            }
        }

        public PropData GetPropData(GenderType gender, PropType type, int id)
        {
            foreach (var propData in _propDatas)
            {
                if ((propData.gender == gender || propData.gender == GenderType.none) && propData.type == type && propData.id == id)
                    return propData;
            }
            return null;
        }

        public List<PropData> GetProps(GenderType gender, PropType type)
        {
            List<PropData> outputs = new List<PropData>();
            foreach (var propData in _propDatas)
            {
                if ((propData.gender == gender || propData.gender == GenderType.none) && propData.type == type)
                    outputs.Add(propData);
            }
            return outputs;
        }

        public void ParseCharacterProps()
        {
            var genders = new List<string>() { "male", "female" };
            var categories = new List<PropCategory>() { PropCategory.top, PropCategory.head, PropCategory.bottom, PropCategory.shoes, PropCategory.accessories };
            var types = new List<PropType>() { PropType.beard, PropType.hair, PropType.top, PropType.bottom, PropType.shoes, PropType.glasses, PropType.hat, PropType.bag };
            var both = new List<PropType>() { PropType.hat, PropType.bag, PropType.shoes, PropType.glasses };

            var propItems = new List<PropData>();
            foreach (var gender in genders)
            {
                GenderType genderType = GenderType.none;
                System.Enum.TryParse<GenderType>(gender, out genderType);
                if (_characterConfig.ContainsKey(gender))
                {
                    var cateData = _characterConfig[gender];
                    foreach (var cate in categories)
                    {
                        if (cateData.ContainsKey(cate.ToString()))
                        {
                            var typeData = cateData[cate.ToString()];
                            foreach (var type in types)
                            {
                                if (typeData.ContainsKey(type.ToString()))
                                {
                                    var items = typeData[type.ToString()];
                                    foreach (var item in items)
                                    {
                                        bool isGenderBoth = false;
                                        if (both.Contains(type))
                                        {
                                            isGenderBoth = true;
                                            if (propItems.Find(val => val.category == cate && val.type == type && val.id == item.id) != null)
                                                continue;
                                        }
                                        var propData = new PropData();
                                        propData.id = item.id;
                                        propData.name = item.name;
                                        propData.path = item.path;
                                        propData.gender = isGenderBoth ? GenderType.none : genderType;
                                        propData.category = cate;
                                        propData.type = type;
                                        propItems.Add(propData);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            System.IO.File.WriteAllText(Application.dataPath + "/character_props.json", JsonConvert.SerializeObject(propItems));
        }
    }
}