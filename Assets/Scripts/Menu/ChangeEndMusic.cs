using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeEndMusic : MonoBehaviour
{
    public AudioClip nextClip;
    public AudioSource audioSource;

    float clipDuration;
    float time;
    bool clipChanged;

    private void Start()
    {
        clipDuration = audioSource.clip.length;
        clipChanged = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!clipChanged)
        {
            time += Time.deltaTime;
            if (time > clipDuration)
            {
                audioSource.clip = nextClip;
                audioSource.Play();
                clipChanged = true;
            }
        }
    }
}
