using System;
using System.Collections.Generic;
using System.Threading;

namespace GrandCru.Util
{
	
	public class Profiler {
		
		[ThreadStatic]
		private static Tracker trackerObj;
		
		public static void StartNewFrame()
		{
			if (trackerObj == null) {
				trackerObj = new Tracker();
			}
			trackerObj.StartNewFrame();
		}
		
		public static List<FlatProfileDataItem> GetData()
		{
			if (trackerObj != null) {
				return trackerObj.GetData();
			} else {
				return new List<FlatProfileDataItem>();
			}
		}
		
		public static IDisposable Track(string key)
		{
			if (trackerObj == null) StartNewFrame();
			return trackerObj.Track(key);
		}
		
		class Tracker : IDisposable {
			
			private readonly Stack<ProfilerDataNode> stack = new Stack<ProfilerDataNode>();
			
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
			
			public void Dispose()
			{
				var root = stack.Pop();
				root.Leave();
			}
		}
		
		public class FlatProfileDataItem
		{
			public string key;
			public int totalMs;
			public int callCount;
		}
		
		
		class ProfilerDataNode {
			
			public string key;
			public int totalMs;
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
			
			public void Enter()
			{
				callCount++;
				startms = (Environment.TickCount & Int32.MaxValue);
			}
			
			public void Leave()
			{
				totalMs += (Environment.TickCount & Int32.MaxValue) - startms;
			}
			
			public void Reset()
			{
				totalMs = 0;
				callCount = 0;
				startms = 0;
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

