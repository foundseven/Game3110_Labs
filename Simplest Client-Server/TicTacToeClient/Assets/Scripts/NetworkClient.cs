using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Text;

public enum GameState
{
    Login,
    InGame,
}

public class NetworkClient : MonoBehaviour
{
    NetworkDriver networkDriver;
    NetworkConnection networkConnection;
    NetworkPipeline reliableAndInOrderPipeline;
    NetworkPipeline nonReliableNotInOrderedPipeline;
    const ushort NetworkPort = 9001;
    const string IPAddress = /*"192.168.2.20"*/"192.168.2.21";
    private GameStateManager gameStateManager;

    void Start()
    {
        gameStateManager = FindObjectOfType<GameStateManager>();

        networkDriver = NetworkDriver.Create();
        reliableAndInOrderPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage), typeof(ReliableSequencedPipelineStage));
        nonReliableNotInOrderedPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage));
        networkConnection = default(NetworkConnection);
        NetworkEndpoint endpoint = NetworkEndpoint.Parse(IPAddress, NetworkPort, NetworkFamily.Ipv4);
        networkConnection = networkDriver.Connect(endpoint);
    }

    public void OnDestroy()
    {
        networkConnection.Disconnect(networkDriver);
        networkConnection = default(NetworkConnection);
        networkDriver.Dispose();
    }

    void Update()
    {
        #region Check Input and Send Msg

        if (Input.GetKeyDown(KeyCode.A))
            SendMessageToServer("Hello server's world, sincerely your network client");

        #endregion

        networkDriver.ScheduleUpdate().Complete();

        #region Check for client to server connection

        if (!networkConnection.IsCreated)
        {
            Debug.Log("Client is unable to connect to server");
            return;
        }

        #endregion

        #region Manage Network Events

        NetworkEvent.Type networkEventType;
        DataStreamReader streamReader;
        NetworkPipeline pipelineUsedToSendEvent;

        while (PopNetworkEventAndCheckForData(out networkEventType, out streamReader, out pipelineUsedToSendEvent))
        {
            if (pipelineUsedToSendEvent == reliableAndInOrderPipeline)
                Debug.Log("Network event from: reliableAndInOrderPipeline");
            else if (pipelineUsedToSendEvent == nonReliableNotInOrderedPipeline)
                Debug.Log("Network event from: nonReliableNotInOrderedPipeline");

            switch (networkEventType)
            {
                case NetworkEvent.Type.Connect:
                    Debug.Log("We are now connected to the server");
                    break;
                case NetworkEvent.Type.Data:
                    int sizeOfDataBuffer = streamReader.ReadInt();
                    NativeArray<byte> buffer = new NativeArray<byte>(sizeOfDataBuffer, Allocator.Persistent);
                    streamReader.ReadBytes(buffer);
                    byte[] byteBuffer = buffer.ToArray();
                    string msg = Encoding.Unicode.GetString(byteBuffer);
                    ProcessReceivedMsg(msg);
                    buffer.Dispose();
                    break;
                case NetworkEvent.Type.Disconnect:
                    Debug.Log("Client has disconnected from server");
                    networkConnection = default(NetworkConnection);
                    break;
            }
        }

        #endregion
    }

    private bool PopNetworkEventAndCheckForData(out NetworkEvent.Type networkEventType, out DataStreamReader streamReader, out NetworkPipeline pipelineUsedToSendEvent)
    {
        networkEventType = networkConnection.PopEvent(networkDriver, out streamReader, out pipelineUsedToSendEvent);

        if (networkEventType == NetworkEvent.Type.Empty)
            return false;
        return true;
    }

    public void RequestGameRoom(string roomName)
    {
        string message = "CreateOrJoinRoom," + roomName; // Format the message to send
        SendMessageToServer(message);
    }


    private void ProcessReceivedMsg(string msg)
    {
        Debug.Log("Msg received = " + msg);

        //if (msg == "LoginSuccess")
        //{
        //    gameStateManager.OnServerMessageReceived("LoginSuccess");
        //}
        //else if (msg == "LoginFailed")
        //{
        //    gameStateManager.OnServerMessageReceived("LoginFailed");
        //}
        //else if (msg == "RoomCreated")
        //{
        //    Debug.Log("Room created successfully");
        //    FindObjectOfType<GameRoomUI>().OnRoomCreated();
        //}
        //else if (msg == "RoomJoined")
        //{
        //    Debug.Log("Joined an existing room");
        //    FindObjectOfType<GameRoomUI>().OnOpponentJoined();
        //}
        //else if (msg == "RoomFull")
        //{
        //    Debug.Log("Room is full. Please try another room");
        //    FindObjectOfType<GameRoomUI>().OnRoomFull();
        //}

        string[] parts = msg.Split(':');

        // Check the first part for the message type
        if (parts[0] == "LoginSuccess")
        {
            gameStateManager.OnServerMessageReceived("LoginSuccess");
        }
        else if (parts[0] == "LoginFailed")
        {
            Debug.Log("lOGIN FAILED!");
            if (parts.Length > 1) // Check for specific failure reasons
            {
                if (parts[1] == "WrongPassword")
                {
                    Debug.Log("Login failed: Incorrect password");
                    FindObjectOfType<Login>().feedbackText.text = "Incorrect password. Please try again.";
                }
                else if (parts[1] == "UserNotFound")
                {
                    Debug.Log("Login failed: Username not found");
                    FindObjectOfType<Login>().feedbackText.text = "Username not found. Please create an account.";
                }
            }
            else
            {
                Debug.Log("Login failed: General error");
                FindObjectOfType<Login>().feedbackText.text = "Login failed. Please try again.";
            }
        }
        else if (msg == "RoomCreated")
        {
            Debug.Log("Room created successfully");
            FindObjectOfType<GameRoomUI>().OnRoomCreated();
        }
        else if (msg == "RoomJoined")
        {
            Debug.Log("Joined an existing room");
            FindObjectOfType<GameRoomUI>().OnOpponentJoined();
        }
        else if (msg == "RoomFull")
        {
            Debug.Log("Room is full. Please try another room");
            FindObjectOfType<GameRoomUI>().OnRoomFull();
        }
        else
        {
            Debug.LogWarning("Unknown message received: " + msg);
        }
    }

    public void SendMessageToServer(string msg)
    {
        byte[] msgAsByteArray = Encoding.Unicode.GetBytes(msg);
        NativeArray<byte> buffer = new NativeArray<byte>(msgAsByteArray, Allocator.Persistent);

        DataStreamWriter streamWriter;
        networkDriver.BeginSend(reliableAndInOrderPipeline, networkConnection, out streamWriter);
        streamWriter.WriteInt(buffer.Length);
        streamWriter.WriteBytes(buffer);
        networkDriver.EndSend(streamWriter);

        buffer.Dispose();
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