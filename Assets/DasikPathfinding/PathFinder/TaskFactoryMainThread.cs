using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskFactoryMainThread : MonoBehaviour
{
    public static TaskFactoryMainThread Instance;
    private static readonly Queue<TaskFactory.InnerTask> TasksQueue = new Queue<TaskFactory.InnerTask>();
    public const int WorkersCount = 3;
    // Use this for initialization
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        StartCoroutine(Work());
    }

    private IEnumerator Work()
    {
        while (true)
        {
            TaskFactory.InnerTask task = Dequeue();
            if (task == null)
            {

                yield return new WaitForEndOfFrame();
            }
            else
            {
                task.Task.Invoke();
                if (task.Callback != null)
                    task.Callback.Invoke();
                task.IsCompleted = true;
            }
        }

    }

    public static void Add(Action task)
    {
        TasksQueue.Enqueue(new TaskFactory.InnerTask(task));

    }

    public static void Add(Action task, Action callback)//todo: may be add onSuccess and onFailure
    {
        TasksQueue.Enqueue(new TaskFactory.InnerTask(task, callback));

    }

    public static void ExecuteSync(Action task)
    {
        var innerTask = new TaskFactory.InnerTask(task);
        TasksQueue.Enqueue(innerTask);
        while (!innerTask.IsCompleted)
        { }
        return;
    }

    private static TaskFactory.InnerTask Dequeue()
    {
        return TasksQueue.Count == 0 ? null : TasksQueue.Dequeue();
    }
}
