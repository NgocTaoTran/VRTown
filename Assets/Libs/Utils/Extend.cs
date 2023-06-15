using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extend
{
    public static Vector3 position(this Component comp) => comp.transform.position;
    public static Vector3 localPosition(this Component comp) => comp.transform.localPosition;
    public static void setActive<T>(this T p, bool pActive) where  T: Component
    {
        p.transform.gameObject.SetActive(pActive);
    }
    public static bool activeSelf<T>(this T p) where  T: Component
    {
        return p.transform.gameObject.activeSelf;
    }
    public static T random<T>(this List<T> list)
    {
        var c = list.Count;
        if (c > 1)
        {
            return list[Random.Range(0, c)];
        }
        return list[0];
    }
    public static T random<T>(this T[] list)
    {
        var c = list.Length;
        if (c > 1)
        {
            return list[Random.Range(0, c)];
        }
        return list[0];
    }

    public static int randomChance(this int[] ratio)
    {
        var total = 0;
        foreach (var i in ratio)
        {
            total += i;
        }
        var r = Random.Range(1, total + 1);
        var t = 0;
        for (var i = 0; i < ratio.Length; i++)
        {
            if (r > 0 && r <= ratio[i] + t)
            {
                return i;
            }
            t += ratio[i];
        }
        return -1;
    }

    public static int[] clone(this int[] arr)
    {
        var a = new int[arr.Length];
        for (var i = 0; i < arr.Length; i++)
        {
            a[i] = arr[i];
        }
        return a;
    }

    public static T Find<T>(this T[] array, System.Predicate<T> predicate)
    {
        foreach (var t in array)
        {
            if (predicate(t))
            {
                return t;
            }
        }
        return default(T);
    }
    public static Vector2 WorldToCanvas(this Canvas canvas,
                                            Vector3 world_position,
                                            Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        var viewport_position = camera.WorldToViewportPoint(world_position);
        var canvas_rect = canvas.GetComponent<RectTransform>();

        return new Vector2((viewport_position.x * canvas_rect.sizeDelta.x) - (canvas_rect.sizeDelta.x * 0.5f),
                           (viewport_position.y * canvas_rect.sizeDelta.y) - (canvas_rect.sizeDelta.y * 0.5f));
    }
    public static Vector2 WorldToCanvasPosition(this RectTransform canvas, Vector3 position, Camera camera = null)
    {
        camera = camera ? camera : Camera.main;
        var canvasRect = canvas;
        Vector2 ViewportPosition = camera.WorldToViewportPoint(position);
        Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
            ((ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));
        return WorldObject_ScreenPosition;
    }
    public static T obtain<T>(this List<T> pool, Transform parent = null, string name = null) where T : Component
    {
        var firstItem = pool[0];
        var isPrefab = firstItem.transform.parent == null;
        var startIdx = isPrefab ? 1 : 0;

        var parentLayer = parent ? parent : pool[0].transform.parent;
        if (!parentLayer)
        {
           
        }
        for (int i = startIdx; i < pool.Count; i++)
        {
            var t = pool[i];
            if (!t.gameObject.activeSelf)
            {
                t.transform.SetParent(parentLayer);
                t.transform.localPosition = Vector3.zero;
                t.gameObject.SetActive(true);
                return pool[i];
            }
        }

        var temp = UnityEngine.Object.Instantiate(firstItem, parentLayer);
        temp.name = name == null ? string.Format("{0}_{1}", firstItem.name, (pool.Count + 1)) : name;
        temp.transform.localPosition = Vector3.zero;      
        pool.Add(temp);
        temp.gameObject.SetActive(true);
        return temp;
    }
    public static void freeAll<T>(this List<T> pool) where T : Component
    {
        foreach(var t in pool)
        {
            t.gameObject.SetActive(false);
        }
    }
    public static int[] ToArrayFormat(this string obj, char seperate = ',')
    {
        if (obj.Length > 0)
        {
            return obj.Split(seperate).Select(int.Parse).ToArray();
        }
        else
        {
            return null;
        }
    }
    
    public static void setLayerRecursively(this GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        var trans = obj.transform;
        var count = trans.childCount;
        for(var i = 0; i < count; i++)
        {
            setLayerRecursively(trans.GetChild(i).gameObject, newLayer);
        }
    }

    private static Transform _findInChildren(Transform trans, string name)
    {
        if(trans.name == name)
            return trans;
        else
        {
            Transform found;
 
            for(int i = 0; i < trans.childCount; i++)
            {                
                found = _findInChildren(trans.GetChild(i), name);
                if(found != null)
                    return found;
            }
 
            return null;
        }
    }
 
    public static Transform FindInChildren(this Transform trans, string name)
    {
        return _findInChildren(trans, name);
    }

    public static GameObject GetCameraMain()
    {
        return GameObject.FindGameObjectWithTag("MainCamera");
    }

    public static string CalculateBetweenDates(System.DateTime start, System.DateTime end)
    {
        var diffOfDates = end - start;

        if (diffOfDates.Days == 1) return "1 day";
        if (diffOfDates.Days == 7) return "1 week";

        if (diffOfDates.Days > 7 && diffOfDates.Days % 7 == 0) return diffOfDates.Days/7 + " weeks";

        if (diffOfDates.Days > 7)
        {
            int days = diffOfDates.Days % 7;
            int weeks = (diffOfDates.Days - days) / 7;
            if (days == 1) return weeks + " weeks " + days + " day";

            return weeks + " weeks " + days + " days";
        }
            
        if (diffOfDates.Days > 1) return diffOfDates.Days + " days";

        if (diffOfDates.Hours == 1) return "1 hour";

        if (diffOfDates.Hours > 1) return diffOfDates.Hours + " hours";

        if (diffOfDates.Minutes > 1) return diffOfDates.Minutes + " minutes";

        if (diffOfDates.Minutes == 1) return diffOfDates.Minutes + "1 minute";

        if (diffOfDates.Seconds > 1) return diffOfDates.Seconds + " secconds";

        return "now";

    }

    public static string GetTime(System.DateTime dateTime)
    {
        return dateTime.Hour + ":" + dateTime.Minute;
    }
}

