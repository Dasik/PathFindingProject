using System.Collections;
using System.Collections.Generic;
using Dasik.PathFinder;
using UnityEngine;

public class MapObject : MonoBehaviour
{
    public delegate void onPositionChangeDelegate(GameObject gameObject, Vector2 oldPosition, Vector2 newPosition);
    public event onPositionChangeDelegate OnPositionChange;

    public delegate void onTypeChangedDelegate(GameObject gameObject, CellType oldType, CellType newType);
    public event onTypeChangedDelegate OnTypeChanged;

    public CellType Type;
    public float ChangesUpdateTime = 5f;
    private CellType _previousType;
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
        _previousPosition =Utils.floorVector2(_thisTransform.position);
        _previousType = Type;
        StartCoroutine(checkChanges());
    }

    IEnumerator checkChanges()//TODO: Add onRotationChange
    {
        while (true)
        {
            yield return new WaitForSeconds(ChangesUpdateTime);
            var flooredPos = Utils.floorVector2(_thisTransform.position);
            var deltaPos = generateFloorDistance(_previousPosition, flooredPos);
            if (deltaPos.x >= 1 ||
                 deltaPos.y >= 1)
            {
                InvokeOnPositionChange(_gameobject, _previousPosition, flooredPos);
                _previousPosition = flooredPos;
            }
            if (Type != _previousType)
            {
                InvokeOnTypeChanged(_gameobject, _previousType, Type);
                _previousType = Type;
            }
        }
    }

    private Vector2 generateFloorDistance(Vector2 v1, Vector2 v2)
    {
        var delta = new Vector2(Mathf.Floor(Mathf.Abs(_thisTransform.position.x - _previousPosition.x)),
                                Mathf.Floor(Mathf.Abs(_thisTransform.position.y - _previousPosition.y)));
        return delta;
    }

    protected virtual void InvokeOnTypeChanged(GameObject gameObject, CellType oldtype, CellType newtype)
    {
        if (OnTypeChanged != null)
            OnTypeChanged(gameObject, oldtype, newtype);
    }

    protected virtual void InvokeOnPositionChange(GameObject gameObject, Vector2 oldPosition, Vector2 newPosition)
    {
        if (OnPositionChange != null)
            OnPositionChange(gameObject, oldPosition, newPosition);
    }

    protected List<object> OnTypeChangedListeners=new List<object>();
    public void AddOnTypeChangedEventHandler(object sender, onTypeChangedDelegate handler)
    {
        if (!OnTypeChangedListeners.Contains(sender))
        {
            OnTypeChanged += handler;
            OnTypeChangedListeners.Add(sender);
        }
    }

    public void RemoveOnTypeChangedEventHandler(object sender, onTypeChangedDelegate handler = null)
    {
        if (OnTypeChangedListeners.Contains(sender))
            OnTypeChangedListeners.Remove(sender);
        if (handler!=null)
            OnTypeChanged -= handler;
    }

    protected List<object> OnPositionChangeListeners = new List<object>();
    public void AddOnPositionChangeEventHandler(object sender, onPositionChangeDelegate handler)
    {
        if (!OnPositionChangeListeners.Contains(sender))
        {
            OnPositionChange += handler;
            OnPositionChangeListeners.Add(sender);
        }
    }

    public void RemoveOnPositionChangeEventHandler(object sender, onPositionChangeDelegate handler = null)
    {
        if (OnPositionChangeListeners.Contains(sender))
            OnPositionChangeListeners.Remove(sender);
        if (handler != null)
            OnPositionChange -= handler;
    }
}
