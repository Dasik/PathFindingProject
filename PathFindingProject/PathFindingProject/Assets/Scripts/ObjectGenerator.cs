using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class ObjectGenerator : MonoBehaviour
{
    public GameObject AgentPrefabForInit;
    public GameObject[] ObstaclesPrefabForInit;
    public GameObject PlayerObstacle;
    public int z;
    public static ObjectGenerator Instance;
    public Transform AgentsParent;
    public Transform ObstaclesParent;
    private Random random = new Random();
    public List<GameObject> Agents;

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
                InitAgent(new Vector3(pos.x, pos.y, z));
            }
    }

    public void InitObstacles(Vector2 leftBottomPoint, Vector2 rightTopPoint, int objectsCount)
    {
        Vector2 step = (rightTopPoint - leftBottomPoint) / (float)Math.Sqrt(objectsCount);
        leftBottomPoint += step;
        for (Vector2 pos = leftBottomPoint; pos.x < rightTopPoint.x; pos.x += step.x)
            for (pos.y = leftBottomPoint.y; pos.y < rightTopPoint.y; pos.y += step.y)
            {
                var obj = Instantiate(ObstaclesPrefabForInit[random.Next(0, ObstaclesPrefabForInit.Length)],
                                        new Vector3(pos.x, pos.y, z),
                                        Quaternion.identity,
                                        ObstaclesParent);
            }
    }

    public void InitAgent(Vector2 position)
    {
        InitAgent(new Vector3(position.x, position.y, z));
    }

    public void InitAgent(Vector3 position)
    {
        var obj = Instantiate(AgentPrefabForInit, position, Quaternion.identity, AgentsParent);
        Agents.Add(obj);
    }

    public void InitPlayerObstacle(Vector2 position)
    {
        InitPlayerObstacle(new Vector3(position.x, position.y, z));
    }

    public void InitPlayerObstacle(Vector3 position)
    {
        var obj = Instantiate(PlayerObstacle,
                                position,
                                Quaternion.identity,
                                ObstaclesParent);
    }
}
