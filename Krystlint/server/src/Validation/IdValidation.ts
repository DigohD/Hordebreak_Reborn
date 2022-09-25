import { Diagnostic, DiagnosticSeverity, TextDocument } from 'vscode-languageserver';
import { ExampleSettings, IndexBankType } from '../server';

import {DoesIdExistMoreThanOnce} from '../utils/ListSearch';

export function ValidateFileIds(
	text: string, 
	diagnostics: Diagnostic[], 
	textDocument: TextDocument,
	IndexBank: IndexBankType
){
	var regex = /<id>(.*?)<\/id>/gm;
	let m: RegExpExecArray | null;

	while ((m = regex.exec(text))) {
		var IdRegex = /^[a-z0-9_]*$/;
		if(!IdRegex.test(m[1])){
			let diagnostic: Diagnostic = {
				severity: DiagnosticSeverity.Error,
				range: {
					start: textDocument.positionAt(m.index),
					end: textDocument.positionAt(m.index + m[0].length)
				},
				message: `${m[1]} may only contain lower case letters, numbers and \'_\'.`,
				source: 'Krystlint'
			};

			diagnostics.push(diagnostic);
			continue;
		}

		var doesStringExist = DoesIdExistMoreThanOnce(
			IndexBank.Ids,
			m[1],
			0,
			IndexBank.Ids.length - 1
		);
		if(doesStringExist){
			let diagnostic: Diagnostic = {
				severity: DiagnosticSeverity.Error,
				range: {
					start: textDocument.positionAt(m.index),
					end: textDocument.positionAt(m.index + m[0].length)
				},
				message: `${m[1]} already exists as a DataDef Id.`,
				source: 'Krystlint'
			};

			diagnostics.push(diagnostic);
		}
	}
}