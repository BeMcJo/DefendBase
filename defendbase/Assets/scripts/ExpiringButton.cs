using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpiringButton : MonoBehaviour
{
    public int timer;
    public Host host;

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

    public void Join()
    {
        NetworkManager.nm.ConnectTo(host);
        GetComponent<Button>().interactable = false;
        NetworkManager.nm.ClearHostList();
    }
}
