using UnityEngine;

namespace Dasik.PathFinder.Example
{
	public class RandomScale : MonoBehaviour
	{
		public float MinScale = 0.25f;
		public float MaxScale = 10f;
		private Transform _transform;
		private Vector3 _originalTransformScale;

		void Awake()
		{
			_transform = GetComponent<Transform>();
			_originalTransformScale = _transform.localScale;
		}

		void OnEnable()
		{
			float tmpScale = Random.Range(MinScale, MaxScale);
			Vector3 newTransformScale = _originalTransformScale * tmpScale;
			newTransformScale.z = 1f;
			_transform.localScale = newTransformScale;
		}
	}
}