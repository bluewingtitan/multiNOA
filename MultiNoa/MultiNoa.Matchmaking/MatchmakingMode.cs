namespace MultiNoa.Matchmaking
{
    public enum MatchmakingMode
    {
        /// <summary>
        /// This mode does not care about MMR. It only cares about matching as fast as possible.
        /// Awesome for fully unranked matches.
        /// </summary>
        Fast,
        
        /// <summary>
        /// This mode cares about MMR. It will never change the accepted range. If two people on different ends of the MMR-Scale search a match, they will NEVER be matched.
        /// </summary>
        Static,
        
        /// <summary>
        /// This mode only cares about MMR. It will slowly widen the accepted range. If two people on different ends of the MMR-Scale search a match, they will be matched after some time.
        /// </summary>
        Flexible,
    }
}