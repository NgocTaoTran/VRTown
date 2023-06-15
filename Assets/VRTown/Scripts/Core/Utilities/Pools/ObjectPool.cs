using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Utilities
{
    public static class Extensions
    {
        public static T[] SubArray<T>(this T[] array, int offset, int length)
        {
            T[] result = new T[length];
            System.Array.Copy(array, offset, result, 0, length);
            return result;
        }
    }

    public class ObjectPool<T> where T : Component
    {
        Transform _rootParent;
        T _sample;
        List<T> _poolElements = new List<T>();

        public ObjectPool(T element)
        {
            _sample = element;
            // _poolElements.Add(_sample);
            _rootParent = element.transform.parent;
        }

        public T Get()
        {
            if (_poolElements.Count == 0)
            {
                var newElement = Object.Instantiate(_sample, _sample.transform.parent);
                newElement.gameObject.SetActive(true);
                newElement.gameObject.name = _sample.gameObject.name;
                return newElement;
            }

            var element = _poolElements[0];
            element.gameObject.SetActive(true);
            _poolElements.RemoveAt(0);
            return element;
        }

        public void Store(T element)
        {
            element.transform.SetParent(_rootParent);
            element.gameObject.SetActive(false);
            element.transform.rotation = Quaternion.identity;
            element.transform.localPosition = Vector3.zero;
            _poolElements.Add(element);
        }
    }
}