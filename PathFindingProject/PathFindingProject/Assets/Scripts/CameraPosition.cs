using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    private Transform _transform;
	// Use this for initialization
	void Start ()
	{
	    _transform = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {

        var zPos = _transform.position.z;
        Vector3 pos=Vector3.zero;
	    foreach (var item in ObjectGenerator.Instance.Agents)
	    {
	        pos += item.transform.position;
	    }
	    pos /= ObjectGenerator.Instance.Agents.Count;
	    pos.z = zPos;
	    _transform.position = pos;
	}
}
