using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Dasik.PathFinder;
using Debug = UnityEngine.Debug;

public class Drawpath : MonoBehaviour
{
    /// <summary>
    /// Модуль, выполняющий оптимизацию пути
    /// </summary>
    public PathFinding PathFinding;
    /// <summary>
    /// Результирующий путь
    /// </summary>
    private List<Cell> path;
    /// <summary>
    /// Координаты начальной точки
    /// </summary>
    public Vector2 StartPoint;
    /// <summary>
    /// Координаты точки назначения
    /// </summary>
    public Vector2 GoalPoint;

    void Start()
    {
        EventManager.OnMapScanned.Add(10000, () =>
        {
            Debug.Log("Starting founding pathing)");
            System.GC.Collect();
            var memoryStart = System.GC.GetTotalMemory(true);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            currentThread= PathFinding.GetPathAsync<object>(StartPoint,
                GoalPoint,
                (param, list) =>
                {
                    path = list;
                    currentThread = -1;
                    sw.Stop();
                    System.GC.Collect();
                    var memoryEnd = System.GC.GetTotalMemory(true);
                    Debug.Log("Path finded for " + sw.Elapsed + ", RAM used " + (memoryEnd - memoryStart) / 1024 / 1024 + "MB");//approximate value
                });
        });
        prevStartPos = StartPoint;
        prevGoalPos = GoalPoint;
    }


    private Vector2 prevStartPos = Vector2.zero;
    private Vector2 prevGoalPos = Vector2.zero;
    private long currentThread = -1;
    void Update()
    {
        if (((prevStartPos - StartPoint).sqrMagnitude > 0.1) ||
            (prevGoalPos - GoalPoint).sqrMagnitude > 0.1)
        {
            if (currentThread != -1)
                PathFinding.ClosePathFindingThread(currentThread);
            if (path!=null)
            path.Clear();
            Debug.Log("Starting founding pathing)");
            System.GC.Collect();
            var memoryStart = System.GC.GetTotalMemory(true);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            currentThread = PathFinding.GetPathAsync<object>(StartPoint,
                GoalPoint,
                (param, list) =>
                {
                    path = list;
                    currentThread = -1;
                    sw.Stop();
                    System.GC.Collect();
                    var memoryEnd = System.GC.GetTotalMemory(true);
                    Debug.Log("Path finded for " + sw.Elapsed + ", RAM used " + (memoryEnd - memoryStart) / 1024 / 1024 + "MB");//approximate value
                });
            prevStartPos = StartPoint;
            prevGoalPos = GoalPoint;
        }

    }

    void OnDrawGizmosSelected()
    {
        if (path != null)
            //визуализация проложенного пути
            DrawPath();
    }

    private void DrawPath()
    {
        if (path.Count==0)
            return;
        var previous = path[0];
        foreach (var item in path)
        {
            Debug.DrawLine(previous.Position, new Vector3(item.Position.x, item.Position.y, 0),Color.green);
            previous = item;
        }
    }
}

