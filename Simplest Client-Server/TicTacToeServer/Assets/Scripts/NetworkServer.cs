using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Text;
using System.IO;
using UnityEditor.MemoryProfiler;
using Unity.VisualScripting;

public class NetworkServer : MonoBehaviour
{
    public NetworkDriver networkDriver;
    private NativeList<NetworkConnection> networkConnections;

    NetworkPipeline reliableAndInOrderPipeline;
    NetworkPipeline nonReliableNotInOrderedPipeline;

    const ushort NetworkPort = /*50931*/9001;

    const int MaxNumberOfClientConnections = 1000;

    //similar to SDL
    List<Account> savedAccounts;
    string filePath;

    void Start()
    {
        networkDriver = NetworkDriver.Create();
        reliableAndInOrderPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage), typeof(ReliableSequencedPipelineStage));
        nonReliableNotInOrderedPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage));
        NetworkEndpoint endpoint = NetworkEndpoint.AnyIpv4;
        endpoint.Port = NetworkPort;

        int error = networkDriver.Bind(endpoint);
        if (error != 0)
            Debug.Log("Failed to bind to port " + NetworkPort);
        else
            networkDriver.Listen();
        Debug.Log("Successfully was able to bind to port " + NetworkPort);


        networkConnections = new NativeList<NetworkConnection>(MaxNumberOfClientConnections, Allocator.Persistent);

        //setting up the save path
        savedAccounts = new List<Account>();
        filePath = Application.dataPath + Path.DirectorySeparatorChar + "savedAccountData.txt";
        if(File.Exists(filePath))
        {
            Debug.Log("File found!");
        }
    }

    void OnDestroy()
    {
        networkDriver.Dispose();
        networkConnections.Dispose();
    }

    void Update()
    {
        #region Check Input and Send Msg

        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int i = 0; i < networkConnections.Length; i++)
            {
                SendMessageToClient("Hello client's world, sincerely your network server", networkConnections[i]);
            }
        }

        #endregion

        networkDriver.ScheduleUpdate().Complete();

        #region Remove Unused Connections

        for (int i = 0; i < networkConnections.Length; i++)
        {
            if (!networkConnections[i].IsCreated)
            {
                networkConnections.RemoveAtSwapBack(i);
                i--;
            }
        }

        #endregion

        #region Accept New Connections

        while (AcceptIncomingConnection())
        {
            Debug.Log("Accepted a client connection");
        }

        #endregion

        #region Manage Network Events

        DataStreamReader streamReader;
        NetworkPipeline pipelineUsedToSendEvent;
        NetworkEvent.Type networkEventType;

        for (int i = 0; i < networkConnections.Length; i++)
        {
            if (!networkConnections[i].IsCreated)
                continue;

            while (PopNetworkEventAndCheckForData(networkConnections[i], out networkEventType, out streamReader, out pipelineUsedToSendEvent))
            {
                if (pipelineUsedToSendEvent == reliableAndInOrderPipeline)
                    Debug.Log("Network event from: reliableAndInOrderPipeline");
                else if (pipelineUsedToSendEvent == nonReliableNotInOrderedPipeline)
                    Debug.Log("Network event from: nonReliableNotInOrderedPipeline");

                switch (networkEventType)
                {
                    case NetworkEvent.Type.Data:
                        int sizeOfDataBuffer = streamReader.ReadInt();
                        NativeArray<byte> buffer = new NativeArray<byte>(sizeOfDataBuffer, Allocator.Persistent);
                        streamReader.ReadBytes(buffer);
                        byte[] byteBuffer = buffer.ToArray();
                        string msg = Encoding.Unicode.GetString(byteBuffer);
                        ProcessReceivedMsg(msg);
                        buffer.Dispose();

                        if (msg.StartsWith((char)ClientServerSignifiers.Login))
                        {
                            // Extract username and password from message
                            string[] msgParts = msg.Split(',');
                            if (msgParts.Length == 3)
                            {
                                string username = msgParts[1];
                                string password = msgParts[2];

                                // Handle the login logic
                                HandleLogin(username, password, networkConnections[i]);
                            }
                        }

                        break;
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("Client has disconnected from server");
                        networkConnections[i] = default(NetworkConnection);
                        break;
                }
            }
        }

        #endregion
    }

    private bool AcceptIncomingConnection()
    {
        NetworkConnection connection = networkDriver.Accept();
        if (connection == default(NetworkConnection))
            return false;

        networkConnections.Add(connection);
        return true;
    }

    private bool PopNetworkEventAndCheckForData(NetworkConnection networkConnection, out NetworkEvent.Type networkEventType, out DataStreamReader streamReader, out NetworkPipeline pipelineUsedToSendEvent)
    {
        networkEventType = networkConnection.PopEvent(networkDriver, out streamReader, out pipelineUsedToSendEvent);

        if (networkEventType == NetworkEvent.Type.Empty)
            return false;
        return true;
    }

    //this need to get reworked
    private void ProcessReceivedMsg(string msg)
    {
        Debug.Log("Msg received = " + msg);

        //process each line from the file
        string[] charParse = msg.Split(',');
        //int identifier = int.Parse(charParse[0]);
        Debug.Log("Parsed parts: " + string.Join(", ", charParse));

        if (charParse.Length < 3)
        {
            Debug.LogError("Invalid message format. Expected at least 3 parts.");
            return;
        }

        // Try parsing the identifier
        int identifier;
        if (!int.TryParse(charParse[0], out identifier))
        {
            Debug.LogError("Failed to parse identifier: " + charParse[0]);
            return;
        }

        string userName = charParse[1];
        string password = charParse[2];

        //so now i need to see if they are creating an account or not
        if(identifier == ClientServerSignifiers.CreateAccount)
        {
            bool checkIsUsed = false;
            //iterate through all the accounts to check
            foreach(Account a in savedAccounts)
            {
                //if the username matches with one thats already existing
                if(userName == a.username)
                {
                    checkIsUsed = true;
                }
            }

            if(checkIsUsed)
            {
                foreach (NetworkConnection connection in networkConnections)
                {
                    if (connection.IsCreated)
                    {
                        SendMessageToClient(ServerClientSignifiers.AccountCreationFailed + ", username is already in use!", connection);
                        Debug.Log("Failed to create!");
                    }
                }
            }
            else
            {
                foreach (NetworkConnection connection in networkConnections)
                {
                    if (connection.IsCreated)
                    {
                        SaveNewUser(new Account(userName, password));
                        SendMessageToClient(ServerClientSignifiers.AccountCreated + ", the new account has been created", connection);
                        Debug.Log("New user created!");
                    }
                }
            }
        }
    }

    public void SendMessageToClient(string msg, NetworkConnection networkConnection)
    {
        byte[] msgAsByteArray = Encoding.Unicode.GetBytes(msg);
        NativeArray<byte> buffer = new NativeArray<byte>(msgAsByteArray, Allocator.Persistent);


        //Driver.BeginSend(m_Connection, out var writer);
        DataStreamWriter streamWriter;
        //networkConnection.
        networkDriver.BeginSend(reliableAndInOrderPipeline, networkConnection, out streamWriter);
        streamWriter.WriteInt(buffer.Length);
        streamWriter.WriteBytes(buffer);
        networkDriver.EndSend(streamWriter);

        buffer.Dispose();
    }

    //create a save new user 
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
        string line = "";

        StreamReader sr = new StreamReader(filePath);
        while ((line = sr.ReadLine()) != null)
        {
            string[] charParse = line.Split(',');
            savedAccounts.Add(new Account(charParse[0], charParse[1]));
        }
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

    public void HandleLogin(string username, string password, NetworkConnection conn)
    {
        bool loginSuccessful = CheckCredentials(username, password);  // Check credentials in your database or list

        if (loginSuccessful)
        {
            SendMessageToClient("LoginSuccess", conn);
        }
        else
        {
            SendMessageToClient("LoginFailed", conn);
        }
    }

}

#region Signifiers

public static class ClientServerSignifiers
{
    public const int CreateAccount = 1;
    public const int Login = 2;
}

public static class ServerClientSignifiers
{
    public const int LoginComplete = 1;
    public const int LoginFailed = 2;

    public const int AccountCreated = 3;
    public const int AccountCreationFailed = 4;
}

#endregion

//similar to my sending data 
public class Account
{
    #region Variables

    public string username;
    public string password;

    #endregion

    public Account(string username, string password)
    {
        this.username = username;
        this.password = password;
    }

}
