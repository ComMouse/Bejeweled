namespace Bejeweled
{
    /// <summary>
    /// Game constants on levels.
    /// </summary>
    public static class LevelConstant
    {
        /// <summary>
        /// Width of the game map.
        /// </summary>
        public const int MAP_WIDTH = 8;

        /// <summary>
        /// Height of the game map.
        /// </summary>
        public const int MAP_HEIGHT = 8;

        /// <summary>
        /// Base score for a pair.
        /// </summary>
        public const int BASE_SCORE = 10;

        /// <summary>
        /// Rate to add to score multiplier for every additional gem (larger than 3) in a pair.
        /// </summary>
        public const float GEM_MULTIPLIER = .5f;

        /// <summary>
        /// Rate to add for each additional pair (larger than 1 pair) in a combo.
        /// </summary>
        public const float PAIR_MULTIPLIER = .5f;

        /// <summary>
        /// Score multiplier for a magic gem.
        /// </summary>
        public const float MAGIC_MULTIPLIER = 5f;

        /// <summary>
        /// Combo score multiplier for each combo.
        /// </summary>
        public const float COMBO_MULTIPLIER = 2f;

        /// <summary>
        /// Wait time after swap is done.
        /// </summary>
        public const float SWAP_WAIT_TIME = 0.4f;

        /// <summary>
        /// Wait time if the swap fails.
        /// </summary>
        public const float SWAP_FAIL_WAIT_TIME = 0.5f;

        /// <summary>
        /// Wait time after clear is done.
        /// </summary>
        public const float CLEAR_WAIT_TIME = 0.2f;

        /// <summary>
        /// Wait time after gem movement is done.
        /// </summary>
        public const float MOVE_WAIT_TIME = 0.1f;

        /// <summary>
        /// Valid time after a gem is selected.
        /// </summary>
        public const float GEM_SELCT_VALID_TIME = 1.5f;
    }
}