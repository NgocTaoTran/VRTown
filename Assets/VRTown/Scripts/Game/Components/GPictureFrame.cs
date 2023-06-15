using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace VRTown.Game
{
    public class GPictureFrame : MonoBehaviour
    {
        [Header("Reference Data")]
        public SunflotImageSO Data;

        [Header("Elements")]
        [SerializeField] TMP_Text _textTitle;
        [SerializeField] TMP_Text _textDescription;
        [SerializeField] SpriteRenderer _spriteRender;

        bool _isLoading = false;
        Sprite _spriteImage = null;

        void Start()
        {
            _textTitle.gameObject.SetActive(!string.IsNullOrEmpty(Data.Title));
            _textDescription.gameObject.SetActive(!string.IsNullOrEmpty(Data.Message));
            _textTitle.text = Data.Title;
            _textDescription.text = Data.Message;

            StartCoroutine(LoadImage(Data.URL));
        }

        public void OnAfterDeserialize()
        {
        }

        public void OnBeforeSerialize()
        {
            Debug.Log("Enter Here");
            if (Data != null)
            {
                _textTitle.text = Data.Title;
                _textDescription.text = Data.Message;
            }
        }

        IEnumerator LoadImage(string url)
        {
            _isLoading = true;
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                var temp = ((DownloadHandlerTexture)www.downloadHandler).texture;
                _spriteRender.sprite = Sprite.Create(temp, new Rect(0.0f, 0.0f, temp.width, temp.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
        }
    }
}