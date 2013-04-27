using System;
using System.Collections.Generic;
using System.Threading;

namespace GrandCru.Util
{
	
	public class Profiler {
		
		[ThreadStatic]
		private static Tracker trackerObj;

		/// <summary>
		/// Resets the profiler
		/// </summary>
		public static void StartNewFrame()
		{
			if (trackerObj == null) {
				trackerObj = new Tracker();
			}
			trackerObj.StartNewFrame();
		}

		/// <summary>
		/// Flattens profile data, mainly for debug output or dumping to database.
		/// </summary>
		/// <returns>List of </returns>
		public static List<FlatProfileDataItem> GetData()
		{
			if (trackerObj != null) {
				return trackerObj.GetData();
			} else {
				return new List<FlatProfileDataItem>();
			}
		}

		/// <summary>
		/// Track a block of code.
		/// </summary>
		/// <param name="key">Unique name for the code block</param>
		public static IDisposable Track(string key)
		{
			if (trackerObj == null) StartNewFrame();
			return trackerObj.Track(key);
		}

		/// <summary>
		/// Total milliseconds spent inside this code block
		/// </summary>
		public static long Duration()
		{
			if (trackerObj != null) {
				return trackerObj.CurrentDuration();
			}
			return 0;
		}

		/// <summary>
		/// Duration of last tracked block.
		/// </summary>
		/// <returns>The duration.</returns>
		public static long LastDuration()
		{
			if (trackerObj != null) {
				return trackerObj.LastDuration();
			}
			return 0;
		}
		
		class Tracker : IDisposable {
			
			private readonly Stack<ProfilerDataNode> stack = new Stack<ProfilerDataNode>();

			private long lastDuration = 0;
			
			public Tracker()
			{
				stack.Push(new ProfilerDataNode("root"));
			}
			
			public void StartNewFrame()
			{
				var root = stack.Peek();
				root.Reset();
			}
			
			public List<FlatProfileDataItem> GetData()
			{
				var root = stack.Peek();
				return root.Flatten();
			}
			
			public IDisposable Track(string key)
			{
				var root = stack.Peek();
				var child = root.GetOrCreateChild(key);
				child.Enter();
				stack.Push(child);
				return this;
			}

			public long CurrentDuration()
			{
				if (stack.Count > 0) {
					var current = stack.Peek();
					return current.Duration();
				}
				return 0;
			}

			public long LastDuration()
			{
				return lastDuration;
			}
			
			public void Dispose()
			{
				var root = stack.Pop();
				root.Leave();
				lastDuration = root.Duration();
			}
		}
		
		public class FlatProfileDataItem
		{
			public string key;
			public int totalMs;
			public int maxMs;
			public int callCount;
		}
		
		
		class ProfilerDataNode {
			
			public string key;
			public int totalMs;
			public int maxMs;
			public int callCount;
			
			private List<ProfilerDataNode> children;
			private int startms;
			
			public ProfilerDataNode(string key)
			{
				this.key = key;
			}
			
			public ProfilerDataNode GetOrCreateChild(string key)
			{
				if (children == null) children = new List<ProfilerDataNode>();
				
				var child = FindChild(key);
				
				if (child != null) return child;
				
				child = new ProfilerDataNode(key);
				children.Add(child);
				
				return child;
			}
			
			public ProfilerDataNode FindChild(string key)
			{
				if (children != null) {
					for (int idx = 0; idx < children.Count; idx++) {
						if (children[idx].key.Equals(key)) {
							return children[idx];
						}
					}
				}
				return null;
			}

			public long Duration()
			{
				if (totalMs == 0) {
					return (Environment.TickCount & Int32.MaxValue) - startms;
				}
				return totalMs;
			}
			
			public void Enter()
			{
				callCount++;
				startms = (Environment.TickCount & Int32.MaxValue);
			}
			
			public void Leave()
			{
				var duration = (Environment.TickCount & Int32.MaxValue) - startms;
				totalMs += duration;
				if (duration > maxMs) maxMs = duration;
			}
			
			public void Reset()
			{
				totalMs = 0;
				callCount = 0;
				startms = 0;
				maxMs = 0;
				if (children != null) foreach (var child in children) child.Reset();
			}
			
			public List<FlatProfileDataItem> Flatten(string _path = "", List<FlatProfileDataItem> _list = null)
			{
				
				var list = _list ?? new List<FlatProfileDataItem>(8);
				var myPath = string.IsNullOrEmpty(_path) ? (callCount > 0 ? key : "") : string.Format("{0}/{1}", _path, key);
				
				
				if (callCount > 0) {
					var myitem = new FlatProfileDataItem();
					
					myitem.key = myPath;
					myitem.callCount = callCount;
					myitem.totalMs = totalMs;
					myitem.maxMs = maxMs;
					list.Add(myitem);
				}
				
				if (children != null) {
					foreach (var child in children) {
						if (child.callCount > 0) {
							list = child.Flatten(myPath, list);
						}
					}
				}
				
				return list;
			}
			
		}
		
	}

}

