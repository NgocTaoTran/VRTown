using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRepeatRotation : MonoBehaviour
{
    [SerializeField] float _stepAngle;

    void Start()
    {
        StopAllCoroutines();
        StartCoroutine(CoroutineRepeatRotation());
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    IEnumerator CoroutineRepeatRotation()
    {
        float rotation = 0;
        while (true)
        {
            rotation += _stepAngle;
            this.transform.eulerAngles = new Vector3(0, 0, rotation);
            yield return null;
        }
    }
}
