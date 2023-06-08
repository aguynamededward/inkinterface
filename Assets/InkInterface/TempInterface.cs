using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempInterface : InputSOReceiver
{
    [SerializeField] TextAsset inkJson;
    [SerializeField] Transform textSceneParent;
    [SerializeField] Transform inkTextObjectPrefab;

    InkEngine inkEngine;
    // Start is called before the first frame update
    void Start()
    {
        inkEngine = new InkEngine();
        inkEngine.LoadNewStory(inkJson);
        inkEngine.InitializeStory();
        NextStep();
    }

    List<InkTextObject> inkTextObjects = new List<InkTextObject>();

    public void NextStep()
    {
        if(inkEngine.GetCurrentState() == InkEngine.State.Display_Next_Line)
        {
            ObjectPool<InkTextObject>.ReturnAllObjectsToPool();
            inkTextObjects.Clear();
            
            InkTextObject inkTextObj;
            InkParagraph _nextPar;

            currentTextIndex = -1;
            totalText = 0;

            input.ActivateClickProtection();

            while(inkEngine.GetCurrentState() == InkEngine.State.Display_Next_Line)
            {
                inkTextObj = ObjectPool<InkTextObject>.GetPoolObject(textSceneParent, inkTextObjectPrefab);
                inkTextObjects.Add(inkTextObj);

                _nextPar = inkEngine.GetNextLine();
                inkTextObj.Init(_nextPar);

                totalText++;
            }
        }
    }

    int totalText;
    int currentTextIndex;

    public void DisplayNextLine()
    {
        if(totalText <= 0 || currentTextIndex >= totalText)
        {
            NextStep();
            return;
        }

        if(currentTextIndex >= 0)
        {
            inkTextObjects[currentTextIndex].HideText();
        }

        currentTextIndex++;

        if(currentTextIndex >= totalText)
        {
            NextStep();
            return;
        }

        inkTextObjects[currentTextIndex].ShowText();
        input.ActivateClickProtection();
    }


    public override void OnInputEnd(object sendingSO, InputSOData _input)
    {
        if (_input.clickSafe == false) return;

        if (totalText <= 0 || currentTextIndex >= totalText)
        {
            NextStep();
        }

        DisplayNextLine();

    }



    public override void OnInputStart(object sendingSO, InputSOData _input)
    {
        
    }

    public override void OnInputUpdate(object sendingSO, InputSOData _input)
    {
        
    }

}
