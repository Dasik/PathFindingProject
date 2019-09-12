using System;
using System.Collections.Generic;


/// <summary>
/// Biggest priority has a biggest priority 
/// </summary>
/// <typeparam name="T"> Type of value</typeparam>
public class PriorityQueue<T>
{
    private List<KeyValuePair<int, T>> _queue = new List<KeyValuePair<int, T>>();

    public void Add(int priority, T value)
    {
        int index = 0;
        while (index <_queue.Count && _queue[index].Key > priority)
            index++;
        _queue.Insert(index, new KeyValuePair<int, T>(priority, value));
    }

    public bool RemoveAllValues(T value)
    {
        return _queue.RemoveAll(pair => pair.Value.Equals(value)) > 1;
    }

    public void ForEach(Action<T> action)
    {
        foreach (var item in _queue)
        {
            action.Invoke(item.Value);
        }
    }

    public void Clear()
    {
        _queue.Clear();
    }
}
