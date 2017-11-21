using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace FLEx_ChorusPlugin.Infrastructure.ActionHandlers
{
	[DataContract]
	public class SerializableLfAuthorInfo
	{
		[DataMember] public DateTime ModifiedDate { get; set; }
		[DataMember] public DateTime CreatedDate { get; set; }
	}
}
