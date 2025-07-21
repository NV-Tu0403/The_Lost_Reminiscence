#nullable enable
using System.Collections.Generic;

public class TaskQueue
{
    private Queue<FaTask> queue = new Queue<FaTask>();

    public void AddTask(FaTask task)
    {
        queue.Enqueue(task);
    }

    public FaTask? GetNextTask()
    {
        return queue.Count > 0 ? queue.Dequeue() : null;
    }

    public bool HasTask()
    {
        return queue.Count > 0;
    }

    public void Clear()
    {
        queue.Clear();
    }
} 