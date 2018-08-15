using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AchievementType
{
    Cumulative,
    BestScore,
    Conditional
}

public enum ConditionType
{
    moreThan,
    lessThan,
    equalTo,

}

public class Condition
{
    public int count; // requirement to be satisfied
    public int itemID; // ID of reward 
    public int rewardQty; // amount of reward (for arrows)
    public string rewardType; // weapon, attribute, ...
    public Condition(int count, int itemID, string rewardType, int rewardQty=0)
    {
        this.count = count;
        this.itemID = itemID;
        this.rewardType = rewardType;
        this.rewardQty = rewardQty;
    }
}

public class Achievement {
    public static Achievement[] bestScoreAchievements =
    {
        new Achievement(
                "Best Score",
                "",
                false
            ),
        new Achievement(
                "Most Kills In A Game",
                "",
                false
            ),
        new Achievement(
                "What shall this score be?",
                "",
                false
            ),
        new Achievement(
                "Highest Enemy Wave Survived",
                "",
                false
            ),
        new Achievement(
                "Total Enemies Killed",
                "",
                false
            ),
        new Achievement(
                "Most Weak Spots Hit In A Game",
                "",
                false
            ),

    };

    public static Achievement[][] cumulativeScoreAchievements;
    /*=
    {
        new Achievement(
               // AchievementType.Cumulative,
                "Cumulative Score",
                ""
            ),

    };*/

    public AchievementType achievementType;
    public string header, description;
    public Condition[] conditions;
    bool hideIfNoProgress; // hide achievement details unless progress has been made

    // constructor
    public Achievement( string header, string description, bool hide = true, Condition[] conditions=null)
    {
        //achievementType = type;
        this.header = header;
        this.description = description;
        this.conditions = conditions;
        hideIfNoProgress = hide;
    }

    // instantiate cumulative score achievements
    public static void Instantiate()
    {
        cumulativeScoreAchievements = new Achievement[][]
        {
            new Achievement[Enemy.enemyStats.Length], // index 0 = kills by enemy
            new Achievement[Projectile.projectileStats.Length], // index 1 = kills by arrow attribute
            new Achievement[Projectile.projectileStats.Length], // index 2 = arrows shot by attribute
            new Achievement[Enemy.enemyStats.Length], // index 3 = weak spots hit by enemy
            new Achievement[Weapon.weaponStats.Length], // index 4 = weapons used by game
            new Achievement[Weapon.weaponStats.Length], // index 5 = enemy kill count per weapon
            new Achievement[Weapon.weaponStats.Length] // index 6 = weak spots hit per weapon
        };

        for (int i = 0; i < cumulativeScoreAchievements[0].Length; i++)
        {
            cumulativeScoreAchievements[0][i] = new Achievement(Enemy.names[i] + "s killed", "");
            cumulativeScoreAchievements[3][i] = new Achievement(Enemy.names[i] + " weak spots hit", "");
        }
        for (int i = 0; i < cumulativeScoreAchievements[1].Length; i++)
        {
            cumulativeScoreAchievements[1][i] = new Achievement(Projectile.projectileStats[i].name + " kills", "");
        }
        for (int i = 0; i < cumulativeScoreAchievements[2].Length; i++)
        {
            cumulativeScoreAchievements[2][i] = new Achievement(Projectile.projectileStats[i].name + "s shot", "");
        }
        for (int i = 0; i < cumulativeScoreAchievements[4].Length; i++)
        {
            cumulativeScoreAchievements[4][i] = new Achievement(Weapon.weaponStats[i].name + "s used", "");
            cumulativeScoreAchievements[5][i] = new Achievement(Weapon.weaponStats[i].name + " kill count", "");
            cumulativeScoreAchievements[6][i] = new Achievement(Weapon.weaponStats[i].name + " weak spots hit", "");
        }

    }

