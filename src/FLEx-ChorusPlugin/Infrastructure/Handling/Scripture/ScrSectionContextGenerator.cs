// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Xml;
using FLEx_ChorusPlugin.Properties;
using System;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Scripture
{
	/// <summary>
	/// Context generator for ScrBook elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	internal sealed class ScrSectionContextGenerator : FieldWorkObjectContextGenerator
	{
		private int _begRef;
		private int _endRef;
		private string _bookName;

		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForScrSection(start);
		}

		string EntryLabel
		{
			get { return Resources.kScrSectionClassLabel; }
		}

		private string GetLabelForScrSection(XmlNode section)
		{
			var owningBook = section.SelectSingleNode("ancestor-or-self::ScrBook");
			if (owningBook != null)
				_bookName = GetNameOrAbbreviation(owningBook);
			if (string.IsNullOrEmpty(_bookName))
				return EntryLabel;
			GetBeginAndEndVerseRefs(section);
			return EntryLabel + Space + Quote + GetRefFromBookAndIntegers() + Quote;
		}

		private void GetBeginAndEndVerseRefs(XmlNode section)
		{
			const string startName = "VerseRefStart";
			const string endName = "VerseRefEnd";
			var startRef = section.SelectSingleNode(startName);
			if (startRef != null)
				_begRef = Convert.ToInt32(startRef.Attributes[SharedConstants.Val].Value);
			var endRef = section.SelectSingleNode(endName);
			if (endRef != null)
				_endRef = Convert.ToInt32(endRef.Attributes[SharedConstants.Val].Value);
		}

		private string GetRefFromBookAndIntegers()
		{
			if (_begRef < 1000 && _endRef < 1000)
				return _bookName; // No chapter information available?!
			var begChapter = GetChapterFromBcv(_begRef);
			var endChapter = GetChapterFromBcv(_endRef);
			var begVerse = GetVerseFromBcv(_begRef);
			var endVerse = GetVerseFromBcv(_endRef);
			string scrRef;
			if (begChapter == endChapter)
			{
				scrRef = String.Format(Resources.kScrSectionOneChapter, _bookName, begChapter,
					begVerse, endVerse);
				return scrRef;
			}
			scrRef = String.Format(Resources.kScrSectionMultiChapter, _bookName,
				begChapter, begVerse, endChapter, endVerse);
			return scrRef;
		}

		#region Calculate ChapterVerse

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns the verse part of the specified bcv
		/// </summary>
		/// <param name="bcv">The bcv to parse</param>
		/// <returns>The verse part of the specified bcv</returns>
		/// ------------------------------------------------------------------------------------
		private static string GetVerseFromBcv(int bcv)
		{
			return Convert.ToString(bcv%1000);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns the chapter part of the specified bcv
		/// </summary>
		/// <param name="bcv">The bcv to parse</param>
		/// <returns>The chapter part of the specified bcv</returns>
		/// ------------------------------------------------------------------------------------
		private static string GetChapterFromBcv(int bcv)
		{
			return Convert.ToString((bcv/1000)%1000);
		}

		#endregion

	}
}
