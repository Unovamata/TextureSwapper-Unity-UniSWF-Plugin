using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Python.Runtime;
using System;

public class PythonExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start(){
        Runtime.PythonDLL = Application.dataPath + "/StreamingAssets/embedded-python/python311.dll";
        PythonEngine.Initialize();

        try {
            dynamic system = PyModule.Import("sys");
            dynamic version = system.version;
            Debug.Log(version);
        } catch (Exception e){
            print(e);
            print(e.StackTrace);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnApplicationQuit(){
        if(PythonEngine.IsInitialized){
            print("Ending Python");
            PythonEngine.Shutdown();
        }
    }
}
