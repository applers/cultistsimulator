﻿using System;
using System.Collections;
using System.Collections.Generic;
using Noon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashAnimation : MonoBehaviour
{
    public VideoPlayer player;
	void Awake()
	{
		// force invariant culture to fix Linux save file issues
		System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

		// log the current system settings
		string info = "Cultist Simulator Version: "+Application.version+"\n";
		info += "OS: "+SystemInfo.operatingSystem+"\n";
		info += "Processor: "+SystemInfo.processorType+" Count: "+SystemInfo.processorCount+"\n";
		info += "Graphics: "+SystemInfo.graphicsDeviceID+"/"+SystemInfo.graphicsDeviceName+"/"+SystemInfo.graphicsDeviceVendor+"/"+SystemInfo.graphicsDeviceVersion+"/"+SystemInfo.graphicsMemorySize+" Shader: "+SystemInfo.graphicsShaderLevel+"\n";
		info += "Memory: system - "+SystemInfo.systemMemorySize+" graphics - "+SystemInfo.graphicsMemorySize+"\n";		

		NoonUtility.Log(info,0);
	}
	// Use this for initialization
	void Start ()
	{
		if (Config.OldConfig.Instance.skiplogo)	// This will allocate and read in config.ini
		{
			SceneManager.LoadScene(SceneNumber.QuoteScene);
		}

        player.loopPointReached += EndReached;
	    try
	    {
	        player.Play();

        }
	    
    catch (Exception e)
    {
        NoonUtility.Log(e.Message,0);
        SceneManager.LoadScene(SceneNumber.QuoteScene);
        
    }

	    
    }
	
	void EndReached (VideoPlayer p) {
		SceneManager.LoadScene(SceneNumber.QuoteScene);
	}
}
