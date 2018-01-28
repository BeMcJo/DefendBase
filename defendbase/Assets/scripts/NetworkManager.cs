using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player
{
    public int connectionID;
}

public class Host
{
    public string ip;
    public int port;

    public Host()
    {
        ip = "";
        port = -1;
    }

    public Host(string addr, int p)
    {
        ip = addr;
        port = p;
    }

    public bool Equals(Host h)
    {
        return h.ip.Equals(ip) && port == h.port;
    }
}

public class NetworkManager : MonoBehaviour {
    public static NetworkManager nm;

    // Max number of host connections
    private const int MAX_CONNECTION = 100;
    private const int MAX_PLAYERS = 6;

    private int SERVER_PORT = 5920,
                CLIENT_PORT = 5919;
    public int broadcastPort = 47777;
    public int broadcastKey = 1000;
    public int broadcastVersion = 1;
    public int broadcastSubVersion = 1;

    private int hostID;
    private int webHostID;
    private int connectionID;
    private string serverIP;
    private int serverPort;
    private Host connectedHost;

    private int ourClientID;
    // Used for connecting for important data (Paypal, payments...)
    private int reliableChannel;
    // Used for less important data that can tolerate missing packets/corruption
    private int unreliableChannel;


    private float connectionTime;
    private float timer, timeout = 30f;
    private bool isStarted = false;
    public bool inLobby = false;
    private bool requestingConnection = false;
    private bool inGame = false;
    private bool isConnected = false;
    private bool destroyed = false;
    private bool isOriginal = false;
    private bool isDisconnected = false;
    private bool initialConfirmation = true;

    private bool isBroadcasting = false;
    private bool isHost = false;
    private bool isListening = false;
    private bool gameStarted = false;
    private bool doneSetup = false;
    private bool findingHosts = false;


    private string lastRequest = "NOREQUESTS|";
    private byte error;

    // cnnID -> Player
    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    // cnnID -> Player Order Pos
    private Dictionary<int, int> client2player = new Dictionary<int, int>();
    // Player Order Pos -> cnnID
    private Dictionary<int, int> player2client = new Dictionary<int, int>();
    // Player Order Pos -> DC timer
    private Dictionary<int, float> playerDC = new Dictionary<int, float>();
    //private List<Player> playerOrder = new List<Player>();

    public Dictionary<Host, GameObject> hosts = new Dictionary<Host, GameObject>();

    private float lastMovementUpdate;
    //private float timer, timeout = 30f;
    private float movementUpdateRate = 0.05f;

    private byte[] msgOutBuffer = new byte[1024];
    private string awaitingResponseFor;


    public Transform hostsList;
    public GameObject playerPrefab, joinButtonPrefab;

    public List<string> activityLog;

    //public GameObject playerPrefab;
    GameObject //startBroadcastButton,
               //stopBroadcastButton,
               leaveLobbyBtn,
               lobbySearchCanvas,
               startGameBtn,
               settingsBtn,
               playersList,
               playerSpawnPoints,
               lobbyCanvas;
    //multiplayersCanvas;
    private Player myPlayer;

