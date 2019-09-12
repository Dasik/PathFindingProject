using Assets.DasikPathfinding.PathFinder.Task;
using Dasik.PathFinder;
using Dasik.PathFinder.Task;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PathManager : MonoBehaviour
{
	public PathFinding PathFinder;
	private BulkPathTask<AgentScript> bulkPathFinderTask;
	private Dictionary<AgentScript, SinglePathTask> pathFinderTasks;
	public bool useBulkPathFinding = true;

	// Use this for initialization
	private void OnEnable()
	{
		//Debug.Log("starting path oprimize");
		//SetPath(new Vector2(-1015, 0));
	}

	public void Update()
	{
		if (useBulkPathFinding)
		{
			if (bulkPathFinderTask != null && bulkPathFinderTask.Status == PathTaskStatus.Completed)
			{
#if UNITY_EDITOR
				sw.Stop();
				Debug.Log("Path founded in: " + sw.Elapsed);
#endif
				foreach (var path in bulkPathFinderTask.Path)
				{
					path.Key.ApplyPath(path.Value);
				}

				bulkPathFinderTask = null;
			}
		}
		else
		{
			if (pathFinderTasks==null)
				return;

			var endedTasks = new List<AgentScript>();
			foreach (var singlePathTask in pathFinderTasks)
			{
				if (singlePathTask.Value.Status == PathTaskStatus.Completed)
				{
					endedTasks.Add(singlePathTask.Key);
					singlePathTask.Key.ApplyPath(singlePathTask.Value.Path ?? new List<Cell>());
				}
			}

			foreach (var endedTask in endedTasks)
			{
				pathFinderTasks.Remove(endedTask);
			}

			if (pathFinderTasks.Count == 0)
			{
#if UNITY_EDITOR
				sw.Stop();
				Debug.Log("Path founded in: " + sw.Elapsed);
#endif
				pathFinderTasks = null;
			}
		}
	}


#if UNITY_EDITOR
	private Stopwatch sw;
#endif
	public void SetPath(Vector2 targetPoint, double accuracy = 1d)
	{
		if (bulkPathFinderTask != null)
		{
			bulkPathFinderTask.Dispose();
			bulkPathFinderTask = null;
		}

		if (pathFinderTasks != null)
		{
			foreach (var singlePathTask in pathFinderTasks)
			{
				singlePathTask.Value.Dispose();
			}

			pathFinderTasks = null;
		}

		foreach (var item in ObjectGenerator.Instance.Agents)
		{
			item.ApplyPath(new List<Cell>());
		}

#if UNITY_EDITOR
		sw = new Stopwatch();
		sw.Start();
#endif
		if (useBulkPathFinding)
		{
			var objectsStartPosition = ObjectGenerator.Instance.Agents.ToDictionary(agent => agent, agent => agent.Position);
			bulkPathFinderTask = PathFinder.GetPathesAsync(objectsStartPosition, targetPoint);
		}
		else
		{
			pathFinderTasks=new Dictionary<AgentScript, SinglePathTask>();
			foreach (var item in ObjectGenerator.Instance.Agents)
			{
				var singlePathTask = PathFinder.GetPathAsync(item.Position, targetPoint, accuracy);
				pathFinderTasks.Add(item, singlePathTask);
			}
		}
	}
}
