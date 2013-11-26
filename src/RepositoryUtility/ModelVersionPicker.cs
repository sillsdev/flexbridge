// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Windows.Forms;

namespace RepositoryUtility
{
	public partial class ModelVersionPicker : Form
	{
		public ModelVersionPicker()
		{
			InitializeComponent();
		}

		internal uint ModelVersion
		{
			get { return Convert.ToUInt32(_nudModelVersionPicker.Value); }
		}
	}
}
