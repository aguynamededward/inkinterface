using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDirectorDefault : SceneDirector
{
    private InputSO input;
    private InkSceneSO sceneSO;
    
    public override void PrepareScene(InkSceneSO _sceneSO, InputSO _input)
    {
        input = _input;
        sceneSO = _sceneSO;
        inkTextObjectPrefab = sceneSO.InkTextObjectPrefab;
    }

    public void StartScene()
    {
        
    }

    public void HideScene(InkDelegate.CallbackBool callbackBool)
    {
        
    }
}
