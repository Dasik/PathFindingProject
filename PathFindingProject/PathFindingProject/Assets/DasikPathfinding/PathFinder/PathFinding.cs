using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
//using ThreadPriority = System.Threading.ThreadPriority;

namespace Dasik.PathFinder
{
	public class PathFinding : MonoBehaviour
	{
		public delegate void ReturnedPath<T>(T param, List<Cell> path);
		public delegate void ReturnedPathes<T>(T param, IDictionary<T, List<Cell>> pathes);

		private readonly ConcurrentHashSet<long> _runnedThreads = new ConcurrentHashSet<long>();
		private long _threadIdGenerator = 0L;

		/// <summary>
		/// Bulk get patches. First find patch for closest point and then finding path from closest to others and combine this patches
		/// </summary>
		/// <typeparam name="T">Used for param only. It does not matter for calculation</typeparam>
		/// <param name="objectsStartPosition">Key is a parameter necessary to determine the object for which the path was calculated. Value is start position of this object</param>
		/// <param name="goalPosition">Goal position of path finding</param>
		/// <param name="accuracy">Accuracy. Larger - faster, but path maybe not correct</param>
		/// <returns>Dictionary, where key is param from <paramref name="objectsStartPosition"/> and value is path/></returns>
		public IDictionary<T, List<Cell>> GetPathes<T>(Dictionary<T, Vector2> objectsStartPosition, Vector2 goalPosition, double accuracy = 1)
		{
			var thrId = Interlocked.Increment(ref _threadIdGenerator);

			_runnedThreads.Add(thrId);
			IDictionary<T, List<Cell>> result = null;
			GetPathesInternalTask<T>(objectsStartPosition, goalPosition, thrId,
				(param, pathes) => { result = pathes; }, accuracy, default(T));
			return result;
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
		public long GetPathesAsync<T>(Dictionary<T, Vector2> objectsStartPosition, Vector2 goalPosition,
			ReturnedPathes<T> callback, T param = default(T), double accuracy = 1)
		{
			var thrId = Interlocked.Increment(ref _threadIdGenerator);

			_runnedThreads.Add(thrId);
			TaskFactory.Add(() => GetPathesInternalTask(objectsStartPosition, goalPosition, thrId,
				callback, accuracy, param));
			return thrId;
		}

		private void GetPathesInternalTask<T>(Dictionary<T, Vector2> objectsStartPosition, Vector2 goalPosition, long threadId,
			ReturnedPathes<T> callback, double accuracy, T param)
		{
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
				_runnedThreads.Remove(threadId);
				if (callback != null)
				{
					callback(param, null);
				}
				return;
			}

			var result = new ConcurrentDictionary<T, List<Cell>>();
			var nearestThrId =
				GetPathAsync(objectsStartPosition[nearestObject], goalPosition, (identParam, path) =>
				{
					result.TryAdd(identParam, path);
				}, accuracy, nearestObject);


			List<long> localThreadIds = new List<long>();
			foreach (var item in objectsStartPosition)
			{
				if (!item.Key.Equals(nearestObject))
					localThreadIds.Add(
						GetPathAsync(item.Value, objectsStartPosition[nearestObject], (identParam, path) =>
						{
							result.TryAdd(identParam, path);
						}, accuracy, item.Key));
			}

			while (result.Count!=objectsStartPosition.Count)
			{
				if (!_runnedThreads.Contains(nearestThrId) && result[nearestObject].Count == 0)
				{
					callback(param, new ConcurrentDictionary<T, List<Cell>>());
					_runnedThreads.Remove(threadId);
				}

				if (!_runnedThreads.Contains(threadId))//check if task was stoped
				{
					_runnedThreads.Remove(nearestThrId);
					foreach (var localThreadId in localThreadIds)
					{
						_runnedThreads.Remove(localThreadId);
					}
					return;
				}
			}

			foreach (var item in result)
			{
				if (!item.Key.Equals(nearestObject))
					item.Value.AddRange(result[nearestObject]);
			}
			if (callback != null)
			{
				callback(param, result);
			}
			_runnedThreads.Remove(threadId);
		}

		public List<Cell> GetPath<T>(Vector2 startPosition, Vector2 goalPosition,
			 double accuracy = 1)
		{
			var thrId = Interlocked.Increment(ref _threadIdGenerator);
			_runnedThreads.Add(thrId);
			List<Cell> result = null;
			GetPathInternalTask(startPosition, goalPosition, accuracy,
				((param, path) => result = path), thrId, default(T));

			return result;
		}

		public long GetPathAsync<T>(Vector2 startPosition, Vector2 goalPosition,
			ReturnedPath<T> callback, double accuracy = 1, T param = default(T))
		{
			var thrId = Interlocked.Increment(ref _threadIdGenerator);
			_runnedThreads.Add(thrId);
			TaskFactory.Add(() => GetPathInternalTask(startPosition, goalPosition, accuracy, callback, thrId, param));
			return thrId;
		}

		private void GetPathInternalTask<T>(Vector2 startPosition, Vector2 goalPosition, double accuracy,
			ReturnedPath<T> callback, long threadId, T param)
		{
			var startCell = Map.Instance.GetCell(startPosition);
			var goalCell = Map.Instance.GetCell(goalPosition);
			if (startCell == null)
			{
				Debug.Log("Start position " + startPosition + " not scanned");
				if (callback != null)
					callback(param, new List<Cell>());
				_runnedThreads.Remove(threadId);
				return;
			}

			if (goalCell == null)
			{
				Debug.Log("Goal position " + goalPosition + " not scanned");
				if (callback != null)
					callback(param, new List<Cell>());
				_runnedThreads.Remove(threadId);
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
					Debug.Log("goal cell in obstacle");
					if (callback != null)
						callback(param, new List<Cell>());
					_runnedThreads.Remove(threadId);
					return;
				}
			}
			bool closeThread = false;
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
				while (open.Count != 0 && !closeThread)
				{
					closeThread = !_runnedThreads.Contains(threadId);
					//if (closed.Count % 100 == 0)
					//    Thread.Sleep(10);
					var x = PopFromOpen();
					if (x.Cell.Equals(goalCell))
					{
						callback(param, constructPath(x));
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
				callback(param, new List<Cell>());
			}
			finally
			{
				open.Clear();
				sortedOpen.Clear();
				closed.Clear();
				_runnedThreads.Remove(threadId);
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

		private void OnDestroy()
		{
			_runnedThreads.Clear();
		}

		public bool ClosePathFindingThread(long threadId)
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
