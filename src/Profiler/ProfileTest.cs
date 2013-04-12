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
			}

			var data = Profiler.GetData();

			Assert.AreEqual(2, data.Count);
			Assert.AreEqual("Something", data[0].key);
			Assert.AreEqual("Something/Other", data[1].key);

			Assert.AreEqual(1, data[0].callCount);
			Assert.AreEqual(1, data[1].callCount);

			Assert.AreEqual(1, data[0].callCount);

			Assert.GreaterOrEqual(200, data[0].callCount);
			Assert.GreaterOrEqual(100, data[1].callCount);
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

