﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    GameObject sceneController; // may change to the actual SceneController script, will keep like this for now tho

    void Awake() {
        GameObject[] sceneControllers = GameObject.FindGameObjectsWithTag("SceneController");
        if (sceneControllers.Length > 1) {
            // if there are more than 1 scene controllers, choose the "main" one
            for (int index = 0; index < sceneControllers.Length; index++) {
                if (sceneControllers[index].GetComponent<SceneController>().IsMainSceneController()) {
                    sceneController = sceneControllers[index];
                    break;
                }
            }
        } else {
            sceneController = sceneControllers[0];
        }
    }

    // called first
    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {        
        sceneController.GetComponent<SceneController>().SetupScene();
    }


}
