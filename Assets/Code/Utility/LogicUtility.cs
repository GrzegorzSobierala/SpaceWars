namespace Game.Utility
{
    public static class LogicUtility
    {
        public static Option GetNewestOption(bool option1, bool option2, ref Option lastUsedOption)
        {
            if (option1 && option2)
            {
                if (lastUsedOption == Option.Option1)
                {
                    return Option.Option2;
                }
                if (lastUsedOption == Option.Option2)
                {
                    return Option.Option1;
                }
            }
            if (option1)
            {
                lastUsedOption = Option.Option1;
                return Option.Option1;
            }
            if (option2)
            {
                lastUsedOption = Option.Option2;
                return Option.Option2;
            }

            lastUsedOption = Option.Default;
            return Option.Default;
        }
    }
}
