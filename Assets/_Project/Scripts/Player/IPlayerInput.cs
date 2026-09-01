using UnityEngine;

namespace MarioTest.Player
{
    public interface IPlayerInput
    {
        Vector2 Move { get; }
        bool IsJumpPressed { get; }
    }
}
