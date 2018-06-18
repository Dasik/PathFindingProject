using System;
using System.Collections.Generic;
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
            if (!Cells.TryGetValue(position, out result))
                return null;
            return result;
        }
        
        internal static Vector2 floorVector2(Vector2 v)
        {
            return new Vector2(Mathf.Floor(v.x),
                                        Mathf.Floor(v.y));
        }

        internal static bool checkBounds(Vector2 position, Vector2 leftBottomPoint, Vector2 rightTopPoint)
        {
            if (leftBottomPoint.x <= position.x && position.x <= rightTopPoint.x &&
                leftBottomPoint.y <= position.y && position.y <= rightTopPoint.y)
                return true;
            return false;
        }
    }

    internal class Cell
    {
        public readonly Vector2 Position;
        public CellType Type;
        public List<Cell> Neighbours;
        public GameObject GameObject;

        public Cell(Vector2 position, CellType type, List<Cell> neighbours, GameObject gameObject)
        {
            Position = position;
            Type = type;
            Neighbours = neighbours;
            GameObject = gameObject;
        }

        public Cell(Vector2 position)
        {
            Position = position;
        }

        public Cell() { }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(Cell))
                return false;
            var objCell = obj as Cell;
            if (objCell != null &&
                GameObject == objCell.GameObject &&
                Math.Abs((Position - objCell.Position).sqrMagnitude) < 0.02)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }

    public enum CellType
    {
        Static,
        Unwanted,
        Free
    }

    public class SyncList<T>
    {
        private readonly List<T> _list=new List<T>();
        private readonly object locker=new object();

        public long Count
        {
            get
            {
                lock (locker)
                {
                    return _list.Count;
                }
            }
        }

        public void Add(T item)
        {
            lock (locker)
            {
                _list.Add(item);
            }
        }

        public int RemoveAll(Predicate<T> match)
        {
            lock (locker)
            {
                return _list.RemoveAll(match);
            }
        }

        public bool Contains(T item)
        {
            lock (locker)
            {
                return _list.Contains(item);
            }
        }

        public bool Remove(T item)
        {
            lock (locker)
            {
                return _list.Remove(item);
            }    
        }

        public void Clear()
        {
            lock (locker)
            {
                _list.Clear();
            }
        }


    }
}
