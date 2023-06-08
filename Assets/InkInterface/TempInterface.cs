using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempInterface : InputSOReceiver
{
    [SerializeField] TextAsset inkJson;
    [SerializeField] Transform textSceneParent;
    [SerializeField] Transform inkTextObjectPrefab;

    InkEngine inkEngine;

    bool initializedChoices = false;
    // Start is called before the first frame update
    void Start()
    {
        initializedChoices = false;
        inkEngine = new InkEngine();
        inkEngine.LoadNewStory(inkJson);
        inkEngine.InitializeStory();
        NextStep();
    }

    List<InkTextObject> inkTextObjects = new List<InkTextObject>();
    List<InkTextObject> inkChoiceObjects = new List<InkTextObject>();

    InkTextObject selectedChoice;

    public void NextStep()
    {
        switch (inkEngine.GetCurrentState())
        {
            case InkEngine.State.Display_Next_Line: GetAllAvailableLines(); break;
            case InkEngine.State.Choice_Point:
                {
                    if(initializedChoices == false) GetAllAvailableChoices(); 
                    break;
                }
                
        } 
    }

    private void GetAllAvailableChoices()
    {
        if (inkEngine.GetCurrentState() != InkEngine.State.Choice_Point) return;
   
        ObjectPool<InkTextObject>.ReturnAllListItemsToPool(inkChoiceObjects);
        inkChoiceObjects.Clear();

        List<InkParagraph> choices = inkEngine.GetChoices()?.GetChoices();

        if (choices == null) { Debug.LogWarning("Error: No choices available."); return; }

        int totalChoices = choices.Count;

        InkTextObject inkTextObj;
        InkParagraph _nextPar;
        InkTextObject _prevTextObj = null;
        for(var q = 0; q < totalChoices;q++)
        {
            inkTextObj = ObjectPool<InkTextObject>.GetPoolObject(textSceneParent, inkTextObjectPrefab);
            //inkTextObjects.Add(inkTextObj); We keep text separate from choices
            inkChoiceObjects.Add(inkTextObj);

            _nextPar = choices[q];
            inkTextObj.Init(_nextPar);

            if(_prevTextObj != null)
            {
                inkTextObj.SetLocalPosition(new Vector3(inkTextObj.transform.localPosition.x, _prevTextObj.GetBottomOfText(), inkTextObj.transform.localPosition.z));
            }

            inkTextObj.ShowText();
            
            _prevTextObj = inkTextObj;
        }

        input.ActivateClickProtection();
        initializedChoices = true;
    }

    public void ClearAllLines()
    {
        ObjectPool<InkTextObject>.ReturnAllListItemsToPool(inkTextObjects);
        inkTextObjects.Clear();
    }

    public void ClearChoicePoint()
    {
        ObjectPool<InkTextObject>.ReturnAllListItemsToPool(inkChoiceObjects);
        inkChoiceObjects.Clear();
        selectedChoice = null;
        initializedChoices = false;
    }

    private void GetAllAvailableLines()
    {
        ClearChoicePoint();
        ClearAllLines();

        InkTextObject inkTextObj;
        InkParagraph _nextPar;

        currentTextIndex = -1;
        totalText = 0;

        input.ActivateClickProtection();

        while (inkEngine.GetCurrentState() == InkEngine.State.Display_Next_Line)
        {
            inkTextObj = ObjectPool<InkTextObject>.GetPoolObject(textSceneParent, inkTextObjectPrefab);
            inkTextObjects.Add(inkTextObj);

            _nextPar = inkEngine.GetNextLine();
            inkTextObj.Init(_nextPar);

            totalText++;
        }

        DisplayNextLine();
    
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

    #region Handle CLicked Choices
    private void SelectIdentifiedChoice()
    {
        if (selectedChoice == null) { Debug.Log("ERROR: No choice to execute."); return; }

        inkEngine.SelectChoice(selectedChoice.GetChoiceIndex());

        selectedChoice = null;

        NextStep();
    }
   
    private void IdentifyClickedChoice(InputSOData _input)
    {
        int totalChoices = inkChoiceObjects.Count;
        selectedChoice = null;

        Vector3 mouseWorldPosition = FormatMousePositionToWorldPosition(_input.position);

        for(var q = 0; q < totalChoices; q++)
        {

            if (inkChoiceObjects[q].CheckForClick(mouseWorldPosition)) 
            {
                selectedChoice = inkChoiceObjects[q];
                break;
            };
        }
    }

    private bool ConfirmClickedChoice(InputSOData _input)
    {
        if (selectedChoice == null) return false;

        Vector3 mouseWorldPosition = FormatMousePositionToWorldPosition(_input.position);

        bool didWeConfirm = selectedChoice.CheckForClick(mouseWorldPosition);
        Debug.Log("We did" + (!didWeConfirm ? " NOT" : "") + " confirm " + selectedChoice + " as clicked on.",selectedChoice);
        return didWeConfirm;
    }

    #endregion

    #region Input Handling
    public override void OnInputStart(object sendingSO, InputSOData _input)
    {
        if(initializedChoices) IdentifyClickedChoice(_input);
    }

    public override void OnInputUpdate(object sendingSO, InputSOData _input)
    {
        
    }
    public override void OnInputEnd(object sendingSO, InputSOData _input)
    {
        if (_input.clickSafe == false) return;

        if(currentTextIndex < totalText) // We're currently showing text
        {
            DisplayNextLine();
            return;
        }

        InkEngine.State state = inkEngine.GetCurrentState();

        if(state == InkEngine.State.Choice_Point)
        {
            if (initializedChoices)
            {
                if (ConfirmClickedChoice(_input))
                {
                    SelectIdentifiedChoice();
                }
            }
            else
            {
                GetAllAvailableChoices();
            }

            return;
        }
        
        if(state == InkEngine.State.Display_Next_Line)
        {
            GetAllAvailableLines();
            return;
        }

        if(state == InkEngine.State.End)
        {
            Debug.Log("End of story");
            return;
        }

    }


    #endregion
}
