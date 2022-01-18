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
                Mode = MatchmakingMode.Flexible,
                TeamSize = 2,
                InitialRange = 10,
                MaxAllowedRange = 10000,
                FlexibleIncreasePerGeneration = 5
            });

            mm.OnTeamsGenerated += results =>
            {
                if(results.Length == 0 || results.Length>20) return;
                MultiNoaLoggingManager.Logger.Information("----------------GENERATION-----------------");
                foreach (var result in results)
                {
                    var r = result.GetTeamA().Aggregate("", (current, d) => current + (d.GetClient().Username + " "))
                        + "vs " + result.GetTeamB().Aggregate("", (current, d) => current + (d.GetClient().Username + " "));

                    MultiNoaLoggingManager.Logger.Information(r);
                }
                MultiNoaLoggingManager.Logger.Information("-------------------------------------------");
            };

            var clients = new List<DummyClient>();
            var r = new Random();

            for (int i = 0; i < 100000; i++)
            {
                clients.Add(new DummyClient(r.Next(5000)));
            }

            foreach (var c in clients)
            {
                mm.AddClient(c, 0);
            }
        }
    }
}