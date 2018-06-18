using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Dasik.PathFinder
{
    public class Map : MonoBehaviour
    {
        public static Map Instance;

        public delegate void OnOperationComplete();

        internal Dictionary<Vector2, Cell> CellsList = new Dictionary<Vector2, Cell>();
        internal Dictionary<GameObject, List<Cell>> CellsFromGameobject = new Dictionary<GameObject, List<Cell>>();

        void Start()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        /// <summary>   
        /// Выполняет сканирование и обработку местности
        /// </summary>
        /// <param name="leftBottomPoint">Левая нижняя точка выбранной местности</param>
        /// <param name="rightTopPoint">Правая верхняя точка выбранной иестности</param>
        /// <param name="callback">Уведомление о завершении операции</param>
        /// <param name="addToExistingMap">На данный момент не работает</param>
        /// 
        public void ScanArea(Vector2 leftBottomPoint, Vector2 rightTopPoint, OnOperationComplete callback = null, bool addToExistingMap = true)
        {
            if (!addToExistingMap)
            {
                CellsList.Clear();
                CellsFromGameobject.Clear();
            }
            leftBottomPoint = Utils.floorVector2(leftBottomPoint);
            rightTopPoint = Utils.floorVector2(rightTopPoint);
            for (Vector2 pos = leftBottomPoint; pos.x < rightTopPoint.x; pos.x += 1f)//Просто сканирование всей карты и занесение всех элементов в список
                for (pos.y = leftBottomPoint.y; pos.y < rightTopPoint.y; pos.y += 1f)
                {
                    var cell = scanCell(pos);
                    addNeighbours(cell);
                }
            if (callback != null)
                callback();
        }

        internal Cell scanCell(Vector2 pos)
        {
            //Debug.Log("Scaning cell: "+pos.x+":"+pos.y);

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
                    currrentCell.Type = CellType.Static;
                    currrentCell.GameObject = hit.gameObject;
                }
                var goType = hit.GetComponent<MapObject>();
                if (goType != null)
                {
                    currrentCell.Type = goType.Type;
                    goType.AddOnTypeChangedEventHandler(this, (gameobject, type, newType) =>
                     {
                         var cells = GetCells(gameobject);
                         foreach (var cell in cells)
                         {
                             cell.Type = newType;
                         }
                     });
                    goType.AddOnPositionChangeEventHandler(this, (gameobject, oldPosition, newPosition) =>
                     {
                         var direction = Utils.floorVector2(newPosition - oldPosition);
                         var cells = GetCells(gameobject);
                         var newCells = new List<Cell>();
                         CellsFromGameobject[gameobject] = newCells;
                         foreach (var cell in cells)
                         {
                             Vector2 newCellPos = Utils.floorVector2(cell.Position + direction);
                             Cell newCell;
                             if (!CellsList.TryGetValue(newCellPos, out newCell))
                             {
                                 newCell = new Cell(newCellPos)
                                 {
                                     Type = cell.Type,
                                     GameObject = cell.GameObject
                                 };
                                 addNeighbours(newCell);
                                 CellsList.Add(newCellPos, newCell);
                             }
                             else
                             {
                                 newCell.GameObject = cell.GameObject;
                                 newCell.Type = cell.Type;
                             }
                             newCells.Add(newCell);

                             var oldCell = scanCell(cell.Position);
                             cell.GameObject = oldCell.GameObject;
                             cell.Type = oldCell.Type;
                             if (cell.GameObject != null)
                             {
                                 List<Cell> oldCellList = null;
                                 if (CellsFromGameobject.TryGetValue(cell.GameObject, out oldCellList))
                                 {
                                     oldCellList.Add(cell);
                                 }
                                 else
                                 {
                                     oldCellList = new List<Cell>() { cell };
                                     CellsFromGameobject.Add(cell.GameObject, oldCellList);
                                 }

                             }
                         }
                     })
                    ;
                    currrentCell.GameObject = hit.gameObject;
                    break;
                }
            }
            Cell CellInDict;
            if (!CellsList.TryGetValue(currrentCell.Position, out CellInDict))
            {
                CellsList.Add(currrentCell.Position, currrentCell);
                if (currrentCell.GameObject != null)
                {
                    List<Cell> cellFromGameobject;
                    if (CellsFromGameobject.TryGetValue(currrentCell.GameObject, out cellFromGameobject))
                        cellFromGameobject.Add(currrentCell);
                    else
                    {
                        cellFromGameobject = new List<Cell>() { currrentCell };
                        CellsFromGameobject.Add(currrentCell.GameObject, cellFromGameobject);
                    }
                }
            }
            else
            {
                CellInDict.Type = currrentCell.Type;
                CellInDict.GameObject = currrentCell.GameObject;
                currrentCell = CellInDict;
            }
            return currrentCell;
        }

        internal void addNeighbours(Cell cell)
        {
            cell.Neighbours = new List<Cell>(8);

            #region addingNeighbours
            var neighbour = (getCell(cell.Position.x, cell.Position.y + 1));
            if (neighbour != null)
                cell.Neighbours.Add(neighbour);
            neighbour = (getCell(cell.Position.x + 1, cell.Position.y + 1));
            if (neighbour != null)
                cell.Neighbours.Add(neighbour);
            neighbour = (getCell(cell.Position.x + 1, cell.Position.y));
            if (neighbour != null)
                cell.Neighbours.Add(neighbour);
            neighbour = (getCell(cell.Position.x + 1, cell.Position.y - 1));
            if (neighbour != null)
                cell.Neighbours.Add(neighbour);
            neighbour = (getCell(cell.Position.x, cell.Position.y - 1));
            if (neighbour != null)
                cell.Neighbours.Add(neighbour);
            neighbour = (getCell(cell.Position.x - 1, cell.Position.y - 1));
            if (neighbour != null)
                cell.Neighbours.Add(neighbour);
            neighbour = (getCell(cell.Position.x - 1, cell.Position.y));
            if (neighbour != null)
                cell.Neighbours.Add(neighbour);
            neighbour = (getCell(cell.Position.x - 1, cell.Position.y + 1));
            if (neighbour != null)
                cell.Neighbours.Add(neighbour);
            #endregion

            foreach (var currrentCellNeighbour in cell.Neighbours)
            {
                currrentCellNeighbour.Neighbours.Add(cell);
            }
        }

        /// <summary>
        /// Удаляет просканированную область
        /// </summary>
        /// <param name="leftBottomPoint">Левая нижняя точка выбранной местности</param>
        /// <param name="rightTopPoint">Правая верхняя точка выбранной иестности</param>
        /// <param name="callback">Уведомление о завершении операции</param>
        public void RemoveArea(Vector2 leftBottomPoint, Vector2 rightTopPoint, OnOperationComplete callback = null)
        {
            Thread thr = new Thread(() =>
              {
                  var toRemove = CellsList.Where(pair => Utils.checkBounds(pair.Key, leftBottomPoint, rightTopPoint));
                  foreach (var item in toRemove)
                  {
                      CellsList.Remove(item.Key);
                      if (item.Value.GameObject != null)
                          CellsFromGameobject.Remove(item.Value.GameObject);
                  }
                  foreach (var cell in CellsList)
                  {
                      cell.Value.Neighbours.RemoveAll(cellN => Utils.checkBounds(cellN.Position, leftBottomPoint, rightTopPoint));
                  }
                  if (callback != null)
                      callback();
              });
            thr.Start();
        }

        public void ClearMap()
        {
            CellsList.Clear();
            CellsFromGameobject.Clear();
        }


        private Cell getCell(Vector2 pos)
        {
            return getCell(pos.x, pos.y);
        }

        private Cell getCell(float x, float y)
        {
            Cell result = null;
            if (!CellsList.TryGetValue(new Vector2(x, y), out result))
                return null;
            return result;
        }

        internal List<Cell> GetCells(GameObject go)
        {
            if (go == null)
                return null;
            List<Cell> result;
            if (CellsFromGameobject.TryGetValue(go, out result))
                return result;
            return null;
        }
    }
}

