using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ScoreData
{
    public string username;
    public int score;

    public ScoreData(string username, int score)
    {
        this.username = username;
        this.score = score;
    }
}
