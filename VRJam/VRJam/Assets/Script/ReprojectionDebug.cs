using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReprojectionDebug : MonoBehaviour {

	public GameObject textDisplay;
	GlobalReprojectionManager reprojectionScript;
	// Use this for initialization
	void Start ()
	{

		reprojectionScript = GetComponent<GlobalReprojectionManager>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( Input.GetKeyDown( KeyCode.R))
		{
			reprojectionScript.allowReprojection = !reprojectionScript.allowReprojection;
			if (textDisplay)
			{
				if (reprojectionScript.allowReprojection)
				{
					textDisplay.GetComponent<TextMesh>().text = "Reprojection On\n Press R to toggle";
				}
				else
				{
					textDisplay.GetComponent<TextMesh>().text = "Reprojection Off\n Press R to toggle";
				}
			}
		}
	}
}
