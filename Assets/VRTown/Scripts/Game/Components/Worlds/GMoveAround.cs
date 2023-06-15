using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMoveAround : MonoBehaviour
{
    float speed = 1.0f;
    private float t = 0.0f;
    private float w;
    void OnEnable()
    {
        StartCoroutine(Move());
    }
    void OnDisable()
    {
        StopCoroutine(Move());
    }
    IEnumerator Move()
    {
        w = speed * Mathf.PI / 15.0f;
        while (true)
        {
            t += Time.deltaTime;
            float angle = w * t;
            float x = Mathf.Cos(angle) * 15.0f;
            float z = Mathf.Sin(angle) * 15.0f;
            transform.position = new Vector3(x, 10.0f, z);
            transform.LookAt(new Vector3(0, 10, 0), Vector3.up);
            yield return null;
        }
    }
}
