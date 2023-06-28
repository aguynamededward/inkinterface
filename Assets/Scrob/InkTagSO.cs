using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New InkTagSO", menuName = "InkTag")]
public class InkTagSO : ScriptableObject
{
    protected virtual void OnEnable()
    {
        if(!InkTags.tagDictionary.ContainsKey(name.ToLower())) InkTags.AddTag(name.ToLower(),this);
    }

    protected virtual void ParseTagData(string _tagData)
    {

    }

}
