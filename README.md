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

Check out stuff like https://github.com/etsy/statsd/ for gathering timing information.

## Building

The repository contains a solution file Profiler.sln and a NAnt build script. The NAnt script will generate CSProfiler.dll to Profiler/bin folder.

## License

Copyright 2013 Roope Kangas / Grand Cru Games

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
   