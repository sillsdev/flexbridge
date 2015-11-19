// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace LibFLExBridgeChorusPlugin
{
	// Wrapper class that gives access to project splitter/unifier. Those classes are non-static
	// so that can be easier mocked for unit testing.
	public static class FLEx
	{
		static FLEx()
		{
			ProjectUnifier = new FLExProjectUnifier();
			ProjectSplitter = new FLExProjectSplitter();
		}

		public static IProjectSplitter ProjectSplitter;

		public static IProjectUnifier ProjectUnifier;

		internal static FLExProjectSplitter ProjectSplitterInternal
		{
			get { return ProjectSplitter as FLExProjectSplitter;}
		}

		internal static FLExProjectUnifier ProjectUnifierInternal
		{
			get { return ProjectUnifier as FLExProjectUnifier;}
		}
	}

}

