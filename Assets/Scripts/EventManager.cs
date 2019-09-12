using System;

public class EventManager
{
    public static EventsContainer OnStartNewGame = new EventsContainer();
    public static EventsContainer OnGameover = new EventsContainer();
    public static EventsContainer OnBackKeyPressed = new EventsContainer();
    public static EventsContainer OnGameLoaded = new EventsContainer();
    public static EventsContainer OnLanguageChanged = new EventsContainer();
    public static EventsContainer OnNewWave = new EventsContainer();
    public static EventsContainer OnMapScanned = new EventsContainer();
}

public class EventsContainer
{
    private readonly PriorityQueue<Action> priorityQueue;
    public EventsContainer()
    {
        priorityQueue = new PriorityQueue<Action>();
    }

    public void Add(int priority, Action action)
    {
        priorityQueue.Add(priority, action);
    }

    public bool Remove(Action action)
    {
        return priorityQueue.RemoveAllValues(action);
    }

    public void Invoke()
    {
        priorityQueue.ForEach(action =>
            {
                //try
                //{
                action.Invoke();
                //}
                //catch (Exception e)
                //{
                //    Debug.LogError(e);
                //}
            }
        );
    }
}