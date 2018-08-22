using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingSlime : Slime {

    // take damage dmg from source. upon death we can determine what the source was (player, trap, etc)
    public override void TakeDamageFrom(int dmg, string dmgSourceType, int sid)
    {
        int prevHP = health;
        base.TakeDamageFrom(dmg, dmgSourceType, sid);
        if (health <= 0 || dmg == 0)
            return;
        if (NetworkManager.nm.isStarted)
        {
            if (NetworkManager.nm.isHost)
            {
                if (curTarget < pathing.Count)
                {
                    NetworkManager.nm.SpawnEnemy(0, 1, spawnPoint, pathingIndex, curTarget, transform.Find("SpawnPt").position);
                }
                // targetting objective
                else
                {
                    NetworkManager.nm.SpawnEnemy(0, 1, spawnPoint, pathingIndex, curTarget - 1, transform.Find("SpawnPt").position);
                }
            }
            return;
        }

        // if still have pathing to traverse
        if (curTarget < pathing.Count)
        {
            GameManager.gm.SpawnEnemy(0, 1, curTarget, pathing, transform.Find("SpawnPt").position);
        }
        else
        {
            //GameManager.gm.SpawnEnemy(0, target, transform.Find("SpawnPt").position);
            GameManager.gm.SpawnEnemy(0, 1, curTarget-1, pathing, transform.Find("SpawnPt").position);
        }
        
    }
}
