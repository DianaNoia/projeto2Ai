using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyAI : AIPlayer
{
    public override string PlayerName => "MyAI";
    public override IThinker Thinker => thinker;

    private IThinker thinker;
    protected override void Awake()
    {
        base.Awake();
        thinker = new MyAIThinker();
    }
}
