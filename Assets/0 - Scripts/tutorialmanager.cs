using System;
using TMPro;
using UnityEngine;

public class tutorialmanager : MonoBehaviour
{
    public GameObject tutorialUI;
    public TMP_InputField passwordText;
    public TMP_Text wrongInput;
    private string password = "278";
    private string enteredPassword;
    public Camera pauseCamera;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CheckPassword()
    {
        enteredPassword = passwordText.text.Trim(); // Remove leading/trailing whitespace


        if (string.Equals(enteredPassword, password, StringComparison.OrdinalIgnoreCase))
        {
            // Debug.Log("Correct password");
            pauseCamera.gameObject.SetActive(false);
            StartGame();
        }
        else
        {
            wrongInput.gameObject.SetActive(true);
            // Debug.Log("Wrong password");
        }
    }

    public void TypingStarted() {
        wrongInput.gameObject.SetActive(false);
    }


    private void StartGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        tutorialUI.SetActive(false);
        GetComponent<mainmanager>().StartGame(); 
        GetComponent<uimanager>().StartGameUI();
    }
}
