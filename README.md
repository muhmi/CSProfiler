CSProfiler
==========

Simple _thread specific_ profiling information for C# using() statements:

	Profiler.StartNewFrame();
	using (Profiler.Track("GameLogic")) {
		Thread.Sleep(TimeSpan.FromMilliseconds(100));
		using (Profiler.Track("AI")) {
			Thread.Sleep(TimeSpan.FromMilliseconds(100));
		}
	}
	
	var total = Profiler.Duration();

Later in the same thread you can fetch timing information:
	
	var data = Profiler.GetData();
	foreach (var block in data) {
		Console.WriteLine(
			"{0} totalMs:{1} callCount:{2}", 
			block.key, 
			block.totalMs,
			block.callCount
		);
	}
	
Will print something like:

	GameLogic totalMs:200 callCount:1
	GameLogic/AI totalMs:100 callCount:1
	

Obviously this is not a replacement for a _real profiler_ but something that you can run in production servers without causing too much overhead.
