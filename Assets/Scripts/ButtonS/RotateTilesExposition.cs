using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTilesExposition : MonoBehaviour
{
    public bool rotate;
    public Transform rotTarget;

    Quaternion originalRot;
    Vector3 dir;
    // Start is called before the first frame update
    void Start()
    {
        rotate = false;
        dir = (rotTarget.position - transform.position).normalized;
        originalRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (rotate)
        {
            transform.Rotate(dir, Time.deltaTime * 20f);
        }
        else if (transform.rotation != originalRot)
        {
            transform.rotation = originalRot;
        }
    }
}
