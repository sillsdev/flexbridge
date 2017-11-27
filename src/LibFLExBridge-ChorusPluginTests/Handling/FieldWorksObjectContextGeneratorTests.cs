// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.Handling;
using LibFLExBridgeChorusPlugin.Handling.Anthropology;
using LibFLExBridgeChorusPlugin.Handling.Common;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.Discourse;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.Lexicon;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.Phonology;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.Reversal;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.TextCorpus;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.WordformInventory;
using LibFLExBridgeChorusPlugin.Handling.Scripture;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling
{
	[TestFixture]
	public class FieldWorksObjectContextGeneratorTests
	{
		private static IGenerateContextDescriptorFromNode MakeGenerator()
		{
			var result = new FieldWorkObjectContextGenerator();
			var strategies = new MergeStrategies();
			result.MergeStrategies = strategies;
			strategies.SetStrategy("LexEntry", MakeClassStrategy(new LexEntryContextGenerator(), strategies));
			strategies.SetStrategy("ReversalIndexEntry", MakeClassStrategy(new ReversalEntryContextGenerator(), strategies));
			strategies.SetStrategy("WfiWordform", MakeClassStrategy(new WfiWordformContextGenerator(), strategies));
			strategies.SetStrategy("CmPossibilityList", MakeClassStrategy(new PossibilityListContextGenerator(), strategies));
			strategies.SetStrategy("CmPossibility", MakeClassStrategy(new PossibilityContextGenerator(), strategies));
			strategies.SetStrategy("LexEntryType", MakeClassStrategy(new PossibilityContextGenerator(), strategies));
			strategies.SetStrategy("PhEnvironment", MakeClassStrategy(new EnvironmentContextGenerator(), strategies));
			strategies.SetStrategy("DsChart", MakeClassStrategy(new DiscourseChartContextGenerator(), strategies));
			strategies.SetStrategy("DsConstChart", MakeClassStrategy(new DiscourseChartContextGenerator(), strategies));
			strategies.SetStrategy("ConstChartRow", MakeClassStrategy(new DiscourseChartContextGenerator(), strategies));
			strategies.SetStrategy("ConstChartWordGroup", MakeClassStrategy(new DiscourseChartContextGenerator(), strategies));
			strategies.SetStrategy("PhNCSegments", MakeClassStrategy(new MultiLingualStringsContextGenerator("Natural Class", "Name", "Abbreviation"), strategies));
			strategies.SetStrategy("FsClosedFeature", MakeClassStrategy(new MultiLingualStringsContextGenerator("Phonological Features", "Name", "Abbreviation"), strategies));
			strategies.SetStrategy("Text", MakeClassStrategy(new TextContextGenerator(), strategies));
			strategies.SetStrategy("RnGenericRec", MakeClassStrategy(new RnGenericRecContextGenerator(), strategies));
			strategies.SetStrategy("ScrBook", MakeClassStrategy(new ScrBookContextGenerator(), strategies));
			strategies.SetStrategy("ScrSection", MakeClassStrategy(new ScrSectionContextGenerator(), strategies));
			return result;
		}

		private static readonly FindByKeyAttribute GuidKey = new FindByKeyAttribute(FlexBridgeConstants.GuidStr);

		private static ElementStrategy MakeClassStrategy(FieldWorkObjectContextGenerator descriptor, MergeStrategies strategies)
		{
			var classStrat = new ElementStrategy(false)
			{
				MergePartnerFinder = GuidKey,
				ContextDescriptorGenerator = descriptor,
				IsAtomic = false
			};
			descriptor.MergeStrategies = strategies;
			return classStrat;
		}

		[Test]
		public void LexEntryPartsFindLexemeForm()
		{
			const string source =
				@"<LexEntry guid='01efa516-1749-4b60-b43d-00089269e7c5'>
					<HomographNumber val='0' />
					<LexemeForm>
						<MoStemAllomorph guid='8e982d88-0111-43b9-a25c-420bb5c84cf0'>
							<Form>
								<AUni ws='en'>abcdefghijk</AUni>
							</Form>
							<IsAbstract val='False' />
							<MorphType>
								<objsur guid='d7f713e4-e8cf-11d3-9764-00c04f186933' t='r' />
							</MorphType>
						</MoStemAllomorph>
					</LexemeForm>
				</LexEntry>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[0]; // MoStemAllomorph
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry \"abcdefghijk\" LexemeForm"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "8e982d88-0111-43b9-a25c-420bb5c84cf0"));

			// Try a node that is not part of the LexemeForm.
			input = root.ChildNodes[0];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry \"abcdefghijk\" HomographNumber"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "01efa516-1749-4b60-b43d-00089269e7c5"));

			// Try a bit deeper
			input = root.ChildNodes[1].ChildNodes[0].ChildNodes[0]; // the <Form>
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry \"abcdefghijk\" LexemeForm Form"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "8e982d88-0111-43b9-a25c-420bb5c84cf0"));

			// Don't want the AUni level.
			input = input.ChildNodes[0]; // the <AUni>
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry \"abcdefghijk\" LexemeForm Form"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "8e982d88-0111-43b9-a25c-420bb5c84cf0"));
		}

		[Test]
		public void ReversalSubEntryFindName()
		{
			const string source =
				@"<ReversalIndexEntry guid='cdfe2b07-765b-4ebf-b453-ba5f93387773'>
					<PartOfSpeech>
						<objsur guid='a8e41fd3-e343-4c7c-aa05-01ea3dd5cfb5' t='r' />
					</PartOfSpeech>
					<ReversalForm>
						<AUni ws='en'>cat</AUni>
					</ReversalForm>
					<Subentries>
						<ReversalIndexEntry guid='0373eec0-940d-4794-9cfc-8ef351e5699f'>
							<PartOfSpeech>
								<objsur guid='a8e41fd3-e343-4c7c-aa05-01ea3dd5cfb5' t='r' />
							</PartOfSpeech>
							<ReversalForm>
								<AUni ws='en'>kitty</AUni>
							</ReversalForm>
						</ReversalIndexEntry>
					</Subentries>
				</ReversalIndexEntry>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[2].ChildNodes[0].ChildNodes[1]; // Subentry ReversalForm
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Reversal Entry \"kitty\" ReversalForm"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "0373eec0-940d-4794-9cfc-8ef351e5699f"));

			// Try a node that is not part of the Subentry.
			input = root.ChildNodes[0];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Reversal Entry \"cat\" PartOfSpeech"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "cdfe2b07-765b-4ebf-b453-ba5f93387773"));

			// Don't want the AUni level.
			input = root.ChildNodes[1].ChildNodes[0]; // the <AUni> in main ReversalForm
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Reversal Entry \"cat\" ReversalForm"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "cdfe2b07-765b-4ebf-b453-ba5f93387773"));

			// Don't want the Subentry AUni level either.
			input = root.ChildNodes[2].ChildNodes[0].ChildNodes[1].ChildNodes[0]; // the <AUni> of the subentry
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Reversal Entry \"kitty\" ReversalForm"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "0373eec0-940d-4794-9cfc-8ef351e5699f"));
		}

		[Test]
		public void WfiWordformPartsFindForm()
		{
			const string source =
				@"<WfiWordform guid='2a3ccd4f-a2cd-43e5-bd4d-76a84ce00653'>
					<Form>
						<AUni ws='jit'>jitWord</AUni>
					</Form>
					<SpellingStatus val='0' />
				</WfiWordform>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root; // WfiWordform
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Wordform \"jitWord\""));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "2a3ccd4f-a2cd-43e5-bd4d-76a84ce00653"));

			// Try a child node that isn't a part of the word form
			input = root.ChildNodes[1]; //SpellingStatus
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Wordform \"jitWord\" SpellingStatus"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "2a3ccd4f-a2cd-43e5-bd4d-76a84ce00653"));
		}

		[Test]
		public void TextFindName()
		{
			const string source = @"<Text
					guid='e43b93a7-604e-4704-8118-d48999b330e3'>
					<Contents>
						<StText	guid='002c0cdf-e486-460f-b334-505ad66c5b43'>
							<DateModified val='2011-2-3 19:24:58.556' />
							<Paragraphs>
								<ownseq>
									<StTxtPara guid='988597b0-a6fd-4956-b977-92b0992ae123' />
										<Contents>
											<Str>
												<Run ws='en'>Some random paragraph text.</Run>
											</Str>
										</Contents>
										<ParseIsCurrent val='False' />
									<StTxtPara />
								</ownseq>
							</Paragraphs>
						</StText>
					</Contents>
					<IsTranslated val='False' />
					<Name>
						<AUni ws='en'>myEngName</AUni>
						<AUni ws='fr'>monNom</AUni>
					</Name>
				</Text>";
			const string predictedLabel = "Text \"myEngName\", \"monNom\"";
			const string textGuid = "guid=e43b93a7-604e-4704-8118-d48999b330e3";
			const string stTextGuid = "guid=002c0cdf-e486-460f-b334-505ad66c5b43";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root; // Text (CmMajorObject)
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(textGuid));

			// Try a child node that isn't a part of the Text's name
			input = root.ChildNodes[1]; // IsTranslated
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " IsTranslated"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(textGuid));

			// Try a child node that owns the StText
			input = root.ChildNodes[0]; // Contents
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " Contents"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(textGuid));

			// Try deeper down in the StText
			input = input.ChildNodes[0].ChildNodes[0]; // Date Modified
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " Contents DateModified"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(stTextGuid)); // I hope this is right!

			// Try the Name child node
			input = root.ChildNodes[2]; // Name
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " Name"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(textGuid));

			// Try deeper down
			input = root.ChildNodes[2].ChildNodes[0]; // don't want to display AUni
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " Name"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(textGuid));
		}

		[Test]
		public void DataNotebookGenRecFindName()
		{
			const string source =
				@"<RnGenericRec guid='175a2230-0302-4307-8bf4-f3dad9c19710'>
					<Conclusions>
						<StText>
							guid='c5df83ed-1037-438a-a23a-d095cc4bd9c9'>
							<DateModified val='2011-2-3 19:24:58.556' />
							<Paragraphs>
								<ownseq>
									<StTxtPara guid='ef6c8862-5895-4068-a2ab-f9d42022cf82' />
										<Contents>
											<Str>
												<Run ws='en'>Some random conclusion.</Run>
											</Str>
										</Contents>
										<ParseIsCurrent val='False' />
									<StTxtPara />
								</ownseq>
							</Paragraphs>
						</StText>
					</Conclusions>
					<DateCreated val='2007-5-25 18:44:50.767' />
					<DateModified val='2007-5-25 18:46:0.0' />
					<Discussion>
					</Discussion>
					<ExternalMaterials>
					</ExternalMaterials>
					<FurtherQuestions>
					</FurtherQuestions>
					<Hypothesis>
					</Hypothesis>
					<Researchers>
					</Researchers>
					<ResearchPlan>
					</ResearchPlan>
					<Title>
						<Str>
							<Run ws='en'>Some name</Run>
						</Str>
					</Title>
					<Type />
				</RnGenericRec>";
			const string predictedLabel = "Data Notebook Record \"Some name\"";
			const string recordGuid = "guid=175a2230-0302-4307-8bf4-f3dad9c19710";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root; // RnGenericRec
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(recordGuid));

			// Try a child node that isn't a part of the record's Title
			input = root.ChildNodes[0]; // Conclusions
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " Conclusions"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(recordGuid));

			// Try the Title node
			input = root.ChildNodes[9]; // Title
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " Title"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(recordGuid));

			// Try a bit deeper
			input = input.ChildNodes[0]; // Don't want Str node to show in label
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " Title"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(recordGuid));
		}

		private static string GetScrBookXml()
		{
			const string xmlString =
				@"<ScrBook guid='0e876238-341a-4e56-9db5-ed73b05cb8f5'>
					<Abbrev>
						<AUni ws='en'>Luk</AUni>
						<AUni ws='es'>Lc</AUni>
					</Abbrev>
					<BookId>
						<objsur guid='4fbe9226-30ab-44a1-9643-a7072d11f9ff' t='r' />
					</BookId>
					<CanonicalNum val='42' />
					<Footnotes>
						<ownseq>
							<ScrFootnote guid='002c0cdf-e486-460f-b334-505ad66c5b43'>
								<DateModified val='2011-2-3 19:24:58.556' />
								<Paragraphs>
									<ownseq>
										<StTxtPara guid='988597b0-a6fd-4956-b977-92b0992ae123' />
											<Contents>
												<Str>
													<Run ws='en'>Some random footnote.</Run>
												</Str>
											</Contents>
											<ParseIsCurrent val='False' />
										<StTxtPara />
									</ownseq>
								</Paragraphs>
								<FootnoteMarker>
									<Str>
										<Run namedStyle='Note Marker' ws='en'>a</Run>
									</Str>
								</FootnoteMarker>
							</ScrFootnote>
							<ScrFootnote />
							<ScrFootnote />
							<ScrFootnote />
						</ownseq>
					</Footnotes>
					<IdText>
						<Uni>Commence avec l'histoire de Noel</Uni>
					</IdText>
					<Name>
						<AUni ws='en'>Luke</AUni>
						<AUni ws='es'>Lucas</AUni>
					</Name>
					<Sections>
						<ownseq>
							<ScrSection guid='3713db10-ba05-4a42-9685-9fe4dbc2693d'>
								<Content>
									<StText guid='9e0d0f62-1c3a-4dd5-a488-fdf93471137a'>
										<DateModified val='2011-2-3 19:24:58.556' />
										<Paragraphs>
											<ownseq>
												<StTxtPara guid='43a529b9-6fed-430b-b571-26df25dff03c' />
													<Contents>
														<Str>
															<Run ws='en'>Some random scripture.</Run>
														</Str>
													</Contents>
													<ParseIsCurrent val='False' />
												<StTxtPara />
											</ownseq>
										</Paragraphs>
										<RightToLeft val='False' />
										<Tags />
									</StText>
								</Content>
								<Heading>
								</Heading>
								<VerseRefEnd val='41003020' />
								<VerseRefMax val='41003020' />
								<VerseRefMin val='41003001' />
								<VerseRefStart val='41003001' />
							</ScrSection>
							<ScrSection guid='3770c19f-ae61-4364-be08-5e4bf62d861a'>
								<Content>
								</Content>
								<Heading>
									<StText guid='e0eec438-8a60-4586-a73e-6dfd5089becc'>
										<DateModified val='2011-2-3 19:24:58.556' />
										<Paragraphs>
											<ownseq>
												<StTxtPara guid='c83379ed-1043-4bd4-b9c1-b159c47025cf' />
													<Contents>
														<Str>
															<Run ws='en'>Some random scripture heading.</Run>
														</Str>
													</Contents>
													<ParseIsCurrent val='False' />
												<StTxtPara />
											</ownseq>
										</Paragraphs>
										<RightToLeft val='False' />
										<Tags />
									</StText>
								</Heading>
								<VerseRefEnd val='41004005' />
								<VerseRefMax val='41004005' />
								<VerseRefMin val='41003022' />
								<VerseRefStart val='41003022' />
							</ScrSection>
							<ScrSection />
							<ScrSection />
						</ownseq>
					</Sections>
					<Title />
				</ScrBook>";
			return xmlString;
		}

		[Test]
		public void ScrBookFindName()
		{
			string source = GetScrBookXml();
			const string predictedLabel = "Scripture Book \"Luke\"";
			const string bookGuid = "guid=0e876238-341a-4e56-9db5-ed73b05cb8f5";
			const string footnoteGuid = "guid=002c0cdf-e486-460f-b334-505ad66c5b43";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root; // ScrBook
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(bookGuid));

			// Try a child node that isn't a part of the ScrBook's name
			input = root.ChildNodes[2]; // CanonicalNum
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " CanonicalNum"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(bookGuid));

			// Try a child node that owns the StText
			input = root.ChildNodes[7]; // Title
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " Title"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(bookGuid));

			// Try the Name child node
			input = root.ChildNodes[0]; // Abbreviation
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " Abbrev"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(bookGuid));

			// Try deeper down
			input = root.ChildNodes[0].ChildNodes[0]; // don't want to display AUni
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " Abbrev"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(bookGuid));

			// Try a footnote
			input = root.ChildNodes[3].ChildNodes[0].ChildNodes[0]; // 1st footnote?
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + " Footnotes 1"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(footnoteGuid));
		}

		[Test]
		public void ScrSectionFindName()
		{
			string source = GetScrBookXml();
			const string predictedLabel = "Scripture Section \"Luke ";
			const string sectionOneGuid = "guid=3713db10-ba05-4a42-9685-9fe4dbc2693d";
			const string sectionTwoGuid = "guid=3770c19f-ae61-4364-be08-5e4bf62d861a";
			const string sectionOneStTextGuid = "guid=9e0d0f62-1c3a-4dd5-a488-fdf93471137a";
			var root = FieldWorksTestServices.GetNode(source).ChildNodes[6].ChildNodes[0]; // <ownseq> of ScrSections
			var input = root.ChildNodes[0]; // first ScrSection
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + "3:1-20\""));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(sectionOneGuid));

			// Try a child node inside the ScrSection's Contents StText
			input = input.ChildNodes[0].ChildNodes[0].ChildNodes[2]; // Content's RightToLeft
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + "3:1-20\" Content RightToLeft"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(sectionOneStTextGuid));

			// Try the second ScrSection
			input = root.ChildNodes[1]; // 2nd one
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + "3:22-4:5\""));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(sectionTwoGuid));

			// Try the Heading node in the second section
			input = input.ChildNodes[1]; // Heading of 2nd section
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo(predictedLabel + "3:22-4:5\" Heading"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring(sectionTwoGuid));
		}

		[Test]
		public void PossibilityListPartsFindName()
		{
			const string source = @"	<CmPossibilityList
						guid='1ee09905-63dd-4c7a-a9bd-1d496743ccd6'>
						<Abbreviation>
							<AUni
								ws='en'>Junk</AUni>
						</Abbreviation>
						<Depth
							val='127' />
						<ItemClsid
							val='5118' />
						<Name>
							<AUni
								ws='en'>Complex Form Types</AUni>
						</Name>
						<Possibilities>
							<ownseq
								class='LexEntryType'
								guid='1f6ae209-141a-40db-983c-bee93af0ca3c'>
								<Abbreviation>
									<AUni
										ws='en'>comp. of</AUni>
								</Abbreviation>
								<IsProtected
									val='true' />
								<Name>
									<AUni
										ws='en'>Compound</AUni>
								</Name>
								<ReverseAbbr>
									<AUni
										ws='en'>comp.</AUni>
								</ReverseAbbr>
							</ownseq>
						</Possibilities>
						<PreventDuplicates
							val='true' />
						<WsSelector
							val='-3' />
					</CmPossibilityList>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[0].ChildNodes[0]; //<Abbreviation><AUni>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("List \"Complex Form Types\" Abbreviation"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1ee09905-63dd-4c7a-a9bd-1d496743ccd6"));
		}

		[Test]
		public void NaturalClassesPartsFindName()
		{
			const string source = @"	<ownseq
					class='PhNCSegments'
					guid='085e32ec-eb5b-4eed-9dab-e55854ce88fb'>
					<Abbreviation>
						<AUni
							ws='en'>V+back</AUni>
						<AUni
							ws='pt'>V+back</AUni>
					</Abbreviation>
					<Name>
						<AUni
							ws='en'>back vowels</AUni>
					</Name>
					<Segments>
						<objsur
							guid='14bd1795-d041-4c59-91ae-1d5506c63402'
							t='r' />
						<objsur
							guid='70286ab3-04b7-43ab-afb3-76e978e24142'
							t='r' />
					</Segments>
				</ownseq>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[0].ChildNodes[0]; //<Abbreviation><AUni>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("Natural Class \"back vowels\" Abbreviation"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "085e32ec-eb5b-4eed-9dab-e55854ce88fb"));

			input = root.ChildNodes[1].ChildNodes[0]; //<Name><AUni>
			descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.
			Assert.That(descriptor.DataLabel, Is.EqualTo("Natural Class \"back vowels\" Name"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "085e32ec-eb5b-4eed-9dab-e55854ce88fb"));
		}

		[Test]
		public void PhonologicalFeaturesFindName()
		{
			const string source = @"	  <FsClosedFeature
							guid='c70257a9-750b-4fc6-b68b-7194752c77cc'>
				<Abbreviation>
				  <AUni
					ws='en'>cons</AUni>
				</Abbreviation>
				<CatalogSourceId>
				  <Uni>fPAConsonantal</Uni>
				</CatalogSourceId>
				<Description>
				  <AStr
					ws='en'>
					<Run
					  ws='en'>“Consonantal segments are produced with an audible constriction in the vocal tract, like plosives, affricates, fricatives, nasals, laterals and [r]. Vowels, glides and laryngeal segments are not consonantal.”</Run>
				  </AStr>
				</Description>
				<DisplayToRightOfValues
				  val='False' />
				<Name>
				  <AUni
					ws='en'></AUni>
				</Name>
				<ShowInGloss
				  val='False' />
				<Values>
				  <FsSymFeatVal
					guid='06265842-809d-4f42-9a92-570627d1630d'>
					<Abbreviation>
					  <AUni
						ws='en'>-</AUni>
					</Abbreviation>
					<CatalogSourceId>
					  <Uni>vPAConsonantalNegative</Uni>
					</CatalogSourceId>
					<Name>
					  <AUni
						ws='en'>negative</AUni>
					</Name>
					<ShowInGloss
					  val='True' />
				  </FsSymFeatVal>
				  <FsSymFeatVal
					guid='b6901feb-c1d7-43d4-836f-f123ef30e3d2'>
					<Abbreviation>
					  <AUni
						ws='en'>+</AUni>
					</Abbreviation>
					<CatalogSourceId>
					  <Uni>vPAConsonantalPositive</Uni>
					</CatalogSourceId>
					<Name>
					  <AUni
						ws='en'>positive</AUni>
					</Name>
					<ShowInGloss
					  val='True' />
				  </FsSymFeatVal>
				</Values>
			  </FsClosedFeature>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[0].ChildNodes[0]; //<Abbreviation><AUni>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("Phonological Features \"cons\" Abbreviation"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "c70257a9-750b-4fc6-b68b-7194752c77cc"));

			input = root.ChildNodes[2].ChildNodes[0]; //<Description><AUni>
			descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.
			Assert.That(descriptor.DataLabel, Is.EqualTo("Phonological Features \"cons\" Description"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "c70257a9-750b-4fc6-b68b-7194752c77cc"));

			input = root.ChildNodes[4].ChildNodes[0]; //<Name><AUni>
			descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.
			Assert.That(descriptor.DataLabel, Is.EqualTo("Phonological Features \"cons\" Name"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "c70257a9-750b-4fc6-b68b-7194752c77cc"));

			input = root.ChildNodes[6].ChildNodes[1].ChildNodes[0].ChildNodes[0]; //<Values><FsSymFeatVal><Abbreviation><AUni>
			descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.
			Assert.That(descriptor.DataLabel, Is.EqualTo("Phonological Features \"cons\" Values Abbreviation"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "b6901feb-c1d7-43d4-836f-f123ef30e3d2"));

			input = root.ChildNodes[6].ChildNodes[1].ChildNodes[2].ChildNodes[0]; //<Values><FsSymFeatVal><Name><AUni>
			descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.
			Assert.That(descriptor.DataLabel, Is.EqualTo("Phonological Features \"cons\" Values Name"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "b6901feb-c1d7-43d4-836f-f123ef30e3d2"));
		}

		/// <summary>
		/// Given something like a LexEntry containing a LexemeForm containing an MoStemAllomorph containing a Form
		/// which is one or more AUnis, we want to see something like LexemeForm Form.
		/// That is, the MoStemAllomorph level can just be left out.
		/// More generally, all object levels can just be left out, that is, the ones that have guid attributes.
		/// We don't use the standard names here because we may eventually implement a nicer case for all of LexEntry.
		/// </summary>
		[Test]
		public void UnknownMultiStringPathOmitsObjectLevels()
		{
			const string source = @"<LexEntry
					guid='01efa516-1749-4b60-b43d-00089269e7c5'>
					<HomographNumber
						val='0' />
					<Outer>
						<Mid
							guid='8e982d88-0111-43b9-a25c-420bb5c84cf0'>
							<Target>
								<AUni ws='en'>abcdefghijk</AUni>
								<AUni ws='fr'>other</AUni>
							</Target>
							<IsAbstract
								val='False' />
							<MorphType>
								<objsur
									guid='d7f713e4-e8cf-11d3-9764-00c04f186933'
									t='r' />
							</MorphType>
						</Mid>
					</Outer>
				</LexEntry>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[0].ChildNodes[0]; // the Target element.
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry Outer Target"));
		}

		/// <summary>
		/// Given something like a LexEntry containing a LexemeForm containing an MoStemAllomorph containing a MorphType
		/// with an objsur, for the objsur, we want to see something like Entry LexemeForm MorphType
		/// That is, the MoStemAllomorph level can just be left out.
		/// More generally, all object levels can just be left out, that is, the ones that have guid attributes.
		/// We don't use the standard names here because we may eventually implement a nicer case for all of LexEntry.
		/// This is partly to see what happens when we don't have a context generator for the top level.
		/// We have to have one real class (MoStemAllomorph) because that is how the generator knows
		/// it has found a good level to return a guid for.
		/// </summary>
		[Test]
		public void UnknownObjSurPathOmitsObjectLevels()
		{
			const string source = @"<Dummy
					guid='01efa516-1749-4b60-b43d-00089269e7c5'>
					<HomographNumber
						val='0' />
					<Outer>
						<MoStemAllomorph
							guid='8e982d88-0111-43b9-a25c-420bb5c84cf0'>
							<Target>
								<objsur
									guid='d7f713e4-e8cf-11d3-9764-00c04f186933'
									t='r' />
							</Target>
							<IsAbstract
								val='False' />
						</MoStemAllomorph>
					</Outer>
				</Dummy>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0]; // the objsur element.
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.
			Assert.That(descriptor.DataLabel, Is.EqualTo("Dummy Outer Target"));
		}

		/// <summary>
		/// Given something like a LexEntry containing Senses containing a Gloss
		/// which is one or more AUnis, we want to see something like Sense 1.Gloss en: {text}.
		/// We don't use the standard names here because we may eventually implement a nicer case for all of LexEntry.
		/// </summary>
		[Test]
		public void UnknownMultiStringHandlesOwnSeq()
		{
			const string source = @"<LexEntry	guid='01efa516-1749-4b60-b43d-00089269e7c5'>
					<HomographNumber
						val='0' />
					<SeqProp>
						<ownseq
							class='SomeClass'
							guid='9ad7591f-e475-43ab-bc21-db082e3a12e5'>
							<Target>
								<AUni ws='en'>first</AUni>
								<AUni ws='fr'>second</AUni>
							</Target>
						</ownseq>
						<ownseq
							class='SomeClass'
							guid='9ad7591f-e475-43ab-bc21-db082e3a12e6'>
							<Target>
								<AUni ws='en'>third</AUni>
								<AUni ws='fr'>fourth</AUni>
							</Target>
						</ownseq>
					</SeqProp>
				</LexEntry>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[1].ChildNodes[0]; // the Target element (in the second objseq).
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry SeqProp 2 Target"));
		}

		[Test]
		public void TestFirstNonBlankChildsData()
		{
			const string source = @"	<ArbitraryLabel
						guid='1ee09905-63dd-4c7a-a9bd-1d496743ccd6'>
						<Name>
							<AUni ws='fr'></AUni>
							<AUni ws='fa'>   </AUni>
							<AUni ws='hi'>	</AUni>
							<AUni ws='en'>The Name</AUni>
						</Name>
					</ArbitraryLabel>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[0];
			var generator = new FieldWorkObjectContextGenerator();

			Assert.That(generator.FirstNonBlankChildsData(input), Is.EqualTo("The Name"));
		}

		[Test]
		public void PossibilityListPartsFindAbbreviationIfNoName()
		{
			const string source = @"	<CmPossibilityList
						guid='1ee09905-63dd-4c7a-a9bd-1d496743ccd6'>
						<Abbreviation>
							<AUni ws='fr'></AUni>
							<AUni ws='fa'>   </AUni>
							<AUni ws='hi'>	</AUni>
							<AUni ws='en'>Some Random Value</AUni>
						</Abbreviation>
						<Depth
							val='127' />
						<ItemClsid
							val='5118' />
						<Possibilities>
							<ownseq
								class='LexEntryType'
								guid='1f6ae209-141a-40db-983c-bee93af0ca3c'>
								<Abbreviation>
									<AUni
										ws='en'>comp. of</AUni>
								</Abbreviation>
								<IsProtected
									val='true' />
								<Name>
									<AUni
										ws='en'>Compound</AUni>
								</Name>
								<ReverseAbbr>
									<AUni
										ws='en'>comp.</AUni>
								</ReverseAbbr>
							</ownseq>
						</Possibilities>
						<PreventDuplicates
							val='true' />
						<WsSelector
							val='-3' />
					</CmPossibilityList>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[2]; // ItemClsid
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("List \"Some Random Value\" ItemClsid"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1ee09905-63dd-4c7a-a9bd-1d496743ccd6"));
		}

		[Test]
		public void PossibilityFindName()
		{
			const string source = @"	<CmPossibilityList
						guid='1ee09905-63dd-4c7a-a9bd-1d496743ccd6'>
						<Abbreviation>
							<AUni
								ws='en'>Junk</AUni>
						</Abbreviation>
						<Depth
							val='127' />
						<ItemClsid
							val='5118' />
						<Name>
							<AUni
								ws='en'>Complex Form Types</AUni>
						</Name>
						<Possibilities>
							<ownseq
								class='LexEntryType'
								guid='1f6ae209-141a-40db-983c-bee93af0ca3c'>
								<Abbreviation>
									<AUni
										ws='en'>comp. of</AUni>
								</Abbreviation>
								<IsProtected
									val='true' />
								<Name>
									<AUni
										ws='en'>Compound</AUni>
								</Name>
								<ReverseAbbr>
									<AUni
										ws='en'>comp.</AUni>
								</ReverseAbbr>
							</ownseq>
						</Possibilities>
						<PreventDuplicates
							val='true' />
						<WsSelector
							val='-3' />
					</CmPossibilityList>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[4].ChildNodes[0].ChildNodes[3]; // <Possibilities><ownseq><ReverseAbbr>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("Item \"Compound\" from List \"Complex Form Types\" ReverseAbbr"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1f6ae209-141a-40db-983c-bee93af0ca3c"));
		}

		/// <summary>
		/// Make sure that GenerateContextDescriptor/PathToUserUnderstandableElement returns the guid of its parent,
		/// even if it is under and Ownseq node.
		/// </summary>
		[Test]
		public void FindGuidOfParentOwnseq()
		{
			const string source = @"	<CmPossibilityList
						guid='1ee09905-63dd-4c7a-a9bd-1d496743ccd6'>
						<Name>
							<AUni
								ws='en'>Complex Form Types</AUni>
						</Name>
						<Possibilities>
							<ownseq
								class='LexEntryType'
								guid='1f6ae209-141a-40db-983c-bee93af0ca3c'>
								<Name>
									<AUni
										ws='en'>Compound</AUni>
								</Name>
								<ReverseAbbr>
									<AUni
										ws='en'>comp.</AUni>
								</ReverseAbbr>
							</ownseq>
						</Possibilities>
					</CmPossibilityList>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[0].ChildNodes[1]; // <Possibilities><ownseq><ReverseAbbr>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1f6ae209-141a-40db-983c-bee93af0ca3c"));
			Assert.That(descriptor.DataLabel, Is.EqualTo("Item \"Compound\" from List \"Complex Form Types\" ReverseAbbr"));
		}

		/// <summary>
		/// Make sure that GenerateContextDescriptor/PathToUserUnderstandableElement returns the guid of its parent,
		/// even if it is under an ownseq node.
		/// </summary>
		[Test]
		public void FindGuidOfParentOwnseqatomic()
		{
			const string source =
				 @"<StText
						guid='b314f2f8-ea5e-11de-86b7-0013722f8dec'>
						<Paragraphs>
							<ownseq
								class='StTxtPara'
								guid='b31e7c56-ea5e-11de-85d3-0013722f8dec'>
								<Contents>
									<Str>
										<Run
											ws='en'>Example (English)</Run>
									</Str>
								</Contents>
								<ParseIsCurrent
									val='False' />
							</ownseq>
						</Paragraphs>
					</StText>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[0].ChildNodes[0].ChildNodes[0]; // <Paragraphs><ownseq><Contents>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "b31e7c56-ea5e-11de-85d3-0013722f8dec"));
		}

		[Test]
		public void SubPossibilityFindName()
		{
			const string source = @"<CmPossibilityList
					guid='b0a1eb98-ea5e-11de-888e-0013722f8dec'>
					<Abbreviation>
					  <AUni
							ws='en'>AcDom</AUni>
					</Abbreviation>
					<IsSorted
							val='true' />
					<Name>
					  <AUni
							ws='en'>Academic Domains</AUni>
					</Name>
					<Possibilities>
						<ownseq
							class='CmPossibility'
							guid='b0add746-ea5e-11de-8c5c-0013722f8dec'>
							<Abbreviation>
							  <AUni
									ws='en'>Anat</AUni>
							</Abbreviation>
							<Name>
							  <AUni
									ws='en'>anatomy</AUni>
							</Name>
							<UnderColor
									val='-1073741824' />
						  </ownseq>
						  <ownseq
								class='CmPossibility'
								guid='b0b9c2fe-ea5e-11de-8fed-0013722f8dec'>
							<Abbreviation>
							  <AUni
									ws='en'>Anthro</AUni>
							</Abbreviation>
							<Name>
							  <AUni
									ws='en'>anthropology</AUni>
							</Name>
							<SubPossibilities>
							  <ownseq
									class='CmPossibility'
									guid='b0c5aeac-ea5e-11de-9463-0013722f8dec'>
									<Abbreviation>
									  <AUni
											ws='en'>Cult anthro</AUni>
									</Abbreviation>
									<Name>
									  <AUni
											ws='en'>cultural anthropology</AUni>
									</Name>
								<UnderColor
										val='-1073741824' />
							  </ownseq>
							</SubPossibilities>
							<UnderColor
									val='-1073741824' />
					  </ownseq>
					</Possibilities>
				</CmPossibilityList>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[3].ChildNodes[1].ChildNodes[2].ChildNodes[0].ChildNodes[2]; // Possibilities><ownseq>[1]<SubPossibilities><ownseq><UnderColor>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("Item \"cultural anthropology\" from List \"Academic Domains\" UnderColor"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "b0c5aeac-ea5e-11de-9463-0013722f8dec"));
		}

		[Test]
		public void EnvironmentsPartsFindEnvironment()
		{
			const string source = @"<PhonologicalData>
	<PhPhonData
		guid='86980496-9fe8-428d-bd53-fa2e623fe1c0'>
		<Environments>
			<ownseq
				class='PhEnvironment'
				guid='02132942-0ff7-45e8-8f09-f4918535a31e'>
				<Description>
					<AStr
						ws='en'>
						<Run
							ws='en'>used for e&gt;i change on vext</Run>
					</AStr>
				</Description>
				<Name>
					<AUni
						ws='en'>mid vowel in previous syllable</AUni>
				</Name>
				<StringRepresentation>
					<Str>
						<Run
							ws='en'>/[</Run>
						<Run
							ws='en'>V+mid] ([preNas]) [C-nas] ([Mod]) _</Run>
					</Str>
				</StringRepresentation>
			</ownseq>
			<ownseq
				class='PhEnvironment'
				guid='1ae6eb4a-84a0-4134-b684-5b446ad83708'>
				<StringRepresentation>
					<Str>
						<Run
							ws='en'>/ [C] _</Run>
					</Str>
				</StringRepresentation>
			</ownseq>
			<ownseq
				class='PhEnvironment'
				guid='e5e81505-6f2b-42df-88dc-a76c2dfcad87'>
				<StringRepresentation>
					<Str>
						<Run
							ws='en'>/ _ [-Lab-Lat-Nas</Run>
						<Run
							ws='en'>]</Run>
					</Str>
				</StringRepresentation>
			</ownseq>
		</Environments>
	</PhPhonData>
</PhonologicalData>";
			var root = FieldWorksTestServices.GetNode(source); // PhonologicalData
			var input = root.ChildNodes[0].ChildNodes[0].ChildNodes[0]; // 1st ownseq
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Environment \"mid vowel in previous syllable\""));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "02132942-0ff7-45e8-8f09-f4918535a31e"));

			// Try a node that has no name, only a representation.
			input = root.ChildNodes[0].ChildNodes[0].ChildNodes[1]; // 2nd ownseq
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Environment \"/ [C] _\""));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1ae6eb4a-84a0-4134-b684-5b446ad83708"));

			// Try the Description to see we still get the name
			input = root.ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0]; // the <Description>
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Environment \"mid vowel in previous syllable\" Description"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "02132942-0ff7-45e8-8f09-f4918535a31e"));

			// See that the runs are merged
			input = root.ChildNodes[0].ChildNodes[0].ChildNodes[2]; // 3rd ownseq
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Environment \"/ _ [-Lab-Lat-Nas]\""));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "e5e81505-6f2b-42df-88dc-a76c2dfcad87"));

			// See that the first run is prepended to the last one
			input = root.ChildNodes[0].ChildNodes[0].ChildNodes[2].ChildNodes[0].ChildNodes[0].ChildNodes[1]; // 3rd ownseq, 2nd Run
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Environment \"/ _ [-Lab-Lat-Nas]\" StringRepresentation Str Run"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "e5e81505-6f2b-42df-88dc-a76c2dfcad87"));
		}
		[Test]
		public void DiscoursePartsFindChart()
		{
			const string source = @"<Discourse>
	<header>
		<DsDiscourseData
			guid='aae5a99c-2333-4d6f-934e-d2c059de249a'>
			<ChartMarkers />
			<Charts />
			<ConstChartTempl />
		</DsDiscourseData>
	</header>
	<DsChart
		class='DsConstChart'
		guid='78a7df6d-4e58-47ac-8714-52aeeca8b66c'>
		<BasedOn>
			<objsur
				guid='95663db0-64d9-41f8-ab11-e388ce54e3f2'
				t='r' />
		</BasedOn>
		<DateCreated
			val='2009-4-9 16:43:0.290' />
		<DateModified
			val='2009-4-23 20:21:42.447' />
		<Rows>
			<ownseq
				class='ConstChartRow'
				guid='367829ee-3b53-493a-a057-1eccde2c45e4'>
				<Cells>
					<ownseq
						class='ConstChartWordGroup'
						guid='991d2812-1b2c-46ac-bb74-12819e45ce9a'>
						<BeginAnalysisIndex
							val='0' />
						<BeginSegment>
							<objsur
								guid='efa04f83-2d6d-4dd7-90df-d057b4aefe05'
								t='r' />
						</BeginSegment>
						<Column>
							<objsur
								guid='f6468942-5e24-4dcb-9d54-c8a634b07a5a'
								t='r' />
						</Column>
						<EndAnalysisIndex
							val='1' />
						<EndSegment>
							<objsur
								guid='efa04f83-2d6d-4dd7-90df-d057b4aefe05'
								t='r' />
						</EndSegment>
						<MergesAfter
							val='False' />
						<MergesBefore
							val='False' />
					</ownseq>
					<ownseq
						class='ConstChartWordGroup'
						guid='870c0faf-1eb7-4a52-9172-9bb9d338017c'>
						<BeginAnalysisIndex
							val='3' />
						<BeginSegment>
							<objsur
								guid='efa04f83-2d6d-4dd7-90df-d057b4aefe05'
								t='r' />
						</BeginSegment>
						<Column>
							<objsur
								guid='664b434a-e7b1-4c02-8970-d89c09d43c54'
								t='r' />
						</Column>
						<EndAnalysisIndex
							val='4' />
						<EndSegment>
							<objsur
								guid='efa04f83-2d6d-4dd7-90df-d057b4aefe05'
								t='r' />
						</EndSegment>
						<MergesAfter
							val='False' />
						<MergesBefore
							val='False' />
					</ownseq>
				</Cells>
				<ClauseType
					val='0' />
				<EndDependentClauseGroup
					val='False' />
				<EndParagraph
					val='False' />
				<EndSentence
					val='True' />
				<Label>
					<Str>
						<Run
							ws='en'>1</Run>
					</Str>
				</Label>
				<StartDependentClauseGroup
					val='False' />
			</ownseq>
			<ownseq
				class='ConstChartRow'
				guid='449ab63e-33b1-43e8-a7bb-b1fe517b0e7e'>
				<Cells>
					<ownseq
						class='ConstChartWordGroup'
						guid='1d3102c9-7f9d-499f-a85c-22d3c3b1af04'>
						<BeginAnalysisIndex
							val='0' />
						<BeginSegment>
							<objsur
								guid='83360f37-ff92-4910-8ceb-360d51c342c2'
								t='r' />
						</BeginSegment>
						<Column>
							<objsur
								guid='39775c44-e64a-4fb1-b4ca-d5da3d7dbb24'
								t='r' />
						</Column>
						<EndAnalysisIndex
							val='0' />
						<EndSegment>
							<objsur
								guid='83360f37-ff92-4910-8ceb-360d51c342c2'
								t='r' />
						</EndSegment>
						<MergesAfter
							val='False' />
						<MergesBefore
							val='False' />
					</ownseq>
					<ownseq
						class='ConstChartWordGroup'
						guid='3f371913-8108-4e06-8d7d-a5f0fe69c413'>
						<BeginAnalysisIndex
							val='1' />
						<BeginSegment>
							<objsur
								guid='83360f37-ff92-4910-8ceb-360d51c342c2'
								t='r' />
						</BeginSegment>
						<Column>
							<objsur
								guid='7e958a20-1512-4af9-863a-d92f7175a68c'
								t='r' />
						</Column>
						<EndAnalysisIndex
							val='1' />
						<EndSegment>
							<objsur
								guid='83360f37-ff92-4910-8ceb-360d51c342c2'
								t='r' />
						</EndSegment>
						<MergesAfter
							val='False' />
						<MergesBefore
							val='False' />
					</ownseq>
				</Cells>
				<ClauseType
					val='0' />
				<EndDependentClauseGroup
					val='False' />
				<EndParagraph
					val='False' />
				<EndSentence
					val='False' />
				<Label>
					<Str>
						<Run
							ws='en'>2a</Run>
					</Str>
				</Label>
				<StartDependentClauseGroup
					val='False' />
			</ownseq>
		</Rows>
		<Template>
			<objsur
				guid='cb178bdb-97b8-49e2-9436-2f188a2b8589'
				t='r' />
		</Template>
	</DsChart>
	<DsChart
		class='DsConstChart'
		guid='88482f27-8a9f-4d25-b375-2c8720aa2b62'>
		<BasedOn>
			<objsur
				guid='40b3e88f-ae59-48d0-a2ba-e498a27cc9b3'
				t='r' />
		</BasedOn>
		<DateCreated
			val='2008-4-8 21:34:30.653' />
		<DateModified
			val='2008-4-8 21:34:30.653' />
		<Template>
			<objsur
				guid='cb178bdb-97b8-49e2-9436-2f188a2b8589'
				t='r' />
		</Template>
	</DsChart>
</Discourse>";
			var root = FieldWorksTestServices.GetNode(source); // Discourse
			var generator = MakeGenerator();

			// 2nd DsChart
			var input = root.ChildNodes[2];
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 2"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "88482f27-8a9f-4d25-b375-2c8720aa2b62"));

			// 1st DsChart BasedOn
			input = root.ChildNodes[1].ChildNodes[0];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 BasedOn"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "78a7df6d-4e58-47ac-8714-52aeeca8b66c"));

			// 2nd DsChart DateCreated
			input = root.ChildNodes[2].ChildNodes[1];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 2 DateCreated"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "88482f27-8a9f-4d25-b375-2c8720aa2b62"));

			// 2nd DsChart DateModified
			input = root.ChildNodes[2].ChildNodes[2];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 2 DateModified"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "88482f27-8a9f-4d25-b375-2c8720aa2b62"));

			// 1st DsChart Template
			input = root.ChildNodes[1].ChildNodes[4];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Template"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "78a7df6d-4e58-47ac-8714-52aeeca8b66c"));

			// 1st DsChart Rows
			input = root.ChildNodes[1].ChildNodes[3];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Rows"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "78a7df6d-4e58-47ac-8714-52aeeca8b66c"));

			// 1st DsChart Row 2 Cells
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[0];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 2) Cells"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "449ab63e-33b1-43e8-a7bb-b1fe517b0e7e"));

			// 1st DsChart Row 1 Column 2
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[0].ChildNodes[1].ChildNodes[2];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			// Discourse Chart 1 Row 1 Column 2 Column <== should be a way to suppreee FieldWorkObjectContextGenerator.GetLabel()/GetPathAppend()
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 1 Column 2) Column"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "870c0faf-1eb7-4a52-9172-9bb9d338017c"));

			// 1st DsChart Row 1 Column 2 BeginSegment
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[0].ChildNodes[1].ChildNodes[1];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 1 Column 2) BeginSegment"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "870c0faf-1eb7-4a52-9172-9bb9d338017c"));

			// 1st DsChart Row 2 Column 2 EndSegment
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[0].ChildNodes[1].ChildNodes[4];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 2 Column 2) EndSegment"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "3f371913-8108-4e06-8d7d-a5f0fe69c413"));

			// 1st DsChart Row 2 Column 1 BeginAnalysisIndex
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 2 Column 1) BeginAnalysisIndex"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1d3102c9-7f9d-499f-a85c-22d3c3b1af04"));

			// 1st DsChart Row 1 Column 1 EndAnalysisIndex
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[3];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 1 Column 1) EndAnalysisIndex"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "991d2812-1b2c-46ac-bb74-12819e45ce9a"));

			// 1st DsChart Row 2 Column 2 MergesBefore
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[0].ChildNodes[1].ChildNodes[6];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 2 Column 2) MergesBefore"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "3f371913-8108-4e06-8d7d-a5f0fe69c413"));

			// 1st DsChart Row 1 Column 2 MergesAfter
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[0].ChildNodes[1].ChildNodes[5];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 1 Column 2) MergesAfter"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "870c0faf-1eb7-4a52-9172-9bb9d338017c"));

			// 1st DsChart Row 1 ClauseType
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[1];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 1) ClauseType"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "367829ee-3b53-493a-a057-1eccde2c45e4"));

			// 1st DsChart Row 2 EndDependentClauseGroup
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[2];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 2) EndDependentClauseGroup"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "449ab63e-33b1-43e8-a7bb-b1fe517b0e7e"));

			// 1st DsChart Row 2 EndParagraph
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[3];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 2) EndParagraph"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "449ab63e-33b1-43e8-a7bb-b1fe517b0e7e"));

			// 1st DsChart Row 1 EndSentence
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[4];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 1) EndSentence"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "367829ee-3b53-493a-a057-1eccde2c45e4"));

			// 1st DsChart Row 1 Label
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[5];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 1) Label"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "367829ee-3b53-493a-a057-1eccde2c45e4"));

			// 1st DsChart Row 2 StartDependentClauseGroup
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[6];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 (Row 2) StartDependentClauseGroup"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "449ab63e-33b1-43e8-a7bb-b1fe517b0e7e"));
		}
	}
}
