using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MultiNoa.GameSimulation;
using MultiNoa.Matchmaking.Elo;

namespace MultiNoa.Matchmaking.Engine
{
    public class NoaFastMatchmakingChannel: NoaMatchmakingChannel
    {
        public NoaFastMatchmakingChannel(int channelId, MatchmakingChannelConfig config, IDynamicThread thread) : base(channelId, config, thread){}

        public override IMatchmakingResult[] DoAGeneration()
        {
            var result = new List<IMatchmakingResult>();
            
            // Copy clients to working list
            var cCopy = _clients.ToList();
            cCopy.Sort((c1, c2) => c1.GetMmr(_channelId) - c2.GetMmr(_channelId));

            
            var dueIterations = 10000;
            
            while (cCopy.Count >= Config.TeamSize*2 && dueIterations > 0)
            {
                dueIterations--;
                
                // Form the teams.
                var teamA = new List<IMatchmakingClient>();
                var teamB = new List<IMatchmakingClient>();

                for (int i = 0; i < Config.TeamSize; i++)
                {
                    var a = cCopy[i%2==0?0:1];
                    var b = cCopy[i%2==0?1:0];
                    cCopy.RemoveAt(0);
                    cCopy.RemoveAt(0);

                    teamA.Add(a);
                    teamB.Add(b);
                }

                var aAvg = teamA.Average(client => client.GetMmr(_channelId));
                var bAvg = teamA.Average(client => client.GetMmr(_channelId));
                
                var aData = new List<IMatchmakingPlayerData>();
                var bData = new List<IMatchmakingPlayerData>();

                for (int i = 0; i < Config.TeamSize; i++)
                {
                    var a = teamA[i];
                    var aExpected =
                        EloCalculator.ExpectedPoints(
                            (int) Math.Round((aAvg + a.GetMmr(_channelId)) / 2), // 50% team average elo, 50% personal elo.
                            (int) Math.Round(bAvg));
                    
                    aData.Add(new NoaMatchmakingPlayerData(a, aExpected));
                    
                    
                    var b = teamB[i];
                    var bExpected =
                        EloCalculator.ExpectedPoints(
                            (int) Math.Round((bAvg + b.GetMmr(_channelId)) / 2), // 50% team average elo, 50% personal elo.
                            (int) Math.Round(aAvg));
                    
                    bData.Add(new NoaMatchmakingPlayerData(b, bExpected));
                }
                
                result.Add(new NoaMatchmakingResult(aData.ToArray(), bData.ToArray(), Config.Mode, _channelId));
            }

            return result.ToArray();
        }
    }
}