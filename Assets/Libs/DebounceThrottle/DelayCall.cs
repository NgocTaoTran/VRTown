using System;
using System.Collections.Generic;

public class QueueAction
{
    public long expire;
    public Action action;
    public QueueAction(Action action, long expire)
    {
        this.expire = expire;
        this.action = action;
    }
}

public class DelayCall
{
    private List<QueueAction> actions = new List<QueueAction>();

    public DelayCall()
    {
    }

    public void Add(Action action, long delayTime)
    {
        actions.Add(new QueueAction(action, DateTimeOffset.Now.ToUnixTimeMilliseconds() + delayTime));
    }

    public void Check()
    {
        var time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        for (var i = 0; i < actions.Count; i++)
        {
            var action = actions[i];
            if (action.expire <= time)
            {
                action.action();
                actions.RemoveAt(i);
                i--;
            }
        }
    }
}
