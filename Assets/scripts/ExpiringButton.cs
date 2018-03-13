using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Used for keeping track of how long a join button lasts
public class ExpiringButton : MonoBehaviour
{
    public int timer; // How long before destroying this
    public Host host; // Keep track of hosts broadcasting available rooms

    public void Start()
    {
        GetComponent<Button>().onClick.AddListener(Join);
    }

    public void SetExpiringButton(Host h)
    {
        Reset();
        host = new Host();
        host.ip = new string(h.ip.ToCharArray());
        host.port = h.port;
    }

    public void Reset()
    {
        timer = 100;
    }

    public void Decrement()
    {
        timer--;
    }

    public bool Expired()
    {
        return timer <= 0;
    }

    // Attempt to join host room
    public void Join()
    {
        NetworkManager.nm.ConnectTo(host);
        GetComponent<Button>().interactable = false; // Prevent player from spamming to join room
        NetworkManager.nm.ClearHostList();
    }
}
