using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// Used to keep track of information of each player online
public class Player
{
    public int connectionID; // Unique ID per connection
    public string playerName; // Name of player
    public bool isReady; // Used to determine if game can proceed at each break point
    public GameObject playerGO; // Who/what the player is
}

// Used to keep track of and synchronize an enemy attack
public class Attack
{
    public int confirmedAttackCount; // Number of confirmed attack make by the same enemy from all players
    public int tid; // target ID being attacked
    public int dmg; // How much damage will be inflicted on target if attack is confirmed by all players
    
    public Attack(int tid, int dmg)
    {
        this.tid = tid;
        this.dmg = dmg;
        confirmedAttackCount = 0;
    }
    // Check if all players have confirmed this attack to be valid
    public bool ConfirmAttack()
    {
        confirmedAttackCount++;
        return confirmedAttackCount == NetworkManager.nm.PlayerCount() - NetworkManager.nm.PLayerDCCount();
    }
}

// Information about the Host of a game necessary to maintain connection
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

/*
 * Responsible for maintaining connection between players and providing
 * the necessary information across all other players in the game session
 */
public class NetworkManager : MonoBehaviour {
    public static NetworkManager nm; // Singleton

    // Max number of host connections
    private const int MAX_CONNECTION = 100;
    private const int MAX_PLAYERS = 4;
    // Keep as is
    private int SERVER_PORT = 5920,
                CLIENT_PORT = 5919;
    // Keep these as is
    public int broadcastPort = 47777;
    public int broadcastKey = 1000;
    public int broadcastVersion = 1;
    public int broadcastSubVersion = 1;

    private int hostID; // Keeps track of the hostID
    // Unused?
    private int recHostID;
    private int webHostID;
    
    private Host connectedHost; // Keep track of our host

    private int ourClientID = 0; // Unique connection ID when online
    // Used for connecting for important data (Paypal, payments...)
    private int reliableChannel;
    // Used for less important data that can tolerate missing packets/corruption
    private int unreliableChannel;

    public float sendRate = .015f, // How frequent we should send information about our player
                 sendTimer = .015f; // Current time before sending information about our player
    private float timer,  // Time before timing out
                  timeout = 30f; // Default time before timing out
    public bool isStarted = false; // Did we start up using online connectivity?
    public bool inLobby = false; // Are we in lobby waiting to start game with other players?
    private bool requestingConnection = false; // Did we send out request to connect to host?
    public bool isDisconnected = false; // Did we lose connection from host?
    private bool isBroadcasting = false; // Are we announcing that we are hosting?
    public bool isHost = false; // Are we the host or the client?
    private bool findingHosts = false; // Are we client looking for host room to join?
    public bool isSpawningObject;

    private Dictionary<int, Player> players = new Dictionary<int, Player>(); // cnnID -> Player
    //Unused
    private Dictionary<int, int> client2player = new Dictionary<int, int>(); // cnnID -> Player Order Pos
    //Unused
    private Dictionary<int, int> player2client = new Dictionary<int, int>(); // Player Order Pos -> cnnID

    // Ideally remove player if timeout too long
    private Dictionary<int, float> playerDC = new Dictionary<int, float>(); // Player Order Pos -> DC timer
    // Mapping of each attack a unique enemy makes. If all players are confirmed that the unique enemy attacked
    // then the target will take damage
    private Dictionary<int, Dictionary<int, Attack>> enemyAttacks = new Dictionary<int, Dictionary<int, Attack>>();
    
    public Dictionary<Host, GameObject> hosts = new Dictionary<Host, GameObject>(); // Keeps track of available games to join by host

    private byte[] msgOutBuffer = new byte[1024]; // Used to send messages out

    public Transform hostsList; // List of host buttons
    
    public List<string> activityLog, // List of activities occurring in the game in sequential order 
                        activitiesToSend, // List of activities needed to resend due to disconnection
                        debugLog; // For in game debug purposes

    GameObject DCNotification; // Notifies player has disconnected
    //private Player myPlayer;

