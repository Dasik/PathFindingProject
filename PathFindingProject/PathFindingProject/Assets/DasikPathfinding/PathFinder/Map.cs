using ds;
using System.Collections.Generic;
using UnityEngine;

namespace Dasik.PathFinder
{
	public class Map : MonoBehaviour
	{
		public const float MinScanAccuracy = 0.4f;
		//public const float MinScanAccuracy = 10f;

		public static Map Instance;

		public delegate void OnOperationComplete();
		/// <summary>
		/// Dictionary for fast changing cell when change GO
		/// </summary>
		internal Dictionary<GameObject, List<Cell>> CellsFromGameobject = new Dictionary<GameObject, List<Cell>>();

		public AABBTree<Cell> CellTree = new AABBTree<Cell>(new InsertStrategyArea<Cell>(), 0f);

		private void Start()
		{
			if (Instance == null)
				Instance = this;
			else
			{
				Destroy(this);
				return;
			}
		}

#if UNITY_EDITOR
		private System.Random _random = new System.Random();

		private Color randomizeColor(Color color)
		{
			color.r += ((float)_random.NextDouble() * 0.4f) - 0.2f;
			color.g += ((float)_random.NextDouble() * 0.4f) - 0.2f;
			color.b += ((float)_random.NextDouble() * 0.4f) - 0.2f;
			return color;
		}
#endif

		/// <summary>   
		/// Выполняет сканирование и обработку местности
		/// </summary>
		/// <param name="leftBottomPoint">Левая нижняя точка выбранной местности</param>
		/// <param name="rightTopPoint">Правая верхняя точка выбранной иестности</param>
		/// <param name="scanAccuracy"></param>
		/// <param name="callback">Уведомление о завершении операции</param>
		/// <param name="addToExistingMap">Добавить в уже  существующую карту</param>
		public void ScanArea(Vector2 leftBottomPoint, Vector2 rightTopPoint, float scanAccuracy = MinScanAccuracy * 50f, OnOperationComplete callback = null, bool addToExistingMap = true)
		{
			if (!addToExistingMap)
			{
				//CellTree.Clear();
				CellTree = new AABBTree<Cell>(new InsertStrategyArea<Cell>());
				CellsFromGameobject.Clear();
			}
			var scanAreaBounds = new AABB(leftBottomPoint, rightTopPoint);

			//for debug
			//scanAreaBounds = new AABB(leftBottomPoint/10f, rightTopPoint/10f);
			if (scanAccuracy > scanAreaBounds.width)
				scanAccuracy = scanAreaBounds.width - MinScanAccuracy;
			if (scanAccuracy > scanAreaBounds.height)
				scanAccuracy = scanAreaBounds.height - MinScanAccuracy;
			ScanArea(scanAreaBounds/*, hitsTree*/, scanAccuracy);//todo: scan in thread
			TaskFactory.Add(
				() =>
				{
					//CellTree.Build();
					//CellTree.Balance();
					//foreach (var node in CellTree)
					//{
					//	//TaskFactory.Add(() =>
					//	//{
					//	addNeighbours(node);
					//	//});
					//}
					if (callback != null)
						callback();
				});

			Debug.Log(CellTree.Count);
			//Debug.Log(CellTree.Height);
		}

		//protected void ScanArea(FloatWithSizeMath.FloatWithSize x, FloatWithSizeMath.FloatWithSize y, KdTree<FloatWithSizeMath.FloatWithSize, Collider2D> hitsTree, float scanAccuracy = MinScanAccuracy * 25f, OnOperationComplete callback = null)
		/// <summary>
		/// scan area with given accuracy. If scanned cell contains some GO then perform rescan this cell with double accuracy.
		/// </summary>
		/// <param name="x">x line segment of scanned area</param>
		/// <param name="y">y line segment of scanned area</param>
		/// <param name="scanAccuracy">Scan accuracy. If less then min scan accuracy return true</param>
		/// <param name="callback"></param>
		internal void ScanArea(AABB scanAreaBounds, float scanAccuracy)
		{
			if (scanAccuracy < MinScanAccuracy)
				return;
			var size = new Vector2(scanAccuracy, scanAccuracy);
			Vector2 lbPos = Vector2.zero;//left bottom position

			for (lbPos.x = scanAreaBounds.minX; lbPos.x + scanAccuracy <= scanAreaBounds.maxX; lbPos.x += scanAccuracy)
				for (lbPos.y = scanAreaBounds.minY; lbPos.y + scanAccuracy <= scanAreaBounds.maxY; lbPos.y += scanAccuracy)
				{
					Cell currrentCell = scanCell(lbPos + size / 2f, size);
					if (currrentCell == null)
						continue;

#if UNITY_EDITOR
					Color cellColor = Color.red;
					if (currrentCell.Passability <= Cell.MIN_PASSABILITY)
						cellColor = Color.black;
					else if (currrentCell.Passability < Cell.MAX_PASSABILITY)
						cellColor = Color.yellow;
					else
						cellColor = Color.white;
					cellColor.a = 0.5f;
					currrentCell.Color = randomizeColor(cellColor);
#endif
					AddCellToTree(currrentCell);
				}
		}

