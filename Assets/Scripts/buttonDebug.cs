﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonDebug : MonoBehaviour
{
    public GameObject[] Text = new GameObject[6];
     
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClick()
    {
        foreach (GameObject i in Text)
        {
            i.active = !i.active;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
