using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class AgentScript : MonoBehaviour
{

    private Rigidbody2D _rigidbody;
    private List<Vector2> _path;
    private int _currentIndex = 0;
    public float MaxSpeed = 10f;
    public float DeltaSpeed = 5f;
    public float ErrorValue = 1f;
    private LineRenderer LineRenderer;
    private static Random random=new Random();
    //public float ForceMultipler = 10f;
    // Use this for initialization
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        LineRenderer = GetComponent<LineRenderer>();
        MaxSpeed -= (float) ((random.NextDouble() * 2 - 1) * DeltaSpeed);
    }

    private Vector2 direction;
    void FixedUpdate()
    {
        if (_path == null)
            return;
        if (_path.Count == 0 || _currentIndex >= _path.Count-1)
        {
            _rigidbody.velocity = Vector2.zero;
            return;
        }
        direction = (_path[_currentIndex] - _rigidbody.position);
        if (direction.sqrMagnitude < ErrorValue)
            _currentIndex++;
        direction.Normalize();
        //_rigidbody.AddForce(direction*ForceMultipler);
        //var sqrMaxSpeed = Mathf.Pow(MaxSpeed, 2);
        //if (_rigidbody.velocity.sqrMagnitude > sqrMaxSpeed)
        //    _rigidbody.velocity = _rigidbody.velocity.normalized * MaxSpeed;
        _rigidbody.velocity = direction * (MaxSpeed);

    }

    public void ApplyPath(List<Vector2> path)
    {
        _currentIndex = 0;
        _path = path;
        try
        {

            LineRenderer.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                LineRenderer.SetPosition(i, path[i]);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
