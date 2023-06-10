using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkInput_DialogueProgression : InputSOReceiver
{
    public bool activated = false;

    List<InkTextObject> inkTextObjects;
    int currentDialogueIndex = 0;
    bool showMultipleLines = false;

    InkDelegate.Callback callbackOnTextComplete;

    public virtual void Init(List<InkTextObject> _dialogueObjects, InkDelegate.Callback _onCompleteCallback,bool showMultipleLines = false)
    {
        if (activated)
        {
            CleanupCurrentText();
            activated = false;
        }

        inkTextObjects = _dialogueObjects;
        callbackOnTextComplete = _onCompleteCallback;
        currentDialogueIndex = -1;
        this.showMultipleLines = showMultipleLines;
        activated = true;

        ShowNextLineOfDialogue();
    }

    protected virtual void ShowNextLineOfDialogue()
    {
        Debug.Log("DialogueProgression: Show NextLineOfDialogue");
        if(currentDialogueIndex >= inkTextObjects.Count - 1)
        {
            // We're at the end, return the callback
            if (showMultipleLines) CompleteDialogueDisplay();
            else CleanupCurrentText(true);
            return;
        }

        if(!showMultipleLines && currentDialogueIndex >= 0)
        {
            inkTextObjects[currentDialogueIndex].HideText();
        }

        currentDialogueIndex++;
        inkTextObjects[currentDialogueIndex].ShowText();
    }

    protected void CleanupCurrentText(bool runCallbackAtEnd = false)
    {
        Debug.Log("DialogueProgression: CleanupCurrentText");
        if (activated) StartCoroutine(HideTextInStages(1f,runCallbackAtEnd));

    }

    protected IEnumerator HideTextInStages(float totalDuration, bool runCallbackAtEnd)
    {
        if (inkTextObjects.Count < 1 || !activated)
        {

            yield break;
        }


        int totalVisibleInkObjects = 0;
        for(var q = 0; q < inkTextObjects.Count; q++)
        {
            if (inkTextObjects[q].IsVisible()) totalVisibleInkObjects++;
        }

        if (totalVisibleInkObjects <= 1)
        {
            Debug.Log("DialogueProgression: HideTextInStages - only one object, stopping now");
            // Don't hold us up for one object

            foreach(InkTextObject ito in inkTextObjects)
            {
                ito.HideText();
            }

            if (runCallbackAtEnd) CompleteDialogueDisplay();
            yield break;
        }


        float waitDuration = totalDuration / totalVisibleInkObjects;

        for(var q = 0; q < inkTextObjects.Count; q++)
        {
            if (!inkTextObjects[q].IsVisible()) continue;
            inkTextObjects[q].HideText();
            Debug.Log("DialogueProgression: HideTextInStages - hiding textObject #" + q);
            yield return new WaitForSeconds(waitDuration);
        }

        if (runCallbackAtEnd) CompleteDialogueDisplay();

    }

    private void CompleteDialogueDisplay()
    {
        Debug.Log("DialogueProgression: CompleteDialogueDisplay - finished dialogue, running callback");
        activated = false;
        callbackOnTextComplete?.Invoke();
    }
    public override void OnInputEnd(object sendingSO, InputSOData _input)
    {
        Debug.Log("DialogueProgression: OnInputEnd - Received click. activated: " + activated + " | clickSafe: " + _input.clickSafe + " | clickActivationTime: " + input.clickProtectionTimeStamp);
        if (!activated) return;
        if (!_input.clickSafe) return;

        input.ActivateClickProtection();
        ShowNextLineOfDialogue();
        
    }

    public override void OnInputStart(object sendingSO, InputSOData _input)
    {
      
    }

    public override void OnInputUpdate(object sendingSO, InputSOData _input)
    {
      
    }

}
