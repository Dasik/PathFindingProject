using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ThreadPriority = UnityEngine.ThreadPriority;

namespace Dasik.PathFinder
{
    public class Map : MonoBehaviour
    {
        public delegate void OnScaningComplete();

        internal List<Cell> CellsList = new List<Cell>();

        /// <summary>   
        /// Выполняет сканирование и обработку местности
        /// </summary>
        /// <param name="leftBottomPoint">Левая нижняя точка выбранной местности</param>
        /// <param name="rightTopPoint">Правая верхняя точка выбранной иестности</param>
        /// <param name="addToExistingMap">На данный момент не работает</param>
        /// 
        public void ScanArea(Vector2 leftBottomPoint, Vector2 rightTopPoint, OnScaningComplete callback, bool addToExistingMap = false)
        {
            if (!addToExistingMap)
            {
                CellsList.Clear();
            }
            Dictionary<Vector2, Cell> CellDict = new Dictionary<Vector2, Cell>();
            for (Vector2 pos = leftBottomPoint; pos.x < rightTopPoint.x; pos.x += 1f)//Просто сканирование всей карты и занесение всех элементов в список
                for (pos.y = leftBottomPoint.y; pos.y < rightTopPoint.y; pos.y += 1f)
                {
                    var currrentCell = new Cell(pos);
                    var hits = Physics2D.OverlapBoxAll(pos, Vector2.one, 0);
                    currrentCell.Type = CellType.Free;
                    foreach (var hit in hits)
                    {
                        //if (hit.gameObject.isStatic)
                        //{
                        //    var goType = hit.GetComponent<ObstacleType>();
                        //    currrentCell.Type = goType == null ? CellType.Static : goType.Type;
                        //    break;
                        //}
                        if (hit.gameObject.isStatic)
                        {
                            currrentCell.Type =  CellType.Static;
                        }
                        var goType = hit.GetComponent<ObstacleType>();
                        if (goType != null)
                        {
                            currrentCell.Type = goType.Type;
                            break;
                        }
                    }
                    CellDict.Add(currrentCell.Position, currrentCell);
                    currrentCell.Neighbours = new List<Cell>(8);

                    #region addingNeighbours
                    var neighbour = (getCell(currrentCell.Position.x, currrentCell.Position.y + 1, CellDict));
                    if (neighbour != null)
                        currrentCell.Neighbours.Add(neighbour);
                    neighbour = (getCell(currrentCell.Position.x + 1, currrentCell.Position.y + 1, CellDict));
                    if (neighbour != null)
                        currrentCell.Neighbours.Add(neighbour);
                    neighbour = (getCell(currrentCell.Position.x + 1, currrentCell.Position.y, CellDict));
                    if (neighbour != null)
                        currrentCell.Neighbours.Add(neighbour);
                    neighbour = (getCell(currrentCell.Position.x + 1, currrentCell.Position.y - 1, CellDict));
                    if (neighbour != null)
                        currrentCell.Neighbours.Add(neighbour);
                    neighbour = (getCell(currrentCell.Position.x, currrentCell.Position.y - 1, CellDict));
                    if (neighbour != null)
                        currrentCell.Neighbours.Add(neighbour);
                    neighbour = (getCell(currrentCell.Position.x - 1, currrentCell.Position.y - 1, CellDict));
                    if (neighbour != null)
                        currrentCell.Neighbours.Add(neighbour);
                    neighbour = (getCell(currrentCell.Position.x - 1, currrentCell.Position.y, CellDict));
                    if (neighbour != null)
                        currrentCell.Neighbours.Add(neighbour);
                    neighbour = (getCell(currrentCell.Position.x - 1, currrentCell.Position.y + 1, CellDict));
                    if (neighbour != null)
                        currrentCell.Neighbours.Add(neighbour);
                    #endregion

                    foreach (var currrentCellNeighbour in currrentCell.Neighbours)
                    {
                        currrentCellNeighbour.Neighbours.Add(currrentCell);
                    }
                }
            CellsList.AddRange(CellDict.Values);
            if (callback != null)
                callback();
        }



        private Cell getCell(float x, float y, Dictionary<Vector2, Cell> dict)
        {
            Cell result = null;
            if (!dict.TryGetValue(new Vector2(x, y), out result))
                return null;
            //var result = CellsList.Find(cell =>
            //{
            //    if (cell.Position.x.Equals(x) &&
            //        cell.Position.y.Equals(y))
            //        return true;
            //    return false;
            //});

            return result;
            //foreach (var item in CellsList)
            //{
            //    if (item.Position.Equals(position))
            //        return item;
            //}
            //return null;
        }


