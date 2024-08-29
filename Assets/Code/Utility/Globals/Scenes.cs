namespace Game.Utility.Globals
{
    public static class Scenes
    {
        #region SingeScenes
        public static readonly string MainMenu = "MainMenu";
        public static readonly string PlayerTesting = "PlayerScene";
        public static readonly string RoomTesting = "RoomTesting";
        public static readonly string GameInit = "GameInit";
        #endregion

        #region MultiScenes
        public static readonly string[] MainMenuMulti = { MainMenu };
        public static readonly string[] TestingMulti = { PlayerTesting, RoomTesting };
        public static readonly string[] GameInitMulti = { GameInit };
        #endregion
    }
}
