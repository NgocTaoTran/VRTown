using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace VRTown.Service
{
    public partial class LocalizationManager
    {
        public event Action LocalizationChanged = () => { };
        private readonly Dictionary<SystemLanguage, Dictionary<string, object>> Dictionary = new Dictionary<SystemLanguage, Dictionary<string, object>>();
        private SystemLanguage _language = SystemLanguage.English;
        private SystemLanguage _defaultLanguage = SystemLanguage.English;

        private LanguageCode[] _languageCodes = new LanguageCode[2] {
            new LanguageCode("English","en"),
            new LanguageCode("Vietnamese","vn")
        };

        public SystemLanguage Language
        {
            get { return _language; }
            set
            {
                _language = value;
                PlayerPrefs.SetString("language", _language.ToString());
                LocalizationChanged();
            }
        }

        public UniTask Initialize()
        {
            Read();
            return default;
        }

        public void Read(string path = "Localization")
        {
            if (Dictionary.Count > 0) return;

            var textAssets = Resources.LoadAll<TextAsset>(path);

            foreach (var textAsset in textAssets)
            {
                var text = ReplaceMarkers(textAsset.text);
                var matches = Regex.Matches(text, "\"[\\s\\S]+?\"");

                foreach (Match match in matches)
                {
                    text = text.Replace(match.Value, match.Value.Replace("\"", null).Replace(",", "[comma]").Replace("\n", "[newline]"));
                }

                var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var languages = lines[0].Split(',').Select(i => i.Trim()).ToList();

                for (var i = 1; i < languages.Count; i++)
                {
                    SystemLanguage language = SystemLanguage.Unknown;
                    Enum.TryParse<SystemLanguage>(languages[i], out language);

                    if (!Dictionary.ContainsKey(language))
                    {
                        Dictionary.Add(language, new Dictionary<string, object>());
                    }
                }

                for (var i = 1; i < lines.Length; i++)
                {
                    var columns = lines[i].Split(',').Select(j => j.Trim()).Select(j => j.Replace("[comma]", ",").Replace("[newline]", "\n")).ToList();
                    var key = columns[0];
                    for (var j = 1; j < languages.Count; j++)
                    {
                        SystemLanguage language = SystemLanguage.Unknown;
                        Enum.TryParse<SystemLanguage>(languages[j], out language);
                        Dictionary[language].Add(key, columns[j]);
                    }
                }
            }

            var spriteAssets = Resources.LoadAll<Sprite>(path);

            foreach (var spriteAsset in spriteAssets)
            {
                string namex = spriteAsset.name;
                string[] texts = namex.Split("_");
                string language = _languageCodes.First(x => x.Code == texts[1]).Language;
                SystemLanguage eLanguage = SystemLanguage.Unknown;
                Enum.TryParse<SystemLanguage>(language, out eLanguage);
                if (!Dictionary.ContainsKey(eLanguage))
                {
                    Dictionary.Add(eLanguage, new Dictionary<string, object>() { });
                }
                Dictionary[eLanguage].Add(texts[0], spriteAsset);
            }
            //AutoLanguage();
        }

        /// <summary>
        /// Get localized value by localization key.
        /// </summary>
        public T Localize<T>(string localizationKey)
        {
            string LanguageKey = PlayerPrefs.GetString("language", _defaultLanguage.ToString());
            SystemLanguage eLanguage = SystemLanguage.Unknown;
            Enum.TryParse<SystemLanguage>(LanguageKey, out eLanguage);

            if (Dictionary.Count == 0)
            {
                Read();
            }

            if (!Dictionary.ContainsKey(eLanguage) || !Dictionary[eLanguage].ContainsKey(localizationKey))
            {
                return (T)Convert.ChangeType(null, typeof(T));
            }
            return (T)Convert.ChangeType(Dictionary[eLanguage][localizationKey], typeof(T));
        }

        /// <summary>
        /// Get localized value by localization key.
        /// </summary>

        private string ReplaceMarkers(string text)
        {
            return text.Replace("[Newline]", "\n");
        }

        internal static object LocalizationChange()
        {
            throw new NotImplementedException();
        }

        public struct LanguageCode
        {
            public string Language;
            public string Code;

            public LanguageCode(string language, string code)
            {
                Language = language;
                Code = code;
            }
        }
    }
}