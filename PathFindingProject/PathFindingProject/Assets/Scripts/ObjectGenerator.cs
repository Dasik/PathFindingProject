using System;
using UnityEngine;
using Random = System.Random;

public class ObjectGenerator : MonoBehaviour
{
    public GameObject AgentPrefabForInit;
    public GameObject[] ObstaclesPrefabForInit;
    public int z;
    public static ObjectGenerator Instance;
    public Transform AgentsParent;
    public Transform ObstaclesParent;
    private Random random=new Random();
    
    void Start()
    {
        Instance = this;
    }


    public void InitAgents(Vector2 leftBottomPoint, Vector2 rightTopPoint, int objectsCount)
    {
        Vector2 step = (rightTopPoint - leftBottomPoint) / (float)Math.Sqrt(objectsCount);
        leftBottomPoint += step;
        for (Vector2 pos = leftBottomPoint; pos.x < rightTopPoint.x; pos.x += step.x)
            for (pos.y = leftBottomPoint.y; pos.y < rightTopPoint.y; pos.y += step.y)
            {
                var obj = Instantiate(AgentPrefabForInit, new Vector3(pos.x, pos.y, z), Quaternion.identity);
                obj.GetComponent<Transform>().SetParent(AgentsParent);
            }
    }

    public void InitObstacles(Vector2 leftBottomPoint, Vector2 rightTopPoint, int objectsCount)
    {
        Vector2 step = (rightTopPoint - leftBottomPoint) / (float)Math.Sqrt(objectsCount);
        leftBottomPoint += step;
        for (Vector2 pos = leftBottomPoint; pos.x < rightTopPoint.x; pos.x += step.x)
            for (pos.y = leftBottomPoint.y; pos.y < rightTopPoint.y; pos.y += step.y)
            {
                var obj = Instantiate(ObstaclesPrefabForInit[random.Next(0,ObstaclesPrefabForInit.Length)],
                                        new Vector3(pos.x, pos.y, z),
                                        Quaternion.identity);
                obj.GetComponent<Transform>().SetParent(ObstaclesParent);
            }
    }


}