    // Returns true if conditions are met to show progress details of achievement, false otherwise
    public static bool CanShowProgress(AchievementType type, int achievementID, int scoreType = -1)
    {
        switch (type)
        {
            case AchievementType.BestScore:
                return !bestScoreAchievements[achievementID].hideIfNoProgress;
            case AchievementType.Conditional:
                //return true;
                // show progress if we don't need to hide at all OR if we have made progress
                return !conditionalAchievements[achievementID].hideIfNoProgress || GetQuestProgressPercentage(achievementID) > 0;
            case AchievementType.Cumulative:
                //return true;
                bool madeProgress = false;
                switch (scoreType)
                {
                    // kills by enemy
                    case 0:
                        madeProgress = GameManager.gm.personalData.killsByEnemy[achievementID] > 0;
                        break;
                    //  kills by arrow attribute
                    case 1:
                        madeProgress = GameManager.gm.personalData.killsByArrowAttribute[achievementID] > 0;
                        break;
                    // arrows shot by attribute
                    case 2:
                        madeProgress = GameManager.gm.personalData.arrowsShotByAttribute[achievementID] > 0;
                        break;
                    // weak spots hit by enemy
                    case 3:
                        madeProgress = GameManager.gm.personalData.weakSpotsHitByEnemy[achievementID] > 0;
                        break;
                    // weapons used by game
                    case 4:
                        madeProgress = GameManager.gm.personalData.weaponsUsedByGame[achievementID] > 0;
                        break;
                    // enemy kill count per weapon
                    case 5:
                        madeProgress = GameManager.gm.personalData.enemiesKilledByWeapon[achievementID] > 0;
                        break;
                    // weak spots hit per weapon
                    case 6:
                        madeProgress = GameManager.gm.personalData.weakSpotsHitByWeapon[achievementID] > 0;
                        break;
                }
                return !cumulativeScoreAchievements[scoreType][achievementID].hideIfNoProgress || madeProgress;
        }
        return true;
        //return (type == AchievementType.Cumulative) ? cumulativeScoreAchievements[scoreType][achievementID].hideIfNoProgress && : conditionalAchievements[achievementID].hideIfNoProgress;
    }

    // Returns whether or not item used for record is enabled or not
    public static bool IsItemActive(AchievementType type, int achievementID, int scoreType = -1)
    {
        switch (type)
        {
            //case AchievementType.BestScore:
            //    return !bestScoreAchievements[achievementID].hideIfNoProgress;
            //    break;
            //case AchievementType.Conditional:
                //return true;
                // show progress if we don't need to hide at all OR if we have made progress
            //    return !conditionalAchievements[achievementID].hideIfNoProgress || GetQuestProgressPercentage(achievementID) > 0;
            //    break;
            case AchievementType.Cumulative:
                //return true;
                switch (scoreType)
                {
                    // kills by enemy
                    case 0:
                        break;
                    //  kills by arrow attribute
                    case 1:
                        return Projectile.enableArrows[achievementID];
                    // arrows shot by attribute
                    case 2:
                        return Projectile.enableArrows[achievementID];
                    // weak spots hit by enemy
                    case 3:
                        break;
                    // weapons used by game
                    case 4:
                        return Weapon.enableWeps[achievementID];
                    // enemy kill count per weapon
                    case 5:
                        return Weapon.enableWeps[achievementID];
                    // weak spots hit per weapon
                    case 6:
                        return Weapon.enableWeps[achievementID];
                }
                break;
        }
        return true;
        //return (type == AchievementType.Cumulative) ? cumulativeScoreAchievements[scoreType][achievementID].hideIfNoProgress && : conditionalAchievements[achievementID].hideIfNoProgress;
    }


    // Handles fetching the returned detail for best score achievements
    static string GetBestScoreAchievementDetails(int achievementID)
    {
        string details = "";

        switch (achievementID)
        {
            // High Score
            case 0:
                details = GameManager.gm.personalData.bestScore + "";
                break;
            // Most Kills In A Game
            case 1:
                details = GameManager.gm.personalData.mostKillsInGame + "";
                break;
            // Most Money Saved In A Game
            case 2:
                details = GameManager.gm.personalData.mostCurrencySavedInGame + "";
                break;
            // Highest Wave Survived In A Game
            case 3:
                details = GameManager.gm.personalData.highestWaveSurvived + "";
                break;
            // total enemies killed
            case 4:
                int sum = 0;
                for (int i = 0; i < GameManager.gm.personalData.killsByEnemy.Length; i++)
                    sum += GameManager.gm.personalData.killsByEnemy[i];
                details = sum + "";
                break;
            // Most Weak Spots Hit In A Game
            case 5:
                details = GameManager.gm.personalData.mostWeakSpotsHitInAGame + "";
                break;
        }

        return details;
    }

