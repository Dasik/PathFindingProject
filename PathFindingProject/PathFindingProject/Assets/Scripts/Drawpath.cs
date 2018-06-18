using System.Collections.Generic;
using UnityEngine;
using Dasik.PathFinder;

public class Drawpath : MonoBehaviour
{
    /// <summary>
    /// Модуль, выполняющий оптимизацию пути
    /// </summary>
    public PathFinding PathFinding;
    /// <summary>
    /// Результирующий путь
    /// </summary>
    private List<Vector2> path;
    /// <summary>
    /// Координаты начальной точки
    /// </summary>
    public Vector2 StartPoint;
    /// <summary>
    /// Координаты точки назначения
    /// </summary>
    public Vector2 GoalPoint;


    void OnDrawGizmosSelected()
    {
        if (path == null)
        {
            path = new List<Vector2>(0);
            //получение пути
            //Как только алгоритм оптимизации пути завершит свою работу начнется визуализация проложенного пути
            PathFinding.GetPath<object>(StartPoint,
                GoalPoint,
                (param,list) => path = list);
        }else
            //визуализация проложенного пути
            DrawPath();
    }

    private void DrawPath()
    {
        foreach (var item in path)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(new Vector3(item.x, item.y, 0), Vector3.one);
        }
    }
}

