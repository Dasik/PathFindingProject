using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dasik.PathFinder;

public class PathManager : MonoBehaviour
{
    public PathFinding PathFinder;
    private Dictionary<GameObject, List<Vector2>> pathesDictionary = null;
    private bool pathSended = true;
    private long pathFinderId = -1;
    // Use this for initialization
    void OnEnable()
    {
        //Debug.Log("starting path oprimize");
        //SetPath(new Vector2(-1015, 0));
    }

    // Update is called once per frame
    void Update()
    {
        if (!pathSended)
        {
            int counter = 0;
            foreach (var item in pathesDictionary)
            {
                //item.Key.GetComponent<AgentScript>().ApplyPath(item.Value);
                if (item.Value == null)
                    item.Key.SendMessage("ApplyPath", new List<Vector2>());
                else
                    item.Key.SendMessage("ApplyPath", item.Value);
            }
            pathSended = true;
        }

    }

    public void SetPath(Vector2 targetPoint, double accuracy = 1d)
    {
        PathFinder.closePathFindingThread(pathFinderId);
        foreach (var item in ObjectGenerator.Instance.Agents)
        {
            item.SendMessage("ApplyPath", new List<Vector2>());
        }
        //Debug.Log("starting path oprimize");
        var dict = new Dictionary<GameObject, Vector2>();
        foreach (var item in ObjectGenerator.Instance.Agents)
        {
            dict.Add(item, item.transform.position);
        }
        pathFinderId = PathFinder.GetPathes(dict, targetPoint, ((o, pathes) =>
          {
              Debug.Log("Path founded");
              pathesDictionary = pathes;
              pathSended = false;
          }), null, accuracy);
    }
}
