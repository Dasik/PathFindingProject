using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Dasik.PathFinder
{
	public class ShowMapGizmo : MonoBehaviour
	{
		public Map CurrentMap;
		private List<Vector2> path;
		public bool ShowReferences = false;

		//private IEnumerable<Cell> cells;

		private void OnDrawGizmosSelected()
		{
			//if (!wasDrawed && CurrentMap.CellTree != null && CurrentMap.CellTree.Count > 1)
			//if (cells==null)
			//cells = CurrentMap.CellTree.QueryAll();
			//else
			DrawCells(CurrentMap.CellTree);
		}

		//private void DrawCells(KdTree<FloatWithSizeMath.FloatWithSize, Cell> cellTree)
		private void DrawCells(IEnumerable<Cell> cellTree)
		{
			foreach (var currentCell in cellTree)
			{
				try
				{
#if UNITY_EDITOR
					Gizmos.color = currentCell.Color;
					Gizmos.DrawCube(currentCell.Position, currentCell.Size);
					if (!ShowReferences)
						continue;
					Gizmos.color = Color.blue;
					foreach (var neighbour in currentCell.Neighbours)
					{
						//Gizmos.DrawLine(currentCell.Position, neighbour.Key.Position);
						//Gizmos.DrawCube(currentCell.Position, Vector3.one * 0.075f);
						Debug.DrawLine(currentCell.Position, neighbour.Key.Position, Color.blue);

					}
#endif
				}
				catch (Exception e)
				{
					Debug.Log(e);
				}
			}
		}
	}
}
