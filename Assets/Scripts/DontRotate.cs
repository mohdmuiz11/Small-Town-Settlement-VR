using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontRotate : MonoBehaviour
{
    private bool isRotating;

    private void OnEnable()
    {
        isRotating = true;
    }

    private void OnDisable()
    {
        isRotating = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRotating)
        {
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }
}
