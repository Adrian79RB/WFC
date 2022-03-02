using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTilesExposition : MonoBehaviour
{
    public bool rotate;

    Vector3 dir;
    // Start is called before the first frame update
    void Start()
    {
        rotate = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (rotate)
        {

            transform.Rotate(Vector3.up, Time.deltaTime * 20f);
        }
    }
}
