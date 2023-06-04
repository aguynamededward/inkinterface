using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AnimationCurve SO",menuName ="AnimationCurve SO")]
public class AnimationCurveSO : ScriptableObject
{
    [SerializeField] public AnimationCurve acCurve;
}
