using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
	
	void Start ()
	{
		
	}
	
	void Update ()
	{
		
	}

    public void CreateNewGame()
    {
        // TODO: Add game saves
        // for now, just load the test level
        SceneManager.LoadScene("TestArea");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
