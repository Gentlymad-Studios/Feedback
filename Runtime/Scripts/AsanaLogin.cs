using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsanaLogin : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Application.OpenURL("https://asanarequestmanager.janamossner.repl.co/AuthenticateUser");
    }
}
