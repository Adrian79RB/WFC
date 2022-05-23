using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdGeneration : MonoBehaviour
{
    public Transform colliseumCentre;
    public GameObject[] crowdCharacters;

    // Start is called before the first frame update
    void Start()
    {
        //Random generation of the characters that take up the crowd positions
        for (int i = 0; i < transform.childCount; i++)
        {
            var pos = Mathf.RoundToInt(Random.value * (crowdCharacters.Length - 1));
            Vector3 dir = (colliseumCentre.position - transform.GetChild(i).position).normalized;
            Quaternion newRotation = Quaternion.FromToRotation(transform.GetChild(i).forward, dir);
            Transform newCharacter = Instantiate(crowdCharacters[pos], transform.GetChild(i).position, newRotation, transform.GetChild(i)).transform;

            newCharacter.rotation = Quaternion.Euler(0f, newCharacter.rotation.eulerAngles.y, 0f);
        }
    }
}
