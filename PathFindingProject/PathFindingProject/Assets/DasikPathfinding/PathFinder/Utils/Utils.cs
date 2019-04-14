using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Dasik.PathFinder
{
	public static class Utils
	{
		internal static double SearchOccuracy = 0.02d;


		/// <summary>
		/// Возвращает ячейку с указанной позицией из списка ячеек
		/// </summary>
		/// <param name="position">Позиция, по которой следует выполнять поиск</param>
		/// <param name="Cells">Ясейки для поиска</param>
		/// <returns>Найденную ячейку в случае успеха, иначе null</returns>
		internal static Cell GetCell(Vector2 position, Dictionary<Vector2, Cell> Cells)
		{
			Cell result;
			if (Cells.TryGetValue(position, out result)) return result;
			return null;
		}

		internal static double GetDistance(Vector2 v1, Vector2 v2)
		{
			float z = (v1 - v2).sqrMagnitude;
			if (z.CompareTo(0f, 0.00001f) == 0) return 0;
			FloatIntUnion u;
			u.tmp = 0;
			u.f = z;
			u.tmp -= 1 << 23; /* Subtract 2^m. */
			u.tmp >>= 1; /* Divide by 2. */
			u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
			return u.f;
		}
		[StructLayout(LayoutKind.Explicit)]
		private struct FloatIntUnion
		{
			[FieldOffset(0)]
			public float f;

			[FieldOffset(0)]
			public int tmp;
		}

		internal static bool CheckBounds(Vector2 position, Vector2 leftBottomPoint, Vector2 rightTopPoint)
		{
			return leftBottomPoint.x <= position.x && position.x <= rightTopPoint.x &&
				   leftBottomPoint.y <= position.y && position.y <= rightTopPoint.y;
		}
	}

	public class Cell
	{

		public const int MIN_PASSABILITY = 0;
		public const int MAX_PASSABILITY = 100;
		public const float MIN_PASSABILITY_F = MIN_PASSABILITY;
		public const float MAX_PASSABILITY_F = MAX_PASSABILITY;

#if UNITY_EDITOR
		public Color Color;
#endif

		public readonly Vector2 Position;
		public readonly Vector2 Size = Vector2.one;

		public AABB aabb
		{
			get
			{
				var halfSize = Size / 2f;
				return new AABB(Position - halfSize, Position + halfSize);
			}
		}
		public float MinX
		{
			get { return Position.x - Size.x / 2f; }
		}
		public float MaxX
		{
			get { return Position.x + Size.x / 2f; }
		}
		public float MinY
		{
			get { return Position.y - Size.y / 2f; }
		}
		public float MaxY
		{
			get { return Position.y + Size.y / 2f; }
		}
		//public CellType Type;

		[Range(MIN_PASSABILITY, MAX_PASSABILITY)]
		public int Passability;
		public Dictionary<Cell, float> Neighbours = new Dictionary<Cell, float>(9);
		public GameObject GameObject;

		//public Cell(Vector2 position, CellType type, List<Cell> neighbours, GameObject gameObject)
		//{
		//    Position = position;
		//    Type = type;
		//    Neighbours = neighbours;
		//    GameObject = gameObject;
		//}

		public Cell(Vector2 position, int passability, List<Cell> neighbours, GameObject gameObject)
		{
			Position = position;
			Passability = passability;
			GameObject = gameObject;
			foreach (var neighbour in neighbours)
			{
				AddNeighbour(neighbour);
			}
		}

		public Cell(Vector2 position, Vector2 size)
		{
			Position = position;
			Size = size;
		}

		public Cell(AABB aabb)
		{
			Position = new Vector2(aabb.x,aabb.y);
			Size = new Vector2(aabb.width,aabb.height);
		}

		public Cell() { }

		public void AddNeighbour(Cell neighbour)
		{
			AddNeighbour(neighbour, Utils.GetDistance(Position, neighbour.Position));
		}

		public void AddNeighbour(Cell neighbour, double distance)
		{
			AddNeighbour(neighbour, (float)distance);
		}

		public void AddNeighbour(Cell neighbour, float distance)
		{
			if (this.Equals(neighbour))
				return;
			if (Neighbours.ContainsKey(neighbour))
				Neighbours[neighbour] = distance;
			else
				Neighbours.Add(neighbour, distance);

			if (neighbour.Neighbours.ContainsKey(this))
				neighbour.Neighbours[this] = distance;
			else
				neighbour.Neighbours.Add(this, distance);
		}

		public void RemoveNeighbourhood(Cell neighbour)
		{
			neighbour.Neighbours.Remove(this);
			Neighbours.Remove(neighbour);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (obj.GetType() != typeof(Cell))
				return false;
			if (this == obj)
				return true;
			var objCell = obj as Cell;
			if (objCell != null &&
				GameObject == objCell.GameObject &&
				Math.Abs((Position - objCell.Position).sqrMagnitude) < 0.0000002 &&
				Math.Abs((Size - objCell.Size).sqrMagnitude) < 0.0000002
				)
				return true;
			return false;
		}

		public override int GetHashCode()
		{
			return Position.GetHashCode() ^ Size.GetHashCode() << 2;
		}
	}

	//public class SyncList<T> : List<T>
	//{
	//	//private readonly List<T> _list = new List<T>();
	//	private readonly object locker = new object();

	//	public SyncList(IEnumerable<T> collection) : base(collection)
	//	{
	//	}

	//	public SyncList(int count) : base(count)
	//	{
	//	}

	//	public SyncList() : base()
	//	{
	//	}

	//	public int Count
	//	{
	//		get
	//		{
	//			lock (locker)
	//			{
	//				//return _list.Count;
	//				return base.Count;
	//			}
	//		}
	//	}

	//	public void Add(T item)
	//	{
	//		lock (locker)
	//		{
	//			//_list.Add(item);
	//			base.Add(item);
	//		}
	//	}

	//	public int RemoveAll(Predicate<T> match)
	//	{
	//		lock (locker)
	//		{
	//			//return _list.RemoveAll(match);
	//			return base.RemoveAll(match);
	//		}
	//	}

	//	public bool Contains(T item)
	//	{
	//		lock (locker)
	//		{
	//			//return _list.Contains(item);
	//			return base.Contains(item);
	//		}
	//	}

	//	public bool Remove(T item)
	//	{
	//		lock (locker)
	//		{
	//			//return _list.Remove(item);
	//			return base.Remove(item);
	//		}
	//	}

	//	public void Clear()
	//	{
	//		lock (locker)
	//		{
	//			//_list.Clear();
	//			base.Clear();
	//		}
	//	}
	//}

	public class ConcurrentHashSet<T> : IDisposable
	{
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private readonly HashSet<T> _hashSet = new HashSet<T>();

		#region Implementation of ICollection<T> ...ish
		public bool Add(T item)
		{
			_lock.EnterWriteLock();
			try
			{
				return _hashSet.Add(item);
			}
			finally
			{
				if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
			}
		}

		public void Clear()
		{
			_lock.EnterWriteLock();
			try
			{
				_hashSet.Clear();
			}
			finally
			{
				if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
			}
		}

		public bool Contains(T item)
		{
			_lock.EnterReadLock();
			try
			{
				return _hashSet.Contains(item);
			}
			finally
			{
				if (_lock.IsReadLockHeld) _lock.ExitReadLock();
			}
		}

		public bool Remove(T item)
		{
			_lock.EnterWriteLock();
			try
			{
				return _hashSet.Remove(item);
			}
			finally
			{
				if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
			}
		}

		public int Count
		{
			get
			{
				_lock.EnterReadLock();
				try
				{
					return _hashSet.Count;
				}
				finally
				{
					if (_lock.IsReadLockHeld) _lock.ExitReadLock();
				}
			}
		}
		#endregion

		#region Dispose
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				if (_lock != null)
					_lock.Dispose();
		}
		~ConcurrentHashSet()
		{
			Dispose(false);
		}
		#endregion
	}
}
