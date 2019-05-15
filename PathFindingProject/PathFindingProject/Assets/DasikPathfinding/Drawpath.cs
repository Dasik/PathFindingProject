using Dasik.PathFinder;
using Dasik.PathFinder.Task;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Drawpath : MonoBehaviour
{
	/// <summary>
	/// Модуль, выполняющий оптимизацию пути
	/// </summary>
	public PathFinding PathFinding;
	/// <summary>
	/// Результирующий путь
	/// </summary>
	private IEnumerable<Cell> path;
	/// <summary>
	/// Координаты начальной точки
	/// </summary>
	public Vector2 StartPoint;
	/// <summary>
	/// Координаты точки назначения
	/// </summary>
	public Vector2 GoalPoint;


	private SinglePathTask currentTask;

	private void Start()
	{
		EventManager.OnMapScanned.Add(10000, () =>
		{
			Debug.Log("Starting founding pathing)");
			System.GC.Collect();
			var memoryStart = System.GC.GetTotalMemory(true);
			Stopwatch sw = new Stopwatch();
			sw.Start();

			currentTask = PathFinding.GetPathAsync(StartPoint, GoalPoint);
			path = currentTask.WaitForResult();

			sw.Stop();
			System.GC.Collect();
			var memoryEnd = System.GC.GetTotalMemory(true);
			Debug.Log("Path finded for " + sw.Elapsed + ", RAM used " + (memoryEnd - memoryStart) / 1024 / 1024 + "MB");//approximate value
		});
		prevStartPos = StartPoint;
		prevGoalPos = GoalPoint;
	}


	private Vector2 prevStartPos = Vector2.zero;
	private Vector2 prevGoalPos = Vector2.zero;
	private Stopwatch sw;
	private long memoryStart;

	private void Update()
	{
		if (((prevStartPos - StartPoint).sqrMagnitude > 0.1) ||
			(prevGoalPos - GoalPoint).sqrMagnitude > 0.1)
		{
			currentTask?.Stop();
			path = null;
			Debug.Log("Start founding path)");
			System.GC.Collect();
			memoryStart = System.GC.GetTotalMemory(true);
			sw = new Stopwatch();
			sw.Start();

			currentTask = PathFinding.GetPathAsync(StartPoint, GoalPoint);
			prevStartPos = StartPoint;
			prevGoalPos = GoalPoint;
		}

		if (currentTask != null && currentTask.Status == PathTaskStatus.Completed)
		{
			path = currentTask.Path;
			currentTask = null;
			sw.Stop();
			System.GC.Collect();
			var memoryEnd = System.GC.GetTotalMemory(true);
			Debug.Log("Path finded for " + sw.Elapsed + ", RAM used " + (memoryEnd - memoryStart) / 1024 / 1024 + "MB");//approximate value
		}

	}

	private void OnDrawGizmosSelected()
	{
		if (path != null)
			//визуализация проложенного пути
			DrawPath();
	}

	private void DrawPath()
	{
		if (path == null)
			return;
		Cell previous = null;
		foreach (var item in path)
		{
			if (previous != null)
				Debug.DrawLine(previous.Position, new Vector3(item.Position.x, item.Position.y, 0), Color.green);
			previous = item;
		}
	}
}