        //currentCell.Neighbours = new Cell[8]
        //{
        //    GenerateOrGetCell(new Vector2(currentCell.Position.x,currentCell.Position.y+1)),
        //    GenerateOrGetCell(new Vector2(currentCell.Position.x+1,currentCell.Position.y+1)),
        //    GenerateOrGetCell(new Vector2(currentCell.Position.x+1,currentCell.Position.y)),
        //    GenerateOrGetCell(new Vector2(currentCell.Position.x+1,currentCell.Position.y-1)),
        //    GenerateOrGetCell(new Vector2(currentCell.Position.x,currentCell.Position.y-1)),
        //    GenerateOrGetCell(new Vector2(currentCell.Position.x-1,currentCell.Position.y-1)),
        //    GenerateOrGetCell(new Vector2(currentCell.Position.x-1,currentCell.Position.y)),
        //    GenerateOrGetCell(new Vector2(currentCell.Position.x-1,currentCell.Position.y+1))
        //};

        //internal Cell InitialCell;
        //private Vector2 _leftBottomPoint;
        //private Vector2 _rightTopPoint;

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="leftBottomPoint"></param>
        ///// <param name="rightTopPoint"></param>
        ///// <param name="addToExistingMap">На данный момент не работает</param>
        //public void ScanArea(Vector2 leftBottomPoint, Vector2 rightTopPoint, bool addToExistingMap = false)
        //{
        //    _leftBottomPoint = leftBottomPoint;
        //    _rightTopPoint = rightTopPoint;
        //    if (!addToExistingMap)
        //    {
        //        InitialCell = new Cell(leftBottomPoint+Vector2.one);
        //        var hits = Physics2D.OverlapBoxAll(leftBottomPoint, Vector2.one, 0);
        //        foreach (var hit in hits)
        //        {
        //            if (hit.gameObject.isStatic)
        //            {
        //                var goType = hit.GetComponent<ObstacleType>();
        //                InitialCell.Type = goType == null ? CellType.Static : goType.Type;
        //                break;
        //            }
        //        }
        //    }
        //    ScanNeighbours(InitialCell);
        //}

        //private void ScanNeighbours(Cell currentCell)
        //{
        //    if (currentCell == null || !Utils.checkBounds(currentCell.Position, _leftBottomPoint, _rightTopPoint))
        //        return;
        //    //var hits = Physics2D.OverlapBoxAll(currentCell.Position, Vector2.one, 0);
        //    //foreach (var hit in hits)
        //    //{
        //    //    if (hit.gameObject.isStatic)
        //    //    {
        //    //        var goType = hit.GetComponent<ObstacleType>();
        //    //        currentCell.Type = goType == null ? CellType.Static : goType.Type;
        //    //        break;
        //    //    }
        //    //}
        //    currentCell.Neighbours = new Cell[8]
        //    {
        //        GenerateOrGetCell(new Vector2(currentCell.Position.x,currentCell.Position.y+1)),
        //        GenerateOrGetCell(new Vector2(currentCell.Position.x+1,currentCell.Position.y+1)),
        //        GenerateOrGetCell(new Vector2(currentCell.Position.x+1,currentCell.Position.y)),
        //        GenerateOrGetCell(new Vector2(currentCell.Position.x+1,currentCell.Position.y-1)),
        //        GenerateOrGetCell(new Vector2(currentCell.Position.x,currentCell.Position.y-1)),
        //        GenerateOrGetCell(new Vector2(currentCell.Position.x-1,currentCell.Position.y-1)),
        //        GenerateOrGetCell(new Vector2(currentCell.Position.x-1,currentCell.Position.y)),
        //        GenerateOrGetCell(new Vector2(currentCell.Position.x-1,currentCell.Position.y+1))
        //    };
        //    foreach (var currentCellNeighbour in currentCell.Neighbours)
        //    {
        //        if (currentCellNeighbour != null)
        //            ScanNeighbours(currentCellNeighbour);
        //    }
        //}

        //private Cell GenerateOrGetCell(Vector2 position)
        //{
        //    if (!Utils.checkBounds(position, _leftBottomPoint, _rightTopPoint))
        //        return null;
        //    var result = Utils.GetCell(position, InitialCell);
        //    if (result == null)
        //    {
        //        result = new Cell(position);
        //        var hits = Physics2D.OverlapBoxAll(result.Position, Vector2.one, 0);
        //        foreach (var hit in hits)
        //        {
        //            if (hit.gameObject.isStatic)
        //            {
        //                var goType = hit.GetComponent<ObstacleType>();
        //                result.Type = goType == null ? CellType.Static : goType.Type;
        //                break;
        //            }
        //            result.Type=CellType.Free;
        //        }
        //    }
        //    return result;
        //}
    }
}

