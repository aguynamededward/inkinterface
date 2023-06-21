using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New InkTagSO", menuName = "InkTag")]
public class InkTagSO : ScriptableObject
{
    private void Awake()
    {
        InkTags.AddTag(name.ToLower(), this);
    }


}
