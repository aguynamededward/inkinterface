using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


[CreateAssetMenu(fileName = "New InkScene SO",menuName = "InkScene")]
public class InkSceneSO : ScriptableObject
{
    public Transform InkScenePrefab;
    public Transform InkTextObjectPrefab;
    public TMP_FontAsset font;


}
