using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeColorWithFloatValueSO : MonoBehaviour
{
    [SerializeField] Graphic imageToFade;
    [SerializeField] float maxPercent = 0.4f;
    [SerializeField] FloatValueSO screenPercent;
    [SerializeField] AnimationCurveSO animationCurve;

    // Update is called once per frame
    void Update()
    {
        Color c = imageToFade.color;
        
        c.a = animationCurve.acCurve.Evaluate(maxPercent * screenPercent.value);
        imageToFade.color = c;

    }
}
