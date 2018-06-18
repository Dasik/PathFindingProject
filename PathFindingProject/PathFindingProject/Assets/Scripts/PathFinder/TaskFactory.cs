using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

internal static class TaskFactory
{

    //public delegate void TaskDelegate();
    private static readonly Queue<Action> TasksQueue = new Queue<Action>();
    private static readonly List<Thread> Workers = new List<Thread>(3);
    private static readonly object locker = new object();
    public const int WorkersCount = 5;

    static TaskFactory()
    {
        for (int i = 0; i < WorkersCount; i++)
        {
            InitWorker();
        }
    }

    private static void InitWorker()
    {
        Thread worker = new Thread(() =>
        {
            Thread.Sleep(1000);
            while (true)
            {
                //try
                //{

                Action task = Dequeue();
                if (task == null)
                    Thread.Sleep(10);
                else
                    task.Invoke();
                Debug.Log("Working: " + Thread.CurrentThread.Name);
                //}
                //catch (Exception ex)
                //{
                //    Debug.LogError(ex);
                //    InitWorker();
                //    throw new Exception(ex.Message,ex);
                //}
            }
        })
        {
            Name = "Thread #" + Workers.Count,
            Priority = ThreadPriority.Normal
        };
        worker.Start();
        Workers.Add(worker);
    }

    public static void Add(Action task)
    {
        lock (locker)
        {
            TasksQueue.Enqueue(task);
            //Debug.Log("Adding task to queue: " + TasksQueue.Count);
        }
    }

    private static Action Dequeue()
    {
        lock (locker)
        {
            return TasksQueue.Count == 0 ? null : TasksQueue.Dequeue();
        }
    }

}
