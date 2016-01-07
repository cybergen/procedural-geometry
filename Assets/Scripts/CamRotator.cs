using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CamRotator : MonoBehaviour
{
    public float RotationSpeed;

    private void Update()
    {
        var euler = transform.rotation.eulerAngles;
        euler.y += RotationSpeed;
        transform.rotation = Quaternion.Euler(euler);
    }
}