    // Handles fetching the returned detail for cumulative score achievements
    static string GetCumulativeScoreAchievementDetails(int achievementID, int scoreType)
    {
        string details = "";

        switch (scoreType)
        {
            // kills by enemy
            case 0:
                details = GameManager.gm.personalData.killsByEnemy[achievementID] + "";
                break;
            //  kills by arrow attribute
            case 1:
                details = GameManager.gm.personalData.killsByArrowAttribute[achievementID] + "";
                break;
            // arrows shot by attribute
            case 2:
                details = GameManager.gm.personalData.arrowsShotByAttribute[achievementID] + "";
                break;
            // weak spots hit by enemy
            case 3:
                details = GameManager.gm.personalData.weakSpotsHitByEnemy[achievementID] + "";
                break;
            // weapons used by game
            case 4:
                details = GameManager.gm.personalData.weaponsUsedByGame[achievementID] + "";
                break;
            // enemy kill count per weapon
            case 5:
                details = GameManager.gm.personalData.enemiesKilledByWeapon[achievementID] + "";
                break;
            // weak spots hit per weapon
            case 6:
                details = GameManager.gm.personalData.weakSpotsHitByWeapon[achievementID] + "";
                break;
        }

        return details;
    }

    public static Achievement[] conditionalAchievements =
    {
        // unlock bomb arrow
        new Achievement(
                "Kill 1000 enemies using " + Projectile.projectileStats[0].name,
                "",
                true,
                new Condition[] {
                    new Condition(1000,1,"attribute",20)
                }
            ),
        // unlock fire arrow
        new Achievement(
                "Kill 2,500 enemies using " + Projectile.projectileStats[1].name,
                "",
                true,
                new Condition[] {
                    new Condition(2500,5,"attribute",10)
                }
            ),
        // unlock sniper bow
        new Achievement(
                "Hit 7,500 enemy weak spots",
                "",
                true,
                new Condition[] {
                    new Condition(7500,2,"weapon")
                }
            ),
        
        // unlock bullet arrow
        new Achievement(
                "Hit 2,500 enemy weak spots using " + Weapon.weaponStats[2].name,
                "",
                true,
                new Condition[] {
                    new Condition(2500,3,"attribute", 20)
                }
            ),
        
        // unlock ice arrow
        new Achievement(
                "Kill 1,500 " + Enemy.names[2] +"s",
                "",
                true,
                new Condition[] {
                    new Condition(1500,4,"attribute", 20)
                }
            ),

    };

    public static bool[] enableQuests =
    {
        true,true,true,false,true
    };

    // Fetches the percentage of quest completion
    public static float GetQuestProgressPercentage(int achievementID)
    {
        float details = 0;

        switch (achievementID)
        {
            // Kill 1000 enemies using standard arrow - REWARD: Unlock Bomb Arrow
            case 0:
                details = (float)GameManager.gm.personalData.killsByArrowAttribute[0] / conditionalAchievements[achievementID].conditions[0].count;
                break;
            // Kill 2,500 enemies using bomb arrow - REWARD: Unlock Fire Arrow
            case 1:
                details = (float)GameManager.gm.personalData.killsByArrowAttribute[1] / conditionalAchievements[achievementID].conditions[0].count;
                break;
            // Hit 7,500 enemy weak spots - REWARD: Unlock Sniper Bow
            case 2:
                int totalWeakSpotsHit = 0;
                for (int i = 0; i < GameManager.gm.personalData.weakSpotsHitByEnemy.Length; i++)
                {
                    totalWeakSpotsHit += GameManager.gm.personalData.weakSpotsHitByEnemy[i];
                }
                details = (float)totalWeakSpotsHit / conditionalAchievements[achievementID].conditions[0].count;
                break;
            // Hit 2,500 enemy weak spots using Sniper Bow - REWARD: Unlock Bullet Arrow
            case 3:
                details = (float)GameManager.gm.personalData.weakSpotsHitByWeapon[2] / conditionalAchievements[achievementID].conditions[0].count;
                break;
            // Kill 1,500 Ice Flyglets - REWARD: Unlock Ice Arrow
            case 4:
                details = (float)GameManager.gm.personalData.killsByEnemy[2] / conditionalAchievements[achievementID].conditions[0].count;
                break;

        }

        return details;
    }

