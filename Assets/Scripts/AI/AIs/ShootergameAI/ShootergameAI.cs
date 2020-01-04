using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootergameAI : AIPlayer
{
    public override string PlayerName => "Shootergame.ex";
    public override IThinker Thinker => thinker;

    private IThinker thinker;
    public override void Setup()
    {
        base.Awake();
        thinker = new ShootergameAIThinker();
    }
}
