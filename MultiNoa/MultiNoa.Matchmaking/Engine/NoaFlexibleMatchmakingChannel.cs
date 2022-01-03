using System;
using System.Collections.Generic;
using System.Linq;
using MultiNoa.GameSimulation;
using MultiNoa.Logging;
using MultiNoa.Matchmaking.Elo;

namespace MultiNoa.Matchmaking.Engine
{
    public class NoaFlexibleMatchmakingChannel: NoaMatchmakingChannel
    {
        private readonly Dictionary<ulong, int> _waitGenerations = new Dictionary<ulong, int>();


        protected override void OnAddClient(ulong id)
        {
            _waitGenerations[id] = 0;
        }

        protected override void OnRemoveClient(ulong id)
        {
            _waitGenerations.Remove(id);
        }

        private int GetRange(IMatchmakingClient c1, IMatchmakingClient c2)
        {
            
            if (!_waitGenerations.ContainsKey(c1.GetId()))
                _waitGenerations[c1.GetId()] = 0;
            
            if (!_waitGenerations.ContainsKey(c2.GetId()))
                _waitGenerations[c2.GetId()] = 0;
            
            var r1 = Math.Min(
            Config.InitialRange + Config.FlexibleIncreasePerGeneration * _waitGenerations[c1.GetId()],
            Config.MaxAllowedRange);
            
            var r2 = Math.Min(
                Config.InitialRange + Config.FlexibleIncreasePerGeneration * _waitGenerations[c2.GetId()],
                Config.MaxAllowedRange);

            return Math.Max(r1, r2);
            
        }
        
        public override IMatchmakingResult[] DoAGeneration()
        {
            var result = new List<IMatchmakingResult>();
            
            // Copy clients to working list
            var cCopy = _clients.ToList();
            cCopy.Sort((c1, c2) => c1.GetMmr(_channelId) - c2.GetMmr(_channelId));

            var dueIterations = cCopy.Count;

            while (cCopy.Count >= Config.TeamSize*2 && dueIterations > 0)
            {
                dueIterations--;
                var potentialGameMembers = new List<IMatchmakingClient>();
                var totalDif = 0;
                for (int x = 0; x < cCopy.Count - 1; x++)
                {
                    var currentClient = cCopy[x];
                    var nextClient = cCopy[x + 1];
                    var differenceToNext = Math.Abs(currentClient.GetMmr(_channelId) - nextClient.GetMmr(_channelId));

                    var range = GetRange(currentClient, nextClient);

                    if (differenceToNext > range)
                    {
                        if (!_waitGenerations.ContainsKey(currentClient.GetId()))
                            _waitGenerations[currentClient.GetId()] = 0;
                        _waitGenerations[currentClient.GetId()]++;
                        cCopy.RemoveAt(x);
                        break;
                    }

                    totalDif += differenceToNext;

                    if (totalDif > range)
                    {
                        foreach (var member in potentialGameMembers)
                        {
                            if (!_waitGenerations.ContainsKey(member.GetId()))
                                _waitGenerations[member.GetId()] = 0;
                            
                            _waitGenerations[member.GetId()]++;
                            cCopy.Remove(member);
                        }
                        break;
                    }
                    
                    if(potentialGameMembers.Count <= 0)
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
                    var bAvg = teamB.Average(client => client.GetMmr(_channelId));

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

        public NoaFlexibleMatchmakingChannel(int channelId, MatchmakingChannelConfig config, IDynamicThread thread) : base(channelId, config, thread)
        {}
    }
}