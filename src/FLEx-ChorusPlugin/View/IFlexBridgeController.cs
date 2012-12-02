using FLEx_ChorusPlugin.Model;
using TriboroughBridge_ChorusPlugin.Controller;

namespace FLEx_ChorusPlugin.View
{
	internal interface IFlexBridgeController : IBridgeController
	{
		LanguageProject CurrentProject { get; }
	}
}