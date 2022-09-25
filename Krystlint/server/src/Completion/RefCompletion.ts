import {IsCaretWithinRefTag} from '../utils/CaretUtils';
import { Position, CompletionItemKind, CompletionItem } from 'vscode-languageserver';
import { IndexBankType } from '../server';

export function GetRefCompletionList(
	text: string,
	caretPos: Position,
	IndexBank: IndexBankType,
	
) : CompletionItem[]{
	var isWithinRefTag = IsCaretWithinRefTag(text, caretPos.line, caretPos.character);

	return isWithinRefTag ? IndexBank.Ids.map((id, index) => {
		return {
			label: id,
			kind: CompletionItemKind.Text,
			data: "IDREF::" + index
		}
	}) : [];
}