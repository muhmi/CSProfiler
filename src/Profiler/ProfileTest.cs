using System;
using NUnit.Framework;
using System.Threading;
using System.Collections.Generic;

namespace GrandCru.Util.Tests
{
	[TestFixture()]
	public class ProfileTest
	{
		[Test()]
		public void NewFrame ()
		{
			Profiler.StartNewFrame();
			using (Profiler.Track("Something")) {
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
			}
			Assert.IsNotEmpty(Profiler.GetData());
			Profiler.StartNewFrame();
			Assert.IsEmpty(Profiler.GetData());
		}

		[Test()]
		public void EmbeddedTraces ()
		{
			Profiler.StartNewFrame();

			using (Profiler.Track("Something")) {
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
				using (Profiler.Track("Other")) {
					Thread.Sleep(TimeSpan.FromMilliseconds(100));
				}
				Assert.GreaterOrEqual(Profiler.Duration(), 200);
			}

			var data = Profiler.GetData();

			Assert.AreEqual(2, data.Count);
			Assert.AreEqual("Something", data[0].key);
			Assert.AreEqual("Something/Other", data[1].key);

			Assert.AreEqual(1, data[0].callCount);
			Assert.AreEqual(1, data[1].callCount);

			Assert.AreEqual(1, data[0].callCount);

			Assert.GreaterOrEqual(data[0].totalMs, 200);
			Assert.GreaterOrEqual(data[1].totalMs, 100);
		}

		[Test()]
		public void Threaded ()
		{
			var threads = new List<Thread>();

			for (int i = 0; i < 100; i++) {
				Profiler.StartNewFrame();
				var th = new Thread(new ThreadStart(EmbeddedTraces));
				th.Start();
				Profiler.StartNewFrame();
				threads.Add (th);
				Profiler.StartNewFrame();
			}

			foreach(var thread in threads) {
				thread.Join();
			}
		}
	}
}

