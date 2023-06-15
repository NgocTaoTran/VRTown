using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace VRTown.Game.UI
{
    class MiniMapZone
    {
        public string name;
        public int zoneX;
        public int zoneY;
        public Sprite zoneSprite;
        public Texture2D tex;
        public void unload()
        {
            GameObject.Destroy(tex);
        }
    }

    public interface IMapListener
    {
        void OnTransformChanged(Vector3 position);
    }

    public class UIMiniMap : MonoBehaviour, IMapListener
    {
        [SerializeField] Image[] mapViews;
        [SerializeField] Transform playerIndicator;
        [SerializeField] TMP_Text mapCoordinates;

        private const int edgeLimit = 20;
        private bool[] directions = new bool[4];
        private Dictionary<string, MiniMapZone> loadedZones = new Dictionary<string, MiniMapZone>();
        private List<string> loadingZones = new List<string>();
        private List<string> willRemoveZones = new List<string>();
        private RectTransform[] _mapViewRts;
        private const int tileSize = 10;
        private const int totalTiles = 20;
        private const int zoneSize = tileSize * totalTiles;
        private const float miniMapScaleFactor = 1.6f;

        private bool requestUpdate;
        private Vector3 lastPlayerPosition;

        Vector3 _targetPosition;

        public void Setup()
        {
            _mapViewRts = new RectTransform[mapViews.Length];
            for (var i = 0; i < _mapViewRts.Length; i++)
            {
                _mapViewRts[i] = mapViews[i].GetComponent<RectTransform>();
                _mapViewRts[i].sizeDelta = Vector2.one * zoneSize;
                var c = mapViews[i].color;
                mapViews[i].color = new Color(c.r, c.g, c.b, 0.3f);
            }
            requestUpdate = true;
        }

        void Update()
        {
            var mainCam = Camera.main;
            if (mainCam)
            {
                var cameraPosition = mainCam.transform.position;
                var cameraStepForward = cameraPosition + mainCam.transform.forward.normalized * 10;
                var moveVector = cameraStepForward - cameraPosition;
                var dx = moveVector.x;
                var dz = moveVector.z;
                playerIndicator.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dz, dx) * Mathf.Rad2Deg - 90);
            }

            if (requestUpdate || (_targetPosition - lastPlayerPosition).magnitude >= 0.1f)
            {
                requestUpdate = false;
                lastPlayerPosition = _targetPosition;
                var x = _targetPosition.x / miniMapScaleFactor;
                var y = _targetPosition.z / miniMapScaleFactor;
                onMove(x, y);
                var CoordinatesX = Mathf.FloorToInt(_targetPosition.x / 16);
                var CoordinatesY = Mathf.FloorToInt(_targetPosition.z / 16);
                displayCoordinates(CoordinatesX, CoordinatesY);
            }
        }

        public void OnTransformChanged(Vector3 position)
        {
            _targetPosition = position;
        }

        void onMove(float playerX, float playerY)
        {
            var halfZoneSize = zoneSize / 2;
            var zX = Mathf.FloorToInt((playerX + halfZoneSize) / zoneSize);
            var zY = Mathf.FloorToInt((playerY + halfZoneSize) / zoneSize);
            var key = formatZoneKey(zX, zY);
            loadedZones.TryGetValue(key, out var zone);
            if (zone == null)
            {
                loadZone(zX, zY);
            }
            checkZoneBoundary(zX, zY, playerX, playerY);
            if (zone != null)
            {
                checkZoneBoundary(zone.zoneX, zone.zoneY, playerX, playerY);
            }
            // update map
            if (zone != null)
            {
                var mainView = mapViews[0];
                var mainRt = _mapViewRts[0];
                if (mainView.sprite != zone.zoneSprite)
                {
                    mainView.sprite = zone.zoneSprite;
                    mainView.name = zone.name;
                }
                var hh = zoneSize / 2;
                var x = (-playerX % zoneSize);
                var dx = (Mathf.FloorToInt(Mathf.Abs(x) / hh) * zoneSize);
                x -= Mathf.Sign(x) * dx;
                var y = (-playerY % zoneSize);
                var dy = (Mathf.FloorToInt(Mathf.Abs(y) / hh) * zoneSize);
                y -= Mathf.Sign(y) * dy;

                mainRt.anchoredPosition = new Vector2(x, y);
                var view01X = 0;
                var view02Y = 0;
                var view03X = 0;
                var view03Y = 0;
                if (mainRt.anchoredPosition.x > 0)
                {
                    view01X = -1;
                    view03X = -1;
                }
                else
                {
                    view01X = +1;
                    view03X = +1;
                }
                if (mainRt.anchoredPosition.y > 0)
                {
                    view02Y = -1;
                    view03Y = -1;
                }
                else
                {
                    view02Y = +1;
                    view03Y = +1;
                }

                _mapViewRts[1].anchoredPosition = mainRt.anchoredPosition + new Vector2(view01X, 0) * zoneSize;
                _mapViewRts[2].anchoredPosition = mainRt.anchoredPosition + new Vector2(0, view02Y) * zoneSize;
                _mapViewRts[3].anchoredPosition = mainRt.anchoredPosition + new Vector2(view03X, view03Y) * zoneSize;
                //
                updateMapView(mapViews[1], zone.zoneX + view01X, zone.zoneY);
                updateMapView(mapViews[2], zone.zoneX + 0, zone.zoneY + view02Y);
                updateMapView(mapViews[3], zone.zoneX + view03X, zone.zoneY + view03Y);
            }
            // unload unused map
            foreach (var zoneInfo in loadedZones)
            {
                var z = zoneInfo.Value;
                if (Mathf.Abs(z.zoneX - zX) > 2 || Mathf.Abs(z.zoneY - zY) > 2)
                {
                    z.unload();
                    willRemoveZones.Add(zoneInfo.Key);
                }
            }
            if (willRemoveZones.Count > 0)
            {
                foreach (var removeKey in willRemoveZones)
                {
                    loadedZones.Remove(removeKey);
                }
                willRemoveZones.Clear();
            }
        }

        void updateMapView(Image view, int zoneX, int zoneY)
        {
            var s = loadedZones.TryGetValue(formatZoneKey(zoneX, zoneY), out var z1);
            if (s && view.sprite != z1.zoneSprite)
            {
                view.sprite = z1.zoneSprite;
                view.name = z1.name;
            }
        }

        void loadZone(int zoneX, int zoneY)
        {
            var key = formatZoneKey(zoneX, zoneY);
            if (loadedZones.ContainsKey(key))
            {
                //onZoneLoaded(loadedZones[key]);
            }
            else
            {
                StartCoroutine(ieloadZone(key, zoneX, zoneY));
            }
        }

        IEnumerator ieloadZone(string key, int zoneX, int zoneY)
        {
            if (loadingZones.Contains(key))
            {
                yield break;
            }
            loadingZones.Add(key);
            var f = totalTiles;
            var url = $"https://atlas-server-dev.vrtown.io/v1/map.png?center={zoneX * f},{zoneY * f}&width={zoneSize}&height={zoneSize}&size={tileSize}";
            var www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();
            loadingZones.Remove(key);
            var tex = DownloadHandlerTexture.GetContent(www);
            var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            var zone = new MiniMapZone
            {
                name = key,
                zoneX = zoneX,
                zoneY = zoneY,
                zoneSprite = sprite,
                tex = tex
            };
            loadedZones.Add(key, zone);
            onZoneLoaded(zone);
            //Debug.Log($"textureLoaded {zone.name}");
        }

        void onZoneLoaded(MiniMapZone zone)
        {
            requestUpdate = true;
        }

        string formatZoneKey(int x, int y)
        {
            return $"{x}.{y}";
        }

        private void displayCoordinates(int playerX, int playerY)
        {
            mapCoordinates.text = $"{GHelper.Localization.Localize<string>("TXT_MAP")} ({playerX},{playerY})";
        }

        void checkZoneBoundary(int zX, int zY, float playerX, float playerY)
        {
            if (isPlayerReachedBound(zX, zY, playerX, playerY, directions))
            {
                if (directions[0])
                {
                    loadZone(zX - 1, zY + 0);
                }
                if (directions[1])
                {
                    loadZone(zX + 1, zY + 0);
                }
                if (directions[2])
                {
                    loadZone(zX + 0, zY - 1);
                    if (directions[0])
                    {
                        loadZone(zX - 1, zY - 1);
                    }
                    else if (directions[1])
                    {
                        loadZone(zX + 1, zY - 1);
                    }
                }
                if (directions[3])
                {
                    loadZone(zX + 0, zY + 1);
                    if (directions[0])
                    {
                        loadZone(zX - 1, zY + 1);
                    }
                    else if (directions[1])
                    {
                        loadZone(zX + 1, zY + 1);
                    }
                }
            }
        }

        bool isPlayerReachedBound(int zoneX, int zoneY, float playerX, float playerY, bool[] direction)
        {
            for (var i = 0; i < direction.Length; i++)
            {
                direction[i] = false;
            }
            var f = totalTiles;
            var globalZoneX = zoneX * f;
            var globalZoneY = zoneY * f;
            var halfZoneSize = zoneSize / 2;
            var left = globalZoneX - halfZoneSize;
            var right = globalZoneX + halfZoneSize;
            var top = globalZoneY + halfZoneSize;
            var bot = globalZoneY - halfZoneSize;
            var reach = false;
            if (playerX <= left + edgeLimit)
            {
                reach = true;
                direction[0] = true;
            }
            if (playerX >= right - edgeLimit)
            {
                reach = true;
                direction[1] = true;
            }
            if (playerY <= bot + edgeLimit)
            {
                reach = true;
                direction[2] = true;
            }
            if (playerY >= top - edgeLimit)
            {
                reach = true;
                direction[3] = true;
            }
            return reach;
        }
    }
}