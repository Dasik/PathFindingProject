using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dasik.PathFinder
{
    public class ShowMapGizmo : MonoBehaviour
    {
        public Map CurrentMap;



        void OnDrawGizmosSelected()
        {
            DrawCells(CurrentMap.CellsList);
            Debug.Log("count=" + CurrentMap.CellsList.Count);
        }

        private void DrawCells(List<Cell> cell)
        {
            foreach (var currentCell in cell)
            {
                if (currentCell == null)
                    return;
                if (currentCell.Type == CellType.Static)
                    Gizmos.color = Color.red;
                else if (currentCell.Type == CellType.Unwanted)
                    Gizmos.color = Color.yellow;
                if (currentCell.Type != CellType.Free)
                    Gizmos.DrawCube(new Vector3(currentCell.Position.x, currentCell.Position.y, 0), Vector3.one);
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
