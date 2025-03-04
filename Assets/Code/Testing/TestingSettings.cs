using System;

namespace Game.Testing
{
    [Serializable]
    public class TestingSettings
    {
        public bool AutoLoadRoom = false;
        public string TimeScale = "";
        public string PlayerHp = "";
        public bool ShowEnemiesFov = false;
        public bool EnableEnemyShooting = false;
        public float EnemySpeedMulti = 1;
        public int RoomSceneIndex = 0;
        public bool ShowEnemyGuardPoints = false;
    }
}
