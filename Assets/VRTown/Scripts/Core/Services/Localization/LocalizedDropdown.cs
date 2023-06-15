// using UnityEngine;
// using UnityEngine.UI;

// namespace Assets.SimpleLocalization
// {
// 	/// <summary>
// 	/// Localize dropdown component.
// 	/// </summary>
//     [RequireComponent(typeof(Dropdown))]
//     public class LocalizedDropdown : MonoBehaviour
//     {
//         public string[] LocalizationKeys;

//         public void Start()
//         {
//             Localize();
//             Localization.LocalizationChanged += Localize;
//         }

//         public void OnDestroy()
//         {
//             Localization.LocalizationChanged -= Localize;
//         }

//         private void Localize()
//         {
// 	        var dropdown = GetComponent<Dropdown>();

// 			for (var i = 0; i < LocalizationKeys.Length; i++)
// 	        {
// 		        dropdown.options[i].text = Localization.Localize<string>(LocalizationKeys[i]);
// 	        }

// 	        if (dropdown.value < LocalizationKeys.Length)
// 	        {
// 		        dropdown.captionText.text = Localization.Localize<string>(LocalizationKeys[dropdown.value]);
// 	        }
//         }
//     }
// }