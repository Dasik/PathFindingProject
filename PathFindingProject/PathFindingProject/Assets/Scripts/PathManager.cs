using System;
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
    private List<long> pathFinderIds = new List<long>();
    // Use this for initialization
    void OnEnable()
    {
        //Debug.Log("starting path oprimize");
        //SetPath(new Vector2(-1015, 0));
    }

#if UNITY_EDITOR
    private TimeSpan avgCalculationTime = TimeSpan.Zero;
#endif
    public void SetPath(Vector2 targetPoint, double accuracy = 1d)
    {
        foreach (var pathFinderId in pathFinderIds)
            PathFinder.ClosePathFindingThread(pathFinderId);
        pathFinderIds.Clear();
        foreach (var item in ObjectGenerator.Instance.Agents)
        {
            item.ApplyPath(new List<Vector2>());
        }

#if UNITY_EDITOR
        Stopwatch sw = new Stopwatch();
        sw.Start();
#endif
        foreach (var item in ObjectGenerator.Instance.Agents)
        {
            int foundedCount = 0;
            var pathFinderId = PathFinder.GetPath(item.Position, targetPoint, (o, pathes) =>
            {
#if UNITY_EDITOR
                foundedCount++;

                sw.Stop();
                avgCalculationTime += sw.Elapsed;
                avgCalculationTime= new TimeSpan(avgCalculationTime.Ticks / 2); ;
                Debug.Log("Path founded in: " + sw.Elapsed);

#endif
                //pathesDictionary.Add(o, pathes);

                o.ApplyPath(pathes ?? new List<Vector2>());
                pathSended = false;
            }, accuracy, item);
            pathFinderIds.Add(pathFinderId);
        }

    }
}
