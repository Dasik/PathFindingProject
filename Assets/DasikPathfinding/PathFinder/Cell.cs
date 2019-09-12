using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dasik.PathFinder
{
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
			Position = new Vector2(aabb.x, aabb.y);
			Size = new Vector2(aabb.width, aabb.height);
		}

		public Cell()
		{
		}

		public void AddNeighbour(Cell neighbour)
		{
			AddNeighbour(neighbour, Dasik.PathFinder.Utils.GetDistance(Position, neighbour.Position));
		}

		public void AddNeighbour(Cell neighbour, double distance)
		{
			AddNeighbour(neighbour, (float) distance);
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
}