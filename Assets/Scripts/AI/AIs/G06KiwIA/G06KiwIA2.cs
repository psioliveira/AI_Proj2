using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.AI.AIs.G06KiwIA
{



    public class G06KiwIA2 : AIPlayer
    {
        public override string PlayerName => "KiwIA2";
        public override IThinker Thinker => thinker;

        public int depth = 2;
        private IThinker thinker;
        public override void Setup()
        {
            base.Awake();
            thinker = new G06KiwIAThinker(depth);
        }
    }

}