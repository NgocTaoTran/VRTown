using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Utilities
{
    public class PriorityList<T> : IEnumerable<T> where T : IComparable<T>
    {
        private List<T> data;

        public PriorityList()
        {
            data = new List<T>();
        }

        public int Count { get { return data.Count; } }

        public T this[int index]
        {
            get { return data[index]; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Add(T newItem)
        {
            if (data.Count == 0 || newItem.CompareTo(data[0]) <= 0)
            {
                data.Insert(0, newItem);
                return 0;
            }

            if (newItem.CompareTo(data[data.Count - 1]) >= 0)
            {
                data.Insert(data.Count, newItem);
                return data.Count - 1;
            }

            int i1 = 0;
            int i2 = data.Count - 1;

            while (i1 < i2 - 1)
            {
                int i = (i1 + i2) / 2;
                if (newItem.CompareTo(data[i]) > 0)
                {
                    i1 = i;
                }
                else
                {
                    i2 = i;
                }
            }

            data.Insert(i1 + 1, newItem);
            return i1 + 1;
        }

        public void Clears()
        {
            data.Clear();
        }
    }
}