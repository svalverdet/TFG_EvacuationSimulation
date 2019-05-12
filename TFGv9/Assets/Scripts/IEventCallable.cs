using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IEventCallable
{
    void MouseEvent(Utils.MouseInputEvents mouseEvent);
    void KeyboardEvent();
    void DoAction(Utils.Actions action_);
}
