using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingSlime : Slime {

    // take damage dmg from source. upon death we can determine what the source was (player, trap, etc)
    public override void TakeDamageFrom(int dmg, string dmgSourceType, int sid)
    {
        int prevHP = health;
        base.TakeDamageFrom(dmg, dmgSourceType, sid);
        if(health != 0)
        {
            if (curTarget < pathing.Count)
            {
                print("followme");
                GameManager.gm.SpawnEnemy(0, curTarget, pathing, transform.Find("SpawnPt").position);
            }
            else
            {
                print("CHARGE");
                GameManager.gm.SpawnEnemy(0, target, transform.Find("SpawnPt").position);
            }
            print("OW");
        }
    }
}
