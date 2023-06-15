using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Utilities;

namespace VRTown.Game
{
    public class EntityFerris : MonoBehaviour
    {
        [SerializeField] Transform _main;
        [SerializeField] int _numberBox;
        [SerializeField] float _distance;
        [SerializeField] float _beginAngle;
        [SerializeField] Transform _prefabBox;

        ObjectPool<Transform> _poolBoxs;

        List<Transform> _boxes = new List<Transform>();

        bool _isStart = false;
        float _beginRotate = 0f;
        float _angle = 0f;

        void Start()
        {
            _poolBoxs = new ObjectPool<Transform>(_prefabBox);
            _beginRotate = 0f;
            _angle = 360f / _numberBox;
            for (int i = 0; i < _numberBox; i++)
            {
                var nextPos = Quaternion.AngleAxis(_angle * i + _beginAngle, Vector3.forward) * Vector3.right * _distance;
                var tfBox = _poolBoxs.Get();
                tfBox.transform.localPosition = nextPos;
                _boxes.Add(tfBox);
            }

            Rotate();
            _isStart = true;
        }

        void Update()
        {
            if (_isStart)
            {
                Rotate();
            }
        }

        void Rotate()
        {
            _beginRotate += 0.5f;
            _main.transform.rotation = Quaternion.AngleAxis(_beginRotate, _main.transform.right);
            
            for (int i = 0; i < _numberBox; i++)
            {
                var nextPos = Quaternion.AngleAxis(_angle * i + _beginRotate, Vector3.forward) * Vector3.right * _distance;
                _boxes[i].transform.localPosition = nextPos;
            }
        }
    }
}