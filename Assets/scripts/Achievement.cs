﻿using System.Collections;
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
    int count; // requirement to be satisfied
    public Condition(int count)
    {
        this.count = count;
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
                "Most Money Saved In A Game",
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

    public static Achievement[] conditionalAchievements =
    {
        new Achievement(
                "Kill 10 enemies using " + Projectile.projectileStats[0].name,
                "",
                true,
                new Condition[] {
                    new Condition(10)
                }
            ),

    };

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

    public static void Instantiate()
    {
        cumulativeScoreAchievements = new Achievement[6][];
        cumulativeScoreAchievements[0] = new Achievement[Enemy.difficulties.Length]; // index 0 = kills by enemy
        cumulativeScoreAchievements[1] = new Achievement[Projectile.projectileStats.Length]; // index 1 = kills by arrow attribute
        cumulativeScoreAchievements[2] = new Achievement[Projectile.projectileStats.Length]; // index 2 = arrows shot by attribute
        cumulativeScoreAchievements[3] = new Achievement[Enemy.difficulties.Length]; // index 3 = weak spots hit by enemy
        cumulativeScoreAchievements[4] = new Achievement[Weapon.weaponStats.Length]; // index 4 = weapons used by game
        cumulativeScoreAchievements[5] = new Achievement[Weapon.weaponStats.Length]; // index 5 = enemy kill count per weapon
        for (int i = 0; i < cumulativeScoreAchievements[0].Length; i++)
        {
            cumulativeScoreAchievements[0][i] = new Achievement(Enemy.names[i] + " killed", "");
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
            cumulativeScoreAchievements[4][i] = new Achievement(Weapon.weaponStats[i].name + " used", "");
            cumulativeScoreAchievements[5][i] = new Achievement(Weapon.weaponStats[i].name + " kill count", "");
        }

    }

    // Returns true if conditions are met to show progress details of achievement, false otherwise
    public static bool CanShowProgress(AchievementType type, int achievementID, int scoreType=-1)
    {
        switch (type)
        {
            case AchievementType.BestScore:
                return !bestScoreAchievements[achievementID].hideIfNoProgress;
                break;
            case AchievementType.Conditional:
                return !conditionalAchievements[achievementID].hideIfNoProgress;
                break;
            case AchievementType.Cumulative:
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
                }
                return !cumulativeScoreAchievements[scoreType][achievementID].hideIfNoProgress || madeProgress;
                break;
        }
        return true;
        //return (type == AchievementType.Cumulative) ? cumulativeScoreAchievements[scoreType][achievementID].hideIfNoProgress && : conditionalAchievements[achievementID].hideIfNoProgress;
    }
    
    bool CanShowProgressByAchievementType(int achievementID)
    {
        
        return false;
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
        }

        return details;
    }

    // Handles fetching the returned detail for conditional achievements
    static string GetConditionalAchievementDetails(int achievementID)
    {
        string details = "";

        switch (achievementID)
        {
            // Kill 10 enemies using standard arrow - REWARD: Get Bomb Arrow
            case 0:
                details = "" + GameManager.gm.personalData.killsByArrowAttribute[0] + "/" + conditionalAchievements[achievementID].conditions[0];
                break;
            
        }

        return details;
    }
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

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}