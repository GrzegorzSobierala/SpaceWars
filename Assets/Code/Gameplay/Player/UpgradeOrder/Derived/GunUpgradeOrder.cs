using NaughtyAttributes;
using UnityEngine;

namespace Game.Player.Ship
{
    [CreateAssetMenu(fileName = "GunUpgradeOrder", menuName = "UpgradeOrder/GunUpgradeOrder")]
    public class GunUpgradeOrder : UpgradeOrderBase<GunUpgradeBase>
    {
        [Button]
        private void Test()
        {

        }
    }
}
