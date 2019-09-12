using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Dasik.PathFinder.Example
{
	public class AgentScript : MonoBehaviour
	{

		private Rigidbody2D _rigidbody;
		private List<Cell> _path = new List<Cell>();
		private int _currentIndex;
		public float MaxSpeed = 10f;
		public float DeltaSpeed = 5f;
		public float ErrorValue = 1f;
		private LineRenderer LineRenderer;
		private static readonly Random random = new Random();

		public Vector2 Position
		{
			get { return _rigidbody.position; }
		}

		//public float ForceMultipler = 10f;
		// Use this for initialization
		void Start()
		{
			_rigidbody = GetComponent<Rigidbody2D>();
			LineRenderer = GetComponent<LineRenderer>();
			MaxSpeed -= (float) ((random.NextDouble() * 2 - 1) * DeltaSpeed);
		}

		private bool wasRendered = true;

		void Update()
		{
			if (!wasRendered)
			{
				if (_path == null || _path.Count == 0)
					return;
				LineRenderer.positionCount = _path.Count - 1;
				for (int i = 0; i < _path.Count - 1; i++)
				{
					LineRenderer.SetPosition(i, _path[i].Position);
				}
			}
		}

		void FixedUpdate()
		{
			if (_path.Count == 0 || _currentIndex >= _path.Count - 1)
			{
				_rigidbody.velocity = Vector2.zero;
				return;
			}

			var direction = (_path[_currentIndex].Position - _rigidbody.position);
			if (direction.sqrMagnitude < ErrorValue)
				_currentIndex++;
			direction.Normalize();
			//_rigidbody.AddForce(direction*ForceMultipler);
			//var sqrMaxSpeed = Mathf.Pow(MaxSpeed, 2);
			//if (_rigidbody.velocity.sqrMagnitude > sqrMaxSpeed)
			//    _rigidbody.velocity = _rigidbody.velocity.normalized * MaxSpeed;
			_rigidbody.velocity = direction * (MaxSpeed);

		}

		public void ApplyPath(IEnumerable<Cell> path)
		{
			_currentIndex = 0;
			_path.Clear();
			_path.AddRange(path);
			wasRendered = false;
		}
	}
}