using System.Collections;
using System.Collections.Generic;
using Megingjord.Tools.Dialogue_Manager.API.Exposed;
using UnityEngine;

public class Test : MonoBehaviour {
    public void ChangeBool() {
        var current = DialogueManager.GetBoolProperty("test");
        DialogueManager.SetProperty("test", !current);
    }
}
