using System;
using System.Collections.Generic;
using TriboroughBridge_ChorusPlugin.Controller;
using TriboroughBridge_ChorusPlugin.View;

namespace TriboroughBridge_ChorusPlugin.Model
{
	/// <summary>
	/// Interface for some entire bridging system. Each such system needs to implement this interface, which will be used by the various controllers.
	/// </summary>
	public interface IBridgeModel : IDisposable
	{
		/// <summary>
		/// Get the complete path to the folder that contains the repository folder.
		/// </summary>
		string PathToRepository { get; }

		/// <summary>
		/// Get the project name.
		/// </summary>
		string ProjectName { get; }

		/// <summary>
		/// Get the type of repository the model supports
		/// </summary>
		BridgeModelType ModelType { get; }

		/// <summary>
		/// Do S/R on model.
		/// </summary>
		/// <returns>'true' if new stuff came in, otherwise 'false'.</returns>
		bool Syncronize();

		/// <summary>
		/// Get a clone of a repository.
		/// </summary>
		void ObtainRepository();

		/// <summary>
		/// Get the current controller for the given startup options.
		/// </summary>
		IBridgeController CurrentController { get; }

		/// <summary>
		/// Initialize the current instance.
		/// </summary>
		void InitializeModel(MainBridgeForm mainForm, Dictionary<string, string> options, ActionType actionType);
	}
}
