using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    //index 0 -> up; index 1 -> right; index 2 -> down; index 3 -> left;
    [Tooltip("Each index indicates the possible connection with each side.")]
    public int[] sideIndex;

}
