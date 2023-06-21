using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Lower-case string tags correlate to scriptable objects.  What exactly I do with them, I'm not sure yet. 


 */
public static class InkTags
{
    public static Dictionary<string, InkTagSO> tagDictionary = new Dictionary<string, InkTagSO>();

    public static void AddTag(string hash, InkTagSO inkTagSO)
    {
        if (!tagDictionary.ContainsKey(hash)) tagDictionary.Add(hash, inkTagSO);
    }

    
    public static InkTagSO ParseTags(string rawTag)
    {
        // Parse through all the possible tags, store them in the inkPar
        rawTag = rawTag.ToLower();
        if (tagDictionary.ContainsKey(rawTag)) return tagDictionary[rawTag];
        else return null;
    }


}
