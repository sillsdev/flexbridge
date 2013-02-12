using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.Reversal
{
	/// <summary>
	/// Context generator for Reversal Index entry elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	class ReversalEntryContextGenerator : FieldWorkObjectContextGenerator
	{
	//<ReversalIndexEntry
	//    guid="cdfe2b07-765b-4ebf-b453-ba5f93387773">
	//    <PartOfSpeech>
	//        <objsur
	//            guid="a8e41fd3-e343-4c7c-aa05-01ea3dd5cfb5"
	//            t="r" />
	//    </PartOfSpeech>
	//    <ReversalForm>
	//        <AUni
	//            ws="en">cat</AUni>
	//    </ReversalForm>
	//    <Subentries>
	//        <ReversalIndexEntry
	//            guid="0373eec0-940d-4794-9cfc-8ef351e5699f">
	//            <PartOfSpeech>
	//                <objsur
	//                    guid="a8e41fd3-e343-4c7c-aa05-01ea3dd5cfb5"
	//                    t="r" />
	//            </PartOfSpeech>
	//            <ReversalForm>
	//                <AUni
	//                    ws="en">cat-o-ten-tails</AUni>
	//            </ReversalForm>
	//        </ReversalIndexEntry>
	//    </Subentries>
	//</ReversalIndexEntry>
	}
}
