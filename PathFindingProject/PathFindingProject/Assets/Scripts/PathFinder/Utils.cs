﻿using System;
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
        internal static Cell GetCell(Vector2 position, List<Cell> Cells)
        {
            foreach (var cell in Cells)
            {
                if (cell.Position == position)
                    return cell;
            }
            return null;
        }

        //private static readonly List<Cell> passedCells = new List<Cell>();
        //internal static Cell GetCell(Vector2 position, Cell initialCell)
        //{
        //    var result= GetCell(initialCell, position);
        //    passedCells.Clear();
        //    return result;
        //}

        //private static Cell GetCell(Cell currentCell, Vector2 position)
        //{
        //    if (currentCell == null || passedCells.Contains(currentCell))
        //        return null;
        //    if (Math.Abs((currentCell.Position - position).sqrMagnitude) < SearchOccuracy)
        //        return currentCell;
        //    passedCells.Add(currentCell);
        //    Cell result = null;
        //    foreach (var neighbour in currentCell.Neighbours)
        //    {
        //        if (neighbour==null)
        //            continue;
        //        result = GetCell(neighbour, position);
        //        if (result != null)
        //            break;
        //    }
        //    return result;
        //}
        //Код, который ниже будет проходить по уже пройденным ячейкам
        //internal static Cell GetCell(Vector2 position, Cell initialCell)
        //{
        //    return GetCell(initialCell, position, initialCell);
        //}

        //private static Cell GetCell(Cell currentCell, Vector2 position, Cell initialCell)
        //{
        //    if (currentCell == null)
        //        return null;
        //    if (Math.Abs((currentCell.Position - position).sqrMagnitude) < SearchOccuracy)
        //        return currentCell;
        //    Cell result = null;
        //    var currentDistance = (currentCell.Position - initialCell.Position).sqrMagnitude;
        //    foreach (var neighbour in currentCell.Neighbour)
        //    {
        //        var neighbourDistance = (neighbour.Position - initialCell.Position).sqrMagnitude;
        //        if (neighbourDistance < currentDistance)
        //            continue;
        //        result = GetCell(neighbour, position, initialCell);
        //        if (result != null)
        //            break;
        //    }
        //    return result;
        //}

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
        public Vector2 Position;
        public CellType Type;
        public Cell[] Neighbours;

        public Cell(Vector2 position, CellType type, Cell[] neighbour)
        {
            Position = position;
            Type = type;
            Neighbours = neighbour;
        }

        public Cell(Vector2 position)
        {
            Position = position;
            Neighbours = new Cell[8];
        }

        public Cell() { Neighbours = new Cell[8]; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(Cell))
                return false;
            var objCell = obj as Cell;
            if (objCell != null && Math.Abs((this.Position - objCell.Position).sqrMagnitude) < 0.02)
                return true;
            return false;
        }
    }

    public enum CellType
    {
        Static,
        Unwanted,
        Free
    }
}