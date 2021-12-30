namespace MultiNoa.Matchmaking
{
    public enum MatchmakingMode
    {
        /// <summary>
        /// This mode does care about MMR a bit, matching closer players more often, but prioritising fast matches over fair matches.
        /// Useful for unranked matches.
        /// </summary>
        Fast,
        
        /// <summary>
        /// This mode cares about MMR. It will never change the accepted range. If two people on different ends of the MMR-Scale search a match, they will NEVER be matched.
        /// Useful for very competitive matches that need to be balanced.
        /// </summary>
        Static,
        
        /// <summary>
        /// This mode only cares about MMR. It will slowly widen the accepted range. If two people on different ends of the MMR-Scale search a match, they will be matched after some time.
        /// Useful for very competitive environments with low player numbers.
        /// </summary>
        Flexible,
    }
}