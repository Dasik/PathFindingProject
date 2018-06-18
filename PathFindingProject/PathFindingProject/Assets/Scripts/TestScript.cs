using System.Collections;
using System.Collections.Generic;
using Dasik.PathFinder;
using UnityEngine;

public class TestScript : MonoBehaviour {
    public Map CurrentMap;
    public PathFinding PathFinding;
    private List<Vector2> psss;
    // Use this for initialization
    void Start () {
        
    }
    void OnDrawGizmosSelected()
    {
        if (psss == null)
        {
            psss = new List<Vector2>();
            var dict = new Dictionary<object, Vector2>();
            foreach (var item in ObjectGenerator.Instance.Agents)
            {
                dict.Add(item, item.transform.position);
            }
            PathFinding.GetPathes(dict, new Vector2(-1015, 0), ((o, pathes) =>
            {
                foreach (var item in pathes)
                {
                    psss.AddRange(item.Value);
                    Debug.Log("Item.Length: "+item.Value.Count);
                }
            }));
        }
        else
        {
            Gizmos.color = Color.white;
            foreach (var item in psss)
            {

                Gizmos.DrawCube(item, Vector3.one);
            }
        }

        //Debug.Log("count=" + path.Count);
    }
}
