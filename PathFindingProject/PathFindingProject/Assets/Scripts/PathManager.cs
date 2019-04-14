using Dasik.PathFinder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PathManager : MonoBehaviour
{
	public PathFinding PathFinder;
	private List<long> pathFinderIds = new List<long>();
	public bool useBulkPathFinding = true;

	// Use this for initialization
	private void OnEnable()
	{
		//Debug.Log("starting path oprimize");
		//SetPath(new Vector2(-1015, 0));
	}
	
	public void SetPath(Vector2 targetPoint, double accuracy = 1d)
	{
		foreach (var pathFinderId in pathFinderIds)
			PathFinder.ClosePathFindingThread(pathFinderId);
		pathFinderIds.Clear();
		foreach (var item in ObjectGenerator.Instance.Agents)
		{
			item.ApplyPath(new List<Cell>());
		}

#if UNITY_EDITOR
		Stopwatch sw = new Stopwatch();
		sw.Start();
#endif
		if (useBulkPathFinding)
		{
			var objectsStartPosition = ObjectGenerator.Instance.Agents.ToDictionary(agent => agent, agent => agent.Position);
			var pathFinderId = PathFinder.GetPathesAsync(objectsStartPosition, targetPoint, (param, pathes) =>
			 {
#if UNITY_EDITOR
				sw.Stop();
				 Debug.Log("Path founded in: " + sw.Elapsed);
#endif
				 foreach (var path in pathes)
				 {
					 path.Key.ApplyPath(path.Value);
				 }
			 });
			pathFinderIds.Add(pathFinderId);
		}
		else
		{
			int foundedCount = 0;
			foreach (var item in ObjectGenerator.Instance.Agents)
			{
				var pathFinderId = PathFinder.GetPathAsync(item.Position, targetPoint, (o, pathes) =>
				{
#if UNITY_EDITOR
					foundedCount++;
					if (foundedCount == ObjectGenerator.Instance.Agents.Count)
					{
						sw.Stop();
						Debug.Log("Path founded in: " + sw.Elapsed);
					}
#endif
					o.ApplyPath(pathes ?? new List<Cell>());
				}, accuracy, item);
				pathFinderIds.Add(pathFinderId);
			}
		}
	}
}
