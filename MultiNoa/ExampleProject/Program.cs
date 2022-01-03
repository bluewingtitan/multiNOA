using System;
using System.Collections.Generic;
using System.Linq;
using ExampleProject.Packets;
using MultiNoa;
using MultiNoa.Logging;
using MultiNoa.Matchmaking;
using MultiNoa.Matchmaking.Engine;
using MultiNoa.Networking.PacketHandling;

namespace ExampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            // Let's test matchmaking.
            
            var mm = new NoaMatchmakingEngine();
            mm.DefineChannel(0, new MatchmakingChannelConfig
            {
                Mode = MatchmakingMode.Static,
                TeamSize = 4,
                InitialRange = 50,
                MaxAllowedRange = 10000,
                FlexibleIncreasePerGeneration = 10
            });

            mm.OnTeamsGenerated += results =>
            {
                foreach (var result in results)
                {
                    var r = result.GetTeamA().Aggregate("", (current, d) => current + (d.GetClient().GetUsername() + " "))
                        + "vs " + result.GetTeamB().Aggregate("", (current, d) => current + (d.GetClient().GetUsername() + " "));

                    MultiNoaLoggingManager.Logger.Information(r);
                }
            };

            var clients = new List<DummyClient>();
            var r = new Random();

            for (int i = 0; i < 100; i++)
            {
                clients.Add(new DummyClient(r.Next(1000)));
            }

            foreach (var c in clients)
            {
                mm.AddClient(c, 0);
            }
        }
    }
}