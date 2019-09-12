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
			IDictionary<T, IEnumerable<Cell>> result = new Dictionary<T, IEnumerable<Cell>>();
			var startCells = new Dictionary<T, Cell>();
			foreach (var startPos in objectsStartPosition)
			{
				var startCell = Map.Instance.GetCell(startPos.Value);
				if (startCell == null)
				{
					Debug.LogWarning("Start positions " + startPos.Value + " not scanned");
					pathTask.Fail();
					return;
				}
				startCells.Add(startPos.Key, startCell);
			}
			var goalCell = Map.Instance.GetCell(goalPosition);

			if (goalCell == null)
			{
				Debug.LogWarning("Goal position " + goalPosition + " not scanned");
				pathTask.Fail();
				return;
			}

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
					Debug.LogWarning("goal cell is obstacle. Cell:" + goalCell.Position);
					pathTask.Fail();
					return;
				}
			}
			var pathesCache = new Dictionary<Cell, IEnumerable<Cell>>();

			var closed = new Dictionary<Cell, Node>();
			var open = new Dictionary<Cell, Node>();
			var sortedOpen = new List<Node>();//todo: change to sorted list
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
					var popItem = sortedOpen[0];
					sortedOpen.RemoveAt(0);
					open.Remove(popItem.Cell);
					return popItem;
				});
				foreach (var startPair in startCells)
				{
					open.Clear();
					sortedOpen.Clear();
					closed.Clear();
					var startCell = startPair.Value;
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
						if (pathesCache.ContainsKey(x.Cell))
						{
							var cachedPath = pathesCache[x.Cell];
							ConstructPath(x.Parent, pathesCache, (ImmutableStack<Cell>)cachedPath);
							result.Add(startPair.Key, pathesCache[startCell]);
							break;
						}
						if (x.Cell.Equals(goalCell))
						{
							ConstructPath(x, pathesCache);
							result.Add(startPair.Key, pathesCache[startCell]);
							break;
						}

						closed.Add(x.Cell, x);
						var pathFounded = false;
						foreach (var yy in x.Cell.Neighbours)
						{
							if (pathesCache.ContainsKey(yy.Key))
							{
								var cachedPath = pathesCache[yy.Key];
								ConstructPath(x, pathesCache, (ImmutableStack<Cell>)cachedPath);
								result.Add(startPair.Key, pathesCache[startCell]);
								pathFounded = true;
								break;
							}

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
						if (pathFounded)
							break;
					}
					if (result.ContainsKey(startPair.Key)) continue;

					Debug.LogWarning("Goal not founded: StartPos: " + startCell.Position + "\tGoalPos: " + goalCell.Position);
					pathTask.Fail();
				}
				pathTask.Complete(result);
			}
			catch (Exception ex)
			{
				pathTask.Fail();
				throw new Exception(ex.Message, ex);
			}
			finally
			{
				open.Clear();
				sortedOpen.Clear();
				closed.Clear();
			}
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
				Debug.LogWarning("Start position " + startPosition + " not scanned");
				pathTask.Fail();
				return;
			}

			if (goalCell == null)
			{
				Debug.LogWarning("Goal position " + goalPosition + " not scanned");
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
					Debug.LogWarning("goal cell is obstacle. Cell:" + goalCell.Position);
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
						pathTask.Complete(ConstructPath(x));
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
				Debug.LogWarning("Goal not founded: StartPos: " + startCell.Position + "\tGoalPos: " + goalCell.Position);
				pathTask.Fail();
			}
			catch (Exception ex)
			{
				pathTask.Fail();
				throw new Exception(ex.Message, ex);
			}
			finally
			{
				open.Clear();
				sortedOpen.Clear();
				closed.Clear();
			}
		}

		private static Dictionary<Cell, IEnumerable<Cell>> ConstructPath(Node node, Dictionary<Cell, IEnumerable<Cell>> into, ImmutableStack<Cell> currentStack = null)
		{
			if (into == null)
				into = new Dictionary<Cell, IEnumerable<Cell>>();
			if (currentStack == null)
				currentStack = ImmutableStack<Cell>.Empty;
			if (node == null)
				return into;
			var newStack = currentStack.Push(node.Cell);
			//if (into.ContainsKey(node.Cell))
			//	into[node.Cell] = newStack;
			//else
			into.Add(node.Cell, newStack);
			if (node.Parent != null)
				ConstructPath(node.Parent, into, newStack);

			return into;
		}

		private static ImmutableStack<Cell> ConstructPath(Node node)
		{
			var result = ImmutableStack<Cell>.Empty;
			var currentNode = node;
			while (currentNode != null)
			{
				result = result.Push(currentNode.Cell);
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

		public Node(Cell cell)
		{
			Cell = cell;
		}
	}

	//internal class Node : Node<object>
	//{
	//	public new Node Parent;
	//	public Node(Cell cell) : base(cell)
	//	{
	//	}
	//}

	//internal class LeafNode<T> : Node<T>
	//{
	//	public T Obj;

	//	public LeafNode(Cell cell) : base(cell)
	//	{
	//	}
	//}
}
