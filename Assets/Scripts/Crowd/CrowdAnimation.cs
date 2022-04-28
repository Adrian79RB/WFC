using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdAnimation : MonoBehaviour
{
    Animator anim;

    float animTimer;
    float animTime;
    float waitTimer;
    float waitTime;

    bool isFinished;
    bool isStarted;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        isFinished = false;
        isStarted = false;

        animTimer = 30f;
        animTime = 0f;
        waitTimer = 15f;
        waitTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3(0f, 0f, 0f);

        if (!isStarted)
        {
            isStarted = true;
            var value = Random.value;
            if (value < 0.25)
                anim.SetInteger("Selection", 1);
            else if (value < 0.5)
                anim.SetInteger("Selection", 2);
            else if (value < 0.75)
                anim.SetInteger("Selection", 3);
        }
        else if(!isFinished)
        {
            animTime += Time.deltaTime;
            if(animTime > animTimer)
            {
                isFinished = true;
                animTime = 0f;

                anim.SetBool("IsFinished", isFinished);
                anim.SetInteger("Selection", 0);
            }
        }
        else
        {
            waitTime += Time.deltaTime;
            if(waitTime > waitTimer)
            {
                waitTime = 0f;
                isStarted = false;
                isFinished = false;
            }
        }
    }
}
