using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using TMPro;
using System;

public class Ink_Liason : MonoBehaviour
{
    [SerializeField] TextAsset inkJSONAsset = null;
    public Story story;

    [SerializeField] TextMeshPro tmproDisplay;
    
    enum InkLiasonState
    {
        None,
        Display_Next_Line,
        Wait_For_Input_To_Continue,
        Wait_For_Choice,
        Waiting_For_End
    }

    [SerializeField] InkLiasonState state = InkLiasonState.None;

    private KeyCode[] keycodeArray;

    // Start is called before the first frame update
    void Start()
    {
        keycodeArray = new KeyCode[] {KeyCode.Alpha1,KeyCode.Alpha2,KeyCode.Alpha3,KeyCode.Alpha4,KeyCode.Alpha5 };
    }

    
    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case InkLiasonState.None: return;
            case InkLiasonState.Display_Next_Line: ContinueStory();break;
            case InkLiasonState.Wait_For_Input_To_Continue:
            {
                if (Input.GetMouseButtonUp(0))
                {
                    ContinueStory();
                }
                break;
            }
            case InkLiasonState.Wait_For_Choice:
                {
                    int totalChoices = Mathf.Min(story.currentChoices.Count, keycodeArray.Length);
                    for(var q = 0; q < totalChoices; q++)
                    {
                        if (Input.GetKeyUp(keycodeArray[q]))
                        {
                            story.ChooseChoiceIndex(q);
                            state = InkLiasonState.Display_Next_Line;
                            break;
                        }
                    }
                    break;
                }

            case InkLiasonState.Waiting_For_End:
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        StartStory();
                    }
                    break;
                }

        }


    }

    private void ContinueStory()
    {
        if (story.canContinue)
        {
            if(story.currentTags.Count > 0) Debug.Log("Current tags for position" + story.currentFlowName + ": " + story.currentTags[0]);
            string text = story.Continue().Trim();
            if (story.currentTags.Count > 0) Debug.Log("AFTER Current tags for position" + story.currentFlowName + ": " + story.currentTags[0]); 
            tmproDisplay.text = text;
            if (story.canContinue) state = InkLiasonState.Wait_For_Input_To_Continue;
            else if(story.currentChoices.Count > 0)
            {
                string choices = "";
                for(var q = 0; q < story.currentChoices.Count; q++)
                {
                    choices += (q + 1) + ". " + story.currentChoices[q].text.Trim() + "\r\n";
                }

                Debug.Log(choices);
                state = InkLiasonState.Wait_For_Choice;
            }
        }
        else
        {
            tmproDisplay.text = "Story over - CLick to restart?";
            state = InkLiasonState.Waiting_For_End;
        }
    }

    private void StartStory()
    {
        story = new Story(inkJSONAsset.text);
        state = InkLiasonState.Display_Next_Line;
    }
}
