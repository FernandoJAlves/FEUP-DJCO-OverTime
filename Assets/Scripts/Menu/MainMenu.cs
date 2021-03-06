﻿using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class MainMenu : MonoBehaviour {
    // Sound Events
    public StudioEventEmitter clickEvent;
    public StudioEventEmitter enterEvent;

    // UI Objects
    public GameObject mainMenu;
    public GameObject selectLevels;
    public GameObject options;

    private void Awake() {
        Time.timeScale = 1; // here to reset the light "flicking" after coming to this menu from pause state
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (SceneController.Instance != null) {
            SceneController.Instance.DestroyCurrentPlayerAndSceneController();
        }
    }

    public void Level0() {
        enterEvent.Play();
        SceneManager.LoadSceneAsync("Level0");
    }

    public void Level1() {
        enterEvent.Play();
        SceneManager.LoadSceneAsync("Level1");
    }

    public void Level2() {
        enterEvent.Play();
        SceneManager.LoadSceneAsync("Level2");
    }

    public void Level3() {
        enterEvent.Play();
        SceneManager.LoadSceneAsync("Level3");
    }

    public void Level4() {
        enterEvent.Play();
        SceneManager.LoadSceneAsync("Level4");
    }

    public void Level5() {
        enterEvent.Play();
        SceneManager.LoadSceneAsync("Level5");
    }

    public void Level6() {
        enterEvent.Play();
        SceneManager.LoadSceneAsync("Level6");
    }

    public void LevelSelect() {
        clickEvent.Play();
        mainMenu.SetActive(false);
        selectLevels.SetActive(true);
    }

    public void LevelSelectBack() {
        clickEvent.Play();
        mainMenu.SetActive(true);
        selectLevels.SetActive(false);
    }

    public void Options() {
        clickEvent.Play();
        mainMenu.SetActive(false);
        options.SetActive(true);
    }

    public void OptionsBack() {
        clickEvent.Play();
        mainMenu.SetActive(true);
        options.SetActive(false);
    }

    public void PlayClickEvent() {
        clickEvent.Play();
    }

    public void Quit() {
        clickEvent.Play();
        Application.Quit();
    }
}