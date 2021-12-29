using System;

namespace MultiNoa.Matchmaking.Elo
{
    public static class EloCalculator
    {
        public static int AdjustmentFactor = 16;
        
        public static double ExpectedPoints(int playerElo, int enemyElo)
        {
            return 1 / (1 + Math.Pow(10, ((double) playerElo - enemyElo) / 400));
        }

        public static int GetNewElo(int oldElo, double expectedScore, double actualScore)
        {
            return (int) Math.Round(oldElo + AdjustmentFactor * (actualScore - expectedScore));
        }
    }
}