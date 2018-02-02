namespace BitcoinParser.Loader
{
    public static class BlockConstants
    {
        public static class Offsets
        {
            public const int VersionNumber = 0;
            public const int PreviousBlockHash = 0 + 4; 
            public const int MerkelRootHash = 0 + 4 + 32;
            public const int TimeStamp = 0 + 4 + 32 + 32;
            public const int Difficulty = 0 + 4 + 32 + 32 + 4;
            public const int Nounce = 0 + 4 + 32 + 32 + 4 + 4;
            public const int TxnCount = 0 + 4 + 32 + 32 + 4 + 4 + 4;
        }

        public static class Sizes
        {
            public const int VersionNumber = 4;
            public const int PreviousBlockHash = 32;
            public const int MerkelRootHash = 32;
            public const int TimeStamp = 4;
            public const int Difficult = 4;
            public const int Nounce = 4;
        }
    }
}
