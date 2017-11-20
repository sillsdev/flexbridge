using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace FLEx_ChorusPlugin.Infrastructure.ActionHandlers
{
	[DataContract]
	public class SerializableLfCommentRegarding
	{
		[DataMember] public string TargetGuid { get; set; }
		[DataMember] public string Field { get; set; }
		[DataMember] public string FieldNameForDisplay { get; set; }
		[DataMember] public string FieldValue { get; set; }
		[DataMember] public string InputSystem { get; set; }
		[DataMember] public string InputSystemAbbreviation { get; set; }
		[DataMember] public string Word { get; set; }
		[DataMember] public string Meaning { get; set; }
	}
}