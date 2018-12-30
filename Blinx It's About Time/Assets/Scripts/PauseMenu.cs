using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TimeControl;

[RequireComponent(typeof(Animator))]
public class PauseMenu : MonoBehaviour
{
    public PlayerController playerController;
    public TimeEngine engine;

    Animator anim;
    bool paused = false;
    Button resumeButton;

    void Start()
    {
        anim = GetComponent<Animator>();
        resumeButton = transform.Find("Resume").GetComponent<Button>();
    }

    void Update()
    {

        if (paused)
        {
            if (Input.GetButtonDown("Start") || Input.GetButtonDown("Back"))
            {
                paused = false;
                Play();
            }
        }
        else
        {

            if (Input.GetButtonDown("Start"))
            {
                paused = true;
                Pause();
                resumeButton.gameObject.SetActive(true);
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject, null);
            }


        }

        if (!paused)
        {
            if (Input.GetButtonDown("Retry"))
            {
                engine.recordingState = TimeEngine.RecordingState.REWINDING;
                playerController.enabled = false;
            }
            else if (Input.GetButtonUp("Retry"))
            {
                engine.recordingState = TimeEngine.RecordingState.RECORDING;
                playerController.enabled = true;
            }
        }

    }

    public void Pause()
    {
        Time.timeScale = 0f;
        anim.SetBool("open", true);
        playerController.enabled = false;
        engine.recordingState = TimeEngine.RecordingState.PAUSED;
    }

    public void Play()
    {
        Time.timeScale = 1f;
        anim.SetBool("open", false);
        playerController.enabled = true;
        engine.recordingState = TimeEngine.RecordingState.RECORDING;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TestArea");
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
