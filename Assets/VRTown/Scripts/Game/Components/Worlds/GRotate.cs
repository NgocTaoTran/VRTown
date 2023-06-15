using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GRotate : MonoBehaviour
{
    public float rotationSpeed;

    void OnEnable()
    {
        StartCoroutine(Rotate());
    }
    void OnDisable()
    {
        StopCoroutine(Rotate());
    }
    private IEnumerator Rotate()
    {
        rotationSpeed = 5f;
        while (true)
        {
            transform.Rotate(Vector3.up, 360f / rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
