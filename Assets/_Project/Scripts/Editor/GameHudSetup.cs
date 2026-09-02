#if UNITY_EDITOR
using MarioTest.Input;
using MarioTest.Player;
using MarioTest.Systems;
using MarioTest.UI;
using UnityEngine;

namespace MarioTest.Editor
{
    public sealed class GameHudSetup
    {
        public MobileTouchInput TouchInput;
        public GameHud GameHud;
        public CoinsHud CoinsHud;
    }
}
#endif
