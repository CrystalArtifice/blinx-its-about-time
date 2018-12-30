using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class PauseMenu : MonoBehaviour
{
    public PlayerController playerController;

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

    }

    public void Pause()
    {
        Time.timeScale = 0f;
        anim.SetBool("open", true);
        playerController.enabled = false;
    }

    public void Play()
    {
        Time.timeScale = 1f;
        anim.SetBool("open", false);
        playerController.enabled = true;
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
