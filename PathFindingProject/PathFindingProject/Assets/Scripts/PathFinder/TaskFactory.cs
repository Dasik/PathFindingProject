using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class TaskFactory : MonoBehaviour
{
    public static TaskFactory Instance;
    public delegate void TaskDelegate();
    private readonly Queue<TaskDelegate> TasksQueue = new Queue<TaskDelegate>();

    void Start()
    {
        Instance = this;
        StartCoroutine(worker());
    }

    private IEnumerator worker()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();//TODO: Change to value based on FPS
            try
            {
                if (TasksQueue.Count != 0)
                {
                    var task = TasksQueue.Dequeue();
                    task.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }
    }

    public void Add(TaskDelegate task)
    {
        TasksQueue.Enqueue(task);
    }
}
