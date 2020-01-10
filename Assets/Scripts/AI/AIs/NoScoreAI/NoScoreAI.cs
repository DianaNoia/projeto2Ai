using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoScoreAI : AIPlayer
{
    public override string PlayerName => "NoScores";
    public override IThinker Thinker => thinker;

    private IThinker thinker;
    public override void Setup()
    {
        base.Awake();
        thinker = new NoScoreAIThinker();
    }
}