    // Handles fetching the returned detail for conditional achievements
    static string GetConditionalAchievementDetails(int achievementID)
    {
        string details = "";

        switch (achievementID)
        {
            // Kill 1000 enemies using standard arrow - REWARD: Unlock Bomb Arrow
            case 0:
                details = "" + Mathf.Min(GameManager.gm.personalData.killsByArrowAttribute[0], conditionalAchievements[achievementID].conditions[0].count) + "/" + conditionalAchievements[achievementID].conditions[0].count;
                break;
            // Kill 2,500 enemies using bomb arrow - REWARD: Unlock Fire Arrow
            case 1:
                details = "" + Mathf.Min(GameManager.gm.personalData.killsByArrowAttribute[1], conditionalAchievements[achievementID].conditions[0].count) + "/" + conditionalAchievements[achievementID].conditions[0].count;
                break;
            // Hit 7,500 enemy weak spots - REWARD: Unlock Sniper Bow
            case 2:
                int totalWeakSpotsHit = 0;
                for(int i = 0; i < GameManager.gm.personalData.weakSpotsHitByEnemy.Length; i++)
                {
                    totalWeakSpotsHit += GameManager.gm.personalData.weakSpotsHitByEnemy[i];
                }
                details = "" + Mathf.Min(totalWeakSpotsHit, conditionalAchievements[achievementID].conditions[0].count) + "/" + conditionalAchievements[achievementID].conditions[0].count;
                break;
            // Hit 2,500 enemy weak spots using Sniper Bow - REWARD: Unlock Bullet Arrow
            case 3:
                details = "" + Mathf.Min(GameManager.gm.personalData.weakSpotsHitByWeapon[2], conditionalAchievements[achievementID].conditions[0].count) + "/" + conditionalAchievements[achievementID].conditions[0].count;
                break;
            // Kill 1,500 Ice Flyglets - REWARD: Unlock Ice Arrow
            case 4:
                details = "" + Mathf.Min(GameManager.gm.personalData.killsByEnemy[2], conditionalAchievements[achievementID].conditions[0].count) + "/" + conditionalAchievements[achievementID].conditions[0].count;
                break;

        }

        return details;
    }

    // Get the details/progress of achievement based on ID and scoreType if cumulative
    public static string GetAchievementDetails(AchievementType type, int achievementID, int scoreType=-1)
    {
        string details = "";

        switch (type)
        {
            case AchievementType.BestScore:
                details = GetBestScoreAchievementDetails(achievementID);
                break;
            case AchievementType.Conditional:
                details = GetConditionalAchievementDetails(achievementID);
                break;
            case AchievementType.Cumulative:
                details = GetCumulativeScoreAchievementDetails(achievementID, scoreType);
                break;
        }

        return details;
    }

    public static void CheckForAchievementProgress()
    {
        Debug.Log("Checking for achievements");
        PersonalData records = GameManager.gm.personalData;
        PlayerData gameSession = GameManager.gm.data;

        // Best Score
        records.bestScore = Mathf.Max(records.bestScore, gameSession.score);

        int kills = 0, weakSpotsHit = 0;
        for(int i = 0; i < Enemy.names.Length; i++)
        {
            // cumulative kill count per enemy
            records.killsByEnemy[i] += gameSession.killsByEnemy[i];

            kills += gameSession.killsByEnemy[i];

            // cumulative weak spot hit per enemy
            records.weakSpotsHitByEnemy[i] += gameSession.weakSpotsHitByEnemy[i];

            weakSpotsHit += gameSession.weakSpotsHitByEnemy[i];
        }
        // Most Kills in Game
        records.mostKillsInGame = Mathf.Max(records.mostKillsInGame, kills);

        // Most Weak Spots Hit in Game
        records.mostWeakSpotsHitInAGame = Mathf.Max(records.mostWeakSpotsHitInAGame, weakSpotsHit);

        // Highest Wave Survived
        records.highestWaveSurvived = Mathf.Max(records.highestWaveSurvived, gameSession.wave);

        // If game session ended, increment win or lost
        if (gameSession.gameOver)
        {
            if (gameSession.hasWon)
            {
                records.totalVictories++;
            }
            else
            {
                records.totalDefeats++;
            }
        }

        // increment usage of equipped weapon
        records.weaponsUsedByGame[gameSession.wepID]++;

        // number of kills used by equipped weapon
        records.enemiesKilledByWeapon[gameSession.wepID] += kills;

        // number of weak spots hit by weapon;
        records.weakSpotsHitByWeapon[gameSession.wepID] += weakSpotsHit;

        for(int i = 0; i < Projectile.projectileStats.Length; i++)
        {
            // cumulative count of kills per arrow
            records.killsByArrowAttribute[i] += gameSession.killsByArrowAttribute[i];
            // cumulative arrows shot
            records.arrowsShotByAttribute[i] += gameSession.arrowsShotByAttribute[i];
        }
    }