    private void Start()
    {
        if (!nm)
        {
            nm = this;
            DontDestroyOnLoad(nm);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //LoadLobbyScene();
        //LoadSceneObjects();
        NetworkTransport.Init();
        //multiplayersCanvas = GameObject.Find("MultiplayersCanvas") as GameObject;
        //multiplayersCanvas.SetActive(false);

        /*
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, 0);

        
        StartBroadcast();
        
        */


        //webHostId = NetworkTransport.AddWebsocketHost(topo, port);

        /*ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port);

        byte error;
        msgOutBuffer = Encoding.Unicode.GetBytes("SERVER|");
        Debug.Log("braodcast");
        if (!NetworkTransport.StartBroadcastDiscovery(hostId, broadcastPort, broadcastKey, broadcastVersion, broadcastSubVersion, msgOutBuffer, msgOutBuffer.Length, 1000, out error))
        {
            Debug.LogError("NetworkDiscovery StartBroadcast failed err: " + error);
            return;
        }*/

        //isStarted = true;
    }

    private void Update()
    {
        //Debug.Log("Time" + Time.time);
        //Debug.Log("DeltaTime" + Time.deltaTime);
        if (!isStarted)// || !isListening)
        {
            return;
        }
        if (isHost)
        {
            PerformHostActivities();
        }
        else
        {
            PerformClientActivities();
        }
        //if (!gameStarted)
        //{
        //    return;
        //}

        if (GameManager.gm.inGame)
        {
            /*string s = "";
            foreach (string str in activityLog)
                s += str + "\n";
            GameManager.gm.gameCanvas.transform.Find("log").GetChild(0).GetComponent<Text>().text = s;
            */
            //List<int> dc = new List<int>();
            /*foreach (KeyValuePair<int, float> kvp in playerDC)
            {
                Debug.Log(Time.time - kvp.Value + " " + kvp.Key);
                if (Time.time - kvp.Value >= timeout)
                    LeaveGame();
            }*/
        }

        
        /*
        // Ask player for their position
        if (Time.time - lastMovementUpdate > movementUpdateRate && clients.Count > 0)
        {
            lastMovementUpdate = Time.time;
            string m = "ASKPOSITION|";
            foreach (Player sc in clients)
            {
                m += sc.connectionId.ToString() + '%' + sc.position.x.ToString() + '%' + sc.position.y.ToString() + '|';
            }
            m = m.Trim('|');
            Send(m, unreliableChannel, clients);
        }*/

    }

    public void SetupAsHost()
    {
        isHost = true;
    }

    public void SetupAsClient()
    {
        isHost = false;
    }

    public void PerformHostActivities()
    {
        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        //Debug.Log(recHostId + " " + connectionId + " " + channelId + " " + recBuffer + " " + bufferSize + " " + dataSize + " " + error);

        if (recData != NetworkEventType.Nothing)
            Debug.Log(recData);
        switch (recData)
        {
            case NetworkEventType.ConnectEvent:    //2
                Debug.Log("Player " + connectionId + " has connected");
                Send("PURPOSE|", reliableChannel, connectionId);
                //OnConnection(connectionId);
                break;
            case NetworkEventType.DataEvent:       //3
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Player " + connectionId + " has sent: " + msg);
                string[] splitData = msg.Split('|');

                switch (splitData[0])
                {
                    /*case "DONESETUP":
                        OnDoneSetup(connectionId);
                        break;
                    case "NAMEIS":
                        OnNameIs(connectionId, splitData[1]);
                        break;
                    case "NEEDDECK":
                        OnNeedDeck(connectionId);
                        break;
                    case "NEEDDECKCOUNT":
                        Send("DECKCOUNT|" + GameManager.gm.deck.Count(), reliableChannel, connectionId);
                        break;
                    case "NEEDHAND":
                        OnNeedHand(connectionId, splitData);
                        break;
                    case "NEEDPLAYERORDER":
                        OnNeedPlayerOrder(connectionId);
                        break;
                    case "NEEDTRUMP":
                        Send("TRUMP|" + GameManager.gm.trump.ToString(), reliableChannel, connectionId);
                        break;
                    case "NEWCNN":
                        OnConnection(connectionId);
                        break;
                    case "PLAYERATTACK":
                        if (OnReceivePlayerAttack(connectionId, splitData))
                            Send(msg, reliableChannel, players);
                        else
                            Send("PLAYERATTACKFAILED|", reliableChannel, connectionId);
                        break;
                    case "PLAYERENDBATTLE":
                        //        Debug.Log("RECEVED ENBGBATTLE");
                        GameManager.gm.PerformCommitEndBattle();
                        Send(msg, reliableChannel, players);
                        activityLog.Add(msg);
                        break;
                    case "PLAYERDEFEND":
                        if (OnReceivePlayerDefend(connectionId, splitData))
                        {
                            Send(msg, reliableChannel, players);
                            activityLog.Add(msg);
                        }
                        else
                            Send("PLAYERDEFENDFAILED|", reliableChannel, connectionId);
                        break;
                    case "PLAYERTRANSFER":
                        if (OnPlayerTransfer(connectionId, splitData))
                        {
                            Send(msg, reliableChannel, players);
                            activityLog.Add(msg);
                        }
                        else
                            Send("PLAYERTRANSFERFAILED|", reliableChannel, connectionId);
                        break;
                    case "RECONNECT":
                        OnReconnect(connectionId, splitData);
                        break;
                    case "STARTGAME":
                        OnReceivedStartGame(connectionId);
                        break;
                    default:
                        Debug.Log("Invalid Message: " + msg);
                        break;
                        */
                }
                break;
            case NetworkEventType.DisconnectEvent: //4
                Debug.Log("Player " + connectionId + " has disconnected");
                if (gameStarted)
                {
                    //if (GameManager.gm.durak != -1)
                    //    return;
                    playerDC.Add(client2player[connectionId], Time.time);
                    //LeaveGame();
                }
                else
                {
                    //OnDisconnection(connectionId);
                }
                break;
        }
    }

    public void PerformClientActivities()
    {
        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        //Debug.Log(recHostId + " " + connectionId + " " + channelId + " " + recBuffer + " " + bufferSize + " " + dataSize + " " + error);

        if (recData != NetworkEventType.Nothing)
            Debug.Log(recData);
        switch (recData)
        {
            case NetworkEventType.ConnectEvent:    //2
                Debug.Log("Player " + connectionId + " has connected");
                Send("PURPOSE|", reliableChannel, connectionId);
                //OnConnection(connectionId);
                break;
            case NetworkEventType.DataEvent:       //3
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Player " + connectionId + " has sent: " + msg);
                string[] splitData = msg.Split('|');

                switch (splitData[0])
                {
                    /*case "DONESETUP":
                        OnDoneSetup(connectionId);
                        break;
                    case "NAMEIS":
                        OnNameIs(connectionId, splitData[1]);
                        break;
                    case "NEEDDECK":
                        OnNeedDeck(connectionId);
                        break;
                    case "NEEDDECKCOUNT":
                        Send("DECKCOUNT|" + GameManager.gm.deck.Count(), reliableChannel, connectionId);
                        break;
                    case "NEEDHAND":
                        OnNeedHand(connectionId, splitData);
                        break;
                    case "NEEDPLAYERORDER":
                        OnNeedPlayerOrder(connectionId);
                        break;
                    case "NEEDTRUMP":
                        Send("TRUMP|" + GameManager.gm.trump.ToString(), reliableChannel, connectionId);
                        break;
                    case "NEWCNN":
                        OnConnection(connectionId);
                        break;
                    case "PLAYERATTACK":
                        if (OnReceivePlayerAttack(connectionId, splitData))
                            Send(msg, reliableChannel, players);
                        else
                            Send("PLAYERATTACKFAILED|", reliableChannel, connectionId);
                        break;
                    case "PLAYERENDBATTLE":
                        //        Debug.Log("RECEVED ENBGBATTLE");
                        GameManager.gm.PerformCommitEndBattle();
                        Send(msg, reliableChannel, players);
                        activityLog.Add(msg);
                        break;
                    case "PLAYERDEFEND":
                        if (OnReceivePlayerDefend(connectionId, splitData))
                        {
                            Send(msg, reliableChannel, players);
                            activityLog.Add(msg);
                        }
                        else
                            Send("PLAYERDEFENDFAILED|", reliableChannel, connectionId);
                        break;
                    case "PLAYERTRANSFER":
                        if (OnPlayerTransfer(connectionId, splitData))
                        {
                            Send(msg, reliableChannel, players);
                            activityLog.Add(msg);
                        }
                        else
                            Send("PLAYERTRANSFERFAILED|", reliableChannel, connectionId);
                        break;
                    case "RECONNECT":
                        OnReconnect(connectionId, splitData);
                        break;
                    case "STARTGAME":
                        OnReceivedStartGame(connectionId);
                        break;
                    default:
                        Debug.Log("Invalid Message: " + msg);
                        break;
                        */
                }
                break;
            case NetworkEventType.DisconnectEvent: //4
                Debug.Log("Player " + connectionId + " has disconnected");
                if (gameStarted)
                {
                    //if (GameManager.gm.durak != -1)
                    //    return;
                    playerDC.Add(client2player[connectionId], Time.time);
                    //LeaveGame();
                }
                else
                {
                    //OnDisconnection(connectionId);
                }
                break;
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        if (this != nm)
            return;
        //Debug.Log("Here" + level);
        switch (level)
        {
            // Main
            case 0:
                //Destroy(gameObject);
                //destroyed = true;
                break;
            // Multiplayer
            case 1:
                //Destroy(gameObject);
                //destroyed = true;
                break;
            // Server
            case 2:
                break;
            // Client
            case 3:
                break;
            // Game (Multiplayer)
            case 4:
                //LoadObjects();
                break;
        }
    }



    public void LoadLobbyScene()
    {
        GameManager.gm.playerRotation = GameObject.Find("Player Rotation");
        playerSpawnPoints = GameManager.gm.playerRotation.transform.Find("Player Spawn Points").gameObject;
        GameObject myPlayer = Instantiate(GameManager.gm.playerPrefab);
        myPlayer.transform.position = playerSpawnPoints.transform.GetChild(0).position;
        myPlayer.transform.SetParent(GameManager.gm.playerRotation.transform);
        GameManager.gm.playerRotation.transform.eulerAngles = GameManager.gm.playerOrientation;
    }

    private void Send(string message, int channelId, int cnnId)
    {
        //if (!players.ContainsKey(cnnId))
        //    return;
        Debug.Log("send" + message);
        NetworkTransport.Send(hostID, cnnId, channelId, Encoding.Unicode.GetBytes(message), message.Length * sizeof(char), out error);
    }

    private void Send(string message, int channelId, Dictionary<int, Player> c)
    {

        Debug.Log("Sending : " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach (KeyValuePair<int, Player> kvp in players)
        {
            Player p = kvp.Value;
            if (p != null && p != myPlayer)
                NetworkTransport.Send(hostID, p.connectionID, channelId, msg, message.Length * sizeof(char), out error);
            //return;
        }
    }


}
