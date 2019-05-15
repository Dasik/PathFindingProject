using Assets.DasikPathfinding.PathFinder.Task;
using Dasik.PathFinder.Task;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
//using ThreadPriority = System.Threading.ThreadPriority;

namespace Dasik.PathFinder
{
	public class PathFinding : MonoBehaviour
	{
		public static PathFinding Instance;

		private readonly ConcurrentHashSet<long> _runnedThreads = new ConcurrentHashSet<long>();
		private long _threadIdGenerator = 0L;

		protected virtual void Start()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
		}

		private void OnDestroy()
		{
			_runnedThreads.Clear();
		}

		/// <summary>
		/// Bulk get patches. First find patch for closest point and then finding path from closest to others and combine this patches
		/// </summary>
		/// <typeparam name="T">Used for param only. It does not matter for calculation</typeparam>
		/// <param name="objectsStartPosition">Key is a parameter necessary to determine the object for which the path would calculated. Value is start position of this object</param>
		/// <param name="goalPosition">Goal position of path finding</param>
		/// <param name="callback">Callback for call after path was calculated</param>
		/// <param name="param">Parameter to determine calculation operation. Used in callback</param>
		/// <param name="accuracy">Accuracy. Larger - faster, but path maybe not correct</param>
		/// <returns>Thread id for cancelation calculation</returns>
		public BulkPathTask<T> GetPathesAsync<T>(IDictionary<T, Vector2> objectsStartPosition, Vector2 goalPosition, double accuracy = 1)
		{
			var thrId = Interlocked.Increment(ref _threadIdGenerator);
			var bulkPathTask = new BulkPathTask<T>(thrId, this);
			_runnedThreads.Add(thrId);
			TaskFactory.Add(() => GetPathesInternalTask(objectsStartPosition, goalPosition, accuracy, bulkPathTask));
			return bulkPathTask;
		}

		protected virtual void GetPathesInternalTask<T>(IDictionary<T, Vector2> objectsStartPosition, Vector2 goalPosition, double accuracy, BulkPathTask<T> pathTask)
		{
			pathTask.Run();
			T nearestObject = default(T);
			var nearestDistance = float.MaxValue;
			foreach (var item in objectsStartPosition)
			{
				var dist = Vector2.SqrMagnitude(goalPosition - item.Value);
				if (dist >= nearestDistance) continue;
				nearestDistance = dist;
				nearestObject = item.Key;
			}

			if (nearestObject == null)
			{
				pathTask.Fail();
				return;
			}

			var result = new Dictionary<T, IEnumerable<Cell>>();
			var nearestTask = GetPathAsync(objectsStartPosition[nearestObject], goalPosition, accuracy);
			pathTask.ChildTasks.Add(nearestTask);

			var localTasks = new Dictionary<T, SinglePathTask>();
			foreach (var item in objectsStartPosition)
			{
				if (item.Key.Equals(nearestObject))
					continue;
				var localTask = GetPathAsync(item.Value, objectsStartPosition[nearestObject], accuracy);
				localTasks.Add(item.Key, localTask);
				pathTask.ChildTasks.Add(localTask);
			}

			var nearestPath = nearestTask.WaitForResult();
			if (nearestTask.Status == PathTaskStatus.Faulted)
			{
				pathTask.Fail();
				return;
			}

			foreach (var localTask in localTasks)
			{
				var localTaskResult = localTask.Value.WaitForResult();
				if (pathTask.Status == PathTaskStatus.Canceled)
					return;
				if (localTask.Value.Status == PathTaskStatus.Faulted)
				{
					pathTask.Fail();
					return;
				}

				((List<Cell>)localTaskResult).AddRange(nearestPath);
				result.Add(localTask.Key, localTaskResult);
			}

			if (pathTask.Status == PathTaskStatus.Canceled)
				return;
			result.Add(nearestObject, nearestPath);
			pathTask.Complete(result);
		}

		public SinglePathTask GetPathAsync(Vector2 startPosition, Vector2 goalPosition, double accuracy = 1)
		{
			var thrId = Interlocked.Increment(ref _threadIdGenerator);
			_runnedThreads.Add(thrId);
			var pathTask = new SinglePathTask(thrId, this);
			TaskFactory.Add(() => GetPathInternalTask(startPosition, goalPosition, accuracy, pathTask));
			return pathTask;
		}

