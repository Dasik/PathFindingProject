using System.Collections;
using System.Collections.Generic;
using Dasik.PathFinder;
using UnityEngine;

namespace Dasik.PathFinder
{
    public class MapObject : MonoBehaviour
    {
        public delegate void onPositionChangeDelegate(GameObject gameObject, Vector2 oldPosition, Vector2 newPosition);

        public event onPositionChangeDelegate OnPositionChange;

        public delegate void onTypeChangedDelegate(GameObject gameObject, int oldPassability, int newPassability);

        public event onTypeChangedDelegate OnTypeChanged;

        public bool Ignored = false;
        public int Passability;
        public float ChangesUpdateTime = 0.1f;
        private int _previousPassability;
        private Vector2 _previousPosition;
        private Transform _thisTransform;
        private GameObject _thisGameObject;

        private GameObject _gameobject
        {
            get
            {
                if (_thisGameObject == null)
                    _thisGameObject = gameObject;
                return _thisGameObject;
            }
        }

        void Start()
        {
            _thisTransform = GetComponent<Transform>();
            //_previousPosition = Utils.floorVector2(_thisTransform.position);

            _previousPosition = _thisTransform.position;
            _previousPassability = Passability;
            StartCoroutine(checkChanges());
        }

        IEnumerator checkChanges() //TODO: add transform matrix
        {
            while (true)
            {
                yield return new WaitForSeconds(ChangesUpdateTime);
                ////var flooredPos = Utils.floorVector2(_thisTransform.position);
                //var newPosition = _thisTransform.position;
                //var deltaPos = calculateDistance(_previousPosition, newPosition);
                //if (deltaPos.x >= Map.MinScanAccuracy ||
                //    deltaPos.y >= Map.MinScanAccuracy)
                //{
                //    InvokeOnPositionChange(_gameobject, _previousPosition, newPosition);
                //    _previousPosition = newPosition;
                //}
                if (_thisTransform.hasChanged)
                {
                    var newPosition = _thisTransform.position;
                    InvokeOnPositionChange(_gameobject, _previousPosition, newPosition);
                    _previousPosition = newPosition;
                }

                if (Passability != _previousPassability)
                {
                    InvokeOnTypeChanged(_gameobject, _previousPassability, Passability);
                    _previousPassability = Passability;
                }
            }
        }

        private Vector2 calculateDistance(Vector2 v1, Vector2 v2)
        {
            var delta = new Vector2(Mathf.Abs(v1.x - v2.x),
               Mathf.Abs(v1.y - v2.y));
            return delta;
        }

        protected virtual void InvokeOnTypeChanged(GameObject gameObject, int oldPassability, int newPassability)
        {
            if (OnTypeChanged != null)
                OnTypeChanged(gameObject, oldPassability, newPassability);
        }

        protected virtual void InvokeOnPositionChange(GameObject gameObject, Vector2 oldPosition, Vector2 newPosition)
        {
            if (OnPositionChange != null)
                OnPositionChange(gameObject, oldPosition, newPosition);
        }

        public void AddOnTypeChangedEventHandler(object sender, onTypeChangedDelegate handler)
        {
                OnTypeChanged += handler;
        }

        public void RemoveOnTypeChangedEventHandler(object sender, onTypeChangedDelegate handler = null)
        {
            if (handler != null)
                OnTypeChanged -= handler;
        }

        public void AddOnPositionChangeEventHandler(object sender, onPositionChangeDelegate handler)
        {
            OnPositionChange += handler;
        }

        public void RemoveOnPositionChangeEventHandler(object sender, onPositionChangeDelegate handler = null)
        {
            if (handler != null)
                OnPositionChange -= handler;
        }
    }
}