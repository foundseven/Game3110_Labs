using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameRoomUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField roomNameInput;
    public Button joinRoomButton;
    public Button backButton;
    public TMP_Text statusText;

    private NetworkClient networkClient;


    // Start is called before the first frame update
    void Start()
    {
       statusText.text = "Input room number to join or create a room";
       networkClient = FindObjectOfType<NetworkClient>();
       joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
       backButton.onClick.AddListener(OnBackButtonClicked);
    }

    void OnJoinRoomButtonClicked()
    {
        string roomName = roomNameInput.text;
        if (!string.IsNullOrEmpty(roomName))
        {
            Debug.Log("Sending message: " + $"JoinOrCreateRoom, {roomName}");

            string msg;

            msg = ClientServerSignifiers.JoinQueue + "," + roomName;

            networkClient.SendMessageToServer(msg);

            statusText.text = "Waiting for opponent...";
            roomNameInput.interactable = false;
            joinRoomButton.interactable = false;
        }
        else if(string.IsNullOrEmpty(roomName))
        {
            Debug.Log("Cannot have empty field!");
            statusText.text = "Room name cannot be empty!";
        }
    }
    void OnBackButtonClicked()
    {
        statusText.text = "Input room number to join or create a room";

        roomNameInput.interactable = true;
        joinRoomButton.interactable = true;
    }

    public void OnOpponentJoined()
    {
        //change the game state here
        statusText.text = "Game starting...";
    }

    public void OnRoomFull()
    {
        statusText.text = "The room is full. Try another room.";
        roomNameInput.interactable = true;
        joinRoomButton.interactable = true;
    }
}
