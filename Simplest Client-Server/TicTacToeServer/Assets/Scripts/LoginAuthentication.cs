using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Networking.Transport;
using UnityEngine;

public class LoginAuthentication : MonoBehaviour
{
    List<Account> savedAccounts;
    string filePath;
    NetworkServer networkServer;

    public LoginAuthentication(string path, NetworkServer server)
    {
        filePath = path;
        networkServer = server;
        savedAccounts = new List<Account>();
        LoadOldUser();
    }

    private void SaveNewUser(Account newAccount)
    {
        //add new account
        savedAccounts.Add(newAccount);

        StreamWriter sw = new StreamWriter(filePath, true);
        sw.WriteLine(newAccount.username + "," + newAccount.password);

        sw.Close();
    }

    private void LoadOldUser()
    {
        if (File.Exists(filePath) == false)
            return;

        string line = "";

        StreamReader sr = new StreamReader(filePath);
        while ((line = sr.ReadLine()) != null)
        {
            string[] charParse = line.Split(',');
            savedAccounts.Add(new Account(charParse[0], charParse[1]));
        }
        sr.Close();
    }

    public bool CheckCredentials(string username, string password)
    {
        foreach (var account in savedAccounts)
        {
            if (account.username == username && account.password == password)
            {
                return true;
            }
        }
        return false;
    }

    private void HandleLogin(string userName, string password, NetworkConnection connection)
    {
        //bool loginSuccess = false;
        bool loginSuccess = CheckCredentials(userName, password);

        // Check if the user exists and the password is correct
        foreach (Account a in savedAccounts)
        {
            if (userName == a.username && password == a.password)
            {
                loginSuccess = true;
                break;
            }
        }

        if (loginSuccess)
        {
            networkServer.SendMessageToClient(ServerClientSignifiers.LoginComplete.ToString(), connection);
            Debug.Log($"Login successful for {userName}");
        }
        else
        {
            networkServer.SendMessageToClient(ServerClientSignifiers.LoginFailed.ToString(), connection);
            Debug.Log($"Login failed for {userName}");
        }
    }


}