    // Used for listening for received messaged
    int recHostId;
    int connectionId;
    int channelId;
    byte[] recBuffer = new byte[1024];
    int bufferSize = 1024;
    int dataSize;
    byte error;

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
        activitiesToSend = new List<string>(); 
        NetworkTransport.Init();
    }

    private void Update()
    {
        
        // Don't do anything unless using online feature
        if (!isStarted)
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
        // While in game, print out debug purpose information
        if (GameManager.gm.inGame && GameManager.Debugging)
        {
            Text activity = GameManager.gm.playerStatusCanvas.transform.Find("ActivitiesTxt").GetComponent<Text>();
            string t = "";
            for (int i = 0; i < 10; i++)
            {
                int j = activityLog.Count - 1 - i;
                if (j >= 0)
                    t = "\n" + activityLog[j] + t;
            }
            t = "Activity Log:" + t;
            activity.text = t;

            Text debug = GameManager.gm.playerStatusCanvas.transform.Find("DebugTxt").GetComponent<Text>();
            t = "";
            for (int i = 0; i < 10; i++)
            {
                int j = debugLog.Count - 1 - i;
                if (j >= 0)
                    t = "\n" + debugLog[j] + t;
            }
            t = "Debug Log:" + t;
            debug.text = t;
        }

        // While in game, send information about my player
        ///*
        if (!GameManager.gm.realTimeAction && GameManager.gm.inGame && !isDisconnected)
        {
            // Don't send anything when wave ended for you to prevent overloading other players
            if (GameManager.gm.onIntermission || !GameManager.gm.startWaves)
                return;
            sendTimer -= Time.deltaTime;
            if (sendTimer <= 0)
            {
                SendPlayerInformation();
                sendTimer = sendRate;
            }
        }
        //*/
        // Notify player of dc
        if(DCNotification)
            DCNotification.SetActive(GameManager.gm.inGame && isStarted && isDisconnected);
    }

    // Number of players in game/lobby
    public int PlayerCount()
    {
        return players.Count;
    }
    // Number of players disconnected
    public int PLayerDCCount()
    {
        return playerDC.Count;
    }

    // Network Information about player
    // Format: 
    // PLAYERSTATS|Unique client ID|Camera orientation without the orientation offset|Weapon Network Information|
    public void SendPlayerInformation()
    {
        string info = "PLAYERSTATS|" + ourClientID + "|";
        PlayerController p = GameManager.gm.player.transform.GetComponent<PlayerController>();
        Camera cam = p.playerCam;
        Vector3 dir = cam.transform.eulerAngles - GameManager.gm.playerOrientation;
        info += dir.x + "," + dir.y + "," + dir.z + "|" + p.wep.NetworkInformation();
        if (isHost)
        {
            Send(info, reliableChannel, players);
        }
        else
        {
            Send(info, reliableChannel);
        }
    }

    // Initializes any basic information necessary to use online feature in the lobby
    public void StartUpNetworkActivities()
    {
        isStarted = true;
        isDisconnected = false;
        GameManager.gm.lobbyCanvas.transform.Find("BackBtn").GetComponent<Button>().interactable = true;
    }

    // Initializes information to start Network Manager as host
    public void SetupAsHost()
    {
        isHost = true;
        GameManager.gm.lobbyCanvas.transform.Find("ReadyBtn").GetComponent<Button>().interactable = false;
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostID = NetworkTransport.AddHost(topo, 0);
        ourClientID = hostID;
        SpawnPlayerUI("HOST", ourClientID); // Spawn my player in the lobby
        players[ourClientID].isReady = false; // Always ready to start game
        StartBroadcast(); // Announce my Network Manager is hosting
    }

    // Announce that Network Manager is hosting
    public void StartBroadcast()
    {
        byte error;
        msgOutBuffer = Encoding.Unicode.GetBytes("HOST|");
        if (!NetworkTransport.StartBroadcastDiscovery(hostID, broadcastPort, broadcastKey, broadcastVersion, broadcastSubVersion, msgOutBuffer, msgOutBuffer.Length, 1000, out error))
        {
            Debug.LogError("NetworkDiscovery StartBroadcast failed err: " + error);
            return;
        }
        isBroadcasting = true;
    }

    // Stop announcing that we are hosting
    public void StopBroadcast()
    {
        NetworkTransport.StopBroadcastDiscovery();
        isBroadcasting = false;
    }

    // Initialize information to start Network Manager as client
    public void SetupAsClient()
    {
        isHost = false;
        requestingConnection = false;
        GameManager.gm.lobbyCanvas.transform.Find("ReadyBtn").GetComponent<Button>().interactable = true;
        inLobby = false;
        hostID = -1;
    }

    // Stop searching for host
    public void StopFindingHosts()
    {
        if (findingHosts)
        {
            findingHosts = false;
            if (hostID != -1)
            {
                NetworkTransport.RemoveHost(hostID);
            }
            hostID = -1;
        }
        isStarted = false;
    }
    // Search for host of a game
    public void FindHosts()
    {
        if (!findingHosts)
        {
            findingHosts = true;
        }
        // Does player know who the host is?
        if (hostID == -1)
        {
            ConnectionConfig cc = new ConnectionConfig();

            reliableChannel = cc.AddChannel(QosType.Reliable);
            unreliableChannel = cc.AddChannel(QosType.Unreliable);

            HostTopology topo = new HostTopology(cc, MAX_CONNECTION);
            hostID = NetworkTransport.AddHost(topo, broadcastPort);
            if (hostID == -1)
            {
                Debug.LogError("NetworkDiscovery StartAsClient - addHost failed");
                return;
            }
        }
        // Enable permission to connect to broadcaster(s)/ host
        NetworkTransport.SetBroadcastCredentials(hostID, broadcastKey, broadcastVersion, broadcastSubVersion, out error);
        
        NetworkEventType recData = NetworkEventType.Nothing;
        recData = NetworkTransport.Receive(out recHostID, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        // Check if we received approval from host to connect
        if (recData == NetworkEventType.BroadcastEvent)
        {
            int rcvSize;
            NetworkTransport.GetBroadcastConnectionMessage(hostID, recBuffer, recBuffer.Length, out rcvSize, out error);
            string senderAddr;
            int senderPort;
            NetworkTransport.GetBroadcastConnectionInfo(hostID, out senderAddr, out senderPort, out error);
            OnReceivedBroadcast(senderAddr, senderPort, Encoding.Unicode.GetString(recBuffer));
        }
    }
    // Officially establish connection to host
    public void ConnectTo(Host host)
    {
        Debug.Log("Connecting" + host == null);
        connectedHost = host;
        if (hostID != -1)
            NetworkTransport.RemoveHost(hostID);

        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);
        hostID = NetworkTransport.AddHost(topo, 0);
        if (hostID == -1)
        {
            Debug.LogError("NetworkDiscovery StartAsClient - addHost failed");
            return;
        }
        Debug.Log(connectedHost == null);
        connectionId = NetworkTransport.Connect(hostID, connectedHost.ip, connectedHost.port, 0, out error);
        // Used to ensure if connection request fails, will stop requesting
        timer = Time.time;
        requestingConnection = true;
    }

    // Asking why is client trying to connect?
    void OnPurpose()
    {
        // Not in game, requesting to join lobby
        if (!GameManager.gm.inGame)
        {
            Send("NEWCNN|", reliableChannel);
        }
        // In game and disconnected, request to get up to date
        else
        {
            Debug.Log("Reconnected");
            RequestActivityLog();
            isDisconnected = false;
            requestingConnection = false;
        }
    }

    // Extract Network Information about the other players
    public void OnPlayerInformation(string[] data)
    {        
        int cnnID = int.Parse(data[1]);
        if (cnnID == ourClientID)
            return;
        string[] orientation = data[2].Split(',');

        PlayerController p = players[cnnID].playerGO.transform.GetComponent<PlayerController>();
        p.playerCam.transform.eulerAngles = GameManager.gm.playerOrientation + new Vector3(float.Parse(orientation[0]), float.Parse(orientation[01]), float.Parse(orientation[2]));

        bool wepInUse = bool.Parse(data[4]);
        float chargePower = float.Parse(data[5]);
        if (wepInUse)
        {
            if (!p.wep.inUse)
            {
                p.wep.StartUse();
            }
            p.wep.Charge(chargePower);
        }
        else
        {
            p.wep.Charge(chargePower);
            if (p.wep.inUse)
            {
                p.wep.EndUse();
            }
        }
    }

    // Check if the received broadcast is a Host, display a join button for that host if new
    public void OnReceivedBroadcast(string fromAddress, int fromPort, string data)
    {
        string[] splitAddr = fromAddress.Split(':');
        string[] splitData = data.Split('|');

        switch (splitData[0])
        {
            case "HOST":
                string tempIp = splitAddr[splitAddr.Length - 1];
                int tempPort = fromPort;
                Host host = new Host(tempIp, tempPort);
                bool newHost = true;
                // Check if host was previously seen and broadcasting, refresh expire time if so
                foreach (KeyValuePair<Host, GameObject> kvp in hosts)
                {
                    if (kvp.Key.Equals(host))
                    {
                        hosts[kvp.Key].transform.GetComponent<ExpiringButton>().Reset();
                        newHost = false;
                        break;
                    }
                }
                // Create new button for host if new
                if (newHost)
                {
                    GameObject joinBtn = Instantiate(GameManager.gm.buttonPrefab) as GameObject;
                    joinBtn.AddComponent<ExpiringButton>();
                    joinBtn.transform.SetParent(GameManager.gm.hostListCanvas.transform.Find("ButtonsContainer"));
                    joinBtn.transform.GetComponent<ExpiringButton>().SetExpiringButton(host);
                    hosts.Add(host, joinBtn);
                    joinBtn.transform.Find("Text").GetComponent<Text>().text = "Game";
                    joinBtn.transform.localScale = new Vector3(1, 1, 1);
                }

                break;
        }
    }

    public void ClearHostList()
    {
        foreach (KeyValuePair<Host, GameObject> kvp in hosts)
        {
            Destroy(kvp.Value);
        }
        hosts.Clear();
    }

    // As host, accept new connection client if there is space or not in game
    private void OnConnection(int cnnId)
    {
        // Cannot play with more than MAX_PLAYERS total
        if (players.Count == MAX_PLAYERS)
        {
            Send("FULLROOM|", reliableChannel, cnnId);
            return;
        }
        // Don't accept if in game
        if (GameManager.gm.inGame)
        {
            Send("FULLROOM|", reliableChannel, (int)cnnId);
            return;
        }
        // Add client to list
        Player c = new Player();
        c.connectionID = cnnId;
        c.playerName = "TEMP";
        c.playerGO = Instantiate(GameManager.gm.playerUIPrefab);
        c.playerGO.SetActive(false);
        //Debug.Log(cnnId);
        players.Add(cnnId, c);
        // Stop broadcasting if full room
        if (players.Count == MAX_PLAYERS - 1)
        {
            StopBroadcast();
        }
        // When player joins server, tell him his ID
        // Request his name and send name of all other players
        string msg = "ASKNAME|" + cnnId + "|";// + myPlayer.playerName + "%0|";
        foreach (KeyValuePair<int, Player> kvp in players)
        {
            if (kvp.Key != cnnId)
            {
                Player sc = kvp.Value;
                msg += sc.playerName + "%" + sc.connectionID + "|";
            }
        }
        msg = msg.Trim('|');
        Send(msg, reliableChannel, cnnId);
    }

    // As host, approve client officially when name is given and announce to other players in lobby
    private void OnNameIs(int cnnId, string playerName)
    {
        // Make sure not self and client exists from OnConnection
        if (players.Count == 1 && !players.ContainsKey(cnnId))
        {
            return;
        }
        Player sc = players[cnnId];
        sc.playerName = playerName;
        sc.playerGO.transform.Find("NameText").GetComponent<Text>().text = playerName + cnnId;
        sc.playerGO.transform.SetParent(GameManager.gm.lobbyCanvas.transform.Find("PlayerList"));
        sc.playerGO.SetActive(true);
        // Tell everybody that new player has connected
        Send("CNN|" + playerName + '|' + cnnId, reliableChannel, players);
    }

    // Handle when someone disconnects in lobby
    private void OnDisconnection(int cnnId)
    {
        // Make sure we aren't the ones dc-ing. Client disconnecting must also exist as a player we knoe
        if (players.Count == 1 || !players.ContainsKey(cnnId))
            return;
        // Remove player object
        Destroy(players[cnnId].playerGO);
        // Remove this player from our client list
        players.Remove(cnnId);
        
        // Broadcast again if not in game
        if (!GameManager.gm.inGame && !isBroadcasting)
        {
            StartBroadcast();
        }
        Debug.Log("DCS: " + cnnId);
        // Tell everyone that someone has disconnected
        Send("DC|" + cnnId, reliableChannel, players);
    }

    // As Host, client requests to reconnect. Update client with any missing information about current
    // game state (missing crucial activities from log and information about enemy positioning etc. details)
    private void OnReconnect(int cnnId, string[] data)
    {
        if (!GameManager.gm.inGame)
            return;
        int lastCommittedActivity = int.Parse(data[1]);

        playerDC.Remove(cnnId); // Remove player from dc list
        ResendCommits(cnnId, lastCommittedActivity); // Send activity that client doesn't know about in game
        // Tell player about the enemies positioning and other network information
        foreach(KeyValuePair<int,Enemy> kvp in GameManager.gm.enemies)
        {
            string msg = kvp.Value.NetworkInformation();
            Send(msg,reliableChannel ,cnnId);
        }
    }

    // Provide information that the client is missing
    private void ResendCommits(int cnnId, int lastCommit)
    {
        if (!GameManager.gm.inGame)
            return;
        // Send information up to the latest starting from last activity known to client
        for (int i = lastCommit; i < activityLog.Count; i++)
            Send(activityLog[i], reliableChannel, cnnId);
    }

    // Handles actions performed as a host
    public void PerformHostActivities()
    {
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            // Upon connection, ask client why connecting
            case NetworkEventType.ConnectEvent:
                Debug.Log("Player " + connectionId + " has connected");
                Send("PURPOSE|", reliableChannel, connectionId);
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                string[] splitData = msg.Split('|');

                switch (splitData[0])
                {
                    // Client says they damaged enemy, relay that to other players
                    case "ENEMYDMG":
                        //Debug.Log(msg);
                        debugLog.Add(msg);
                        //if (activityLog.Count > 0)
                        //    Debug.Log(activityLog[activityLog.Count - 1]);
                        OnEnemyDamaged(splitData);
                        break;
                    // Client formally requests to leave lobby
                    case "LEAVELOBBY?":
                        debugLog.Add("Client " + connectionId + " DCs");
                        Send("DC|" + connectionId, reliableChannel, connectionId);
                        OnDisconnection(connectionId);
                        break;
                    // Client states his name, officially approve client to lobby
                    case "NAMEIS":
                        OnNameIs(connectionId, splitData[1]);
                        break;
                    // Client requests to join lobby
                    case "NEWCNN":
                        OnConnection(connectionId);
                        break;
                    // Client states objective is being damaged
                    case "OBJECTIVEDMG":
                        OnObjectiveDamaged(splitData);
                        break;
                    // Client provides information about own player details in game
                    case "PLAYERSTATS":
                        OnPlayerInformation(splitData);
                        break;
                    // Client requests to approve being ready for the next step in the game
                    case "READY?":
                        OnReady(connectionId);
                        break;
                    // Client is ready
                    case "READY":
                        break;
                    // Client requests to repair objective
                    // Host announces objective has been repaired by someone
                    case "REMOVEDEFENSE":
                        OnRemoveDefense(splitData);
                        break;
                    case "REPAIROBJECTIVE":
                        OnRepairObjective(splitData);
                        break;
                    // Client has disconnected and requests to reconnect
                    case "RECONNECT":
                        OnReconnect(connectionId, splitData);
                        break;
                    case "SPAWN":
                        OnSpawnObject(splitData);
                        break;
                    // Client states he/she has damaged trap
                    case "TRAPDMG":
                        OnTrapDamaged(splitData);
                        break;
                }
                break;
            // Client disconnects
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Player " + connectionId + " has disconnected");
                // Keep track of player if in game
                if (GameManager.gm.inGame)
                {
                    playerDC.Add(connectionId, Time.time);
                }
                // Remove player from lobby
                else
                {
                    OnDisconnection(connectionId);
                }
                break;
        }
    }

    // Handle client activites
    public void PerformClientActivities()
    {
        // Update the expired time for each host button
        if (hosts.Count > 0)
        {
            List<Host> expiredHosts = new List<Host>();
            // Remove any host buttons when expired, indicating they stopped hosting
            foreach (KeyValuePair<Host, GameObject> kvp in hosts)
            {
                kvp.Value.transform.GetComponent<ExpiringButton>().Decrement();
                if (kvp.Value.transform.GetComponent<ExpiringButton>().Expired())
                {
                    Destroy(kvp.Value);
                    expiredHosts.Add(kvp.Key);
                }
            }
            for (int i = 0; i < expiredHosts.Count; i++)
            {
                hosts.Remove(expiredHosts[i]);
            }
        }
        // Search for host if not attempting to join a room, not in lobby, and not in game
        if (!requestingConnection && !inLobby && !GameManager.gm.inGame)
        {   FindHosts();
            return;
        }
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        
        switch (recData)
        {
            // Gained official connection with host
            case NetworkEventType.ConnectEvent:
                Debug.Log("Connected to Host");
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                string[] splitData = msg.Split('|');
                int logID;
                int cnnID;
                switch (splitData[0])
                {
                    // Host ask for your name while providing all players in lobby
                    case "ASKNAME":
                        OnAskName(splitData);
                        break;
                    // Host states someone has connected to lobby, create player object for him/her
                    case "CNN":
                        debugLog.Add(msg);
                        SpawnPlayerUI(splitData[1], int.Parse(splitData[2]));
                        break;
                    // Host states enemy has been damaged, make sure log is synced with the sequence of actions in game
                    case "ENEMYDMG":
                        logID = int.Parse(splitData[1]);
                        //cnnID = int.Parse(splitData[3]);
                        // Request for sequential order of activity log since my log is missing something before this action
                        if (logID != activityLog.Count)
                        {
                            RequestActivityLog();
                            break;
                        }
                        OnEnemyDamaged(splitData);
                        //activityLog.Add(msg);
                        break;
                    // Receiving information about the enemies in the game
                    case "ENEMYINFO":
                        int eid = int.Parse(splitData[1]);
                        // Make sure enemy is alive to adjust their details
                        if (GameManager.gm.enemies.ContainsKey(eid))
                        {
                            GameManager.gm.enemies[eid].SetNetworkInformation(splitData);
                        }
                        // Not existent
                        else
                        {
                            Debug.Log("?");
                        }
                        break;
                    // Spawn Enemy
                    case "ENEMYSPAWN":
                        logID = int.Parse(splitData[1]);
                        // Sync activity log if missing something
                        if (logID != activityLog.Count)
                        {
                            RequestActivityLog();
                            break;
                        }
                        SpawnEnemy(int.Parse(splitData[2]));
                        break;
                    // Someone disconnected, handle it
                    case "DC":
                        PlayerDisconnected(int.Parse(splitData[1]));
                        break;
                    // Tell player to leave lobby
                    case "LEAVELOBBY":
                        LeaveLobby();
                        break;
                    // Host states that objective has taken damage
                    case "OBJECTIVEDMG":
                        logID = int.Parse(splitData[1]);
                        //cnnID = int.Parse(splitData[3]);
                        // Sync activity log if missing something
                        if (logID != activityLog.Count)
                        {
                            RequestActivityLog();
                            break;
                        }
                        OnObjectiveDamaged(splitData);
                        //activityLog.Add(msg);
                        break;
                    // Received information about player
                    case "PLAYERSTATS":
                        OnPlayerInformation(splitData);
                        break;
                    // Host asks why you connecting
                    case "PURPOSE":
                        OnPurpose();
                        break;
                    // Host states someone is ready
                    case "READY":
                        OnReady(int.Parse(splitData[1]));
                        break;
                    // Host announces objective has been repaired by someone
                    case "REMOVEDEFENSE":
                        logID = int.Parse(splitData[1]);
                        // Sync activity log if missing something
                        if (logID != activityLog.Count)
                        {
                            RequestActivityLog();
                            break;
                        }
                        OnRemoveDefense(splitData);
                        break;
                    case "REPAIROBJECTIVE":
                        logID = int.Parse(splitData[1]);
                        // Sync activity log if missing something
                        if (logID != activityLog.Count)
                        {
                            RequestActivityLog();
                            break;
                        }
                        OnRepairObjective(splitData);
                        //activityLog.Add(msg);
                        break;
                    case "SPAWN":
                        logID = int.Parse(splitData[1]);
                        // Sync activity log if missing something
                        if (logID != activityLog.Count)
                        {
                            RequestActivityLog();
                            break;
                        }
                        OnSpawnObject(splitData);
                        //activityLog.Add(msg);
                        break;
                    // Host states to start game
                    case "STARTGAME":
                        StartGame();
                        break;
                    // Host states to start wave
                    case "STARTWAVE":
                        logID = int.Parse(splitData[1]);
                        // Sync log when missing something
                        if(logID != activityLog.Count)
                        {
                            RequestActivityLog();
                            break;
                        }
                        StartWave(int.Parse(splitData[2]));
                        break;
                    // Host states someone damaged trap
                    case "TRAPDMG":
                        logID = int.Parse(splitData[1]);
                        //cnnID = int.Parse(splitData[3]);
                        // Request for sequential order of activity log since my log is missing something before this action
                        if (logID != activityLog.Count)
                        {
                            RequestActivityLog();
                            break;
                        }
                        OnTrapDamaged(splitData);
                        //activityLog.Add(msg);
                        break;
                }
                break;
            // When someone disconnects
            case NetworkEventType.DisconnectEvent:
                // If disconnection was our ID
                if (connectionId == ourClientID)
                {
                    // Request to reconnect if in game
                    if (GameManager.gm.inGame)
                    {
                        // If failed to reconnect, leave game
                        if (isDisconnected)
                        {
                            LeaveGame();
                            return;
                        }
                        debugLog.Add("HOST DC");
                        isDisconnected = true;
                        ReconnectTo(connectedHost);
                        timer = Time.time;
                        isStarted = true;
                    }
                    // Leave lobby when disconnected
                    else
                        LeaveLobby();
                }
                // If requesting connection when disconnected not in game, reset and search for host again
                else if (requestingConnection)
                {
                    Disconnect();
                    SetupAsClient();
                    StartUpNetworkActivities();
                }
                break;
        }

        // Leave game when disconnected for waiting too long
        if (isDisconnected)
        {
            if (Time.time - timer >= timeout)
            {
                if (GameManager.gm.inGame)
                {
                    Debug.Log("U DC");
                    LeaveGame();
                }
            }
        }
    }

    // Attempt to reset connectivity with host
    void ReconnectTo(Host h)
    {
        Debug.Log("Attempt to reconnect");
        Disconnect();
        ConnectTo(h);
    }

    // Inflict damage to enemy
    public void OnEnemyDamaged(string[] data)
    {
        debugLog.Add("ENEMY DAMAGED");
        int targetID = int.Parse(data[2]);
        int sourceID = int.Parse(data[3]);
        int dmg = int.Parse(data[4]);
        // Inflict damage if enemy exists
        if (GameManager.gm.enemies.ContainsKey(targetID))
        {
            GameManager.gm.enemies[targetID].TakeDamage(dmg);
            // Remove enemy if non-existent or dead
            if (GameManager.gm.enemies[targetID] == null || GameManager.gm.enemies[targetID].health <= 0)
            {
                GameManager.gm.enemies.Remove(targetID);
            }
            string msg = data[0] + "|" + activityLog.Count + "|" + data[2] + "|" + data[3] + "|" + data[4] + "|";
            activityLog.Add(msg);
            if (isHost)
            {
                Send(msg, reliableChannel, players);
            }
        }
    }

    // Remove enemy attacks from queue, usually when enemy dies
    public void RemoveEnemyAttacks(int eid)
    {
        if (!enemyAttacks.ContainsKey(eid))
            return;
        enemyAttacks[eid].Clear();
        enemyAttacks.Remove(eid);
    }

    // Inflict damage onto objective
    public void OnObjectiveDamaged(string[] data)
    {
        int oid = int.Parse(data[2]);
        int eid = int.Parse(data[3]);
        int atkID = int.Parse(data[4]);
        int dmg = int.Parse(data[5]);
        string msg = "OBJECTIVEDMG|" + activityLog.Count + "|" + oid + "|" + eid + "|" + atkID + "|" + dmg + "|";
        // As host, check if all players synced enemy attack before inflicting damage
        if (isHost)
        {
            bool canAttack = ConfirmEnemyAttack(eid, oid, atkID, dmg);
            // if all synced, damage objective and announce to players
            if (canAttack)
            {
                if (oid == 0 && GameManager.gm.objective)
                {
                    Objective o = GameManager.gm.objective.transform.GetComponent<Objective>();
                    o.TakeDamage(dmg);

                    Send(msg, reliableChannel, players);
                    activityLog.Add(msg);

                    enemyAttacks[eid].Remove(atkID); // Remove attack from attack queue
                }
            }
        }
        // As client, just perform the attack on objective
        else
        {
            if (oid == 0 && GameManager.gm.objective)
            {   Objective o = GameManager.gm.objective.transform.GetComponent<Objective>();
                o.TakeDamage(dmg);
                activityLog.Add(msg);
            }
        }
    }

    // Ensures that the unique enemy's attack has been synced before inflicting damage to its target
    public bool ConfirmEnemyAttack(int eid, int tid, int atkID, int dmg)
    {
        // Doesn't exist? Don't do anythin
        if (!GameManager.gm.enemies.ContainsKey(eid))
            return false;
        // Enemy exists but not registered as an attacker?
        if (!enemyAttacks.ContainsKey(eid))
        {
            enemyAttacks[eid] = new Dictionary<int, Attack>();
        }
        // Doesn't have attack registered?
        if (!enemyAttacks[eid].ContainsKey(atkID))
        {
            enemyAttacks[eid][atkID] = new Attack(tid, dmg);
        }
        return enemyAttacks[eid][atkID].ConfirmAttack();
    }

    // Used to synchronize attacks from all players where there exists objects shared among all players
    public void QueueAttackOnObject(GameObject source, GameObject target)
    {
        int tid = -1;
        Objective o = null;
        if (target.tag == "Objective")
        {
            o = target.transform.GetComponent<Objective>();
            tid = o.id;
        }
        if(source.tag == "Enemy")
        {
            Enemy e = source.transform.GetComponent<Enemy>();
            bool canAttack = false;
            int atkID = e.attackCt;
            e.attackCt++;
            // As host, check if enemy can attack w/o approval of disconnected players
            if (isHost)
            {
                canAttack = ConfirmEnemyAttack(e.id, tid, atkID, e.dmg);
                // If so, officialize enemy attack
                if (canAttack)
                {
                    // Make sure objective exists
                    if (o)
                    {
                        o.TakeDamage(enemyAttacks[e.id][atkID].dmg);
                        string msg = "OBJECTIVEDMG|" + activityLog.Count + "|" + tid + "|" + e.id + "|" + atkID + "|" + e.dmg + "|";
                        Send(msg, reliableChannel, players);
                        activityLog.Add(msg);
                        enemyAttacks[e.id].Remove(atkID);
                    }
                }
            }
            // As client, tell host that target is being attacked by source
            else
            {
                string msg = "OBJECTIVEDMG|" + activityLog.Count + "|" + tid + "|" + e.id + "|" + atkID + "|" + e.dmg + "|";
                Send(msg, reliableChannel, players);
            }
        }
    }

    // As client out of sync, request the missing information as well as send information that weren't sent when/if disconnected
    public void RequestActivityLog()
    {
        Debug.Log("OUT OF SYNC, REQUESTING LOG ORDER");
        debugLog.Add("OUT OF SYNC, REQUESTING LOG ACTIVITES");
        Send("RECONNECT|" + activityLog.Count + "|", reliableChannel);
        // Send information that I couldn't send when disconnected
        foreach(string msg in activitiesToSend)
        {
            Send(msg, reliableChannel);
        }
        activitiesToSend.Clear();
    }

    // As host, tell game manager to spawn enemy and tell clients to do so as well
    public void SpawnEnemy(int sp)
    {
        if (sp == -1)
            sp = UnityEngine.Random.Range(0, MapManager.mapManager.spawnPoints.Count);
        NotifySpawnEnemyAt(sp);
        StartCoroutine(GameManager.gm.SpawnEnemy(sp));
    }

    // Notify players to spawn enemies
    public void NotifySpawnEnemyAt(int spawnPoint)
    {
        string msg = "ENEMYSPAWN|" + activityLog.Count + "|" + spawnPoint + "|";
        if(isHost)
            Send(msg, reliableChannel, players);
        activityLog.Add(msg);
    }

    // Make request to repair objective
    public void ConfirmObjectiveRepair(int oid, int hp)
    {
        string msg = "REPAIROBJECTIVE|" + activityLog.Count + "|" + ourClientID + "|" + oid + "|" + hp;
        if (isHost)
        {
            OnRepairObjective(msg.Split('|'));
        }
        else
        {
            Send(msg, reliableChannel);
        }
    }

    // Officialize the repair on objective
    public void OnRepairObjective(string[] data)
    {
        int cnnID = int.Parse(data[2]);
        int oid = int.Parse(data[3]);
        int hp = int.Parse(data[4]);
        Objective o = GameManager.gm.objective.transform.GetComponent<Objective>();
        string msg = data[0] + "|" + activityLog.Count + "|" + data[2] + "|" + data[3] + "|" + data[4] + "|";
        // As host, confirm that repairing the objective is valid
        if (isHost)
        {
            if(o.HP < o.maxHP)
            {
                // If I was the one making request, repair and pay price
                if(cnnID == ourClientID)
                {
                    o.Repair();
                }
                // If not, just heal objective
                else
                {
                    o.TakeDamage(hp);
                }
                Send(msg, reliableChannel, players);
                activityLog.Add(msg);
            }
        }
        // As client, check if I made request or not to repair
        else
        {
            if (cnnID == ourClientID)
            {
                o.Repair();
            }
            else
            {
                o.TakeDamage(hp);
            }
            activityLog.Add(msg);
        }
    }

    public void NotifySpawnDefenseAt(string type, int typeID)
    {
        GameManager.gm.mapUICanvas.SetActive(true);
        GameManager.gm.playerStatusCanvas.SetActive(true);
        isSpawningObject = true;
        string msg = "SPAWN|" + activityLog.Count + "|" + ourClientID + "|";
        if(type == "Trap")
        {
            msg += GameManager.gm.selectedDefense.GetComponent<Trap>().NetworkInformation();
        }
        if (isHost)
        {
            OnSpawnObject(msg.Split('|'));
        }
        else
        {
            Send(msg, reliableChannel);
            GameManager.gm.addToMapBtn.SetActive(false);
        }
    }

    public void OnSpawnObject(string[] data)
    {
        int clientID = int.Parse(data[2]);
        string type = data[3];
        int typeID = int.Parse(data[4]);
        string msg = data[0] + "|" + activityLog.Count + "|";// + data[2] + "|" + data[3] + "|" + data[4];
        for(int i = 2; i < data.Length; i++)
        {
            msg += data[i] + "|";
        }
        if (type == "TRAP")
        {
            type = "Trap";
            debugLog.Add("SPAWN TRAP " + typeID);
            // If we requested to spawn defnese
            if(clientID == ourClientID)
            {
                //GameObject trap = GameManager.gm.SpawnDefense(type, typeID);
                Debug.Log("NETWORK SPAWNED DEF");// + trap.transform.localScale);
                //trap.GetComponent<Trap>().SetNetworkInformation(new string[] { data[3], data[4], data[5] });



                GameManager.gm.SpawnDefense(type, typeID, GameManager.gm.selectedDefense);
                int id = typeID;
                GameManager.gm.myTraps[id] -= 1;
                GameManager.gm.defensesContainer.transform.GetChild(id).GetChild(0).GetComponent<Text>().text = "TNT x" + GameManager.gm.myTraps[id];
                GameManager.gm.defensesContainer.transform.GetChild(id).gameObject.SetActive(GameManager.gm.myTraps[id] > 0);
                // Don't show capability to spawn more of this item if no more in inventory
                if (GameManager.gm.myTraps[id] == 0)
                    GameManager.gm.DeselectDescription();
                GameManager.gm.selectedDefense.SetActive(false);
                // Re-enable ability to spawn defenses once this has been confirmed as client
                if(!isHost)
                    GameManager.gm.addToMapBtn.SetActive(true);
                GameManager.gm.mapFingerID = -1;
                GameManager.gm.selectedDefense = null;
                isSpawningObject = false;
            }
            // Another player requested to spawn defense
            else
            {
                GameObject trap = GameManager.gm.SpawnDefense(type, typeID);
                Debug.Log("NETWORK SPAWNED DEF" + trap.transform.localScale);
                trap.GetComponent<Trap>().SetNetworkInformation(new string[] { data[3], data[4], data[5],data[6] });
            }
        }
        if (isHost)
        {
            Send(msg, reliableChannel, players);
        }
        activityLog.Add(msg);
        Debug.Log(isSpawningObject);
    }

    public void NotifyRemoveDefense(string type, int id)
    {
        string msg = "REMOVEDEFENSE|" + activityLog.Count + "|" + ourClientID + "|" + type + "|" + id + "|";
        if (isHost)
        {
            Send(msg, reliableChannel, players);
            OnRemoveDefense(msg.Split('|'));
        }
        else
        {
            Send(msg,reliableChannel);
        }
    }

    public void OnRemoveDefense(string[] data)
    {
        int clientID = int.Parse(data[2]);
        string type = data[3];
        int id = int.Parse(data[4]);
        if(type == "TRAP")
        {
            if (!GameManager.gm.traps.ContainsKey(id))
                return;
        }
        if(clientID == ourClientID)
        {
            GameManager.gm.UpdateInGameCurrency(50);
            GameManager.gm.selectedDefense = null;
        }
        string msg = data[0] + "|" + activityLog.Count + "|" + data[2] + "|" + data[3] + "|" + data[4] + "|";
        activityLog.Add(msg);

        if (isHost)
        {
            debugLog.Add(msg);
            Send(msg, reliableChannel, players);
        }

        GameManager.gm.RemoveSelectedDefense(type, id);
    }

    // Notify players that source is damaged by target
    public void NotifyObjectDamagedBy(GameObject target, GameObject source)
    {
        string msg = "";
        int tid = -1, 
            sid = -1,
            dmg = 0;

        if (target.tag == "Enemy")
        {
            Enemy e = target.transform.GetComponent<Enemy>();
            tid = e.id;
            msg = "ENEMY";
        }
        else if (target.tag == "Objective")
        {
            Objective o = target.transform.GetComponent<Objective>();
            tid = o.id;
            msg = "OBJECTIVE";
        }
        else if (target.tag == "Trap")
        {
            Trap t = target.transform.GetComponent<Trap>();
            tid = t.id;
            msg = "TRAP";
        }

        if (source.tag == "Projectile")
        {
            Projectile p = source.transform.GetComponent<Projectile>();
            sid = p.id;
            dmg = p.dmg;
        }
        else if (source.tag == "Enemy")
        {
            Enemy e = source.transform.GetComponent<Enemy>();
            sid = e.id;
            dmg = e.dmg;
        }
        else if(source.tag == "Explosion")
        {
            Explosion ex = source.transform.GetComponent<Explosion>();
            sid = ex.id;
            dmg = ex.dmg;
        }

        msg += "DMG|" + activityLog.Count + "|" + tid + "|" + sid + "|" + dmg + "|";
        debugLog.Add(msg);
        debugLog.Add(source.tag + " " + source.name);
        Debug.Log(msg);
        // As host, tell clients about target being damaged by source
        if (isHost)
        {
            //Send(msg, reliableChannel,players);
            //activityLog.Add(msg);
            if (target.tag == "Enemy")
                OnEnemyDamaged(msg.Split('|'));
            else if (target.tag == "Trap")
                OnTrapDamaged(msg.Split('|'));
        }
        // As client, notify host about target being attacked by source, if disconnected keep track of activity
        else
        {
            if (isDisconnected)
            {
                activitiesToSend.Add(msg);
                return;
            }
            Send(msg, reliableChannel);
        }
    }

    // Inflict damage to trap
    public void OnTrapDamaged(string[] data)
    {
        int targetID = int.Parse(data[2]);
        int sourceID = int.Parse(data[3]);
        int dmg = int.Parse(data[4]);
        debugLog.Add("TRAP DAMAGED" + targetID);
        // Inflict damage if enemy exists
        if (GameManager.gm.traps.ContainsKey(targetID))
        {
            print("HAVE " + targetID);
            GameManager.gm.traps[targetID].TakeDamage(dmg, sourceID);
            // Remove enemy if non-existent or dead
            if (GameManager.gm.traps[targetID] == null || GameManager.gm.traps[targetID].hp <= 0)
            {
                //GameManager.gm.traps.Remove(targetID);
            }

            string msg = data[0] + "|" + activityLog.Count + "|" + data[2] + "|" + data[3] + "|" + data[4] + "|";
            activityLog.Add(msg);

            if (isHost)
            {
                debugLog.Add(msg);
                Send(msg, reliableChannel, players);
            }
        }
    }

    // Un-ready all players
    public void ResetReadyStatus()
    {
        foreach (KeyValuePair<int, Player> kvp in players)
        {
            kvp.Value.isReady = false;
        }
    }

    // Announce to start game
    public void StartGame()
    {
        enemyAttacks = new Dictionary<int, Dictionary<int, Attack>>();
        activityLog = new List<string>();
        activityLog.Add("STARTGAME|0|");
        inLobby = false;
        // As host, stop broadcasting availability and tell clients to start game
        if (isHost)
        {
            StopBroadcast();
            Send("STARTGAME|0|", reliableChannel, players);
        }
        // As client, tell host that game has started
        else
        {
            Send("STARTGAME|0|", reliableChannel);
        }
        ResetReadyStatus();
        GameManager.gm.GoToGameScene();
        // As client, tell host that I am in game and ready to start wave
        if (!isHost)
        {
            RequestReady();
        }
    }

    // Announce to start wave
    public void StartWave(int wave)
    {
        string msg =  "STARTWAVE|" + activityLog.Count + "|" + wave + "|";
        // As host, tell clients to start the wave
        if (isHost)
        {
            Send(msg, reliableChannel, players);
        }
        GameManager.gm.StartWave(wave);
        activityLog.Add(msg);
        ResetReadyStatus();
    }

    // Remove player from lobby when disconnected
    private void PlayerDisconnected(int cnnId)
    {
        // If I disconnect, leave lobby
        if(cnnId == ourClientID)
        {
            Debug.Log("Leaving from player dc");
            LeaveLobby();
            return;
        }
        // Remove player that's not us from our players list
        Destroy(players[cnnId].playerGO);
        players.Remove(cnnId);
    }

    // As client, tell host our name, also create players for everyone in lobby excluding me
    private void OnAskName(string[] data)
    {
        // Display the lobby canvas upon joining room
        GameManager.gm.ToggleHostListCanvas();
        GameManager.gm.ToggleLobbyCanvas();

        inLobby = true;
        requestingConnection = false;

        ourClientID = int.Parse(data[1]);
        // Send our name to server
        Send("NAMEIS|" + "PLAYER", reliableChannel);

        // Create all the other players
        for (int i = 2; i < data.Length; i++)
        {
            string[] d = data[i].Split('%');
            SpawnPlayerUI(d[0], int.Parse(d[1]));
        }
    }

    // Create lobby objects for player
    private void SpawnPlayerUI(string playerName, int cnnId)
    {
        GameObject go = Instantiate(GameManager.gm.playerUIPrefab);
        
        playerName += "" + cnnId;
        Player p = new Player();
        p.playerGO = go;
        p.playerName = playerName;
        p.connectionID = cnnId;
        p.playerGO.transform.SetParent(GameManager.gm.lobbyCanvas.transform.Find("PlayerList"));
        p.playerGO.transform.Find("NameText").GetComponent<Text>().text = playerName;
        // Our client?
        if (cnnId == ourClientID)
        {
            p.playerGO.transform.Find("NameText").GetComponent<Text>().text = "ME";
        }
        go.transform.localScale = new Vector3(1, 1, 1);
        players.Add(cnnId, p);
    }

    // Stop using online feature
    public void Disconnect()
    {
        if (!isStarted)
            return;
        if (isHost)
        {
            if (isBroadcasting)
            {
                StopBroadcast();
            }
        }
        if (hostID != -1)
        {
            if (isHost)
                NetworkTransport.Disconnect(hostID, connectionId, out error);
            NetworkTransport.RemoveHost(hostID);
        }
        hostID = -1;
        findingHosts = false;
        connectedHost = null;
        isStarted = false;
        Debug.Log("Disconnected");
    }

    // General purpose for requesting actions as client
    public void RequestAction(string action)
    {
        Send(action + "?|", reliableChannel);
    }

    // Gain approval from player to proceed to next step in game
    public void RequestReady()
    {
        // As host, default to being ready
        if (isHost)
        {
            Ready(ourClientID);
            return;
        }
        // As client, tell host that I am ready and awaiting the next step
        RequestAction("READY");
        if (!GameManager.gm.inGame)
        {
            GameManager.gm.lobbyCanvas.transform.Find("BackBtn").GetComponent<Button>().interactable = false;
            GameManager.gm.lobbyCanvas.transform.Find("ReadyBtn").GetComponent<Button>().interactable = false;
        }
        else
        {
            if (GameManager.gm.onIntermission)
            {
                GameManager.gm.intermissionCanvas.transform.Find("ResumeBtn").GetComponent<Button>().interactable = false;
                GameManager.gm.intermissionCanvas.transform.Find("SaveAndQuitBtn").GetComponent<Button>().interactable = false;
            }
        }
    }

    // Confirm the ready status from player
    public void OnReady(int cnnID)
    {
        // As host, tell other players that client is ready
        if (isHost)
            Send("READY|" + cnnID, reliableChannel, players);
        Ready(cnnID);
    }

    // Internal workings of ensuring all players are ready for next step in game
    public bool ConfirmReadyStatus(int cnnID)
    {
        bool readyToStart = true;
        players[cnnID].isReady = !players[cnnID].isReady;
        foreach (KeyValuePair<int, Player> kvp in players)
        {
            if (kvp.Value.connectionID != 0)
            {
                readyToStart = kvp.Value.isReady;
                if (!readyToStart)
                    break;
            }
        }
        return readyToStart;
    }

    // Handles behaviors for being ready for a player throughout the game
    public void Ready(int cnnID)
    {
        bool readyToStart = ConfirmReadyStatus(cnnID); // Check if all players are ready after client cnnID confirmed ready

        // Display player cnnID is ready or not in lobby or in game on intermission
        if (!GameManager.gm.inGame)
        {
            players[cnnID].playerGO.transform.Find("NameText").GetComponent<Text>().text = players[cnnID].playerName;
            if (players[cnnID].isReady)
                players[cnnID].playerGO.transform.Find("NameText").GetComponent<Text>().text += " READY";
            if (isHost)
                GameManager.gm.lobbyCanvas.transform.Find("ReadyBtn").GetComponent<Button>().interactable = readyToStart && players.Count > 1;
        }
        else
        {
            if (GameManager.gm.onIntermission)
            {
                GameManager.gm.intermissionCanvas.transform.Find("ResumeBtn").GetComponent<Button>().interactable = readyToStart;
            }
        }

        // As host, if in lobby and everyone is ready, ...
        if (isHost)
        {
            if (cnnID == ourClientID)
            {
                // In lobby? Start the game
                if (!GameManager.gm.inGame)
                {
                    StartGame();
                    return;
                }
                // In game? Start next wave
                else
                {
                    if(GameManager.gm.onIntermission)
                    {
                        StartWave(GameManager.gm.wave);
                        return;
                    }
                }
            }
            // If in game and not on intermission as host, start the next wave when everyone is done with the current wave
            if (GameManager.gm.inGame && readyToStart && !GameManager.gm.onIntermission)
                StartWave(GameManager.gm.wave);
        }
        // As client, re-enable action buttons when host confirms you are ready or not
        else if(cnnID == ourClientID)
        {
            if (!GameManager.gm.inGame)
            {
                GameManager.gm.lobbyCanvas.transform.Find("BackBtn").GetComponent<Button>().interactable = true;
                GameManager.gm.lobbyCanvas.transform.Find("ReadyBtn").GetComponent<Button>().interactable = true;
            }
            else
            {
                if (GameManager.gm.onIntermission)
                {
                    GameManager.gm.intermissionCanvas.transform.Find("ResumeBtn").GetComponent<Button>().interactable = true;
                    GameManager.gm.intermissionCanvas.transform.Find("SaveAndQuitBtn").GetComponent<Button>().interactable = true;
                }
            }
        }
    }

    // Leave lobby
    public void RequestLeaveLobby()
    {
        // As host, just leave lobby, clients will all leave lobby once disconnected
        if (isHost)
        {
            LeaveLobby();
            return;
        }
        // As client, tell host I am leaving lobby
        RequestAction("LEAVELOBBY");
        // Disable any action until confirmed by host or host dcs
        GameManager.gm.lobbyCanvas.transform.Find("BackBtn").GetComponent<Button>().interactable = false;
        GameManager.gm.lobbyCanvas.transform.Find("ReadyBtn").GetComponent<Button>().interactable = false;
    }
    
    private void OnLevelWasLoaded(int level)
    {
        if (this != nm)
            return;
        switch (level)
        {
            // Main
            case 0:
                LoadMainScene();
                break;
            // Game
            case 1:
                LoadGameScene();
                break;
            // Calibration
            case 2:
                break;
            // Lobby
            case 3:
                LoadLobbyScene();
                break;
            // Game (Multiplayer)
            case 4:
                break;
        }
    }

    // Handle leaving game and go back to main scene
    public void LeaveGame()
    {
        Debug.Log("Leaving Game");
        Disconnect(); // Stop using online feature
        players.Clear();
        playerDC.Clear();
        inLobby = false;
        GameManager.gm.GoToMainScene();
    }

    // Handle leaving lobby and go to multiplayer canvas
    public void LeaveLobby()
    {
        Debug.Log("Leaving Lobby");

        Disconnect(); // Stop using online feature
        // Remove all the player ui objects
        foreach(KeyValuePair<int,Player> kvp in players)
        {
            Destroy(kvp.Value.playerGO);
        }
        players.Clear();
        GameManager.gm.ToggleLobbyCanvas();
        GameManager.gm.ToggleMultiplayerCanvas();
        inLobby = false;
    }
    
    // Handle loading game scene as Network Manager
    public IEnumerator LoadGameScene()
    {
        DCNotification = GameManager.gm.playerStatusCanvas.transform.Find("DC Notification").gameObject;
        DCNotification.SetActive(false);

        GameManager.gm.playerStatusCanvas.transform.Find("ActivitiesTxt").gameObject.SetActive(GameManager.Debugging);
        GameManager.gm.playerStatusCanvas.transform.Find("DebugTxt").gameObject.SetActive(GameManager.Debugging);
        GameObject playerSpawnPoints = GameManager.gm.playerSpawnPoints;
        debugLog = new List<string>();
        int spawnPt = 0;
        // Instantiate player object for each player and given them their weapons
        // Make sure only my player has the camera on
        foreach (KeyValuePair<int, Player> kvp in players)
        {
            GameObject playerGO = Instantiate(GameManager.gm.playerPrefab);
            GameObject wep = Instantiate(GameManager.gm.weaponPrefabs[0]);
            PlayerController pc = playerGO.transform.GetComponent<PlayerController>();
            Weapon w = wep.transform.GetComponent<Weapon>();
            pc.EquipWeapon(w);
            w.purchased = true;
            kvp.Value.playerGO = playerGO;
            if (kvp.Key != ourClientID)
            {
                playerGO.transform.GetComponent<PlayerController>().playerCam.GetComponent<Camera>().enabled = false;
                playerGO.transform.GetComponent<PlayerController>().playerCam.GetComponent<AudioListener>().enabled = false;

            }
            else
            {
                GameManager.gm.player = playerGO;
            }
            playerGO.transform.GetComponent<PlayerController>().id = kvp.Value.connectionID;
            playerGO.transform.position = playerSpawnPoints.transform.GetChild(spawnPt).position;
            playerGO.transform.SetParent(GameManager.gm.playerRotation.transform);
            spawnPt++;
        }
        yield return new WaitForSeconds(0);
    }

    public void LoadMainScene()
    {
        Disconnect(); // Necessary?
        isStarted = false;
        players.Clear();
        playerDC.Clear();
    }

    // unused?
    public void LoadLobbyScene()
    {
        GameManager.gm.playerRotation = GameObject.Find("Player Rotation");
        GameObject playerSpawnPoints = GameManager.gm.playerRotation.transform.Find("Player Spawn Points").gameObject;
        Player p = new Player();
        p.connectionID = ourClientID;
        p.playerName = "You";
        GameObject myPlayer = Instantiate(GameManager.gm.playerPrefab);
        p.playerGO = myPlayer;
        myPlayer.transform.SetParent(GameManager.gm.playerRotation.transform);
        players.Add(ourClientID, p);
        for(int i = 0; i < players.Count; i++)
        {
            players[i].playerGO.transform.position = playerSpawnPoints.transform.GetChild(i).position;
            if(i != ourClientID)
            {
                players[i].playerGO.transform.GetComponent<PlayerController>().playerCam.gameObject.SetActive(false);
            }
        }
        GameManager.gm.playerRotation.transform.eulerAngles = GameManager.gm.playerOrientation;
    }

    // Send message to host
    private void Send(string message, int channelId)
    {
        byte[] msg = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostID, connectionId, channelId, msg, message.Length * sizeof(char), out error);
    }

    // Send message to specific client
    private void Send(string message, int channelId, int cnnId)
    {
        NetworkTransport.Send(hostID, cnnId, channelId, Encoding.Unicode.GetBytes(message), message.Length * sizeof(char), out error);
    }

    // Send message to all players not disconnected
    private void Send(string message, int channelId, Dictionary<int, Player> c)
    {
        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach (KeyValuePair<int, Player> kvp in players)
        {
            Player p = kvp.Value;
            if (p != null && /*p != myPlayer &&*/ !playerDC.ContainsKey(kvp.Key))
                NetworkTransport.Send(hostID, p.connectionID, channelId, msg, message.Length * sizeof(char), out error);
        }
    }
}
