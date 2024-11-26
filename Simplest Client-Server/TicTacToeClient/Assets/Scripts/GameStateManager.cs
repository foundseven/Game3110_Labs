using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public GameState currentState;

    public GameObject loginPanel;
    public GameObject gamePanel;


    // Start is called before the first frame update
    void Start()
    {
        SetState(GameState.Login);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;

        // Hide all panels first
        HideAllPanels();

        // Show the appropriate panel based on the current state
        switch (currentState)
        {
            case GameState.Login:
                loginPanel.SetActive(true);
                gamePanel.SetActive(false);
                break;
            case GameState.InGame:
                loginPanel.SetActive(false);
                gamePanel.SetActive(true);
                break;
        }
    }

    private void HideAllPanels()
    {
        loginPanel.SetActive(false);
        gamePanel.SetActive(false);
    }

    public void OnServerMessageReceived(string message)
    {
        if (message == "LoginSuccess")
        {
            Debug.Log("Changing state...");
            SetState(GameState.InGame);
        }
        else if (message == "LoginFailed")
        {
            SetState(GameState.Login);
        }
    }
}
