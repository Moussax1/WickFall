using UnityEngine;

namespace WickFall.Player
{
    /// <summary>
    /// Minimal wax trail handler. Decal spawning omitted per YAGNI (not in base specs).
    /// </summary>
    public class PlayerWaxTrail : MonoBehaviour
    {
        [SerializeField] private Sprite _footstepSprite;
        [SerializeField] private Sprite _puddleSprite;
        // ponytail: dynamic decal pooling skipped, add if visual footsteps are requested
    }
}
