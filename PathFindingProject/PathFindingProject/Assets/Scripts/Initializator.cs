using System.Collections;
using System.Diagnostics;
using Dasik.PathFinder;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Initializator : MonoBehaviour
{
    public int AgentsCount = 50;
    public int ObstaclesCount = 50;
    public Map CurrentMap;
	public AreaRange AgentsArea1;
	public AreaRange AgentsArea2;
	public AreaRange ObstaclesArea;
    public AreaRange ScanArea;
    public AreaRange RemoveArea;

    void Start()
    {
	    StartCoroutine(StartInitAgents(AgentsArea1.LeftBottomPoint,
		    AgentsArea1.RightTopPoint,
		    AgentsCount/2));
	    StartCoroutine(StartInitAgents(AgentsArea2.LeftBottomPoint,
		    AgentsArea2.RightTopPoint,
		    AgentsCount/2));
		StartCoroutine(StartInitObstacles(ObstaclesArea.LeftBottomPoint,
                                            ObstaclesArea.RightTopPoint,
                                            ObstaclesCount));
#if UNITY_EDITOR
	    Stopwatch sw = new Stopwatch();
	    sw.Start();
#endif
		CurrentMap.ScanArea(ScanArea.LeftBottomPoint,
                            ScanArea.RightTopPoint,callback:() =>
	        {
#if UNITY_EDITOR
		        sw.Stop();
		        Debug.Log("Scanned in: " + sw.Elapsed);
#endif
				Debug.Log("Scan complete");
		        CurrentMap.RemoveArea(RemoveArea.LeftBottomPoint,
			        RemoveArea.RightTopPoint);
			});
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
