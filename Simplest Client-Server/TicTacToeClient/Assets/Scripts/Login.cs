using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Login : MonoBehaviour
{
    #region Variables

    //set to an instance
    public static Login instance;

    [Header("UI Elements")]
    public GameObject loginUI;
    public GameObject createAccountUI;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    //user feedback
    public TMP_Text feedbackText;
    //getting connection
    public GameObject ConnectionToHost;

    bool isNewUser = false;

    #endregion

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        
    }

    public void ConfirmButtonPressed()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) 
        {
            feedbackText.text = "Username and password cannot be empty!";
            return;
        }

        string msg;

        if(isNewUser)
        {
            msg = ClientServerSignifiers.CreateAccount + "," + username + "," + password;
            feedbackText.text = "Creating account.";
        }
        else
        {
            msg = ClientServerSignifiers.Login + "," + username + "," + password;
            feedbackText.text = "Logging in.";
        }

        ConnectionToHost.GetComponent<NetworkClient>().SendMessageToServer(msg);
    }

    public void IsNewUserPressed()
    {
        isNewUser = !isNewUser;

        if(isNewUser)
        {
            loginUI.SetActive(false);
            createAccountUI.SetActive(true);
            feedbackText.text = "Create a new account.";
        }
        else
        {
            loginUI.SetActive(true);
            createAccountUI.SetActive(false);
            feedbackText.text = "Log in to your account.";
        }
    }
}