		/// <summary>
		/// Perform rescan of some area and check neighbor areas. If areas are same then concat this areas. Add resulting areas to CellTree
		/// </summary>
		/// <param name="x">x line segment of scanned area</param>
		/// <param name="y">y line segment of scanned area</param>
		/// <param name="scanAccuracy">Scan accuracy. If less then min scan accuracy return true</param>
		/// <returns>if cell is same type then return<code>true</code></returns>
		internal bool RescanArea(AABB scanAreaBounds, float scanAccuracy)
		{
			if (scanAccuracy < MinScanAccuracy)
				return true;
			//var previousScanAccuracy = x.Size;
			var size = new Vector2(scanAccuracy, scanAccuracy);
			Vector2 lbPos = Vector2.zero;//left bottom position
			List<Cell> resultCells = new List<Cell>(4);
			GameObject prevGameobject = null;
			bool isCellsSame = true;

			for (lbPos.x = scanAreaBounds.minX; lbPos.x + scanAccuracy <= scanAreaBounds.maxX; lbPos.x += scanAccuracy)
				for (lbPos.y = scanAreaBounds.minY; lbPos.y + scanAccuracy <= scanAreaBounds.maxY; lbPos.y += scanAccuracy)
				{
					Cell currrentCell = scanCell(lbPos + size / 2f, size);
					if (currrentCell == null)
					{
						isCellsSame = false;
						continue;
					}

					if (resultCells.Count == 0)//first init
						prevGameobject = currrentCell.GameObject;
					if (prevGameobject != currrentCell.GameObject)
						isCellsSame = false;
					prevGameobject = currrentCell.GameObject;

#if UNITY_EDITOR
					Color cellColor = Color.red;
					if (currrentCell.Passability <= Cell.MIN_PASSABILITY)
						cellColor = Color.black;
					else if (currrentCell.Passability < Cell.MAX_PASSABILITY)
						cellColor = Color.yellow;
					else
						cellColor = Color.white;
					cellColor.a = 0.5f;
					currrentCell.Color = randomizeColor(cellColor);
#endif
					resultCells.Add(currrentCell);
				}

			if (isCellsSame) return true;

			foreach (var resultCell in resultCells)
			{
				AddCellToTree(resultCell);
			}
			return false;
		}

