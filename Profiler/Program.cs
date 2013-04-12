using System;
using System.Threading;
using System.Collections.Generic;

namespace GrandCru.Util.Test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var run = true;
			var start = DateTime.Now;
			long iter = 0;

			Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) => {
				e.Cancel = true;
				run = false;
			};

			Console.WriteLine("Testing profiler press ctrl-c to stop");

			var threads = new List<Thread>();
			for (int count = 0; count < 4; count++) {
				var thread = new Thread(new ThreadStart(() => {
					while (run) {
						if (!TestProfiler()) {
							Console.WriteLine("ERROR");
							break;
						}
						Interlocked.Increment(ref iter);
					}
				}));
				thread.Start ();
				threads.Add(thread);
			}

			foreach (var th in threads) th.Join();

			var took = DateTime.Now-start;
			Console.WriteLine("Took {0}s to run {1} iterations", took.TotalSeconds, iter);
		}

		static bool TestProfiler()
		{
			Profiler.StartNewFrame();
			using (Profiler.Track("First")) {
				var rand = new Random();
				for (int count = 0; count < rand.Next(100, 200); ) {
					using (Profiler.Track("Second")) {
						count++;
					}
				}
			}
			var data = Profiler.GetData();
			return (data[0].key.Equals("First") && data[1].key.Equals("First/Second"));
		}
	}
}
