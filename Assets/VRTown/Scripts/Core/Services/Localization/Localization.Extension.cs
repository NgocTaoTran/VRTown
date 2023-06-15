using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace VRTown.Service
{
    public partial class LocalizationManager
    {
        // public T Localize<T>(this T target,  string localizationKey)
        // {
        //     string LanguageKey = PlayerPrefs.GetString("language", _defaultLanguage);
        //     LanguageKey = String.IsNullOrEmpty(LanguageKey) ? "English" : LanguageKey;
        //     if (Dictionary.Count == 0)
        //     {
        //         Read();
        //     }

        //     if (!Dictionary.ContainsKey(LanguageKey) || !Dictionary[LanguageKey].ContainsKey(localizationKey))
        //     {
        //         return (T)Convert.ChangeType(null, typeof(T));
        //     }
        //     return (T)Convert.ChangeType(Dictionary[LanguageKey][localizationKey], typeof(T));
        // }
    }
}