using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G01AI : AIPlayer
{
    public override string PlayerName => "G01AI";
    public override IThinker Thinker => thinker;

    private IThinker thinker;
    public override void Setup()
    {
        base.Awake();
        thinker = new G0A1Thinker();
    }

}