    // Checks the conditional achievement to see if condition(s) are satisfied. If so, reward player
    public static bool CompletedQuest(int achievementID)
    {
        bool satisfied = false;
        switch (achievementID)
        {
            // Kill 1000 enemies using standard arrow - REWARD: Get Bomb Arrow
            case 0:
                satisfied = GameManager.gm.personalData.killsByArrowAttribute[0] >= conditionalAchievements[achievementID].conditions[0].count;
                GameManager.gm.personalData.isArrowUnlocked[conditionalAchievements[achievementID].conditions[0].itemID] = satisfied;
                break;
            // Kill 25,000 enemies using bomb arrow - REWARD: Get Fire Arrow
            case 1:
                satisfied = GameManager.gm.personalData.killsByArrowAttribute[1] >= conditionalAchievements[achievementID].conditions[0].count;
                GameManager.gm.personalData.isArrowUnlocked[conditionalAchievements[achievementID].conditions[0].itemID] = satisfied;
                break;
            // Hit 30,000 enemy weak spots - REWARD: Get Sniper Bow
            case 2:
                int totalWeakSpotsHit = 0;
                for (int i = 0; i < GameManager.gm.personalData.weakSpotsHitByEnemy.Length; i++)
                {
                    totalWeakSpotsHit += GameManager.gm.personalData.weakSpotsHitByEnemy[i];
                }
                satisfied = totalWeakSpotsHit >= conditionalAchievements[achievementID].conditions[0].count;
                GameManager.gm.personalData.isWeaponUnlocked[conditionalAchievements[achievementID].conditions[0].itemID] = satisfied;
                break;
            // Hit 2,500 enemy weak spots using Sniper Bow - REWARD: Unlock Bullet Arrow
            case 3:
                satisfied = GameManager.gm.personalData.weakSpotsHitByWeapon[2] >= conditionalAchievements[achievementID].conditions[0].count;
                GameManager.gm.personalData.isArrowUnlocked[conditionalAchievements[achievementID].conditions[0].itemID] = satisfied;
                break; 
            // Kill 1,500 Ice Flyglets - REWARD: Unlock Ice Arrow
            case 4:
                satisfied = GameManager.gm.personalData.killsByEnemy[2] >= conditionalAchievements[achievementID].conditions[0].count;
                GameManager.gm.personalData.isArrowUnlocked[conditionalAchievements[achievementID].conditions[0].itemID] = satisfied;
                break;

        }
        if (satisfied)
        {
            switch (conditionalAchievements[achievementID].conditions[0].rewardType)
            {
                case "attribute":
                    int itemID = conditionalAchievements[achievementID].conditions[0].itemID;
                    GameManager.gm.arrowQty.arr[itemID] += conditionalAchievements[achievementID].conditions[0].rewardQty;//Projectile.projectileStats[itemID].purchaseQty;
                    break;
                case "weapon":
                    break;
            }
        }
        return satisfied;
    }

    // fetches image for the specific reward
    public static Sprite GetRewardIcon(int achievementID)
    {
        Sprite icon = null;

        switch (conditionalAchievements[achievementID].conditions[0].rewardType)
        {
            case "attribute":
                icon = GameManager.gm.arrowItemIcons[conditionalAchievements[achievementID].conditions[0].itemID];
                break;
            case "weapon":
                icon = GameManager.gm.weaponItemIcons[conditionalAchievements[achievementID].conditions[0].itemID];
                break;

        }
        return icon;

    }

    // Checks the reward and unlocks it for player
    public static void UnlockQuestReward(int achievementID)
    {

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}