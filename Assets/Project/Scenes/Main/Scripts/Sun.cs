﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.right, 1f * Time.deltaTime);
        transform.LookAt(Vector3.zero);
    }
}
