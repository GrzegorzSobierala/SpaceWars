namespace Game.Utility.Globals
{
    public static class Scenes
    {
        #region SingeScenes
        public static readonly string MainMenu = "MainMenuScene";
        public static readonly string Player = "PlayerScene";
        public static readonly string CargoTestRoom = "CargoTestRoomScene";
        public static readonly string GameInit = "GameInitScene";
        #endregion

        #region MultiScenes
        public static readonly string[] MainMenuMulti = { MainMenu };
        public static readonly string[] TestingMulti = { Player, CargoTestRoom };
        public static readonly string[] GameInitMulti = { GameInit };
        #endregion
    }
}
