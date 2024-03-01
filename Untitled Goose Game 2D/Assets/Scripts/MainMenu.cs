using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    [SerializeField] private Button playButton; 
    [SerializeField] private Button exitButton; 
    private void Awake() {
        playButton.onClick.AddListener(() => {
            SceneManager.LoadScene(1);
        });

        exitButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }

    void Update() {
        
    }
}
