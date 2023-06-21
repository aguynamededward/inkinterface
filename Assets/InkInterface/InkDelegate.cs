using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkDelegate
{
    public delegate void Callback();
    public delegate void CallbackInt(int i);
    public delegate void CallbackIntInt(int i,int j);
    public delegate void CallbackBool(bool _b);
    public delegate void CallbackInkPageSetup(InkPageSO inkPageSO, InputSO input);
}
