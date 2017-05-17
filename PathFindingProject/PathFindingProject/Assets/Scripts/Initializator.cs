using System.Collections;
using Dasik.PathFinder;
using UnityEngine;

public class Initializator : MonoBehaviour
{
    public int AgentsCount = 50;
    public int ObstaclesCount = 50;
    public Map CurrentMap;

    void Start()
    {
        StartCoroutine(StartInitAgents(new Vector2(50, 50),
                                            new Vector2(65, 65),
                                            AgentsCount));
        StartCoroutine(StartInitObstacles(new Vector2(-40, -20),
                                            new Vector2(-10, 20),
                                            ObstaclesCount));
        CurrentMap.ScanArea(new Vector2(-100, -50),
                            new Vector2(0, 50));
        //CurrentMap.ScanArea(new Vector2(-10, -10),
        //            new Vector2(0, 0));
    }

    IEnumerator StartInitAgents(Vector2 leftBottomPoint, Vector2 rightTopPoint, int objectsCount)
    {
        while (ObjectGenerator.Instance==null)
            yield return new WaitForEndOfFrame();
        ObjectGenerator.Instance.InitAgents(leftBottomPoint,rightTopPoint,objectsCount);
    }

    IEnumerator StartInitObstacles(Vector2 leftBottomPoint, Vector2 rightTopPoint, int objectsCount)
    {
        while (ObjectGenerator.Instance == null)
            yield return new WaitForEndOfFrame();
        ObjectGenerator.Instance.InitObstacles(leftBottomPoint, rightTopPoint, objectsCount);
    }
}
