﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// this was the very first code I wrote! just when I was trying to understand how scenes work.
/// </summary>
public class MenuManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log ("Starting");
	}
	
	// Update is called once per frame
	//void Update () {
	
	//}
	public void NewGame()
	{
		Debug.Log ("New");
		SceneManager.LoadScene ("board");
	}
	public void ExitGame()
	{
		Debug.Log ("Exiting");
		Application.Quit ();
	}
}
