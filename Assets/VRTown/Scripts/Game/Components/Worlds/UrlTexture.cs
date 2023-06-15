using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
namespace VRTown.Game
{
public class UrlTexture : MonoBehaviour
{
    [SerializeField]
    string URL = "";

    IEnumerator Start()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(URL);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            var temp = ((DownloadHandlerTexture)www.downloadHandler).texture;
            GetComponent<SpriteRenderer>().sprite = Sprite.Create(temp, new Rect(0.0f, 0.0f, temp.width, temp.height), new Vector2(0.5f, 0.5f), 100.0f);

        }
    }
}
}
