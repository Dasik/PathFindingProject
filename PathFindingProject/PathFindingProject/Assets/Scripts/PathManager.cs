using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Dasik.PathFinder;
using Debug = UnityEngine.Debug;

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
            foreach (var item in pathesDictionary)
            {
                //item.Key.GetComponent<AgentScript>().ApplyPath(item.Value);
                item.Key.SendMessage("ApplyPath", item.Value ?? new List<Vector2>());
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
#if UNITY_EDITOR
        Stopwatch sw = new Stopwatch();
        sw.Start();
#endif
        pathFinderId = PathFinder.GetPathes(dict, targetPoint, ((o, pathes) =>
          {
#if UNITY_EDITOR
              sw.Stop();
              Debug.Log("Path founded in: "+sw.Elapsed);
#endif
              
              pathesDictionary = pathes;
              pathSended = false;
          }), null, accuracy);
    }
}
