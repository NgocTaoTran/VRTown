using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Game
{
    public class GAutoMover : MonoBehaviour, IMover
    {
        public Animator Animator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = GetComponentInChildren<Animator>();
                    _animIDSpeed = Animator.StringToHash("Speed");
                    _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
                }
                return _animator;
            }
        }

        public Vector3 Position
        {
            get
            {
                return transform.position;
            }
        }
        private Animator _animator;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDMotionSpeed;

        private float SpeedChangeRate = 10.0f;
        private float _animationBlend;
        private float _speed = 0f;
        private long _currentTime = 0;
        private float _step;
        private Queue<EnemyPosition> positions = new Queue<EnemyPosition>();
        private EnemyPosition _lastEnemyPosition;
        private Vector3 _endPosition;

        public void Setup()
        {
            Animator.SetFloat(_animIDSpeed, 0f);
            Animator.SetFloat(_animIDMotionSpeed, 1);
        }
        
        public void Teleport(Vector3 pos)
        {

        }

        public void Jump(int id, bool value)
        {
            throw new NotImplementedException();
        }

        public void Move(float x, float y, float z, float d, float gx = 0, float gy = 0)
        {
            var lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Vector3 last;
            if (_lastEnemyPosition == null)
            {
                last = transform.position;
            }
            else
            {
                last = new Vector3(_lastEnemyPosition.x, _lastEnemyPosition.z, _lastEnemyPosition.y);
                lastTime = Math.Max(lastTime, _lastEnemyPosition.time);
            }
            var deltaTime = 5000;
            var moveToPosition = new Vector3(x, z, y);
            float distance = Vector3.Distance(last, moveToPosition);
            _speed = distance / (deltaTime / 1000f);
            if (_speed < 2) _speed = 2;
            deltaTime = (int)Math.Round(distance / _speed * 1000);
            _lastEnemyPosition = new EnemyPosition
            {
                x = x,
                y = y,
                z = z,
                d = gx == 0 ? d : d * -1,
                gx = gx,
                gy = gy,
                time = lastTime + deltaTime,
            };

            positions.Enqueue(_lastEnemyPosition);
        }

        public void Teleport(float x, float y, float z, float d, float gx = 0, float gy = 0)
        {
            Move(x, y, z, d, gx, gy);
        }

        void Update()
        {
            // finish current move 
            if (_currentTime > 0)
            {
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if (currentTime >= _currentTime)
                {
                    if (positions.Count == 0)
                    {
                        Animator.SetFloat(_animIDSpeed, 0);
                        Animator.SetFloat(_animIDMotionSpeed, 1);

                        _speed = 0;
                    }
                    transform.position = _endPosition;
                    _currentTime = 0;
                }
            }

            // current move finish, now there is no move next
            // still wait  in case the enemy still moving
            if (_currentTime == 0 && positions.Count == 0)
            {
                if (Math.Abs(_animationBlend) < 0.001)
                {
                    // stop wait
                    _animationBlend = 0;
                }
                else
                {
                    _animationBlend = Mathf.Lerp(_animationBlend, _speed, Time.deltaTime * SpeedChangeRate / 2);
                    Animator.SetFloat(_animIDSpeed, _animationBlend);
                }
            }

            // current move finish, there is next move, process it
            if (_currentTime == 0 && positions.Count > 0)
            {
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var position = positions.Dequeue();
                if (position.gx != 0)
                {
                    transform.position = new Vector3(position.gx, position.z, position.gy);
                }
                _endPosition = new Vector3(position.x, position.z, position.y);
                float dist = Vector3.Distance(transform.position, _endPosition);
                _speed = dist / (Math.Max(position.time - currentTime, 0) / 1000f);
                _currentTime = position.time;
                transform.rotation = Quaternion.AngleAxis(position.d, Vector3.up);

                _animationBlend = Mathf.Lerp(_animationBlend, _speed, Time.deltaTime * SpeedChangeRate);
                Animator.SetFloat(_animIDSpeed, _animationBlend);
                Animator.SetFloat(_animIDMotionSpeed, 1);
                return;
            }

            // in current move, set enemy position, direction and animation
            if (_currentTime > 0)
            {
                _step = _speed * Time.deltaTime; // calculate distance to move
                transform.position = Vector3.MoveTowards(transform.position, _endPosition, _step);
                _animationBlend = Mathf.Lerp(_animationBlend, _speed, Time.deltaTime * SpeedChangeRate);
                if (Double.IsNaN(_animationBlend)) _animationBlend = _speed;

                Animator.SetFloat(_animIDSpeed, _animationBlend);
                Animator.SetFloat(_animIDMotionSpeed, 1);
            }
        }

        
    }
}