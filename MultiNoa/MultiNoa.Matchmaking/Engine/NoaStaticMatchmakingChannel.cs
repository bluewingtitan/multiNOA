using System;
using System.Collections.Generic;
using System.Linq;
using MultiNoa.GameSimulation;
using MultiNoa.Logging;
using MultiNoa.Matchmaking.Elo;

namespace MultiNoa.Matchmaking.Engine
{
    public class NoaStaticMatchmakingChannel: NoaMatchmakingChannel
    {
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
                var potentialGameMembers = new List<IMatchmakingClient>();
                var totalDif = 0;
                for (int x = 0; x < cCopy.Count - 1; x++)
                {
                    var differenceToNext = Math.Abs(cCopy[x].GetMmr(_channelId) - cCopy[x + 1].GetMmr(_channelId));

                    if (differenceToNext > Config.InitialRange)
                    {
                        cCopy.RemoveAt(x);
                        break;
                    }

                    totalDif += differenceToNext;

                    if (totalDif > Config.InitialRange)
                    {
                        foreach (var member in potentialGameMembers)
                            cCopy.Remove(member);
                        break;
                    }
                    
                    if(x == 0)
                        potentialGameMembers.Add(cCopy[x]);
                    potentialGameMembers.Add(cCopy[x+1]);

                    if (potentialGameMembers.Count < Config.TeamSize * 2) continue;
                    
                    foreach (var member in potentialGameMembers)
                    {
                        cCopy.Remove(member);
                    }
                    // Form the teams.
                    var teamA = new List<IMatchmakingClient>();
                    var teamB = new List<IMatchmakingClient>();

                    for (int i = 0; i < Config.TeamSize; i++)
                    {
                        var a = potentialGameMembers[i%2==0?0:1];
                        var b = potentialGameMembers[i%2==0?1:0];
                        potentialGameMembers.RemoveAt(0);
                        potentialGameMembers.RemoveAt(0);

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
            }
            
            return result.ToArray();
        }

        public NoaStaticMatchmakingChannel(int channelId, MatchmakingChannelConfig config, IDynamicThread thread) :
            base(channelId, config, thread)
        {
            MultiNoaLoggingManager.Logger.Information("You are using static mmr-based matchmaking. Make sure your playerbase is active enough to use this matchmaking style, as it will let players wait indefinitely for a match in their mmr-range!");
        }
    }
}