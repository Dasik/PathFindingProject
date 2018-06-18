using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

namespace Dasik.PathFinder
{
    public class PathFinding : MonoBehaviour
    {
        public delegate void ReturnedPath<T>(T param, List<Vector2> path) where T : class;
        public delegate void ReturnedPathes<T>(T param, Dictionary<T, List<Vector2>> pathes) where T : class;

        private Map map;
        private SyncList<long> _runnedThreads = new SyncList<long>();
        private long _threadIdGenerator = 0l;



        void Start()
        {
            map = Map.Instance;
        }
        public long GetPathes<T>(Dictionary<T, Vector2> objects, Vector2 goalPosition,
            ReturnedPathes<T> callback, T param = null, double accuracy = 1) where T : class
        {
            T nearestObject = null;
            var nearestDistance = float.MaxValue;
            foreach (var item in objects)
            {
                var dist = Vector2.SqrMagnitude(goalPosition - item.Value);
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearestObject = item.Key;
                }
            }
            if (nearestObject == null)
                return -1;


            //List<long> localThreadIds = new List<long>();
            var thrID = Interlocked.Increment(ref _threadIdGenerator);

            Thread worker = new Thread(() => GetPathesParallelWorking<T>(objects, nearestObject, goalPosition, thrID,
                callback, accuracy, param))
            {
                Priority = ThreadPriority.Normal,
                Name = "PathesFinding"
            };
            _runnedThreads.Add(thrID);
            worker.Start();
            return thrID;
        }

        private void GetPathesParallelWorking<T>(Dictionary<T, Vector2> objects, T nearestObject, Vector2 goalPosition, long threadId,
            ReturnedPathes<T> callback, double accuracy, T param) where T : class
        {
            List<long> localThreadIds = new List<long>();
            List<Vector2> nearestPath = new List<Vector2>();
            Debug.Log("Nearest: StartPos: " + objects[nearestObject] + "\tGoalPos: " + goalPosition);
            localThreadIds.Add(
                GetPath<T>(objects[nearestObject], goalPosition, (ident, path) =>
                {
                    if (path == null || path.Count == 0)
                    {
                        nearestPath = null;
                        Debug.Log("Nearest path was not found");
                        _runnedThreads.RemoveAll(l => localThreadIds.Contains(l));
                    }
                    else
                    {
                        nearestPath.AddRange(path);
                        Debug.Log("Nearest path was found: " + nearestPath.Count);
                    }
                }, accuracy)
            );
            var result = new Dictionary<T, List<Vector2>>();
            long completedCount = 0;
            foreach (var item in objects)
            {
                localThreadIds.Add(
                    GetPath<T>(item.Value, objects[nearestObject], (ident, path) =>
                    {
                        completedCount++;
                        if (path == null)
                        {
                            return;
                        }
                        result.Add(ident, path);
                    }, accuracy, item.Key));
            }
            while (completedCount != objects.Count &&
                   _runnedThreads.Contains(threadId))
                ;
            while (nearestPath != null &&
                    nearestPath.Count == 0 &&
                   _runnedThreads.Contains(threadId))
                ;
            if (!_runnedThreads.Contains(threadId) || nearestPath == null)
            {
                _runnedThreads.RemoveAll(l => localThreadIds.Contains(l));
                if (callback != null)
                    callback(param, null);
                return;
            }
            foreach (var item in result.Keys)
            {
                //result[item].InsertRange(0, nearestPath);
                result[item].AddRange(nearestPath);
            }
            if (callback != null)
            {
                callback(param, result);
            }
        }

        public long GetPath<T>(Vector2 startPosition, Vector2 goalPosition,
            ReturnedPath<T> callback = null, double accuracy = 1, T param = null) where T : class
        {
            startPosition = Utils.floorVector2(startPosition);
            goalPosition = Utils.floorVector2(goalPosition);

            var startCell = Utils.GetCell(startPosition, map.CellsList);
            var goalCell = Utils.GetCell(goalPosition, map.CellsList);
            if (startCell == null || goalCell == null)
            {
                if (callback != null)
                    callback(param, null);
                Debug.Log("position not scanned");
                return -1;
            }
            if (startCell.Type == CellType.Static)
            {
                bool freeFounded = false;
                foreach (var item in startCell.Neighbours)
                {
                    if (item.Type == CellType.Free ||
                        item.Type == CellType.Unwanted)
                    {
                        startCell = item;
                        freeFounded = true;
                        break;
                    }
                }
                if (!freeFounded)
                {
                    if (callback != null)
                        callback(param, null);
                    Debug.Log("start cell in obstacle");
                    return -1;
                }
            }
            if (goalCell.Type == CellType.Static)
            {
                bool freeFounded = false;
                foreach (var item in startCell.Neighbours)
                {
                    if (item.Type == CellType.Free ||
                        item.Type == CellType.Unwanted)
                    {
                        startCell = item;
                        freeFounded = true;
                        break;
                    }
                }
                if (!freeFounded)
                {
                    if (callback != null)
                        callback(param, null);
                    Debug.Log("goal cell in obstacle");
                    return -1;
                }
            }
            //open.Clear();
            //sortedOpen.Clear();
            //closed.Clear();
            //var start = new Node(startCell)
            //{
            //    g = 0d,
            //    h = GetDistance(goalPosition, startPosition) * accuracy
            //};
            //start.f = start.g + start.h;
            //PutToOpen(start);
            var thrID = Interlocked.Increment(ref _threadIdGenerator);
            Thread worker = new Thread(() => GetPathParallelWorking(startCell, goalCell, accuracy, callback, thrID, param))
            {
                Priority = ThreadPriority.Normal,
                Name = "PathFinding"
            };
            _runnedThreads.Add(thrID);
            worker.Start();
            return thrID;
        }

