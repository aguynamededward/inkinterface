using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class TestClickBounds : InputSOReceiver
{
    [SerializeField] Transform inkTextObjectPrefab;
    [SerializeField] int loopNumber = 10000;
    InkTextObject inkTextObject;

    public void Start()
    {
        inkTextObject = Instantiate(inkTextObjectPrefab, transform).GetComponent<InkTextObject>();
        inkTextObject.Init(new InkParagraph("Test of the clicking system.",1));
        inkTextObject.ShowText();
    }
    public override void OnInputEnd(object sendingSO, InputSOData _input)
    {
        //DebugClick(_input);

        Vector3 mouseWorldPosition = FormatMousePositionToWorldPosition(_input.position);
        bool checkChar = inkTextObject.CheckForClick(mouseWorldPosition);

        if (checkChar) UnityEngine.Debug.Log("CLICKED OBJECT!");
    }

    public void DebugClick(InputSOData _input)
    {
        Stopwatch w = new Stopwatch();
        w.Start();

        bool checkChar = false;

        for (var q = 0; q < loopNumber; q++)
        {
            checkChar = inkTextObject.CheckForClickOnCharacter(_input.position, cameraMain);
        }

        w.Stop();

        float timeEach = (float)w.ElapsedMilliseconds / (float)loopNumber;
        UnityEngine.Debug.Log("Using RECTTRANSFORM check first: Total " + loopNumber + " loops for " + (checkChar ? " SUCCESSFUL " : " FAILED ") + " character check is " + w.ElapsedMilliseconds + " //// " + timeEach + " each");

        w.Reset();

        w.Start();
        for (var q = 0; q < loopNumber; q++)
        {
            checkChar = inkTextObject.CheckForClickOnLine(_input.position, cameraMain);
        }
        w.Stop();

        timeEach = (float)w.ElapsedMilliseconds / (float)loopNumber;
        UnityEngine.Debug.Log("Using RECTTRANSFORM check first: Total " + loopNumber + " loops for " + (checkChar ? " SUCCESSFUL " : " FAILED ") + " line check is " + w.ElapsedMilliseconds + " //// " + timeEach + " each");

        w.Reset();

        Vector3 mouseWorldPosition;
        w.Start();

        for (var q = 0; q < loopNumber; q++)
        {
            mouseWorldPosition = FormatMousePositionToWorldPosition(_input.position);
            checkChar = inkTextObject.CheckForClick(mouseWorldPosition);
        }
        w.Stop();

        timeEach = (float)w.ElapsedMilliseconds / (float)loopNumber;
        UnityEngine.Debug.Log("Updated Recttransform check: Total loops for " + (checkChar ? " SUCCESSFUL " : " FAILED ") + " world position check is " + w.ElapsedMilliseconds + " //// " + timeEach + " each");
        /*
         * Result of the test:
         
            Actual successful click
         * Total loops for  SUCCESSFUL  character check is 23 //// 0.0023 each
         * Total loops for  SUCCESSFUL  line check is 8 //// 0.0008 each
         * Total loops for  FAILED  world position check is 11 //// 0.0011 each
         * 
         * Actual failed click
         * Total loops for  FAILED  character check is 77 //// 0.0077 each
         * Total loops for  FAILED  line check is 10 //// 0.001 each
         * Total loops for  FAILED  world position check is 11 //// 0.0011 each
         * 
         * 
         * Line checks will return true if you click on the line, even if there's no text there.
         * Character checks will check SPECIFICALLY the character (but it is SLOW comparitively).
         * My textBounds check is justttt bad
         * 
         * 
         * New version, checking with recttransform first
         * Successful click:
         * 
         * Using RECTTRANSFORM check first: Total 10000 loops for  SUCCESSFUL  character check is 21 //// 0.0021 each - first letter
         * Using RECTTRANSFORM check first: Total 10000 loops for  SUCCESSFUL  character check is 89 //// 0.0089 each - Last character - but it was SUPER picky about it. had to click right on it
         * 
         * Using RECTTRANSFORM check first: Total 10000 loops for  SUCCESSFUL  line check is 19 //// 0.0019 each - first line
         * Using RECTTRANSFORM check first: Total 10000 loops for  SUCCESSFUL  line check is 20 //// 0.002 each - Second line
         * 
         * Mine is consistently about 12 ms for 10000 checks, which si pretty good. If I could get the data accurate I'd be killing it.
         * 
         * Failed click:
         * 
         * 
         * 
         * 
         * ------------ END RESULT
         * I think we'll be able to get away with the line check.  Even the failed version is faster than mine.  (I think it's because it's processing the camera data on the C++ side?)
         * 
         * I think I can accept 0.0001ms per click.  I'll try to find a way to survive. ;)
         * 
         */
    }


    public override void OnInputStart(object sendingSO, InputSOData _input)
    {
       
    }

    public override void OnInputUpdate(object sendingSO, InputSOData _input)
    {
       
    }


}
