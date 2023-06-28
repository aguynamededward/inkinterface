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

    
    public static InkTagSO ParseTag(string rawTag)
    {
        // Parse through all the possible tags, return the InkTag
        rawTag = rawTag.ToLower();
        string tagData = "";
        int indexOfColon = rawTag.IndexOf(":");
        
        if (indexOfColon > 0)
        {
            tagData = rawTag.Substring(indexOfColon + 1);
            rawTag = rawTag.Substring(0, indexOfColon);
        }

        if (tagDictionary.ContainsKey(rawTag))
        {
            InkTagSO tag = tagDictionary[rawTag];

            return tag;
        }

        else return null;
    }


}


public struct InkTag
{
    public InkTagSO tag;
    public string tagData;
}
