using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdScript : MonoBehaviour
{
    public AudioSource crowdAudioSource;
    public AudioClip crowdScreaming;
    public AudioClip crowdCheering;
    public Player player;

    bool CheeringHasSound;
    private void Start()
    {
        crowdAudioSource.clip = crowdScreaming;
        CheeringHasSound = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.IsPlayerOnRoom() && !CheeringHasSound)
        {
            StartCoroutine("CrowdCelebration");
        }
    }

    IEnumerator CrowdCelebration()
    {
        CheeringHasSound = true;
        crowdAudioSource.Stop();
        crowdAudioSource.clip = crowdCheering;
        crowdAudioSource.Play();
        yield return new WaitForSeconds(45f);
        crowdAudioSource.Stop();
        crowdAudioSource.clip = crowdScreaming;
        crowdAudioSource.Play();
    }
}
