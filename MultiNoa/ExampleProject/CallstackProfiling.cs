using System.Threading;
using MultiNoa.Profiling;

namespace ExampleProject
{
    public static class CallstackProfiling
    {
        private static CallstackProfiler _profiler = new CallstackProfiler();


        public static void StartCallstackProfiler()
        {
            _profiler = new CallstackProfiler();
            
            _profiler.Start("Profiling");

            Internal1();
            
            _profiler.Stop();
            
            _profiler.StopAllAndPrint();
        }


        private static void Internal1()
        {
            _profiler.Start("Profiling.Function1");
            
            Thread.Sleep(200);

            Internal2();
            Internal2();
            Internal2();
            Internal2();
            Internal2();

            _profiler.Stop();
        }



        private static void Internal2()
        {
            _profiler.Start("Profiling.Function2");
            
            Thread.Sleep(100);

            Internal3();
            Internal3();

            _profiler.Stop();
        }


        private static void Internal3()
        {
            _profiler.Start("Profiling.Function3");
            
            Thread.Sleep(50);
            
            
            _profiler.Stop();
        }
        
        
    }
}