        private void GetPathParallelWorking<T>(Cell startCell, Cell goalCell, double accuracy,
            ReturnedPath<T> callback, long threadId, T param = null) where T : class
        {
            //TODO: Add getting from cache

            bool closeThread = false;
            long lngth = 0;
            Dictionary<Cell, Node> closed = new Dictionary<Cell, Node>();
            Dictionary<Cell, Node> open = new Dictionary<Cell, Node>();
            List<Node> sortedOpen = new List<Node>();
            try
            {

                var PutToOpen = new Action<Node>(node =>
                {
                    for (int i = 0; i < open.Count; i++)
                    {
                        if (sortedOpen[i].f >= node.f)
                        {
                            sortedOpen.Insert(i, node);
                            open.Add(node.Cell, node);
                            return;
                        }
                    }

                    sortedOpen.Insert(open.Count, node);
                    open.Add(node.Cell, node);
                });

                var PopFromOpen = new Func<Node>(() =>
                {
                    var result = sortedOpen[0];
                    sortedOpen.RemoveAt(0);
                    open.Remove(result.Cell);
                    return result;
                });
                var start = new Node(startCell)
                {
                    g = 0d,
                    h = GetDistance(goalCell.Position, startCell.Position) * accuracy
                };
                start.f = start.g + start.h;
                PutToOpen(start);
                Debug.Log("PathFinding: StartPos: " + startCell.Position + "\tGoalPos: " + goalCell.Position +
                          "\n This Thread Id: " + threadId + "\n Runned threads count: " + _runnedThreads.Count);
                while (open.Count != 0 && !closeThread)
                {
                    lngth = open.Count;
                    //Thread.Sleep(1);
                    var x = PopFromOpen();
                    if (x.Cell.Equals(goalCell))
                    {
                        if (!closeThread)
                        {
                            callback(param, constructPath(x));
                        }
                        _runnedThreads.Remove(threadId);
                        Debug.Log("Goal founded");
                        return;
                    }
                    closed.Add(x.Cell, x);
                    foreach (var yy in x.Cell.Neighbours)
                    {
                        if (yy == null)
                            continue;

                        if (yy.Type == CellType.Static)
                        {
                            continue;
                        }
                        //если текущий сосед содержится в списке просмотренных вершин, то пропустить его
                        if (closed.ContainsKey(yy)) continue;
                        bool tentativeIsBetter = true;
                        //var tentativeGScore = x.g + 1d;//1d-расстояние между х и соседом
                        var tentativeGScore = x.g;
                        if (Math.Abs(x.Cell.Position.x - yy.Position.x) < 0.005d ||
                            Math.Abs(x.Cell.Position.y - yy.Position.y) < 0.005d)
                            tentativeGScore += 1d;
                        else
                            tentativeGScore += 1.414d;

                        if (yy.Type == CellType.Unwanted)
                            tentativeGScore += 1d;
                        //Получаем y из open

                        Node y;
                        if (!open.TryGetValue(yy, out y))
                        {
                            y = new Node(yy);
                            //tentativeIsBetter = true;
                        }
                        else
                        {
                            if (tentativeGScore < y.g)
                            {
                                //tentativeIsBetter = true;
                                open.Remove(yy);
                                sortedOpen.Remove(y);
                            }
                            else
                                tentativeIsBetter = false;
                        }
                        if (tentativeIsBetter)
                        {
                            y.Parent = x;
                            y.g = tentativeGScore;
                            //y.h = Vector2.Distance(y.Cell.Position,  goalCell.Position);
                            y.h = GetDistance(y.Cell.Position, goalCell.Position) * accuracy;
                            y.f = y.g + y.h;
                            PutToOpen(y);
                        }
                    }
                    closeThread = !_runnedThreads.Contains(threadId);
                }
                Debug.Log("Goal not founded: StartPos: " + startCell.Position + "\tGoalPos: " + goalCell.Position + "length: " + lngth + "" + closeThread);
                callback(param, null);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
            finally
            {
                open.Clear();
                sortedOpen.Clear();
                closed.Clear();
                _runnedThreads.Remove(threadId);
            }
        }

        //private void PutToOpen(Node node)
        //{
        //    for (int i = 0; i < open.Count; i++)
        //        if (sortedOpen[i].f >= node.f)
        //        {
        //            sortedOpen.Insert(i, node);
        //            open.Add(node.Cell, node);
        //            return;
        //        }
        //    sortedOpen.Insert(open.Count, node);
        //    open.Add(node.Cell, node);
        //}

        //private Node PopFromOpen()
        //{
        //    var result = sortedOpen[0];
        //    sortedOpen.RemoveAt(0);
        //    open.Remove(result.Cell);
        //    return result;
        //}

        private double GetDistance(Vector2 v1, Vector2 v2)
        {
            //return Math.Abs(v1.x - v2.x) + Math.Abs(v1.y - v2.y);
            //return Vector2.Distance(v1, v2);
            //return (v1 - v2).sqrMagnitude;
            float z = (v1 - v2).sqrMagnitude;
            if (z == 0) return 0;
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

        private List<Vector2> constructPath(Node node)
        {
            var result = new List<Vector2>();
            var currentNode = node;
            while (currentNode != null)
            {
                result.Insert(0, currentNode.Cell.Position);
                currentNode = currentNode.Parent;
            }
            return result;
        }

        void OnDestroy()
        {
            _runnedThreads.Clear();
        }

        public bool closePathFindingThread(long threadId)
        {
            return _runnedThreads.Remove(threadId);
        }
    }

    internal class Node
    {
        public Cell Cell;
        public Node Parent;
        public double g;
        public double h;
        public double f;

        public Node(Cell cell, Node parent = null)
        {
            Cell = cell;
            Parent = parent;
        }
    }
}
