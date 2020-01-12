using System;
using System.Threading;

/// <summary>
/// Implementation of an AI that will play randomly.
/// </summary>
namespace Assets.Scripts.AI.AIs.G06KiwIA
{
    class G06KiwIA : AIPlayer
    {

        public override string PlayerName => "KiwIA";
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