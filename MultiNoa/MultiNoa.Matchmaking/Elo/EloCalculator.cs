using System;

namespace MultiNoa.Matchmaking.Elo
{
    /// <summary>
    /// A simple Elo-Calculator
    /// </summary>
    public static class EloCalculator
    {
        private const int DefaultAdjustmentFactor = 24;
        
        /// <summary>
        /// Used to calculate the expected score of a player before a match.
        /// This number is also used to calculate the new elo after a match.
        /// </summary>
        /// <param name="playerElo">Elo Score of the player (single person or team avg)</param>
        /// <param name="enemyElo">Elo Score of the enemy (single person or team avg)</param>
        /// <returns>Expected score between 0 and 1</returns>
        public static double ExpectedPoints(int playerElo, int enemyElo)
        {
            return 1 / (1 + Math.Pow(10, ((double) playerElo - enemyElo) / 400));
        }

        /// <summary>
        /// Used to calculate the new elo score after a match.
        /// </summary>
        /// <param name="oldElo">Old Elo Score of the player</param>
        /// <param name="expectedScore">Expected Score (between 0 and 1)</param>
        /// <param name="actualScore">Actual Score (between 0 and 1)</param>
        /// <param name="adjustmentFactor">Higher means broader adjustments to the elo points. Negative to use default (24)</param>
        /// <returns>The new Elo Score of the player</returns>
        public static int GetNewElo(int oldElo, double expectedScore, double actualScore, int adjustmentFactor = -1)
        {
            if (adjustmentFactor < 0)
                adjustmentFactor = DefaultAdjustmentFactor;
            
            return (int) Math.Round(oldElo + DefaultAdjustmentFactor * (actualScore - expectedScore));
        }
    }
}