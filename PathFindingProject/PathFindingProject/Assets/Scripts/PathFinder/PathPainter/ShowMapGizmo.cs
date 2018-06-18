using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Dasik.PathFinder
{
    public class ShowMapGizmo : MonoBehaviour
    {
        public Map CurrentMap;
        public PathFinding PathFinding;
        private List<Vector2> path;
        public Initializator.AreaRange StartGoalPoints;
        void Start()
        {


        }

        private bool isPathGetting = false;
        void OnDrawGizmosSelected()
        {
            DrawCells(CurrentMap.CellsList);
            //drawOpenList(PathFinding.sortedOpen);
            if (!isPathGetting)
            {
                isPathGetting = true;
                //PathFinding.GetPath(new Vector2(-10f, 0f),
                //    new Vector2(-450f, 0f),
                //    list => path = list);
                PathFinding.GetPath<object>(StartGoalPoints.LeftBottomPoint,
                    StartGoalPoints.RightTopPoint,
                    (param,list) => path = list);
            }
            if (path != null)
                DrawPath();

            //Debug.Log("count=" + path.Count);
        }

        private void DrawCells(Dictionary<Vector2,Cell> cell)
        {
            try
            {
                foreach (var currentCell in cell.Values)
                {
                    if (currentCell == null)
                        return;
                    if (currentCell.Type == CellType.Static)
                        Gizmos.color = Color.black;
                    else if (currentCell.Type == CellType.Unwanted)
                        Gizmos.color = Color.yellow;
                    if (currentCell.Type != CellType.Free)
                        Gizmos.DrawCube(new Vector3(currentCell.Position.x, currentCell.Position.y, 0), Vector3.one);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        void drawOpenList(List<Node> nodes)
        {
            try
            {
                foreach (var currentNode in nodes)
                {
                    if (currentNode == null)
                        continue;
                    Gizmos.color = Color.white;
                    Gizmos.DrawCube(new Vector3(currentNode.Cell.Position.x, currentNode.Cell.Position.y, 0), Vector3.one);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private void DrawPath()
        {
            foreach (var item in path)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(new Vector3(item.x, item.y, 0), Vector3.one);
            }
        }
        //private static readonly List<Cell> passedCells = new List<Cell>();
        //private int controlId = 0;
        //void OnDrawGizmosSelected()
        //{
        //    passedCells.Clear();
        //    DrawCell(CurrentMap.InitialCell);
        //    Debug.Log("controlId="+controlId);
        //    controlId = 0;
        //}

        //private void DrawCell(Cell currentCell)
        //{
        //    if (currentCell == null || passedCells.Contains(currentCell))
        //        return;
        //    if (currentCell.Type == CellType.Static)
        //        Gizmos.color = Color.red;
        //    else if (currentCell.Type == CellType.Unwanted)
        //        Gizmos.color = Color.yellow;
        //    if (currentCell.Type != CellType.Free)
        //        Gizmos.DrawCube(new Vector3(currentCell.Position.x,currentCell.Position.y,0),Vector3.one );
        //    controlId++;
        //    passedCells.Add(currentCell);
        //    foreach (var neighbour in currentCell.Neighbours)
        //    {
        //        DrawCell(neighbour);
        //    }
        //}
    }
}
