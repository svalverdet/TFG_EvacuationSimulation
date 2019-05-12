using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ISceneElement
{
    void MouseEvent(Utils.MouseInputEvents mouseEvent);
    void KeyboardEvent();
    void DoAction(Utils.Actions action_);

    void Save();
    void Load();
    void DeleteAll();
}

