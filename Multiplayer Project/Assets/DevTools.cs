using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevTools : MonoBehaviour
{
    GameObject developerConsole;
    private void Start()
    {
        developerConsole = GameObject.FindGameObjectWithTag("DevConsole");
    }
    public void ToggleConsole()
    {
        Debug.Log("TEst");
        developerConsole.SetActive(!developerConsole.activeSelf);
    }

    public void ResetPos()
    {
        Debug.Log("TEst");
        transform.position = Vector3.zero;
    }
}
