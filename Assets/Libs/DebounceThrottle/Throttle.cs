using System;
using System.Collections;
using UnityEngine;

public class Throttle
{
    private int time = 1000;
    private IEnumerator task;
    private long firstClick = 0;
    private MonoBehaviour behaviour;

    public Throttle(MonoBehaviour behaviour, int time)
    {
        this.time = time;
        this.behaviour = behaviour;
    }

    public void Add(IEnumerator task)
    {
        this.task = task;
        if (firstClick == 0)
        {
            firstClick = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }

    public void Check()
    { 
        if (firstClick > 0 && task != null)
        {
            long delta = DateTimeOffset.Now.ToUnixTimeMilliseconds() - firstClick;
            if (delta >= time)
            {
                behaviour.StartCoroutine(task);
                firstClick = 0;
                task = null;
            }
        }
    }
}
