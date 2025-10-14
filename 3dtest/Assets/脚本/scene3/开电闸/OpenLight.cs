using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLight : MonoBehaviour
{
    public DetectLightSwitch DetectLightSwitch;
    public int lightNum = 1; 
    public Light[] lights;
    private bool lightsTurnedOn = false; // 防止重复开灯

    void Start()
    {
        
        foreach (var l in lights)
        {
            if (l != null) l.enabled = false;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!lightsTurnedOn && DetectLightSwitch.IsSwitchOpen)
        {
            foreach (var l in lights)
            {
                if (l != null) l.enabled = true;
            }
            lightsTurnedOn = true; // 只开一次
        }
    }
}
