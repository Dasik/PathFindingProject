using System.Collections;
using Dasik.PathFinder;
using UnityEngine;

public class Initializator : MonoBehaviour
{
    public int AgentsCount = 50;
    public int ObstaclesCount = 50;
    public Map CurrentMap;
    public AreaRange AgentsArea;
    public AreaRange ObstaclesArea;
    public AreaRange ScanArea;

    void Start()
    {
        StartCoroutine(StartInitAgents(AgentsArea.LeftBottomPoint,
                                            AgentsArea.RightTopPoint,
                                            AgentsCount));
        StartCoroutine(StartInitObstacles(ObstaclesArea.LeftBottomPoint,
                                            ObstaclesArea.RightTopPoint,
                                            ObstaclesCount));
        CurrentMap.ScanArea(ScanArea.LeftBottomPoint,
                            ScanArea.RightTopPoint,
                            null);
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

    [System.Serializable]
    public class AreaRange
    {
        public Vector2 LeftBottomPoint;
        public Vector2 RightTopPoint;
    }
}
