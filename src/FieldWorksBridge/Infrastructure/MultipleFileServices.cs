using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// Service that will manage the multiple files and original fwdata file for a full FW data set.
	/// </summary>
	/// <remarks>
	/// One task of the service is twofold:
	/// 1. Break up the main fwdata file into multiple files
	///		A. one for the custom property declarations, and
	///		B. one for each concrete CmObject class instances
	/// 2. Put the multiple files back together into the main fwdata file, if a Send/Receive had new information brought back into the repos.
	/// </remarks>
	internal class MultipleFileServices
	{
	}
}
