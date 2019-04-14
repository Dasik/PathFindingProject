using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

internal static class TaskFactory
{

    //public delegate void TaskDelegate();
    //private static readonly Queue<InnerTask> TasksQueue = new Queue<InnerTask>();
    //private static readonly List<Thread> Workers = new List<Thread>(3);
    //private static readonly object locker = new object();
    //public const int WorkersCount = 3;

    //static TaskFactory()
    //{
    //    for (int i = 0; i < WorkersCount; i++)
    //    {
    //        InitWorker();
    //    }
    //}

    //private static void InitWorker()
    //{
    //    Thread worker = new Thread(() =>
    //    {
    //        Thread.Sleep(1000);
    //        while (true)
    //        {
    //            //try
    //            //{

    //            InnerTask task = Dequeue();
    //            if (task == null)
    //            {
    //                Debug.Log("Idle: " + Thread.CurrentThread.Name);
    //                Thread.Sleep(100);
    //            }
    //            else
    //            {
    //                task.Task.Invoke();
    //                if (task.Callback!=null)
    //                    task.Callback.Invoke();
    //                task.IsCompleted = true;
    //                Debug.Log("Working: " + Thread.CurrentThread.Name);
    //            }

    //            //}
    //            //catch (Exception ex)
    //            //{
    //            //    Debug.LogError(ex);
    //            //    InitWorker();
    //            //    throw new Exception(ex.Message,ex);
    //            //}
    //        }
    //    })
    //    {
    //        Name = "Thread #" + Workers.Count,
    //        Priority = ThreadPriority.Normal
    //    };
    //    worker.Start();
    //    Workers.Add(worker);
    //}

    //public static void Add(Action task)
    //{
    //    lock (locker)
    //    {
    //        TasksQueue.Enqueue(new InnerTask(task));
    //        //Debug.Log("Adding task to queue: " + TasksQueue.Count);
    //    }
    //}

    //public static void Add(Action task,Action callback)//todo: may be add onSuccess and onFailure
    //{
    //    lock (locker)
    //    {
    //        TasksQueue.Enqueue(new InnerTask(task,callback));
    //        //Debug.Log("Adding task to queue: " + TasksQueue.Count);
    //    }
    //}

    //private static InnerTask Dequeue()
    //{
    //    lock (locker)
    //    {
    //        return TasksQueue.Count == 0 ? null : TasksQueue.Dequeue();
    //    }
    //}

    //public class InnerTask
    //{
    //    public Action Task { get; set; }
    //    public Action Callback { get; set; }
    //    public bool IsCompleted { get; set; }

    //    public InnerTask(Action task)
    //    {
    //        Task = task;
    //        IsCompleted = false;
    //    }

    //    public InnerTask(Action task, Action callback)
    //    {
    //        Task = task;
    //        Callback = callback;
    //        IsCompleted = false;
    //    }
    //}


    public static void Add(Action task)
    {
        ThreadPool.QueueUserWorkItem(threadTask =>
        {
            try { task.Invoke(); }catch(Exception ex) { Debug.LogError(ex);}
        });
    }

    public static void Add<T>(Action<T> task,T param)
    {
        ThreadPool.QueueUserWorkItem(delegate(object state)
        {
            try { task.Invoke((T)state); } catch (Exception ex) { Debug.LogError(ex); }
        },param);
    }

    public static void Add(Action task, Action callback)//todo: may be add onSuccess and onFailure
    {

        ThreadPool.QueueUserWorkItem(threadTask =>
        {
            try { task.Invoke(); callback.Invoke(); } catch (Exception ex) { Debug.LogError(ex); }
        });
    }



    //public static void ForEach<T>(IEnumerable<T> items, Action<T> action)
    //{
    //    var resetEvents = new List<ManualResetEvent>();

    //    foreach (var item in items)
    //    {
    //        var evt = new ManualResetEvent(false);
    //        resetEvents.Add(evt);
    //        var actionInfo = new KeyValuePair<T, ManualResetEvent>(item, evt);
    //        ThreadPool.QueueUserWorkItem((infoObj) =>
    //        {
    //            var info = (KeyValuePair<T, ManualResetEvent>)infoObj;
    //            action(info.Key);
    //            info.Value.Set();
    //        }, actionInfo);
    //    }

    //    foreach (var re in resetEvents)
    //        re.WaitOne();
    //}

    //public static void For(int from,int to, Action<int> action)
    //{
    //    var resetEvents = new List<ManualResetEvent>();

    //    for (int i = from; i < to; i++)
    //    {
    //        var evt = new ManualResetEvent(false);
    //        resetEvents.Add(evt);
    //        var actionInfo=new KeyValuePair<int, ManualResetEvent>(i,evt);
    //        ThreadPool.QueueUserWorkItem((infoObj) =>
    //        {
    //            var info = (KeyValuePair<int, ManualResetEvent>) infoObj;
    //            action(info.Key);
    //            info.Value.Set();
    //        }, actionInfo);

    //    }
    //    foreach (var re in resetEvents)
    //        re.WaitOne();
    //}

    public static class Parallel
    {
        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="iterations"></param>
        /// <param name="function"></param>
        public static void For(int from, int to, Action<int> function)
        {
            int iterationsPassed = 0;
            var iterations = to - from;
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            for (int i = from; i < to; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    int currentIteration = (int)state;

                    try
                    {
                        function(currentIteration);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    if (Interlocked.Increment(ref iterationsPassed) == iterations)
                        resetEvent.Set();
                }, i);
            }

            resetEvent.WaitOne();
        }

        /// <summary>
        /// Executes a foreach loop in which iterations may run in parallel.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="function"></param>
        public static void ForEach<T>(IEnumerable<T> collection, Action<T> function)
        {
            int[] iterations = {0};
            int iterationsPassed = 0;
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            foreach (var item in collection)
            {
                Interlocked.Increment(ref iterations[0]);
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    T subject = (T)state;

                    try
                    {
                        function(subject);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    if (Interlocked.Increment(ref iterationsPassed) == iterations[0])
                        resetEvent.Set();

                }, item);
            }

            resetEvent.WaitOne();
        }
    }

    public class InnerTask
    {
        public Action Task { get; set; }
        public Action Callback { get; set; }
        public bool IsCompleted { get; set; }

        public InnerTask(Action task)
        {
            Task = task;
            IsCompleted = false;
        }

        public InnerTask(Action task, Action callback)
        {
            Task = task;
            Callback = callback;
            IsCompleted = false;
        }
    }
}
