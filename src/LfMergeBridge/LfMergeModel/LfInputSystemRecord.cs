using MongoDB.Bson.Serialization.Attributes;

namespace LfMergeBridge.LfMergeModel
{
	[BsonIgnoreExtraElements]
	public class LfInputSystemRecord
	{
		public string Abbreviation { get; set; }
		public string Tag { get; set; }
		public string LanguageName { get; set; }
		public bool IsRightToLeft { get; set; }

		// We'll store vernacular / analysis writing system info when
		// importing LCM projects, but LF won't be using this information
		public bool VernacularWS { get; set; }
		public bool AnalysisWS { get; set; }
	}
}