		/// <summary>
		/// perform scan cell with given position and size. If scanned area containg GO perform rescan with double accuracy
		/// </summary>
		/// <param name="pos">Position to scan</param>
		/// <param name="size">Size of scanned are</param>
		/// <returns>if scanned area contains GO of the same type then return scanned cell, else null</returns>
		internal Cell scanCell(Vector2 pos, Vector2 size)
		{
			var currrentCell = new Cell(pos, size)
			{
				Passability = Cell.MAX_PASSABILITY
			};

			var physHit = Physics2D.OverlapBox(pos, size, 0);
			if (physHit == null) return currrentCell;

			if (size.x / 2f > MinScanAccuracy)
			{
				var isCellsSame = RescanArea(currrentCell.aabb, size.x / 2f);
				if (!isCellsSame)//was added in tree in RescanArea function
				{
					return null;
				}
			}

			if (physHit.gameObject.isStatic)
			{
				currrentCell.Passability = Cell.MIN_PASSABILITY;
				currrentCell.GameObject = physHit.gameObject;
			}

			var goType = physHit.GetComponent<MapObject>();
			if (goType != null)
			{
				if (goType.Ignored)
				{
					currrentCell.Passability = Cell.MAX_PASSABILITY;
				}
				else
				{
					currrentCell.Passability = goType.Passability;
					currrentCell.GameObject = physHit.gameObject;


					goType.AddOnTypeChangedEventHandler(this, (gameobject, type, newType) =>
					{
						var cells = GetCells(gameobject);
						foreach (var cell in cells)
						{
							cell.Passability = newType;
						}
					});
					goType.AddOnPositionChangeEventHandler(this, (gameobject, oldPosition, newPosition) =>
						{
							//todo: may be apply transform matrix for GO's cells and rescan prev bounds pos
							//var direction = newPosition - oldPosition;
							//var cells = GetCells(gameobject);
							//var newCells = new List<Cell>();
							//CellsFromGameobject[gameobject] = newCells;
							//foreach (var cell in cells)
							//{
							//    var newCellPos = Utils.convertVector2(cell.Position + direction);
							//    Cell newCell;
							//    //if (CellsList.TryGetValue(new Cell(newCellPos), out newCell))
							//    if (CellTree.TryFindValueAt(newCellPos, out newCell))
							//    {
							//        newCell.GameObject = cell.GameObject;
							//        newCell.Passability = cell.Passability;
							//    }
							//    else
							//    {
							//        newCell = new Cell(newCellPos)
							//        {
							//            Passability = cell.Passability,
							//            GameObject = cell.GameObject
							//        };
							//        addNeighbours(newCell);
							//        CellTree.Add(newCellPos, newCell);
							//    }

							//    newCells.Add(newCell);

							//    var oldCell = scanCell(cell.Position);
							//    cell.GameObject = oldCell.GameObject;
							//    cell.Passability = oldCell.Passability;
							//    if (cell.GameObject != null)
							//    {
							//        List<Cell> oldCellList;
							//        if (CellsFromGameobject.TryGetValue(cell.GameObject, out oldCellList))
							//        {
							//            oldCellList.Add(cell);
							//        }
							//        else
							//        {
							//            oldCellList = new List<Cell>() { cell };
							//            CellsFromGameobject.Add(cell.GameObject, oldCellList);
							//        }

							//    }
							//}
						})
					;
				}
			}

			return currrentCell;
		}

		internal void AddNeighbours(Cell cell)
		{
			var neighbours = GetNeighbours(cell);
			foreach (var neighbour in neighbours)
			{
				cell.AddNeighbour(neighbour);
			}
		}

		internal void RemoveNeighborhood(Cell cell, bool withLinearScan = false)
		{
			var neighbours = withLinearScan ? GetNeighbours(cell) : cell.Neighbours.Keys;
			foreach (var neighbour in neighbours)
			{
				cell.RemoveNeighbourhood(neighbour);
			}
		}

		internal IEnumerable<Cell> GetNeighbours(Cell cell)
		{

			var nearestNeighbours = new List<Cell>();
			var cellAABBExt = cell.aabb;
			nearestNeighbours.AddRange(GetCells(cellAABBExt));

			return nearestNeighbours;
		}

		/// <summary>
		/// Удаляет просканированную область
		/// </summary>
		/// <param name="leftBottomPoint">Левая нижняя точка выбранной местности</param>
		/// <param name="rightTopPoint">Правая верхняя точка выбранной иестности</param>
		/// <param name="callback">Уведомление о завершении операции</param>
		public void RemoveArea(Vector2 leftBottomPoint, Vector2 rightTopPoint, OnOperationComplete callback = null)
		{
			TaskFactory.Add(() =>
			{
				var removedArea = new AABB(leftBottomPoint, rightTopPoint);
				var removedCells = CellTree.RemoveAll(removedArea);
				foreach (var removedCell in removedCells)
				{
					foreach (var item in removedCell.Neighbours)
					{
						item.Key.Neighbours.Remove(removedCell);
					}

					List<Cell> gosCellsList = GetCells(removedCell.GameObject);
					if (gosCellsList != null)
					{
						gosCellsList.Remove(removedCell);
					}
				}


				if (callback != null)
					callback();
			});
		}

		public void ClearMap()
		{
			//CellTree.Clear();
			CellTree.Clear(true);
			CellsFromGameobject.Clear();
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

		internal void AddCellToTree(Cell cell)
		{
			AddNeighbours(cell);
			CellTree.Add(cell, cell.aabb);
		}

		internal Cell GetCell(Vector2 cellPosition)
		{
			var result = CellTree.FindValuesAt(new AABB(cellPosition, cellPosition/*,Vector2.one*MinScanAccuracy*/));
			return result.Count > 0 ? result[0] : null;
		}

		internal IEnumerable<Cell> GetCells(AABB aabb)
		{
			var result = CellTree.FindValuesAt(aabb);
			return result;
		}
	}
}