		private void GetPathInternalTask(Vector2 startPosition, Vector2 goalPosition, double accuracy, SinglePathTask pathTask)
		{
			var startCell = Map.Instance.GetCell(startPosition);
			var goalCell = Map.Instance.GetCell(goalPosition);
			if (startCell == null)
			{
				Debug.Log("Start position " + startPosition + " not scanned");
				pathTask.Fail();
				return;
			}

			if (goalCell == null)
			{
				Debug.Log("Goal position " + goalPosition + " not scanned");
				pathTask.Fail();
				return;
			}

			//if (startCell.Passability == Cell.MIN_PASSABILITY)
			//{
			//    foreach (var item in startCell.Neighbours)
			//    {
			//        if (item.Key.Passability > Cell.MIN_PASSABILITY)
			//        {
			//            startCell = item.Key;
			//            break;
			//        }
			//    }
			//    if (startCell == null)
			//    {
			//        _runnedThreads.Remove(threadId);
			//        if (callback != null)
			//            callback(param, null);
			//        Debug.Log("start cell in obstacle");
			//        return;
			//    }
			//}
			if (goalCell.Passability == Cell.MIN_PASSABILITY)
			{
				foreach (var item in goalCell.Neighbours)
				{
					if (item.Key.Passability > Cell.MIN_PASSABILITY)
					{
						goalCell = item.Key;
						break;
					}
				}
				if (goalCell.Passability == Cell.MIN_PASSABILITY)
				{
					Debug.Log("goal cell is obstacle. Cell:" + goalCell.Position);
					pathTask.Fail();
					return;
				}
			}
			Dictionary<Cell, Node> closed = new Dictionary<Cell, Node>();
			Dictionary<Cell, Node> open = new Dictionary<Cell, Node>();
			List<Node> sortedOpen = new List<Node>();//todo: change to sorted list
			try
			{
				var PutToOpen = new Action<Node>(node =>
				{
					for (int i = 0; i < open.Count; i++)
					{
						if (sortedOpen[i].f < node.f) continue;
						sortedOpen.Insert(i, node);
						open.Add(node.Cell, node);
						return;
					}

					sortedOpen.Insert(open.Count, node);
					open.Add(node.Cell, node);
				});

				var PopFromOpen = new Func<Node>(() =>
				{
					var result = sortedOpen[0];
					sortedOpen.RemoveAt(0);
					open.Remove(result.Cell);
					return result;
				});
				var start = new Node(startCell)
				{
					g = 0d,
					h = Utils.GetDistance(goalCell.Position, startCell.Position) * accuracy
				};
				start.f = start.g + start.h;
				PutToOpen(start);
				while (open.Count != 0)
				{
					if (pathTask.Status == PathTaskStatus.Canceled)
						return;
					//if (closed.Count % 100 == 0)
					//    Thread.Sleep(10);
					var x = PopFromOpen();
					if (x.Cell.Equals(goalCell))
					{
						pathTask.Complete(constructPath(x));
						return;
					}
					closed.Add(x.Cell, x);
					foreach (var yy in x.Cell.Neighbours)
					{
						if (yy.Key.Passability == Cell.MIN_PASSABILITY)
						{
							continue;
						}
						//если текущий сосед содержится в списке просмотренных вершин, то пропустить его
						if (closed.ContainsKey(yy.Key)) continue;
						bool tentativeIsBetter = true;
						//var tentativeGScore = x.g + 1d;//1d-расстояние между х и соседом
						var tentativeGScore = x.g + yy.Value / (yy.Key.Passability / Cell.MAX_PASSABILITY_F);
						//Получаем y из open

						Node y;
						if (open.TryGetValue(yy.Key, out y))
						{
							if (tentativeGScore < y.g)
							{
								open.Remove(yy.Key);
								sortedOpen.Remove(y);
							}
							else
								tentativeIsBetter = false;
						}
						else
						{
							y = new Node(yy.Key);
						}
						if (tentativeIsBetter)
						{
							y.Parent = x;
							y.g = tentativeGScore;
							y.h = Utils.GetDistance(y.Cell.Position, goalCell.Position) * accuracy;
							y.f = y.g + y.h;
							PutToOpen(y);
						}
					}
				}
				Debug.Log("Goal not founded: StartPos: " + startCell.Position + "\tGoalPos: " + goalCell.Position);
				pathTask.Fail();
			}
			finally
			{
				open.Clear();
				sortedOpen.Clear();
				closed.Clear();
			}
		}

		private List<Cell> constructPath(Node node)
		{
			var result = new List<Cell>();
			var currentNode = node;
			while (currentNode != null)
			{
				result.Insert(0, currentNode.Cell);
				currentNode = currentNode.Parent;
			}
			return result;
		}

		internal bool ClosePathFindingThread(long threadId)
		{
			return _runnedThreads.Remove(threadId);
		}
	}

	internal class Node
	{
		public Cell Cell;
		public Node Parent;
		public double g;
		public double h;
		public double f;

		public Node(Cell cell, Node parent = null)
		{
			Cell = cell;
			Parent = parent;
		}
	}
}
