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
