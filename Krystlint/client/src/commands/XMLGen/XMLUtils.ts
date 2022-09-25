import { ExtensionContext, Position } from 'vscode';
import {window} from 'vscode';

export function IsXMLDocumentDefined (): boolean{
	let text: string = window.activeTextEditor.document.getText();

	let xmlDefRegex = /<\?xml version='1\.0' encoding="UTF-8"\?>/mg;
	let defsStartTagRegex = /<Defs xmlns:xsd="http:\/\/www\.w3\.org\/2001\/XMLSchema" xmlns:xsi="http:\/\/www\.w3\.org\/2001\/XMLSchema-instance">/mg;
	let defsEndTagRegex = /<\/Defs>/mg;
	
	let toReturn = xmlDefRegex.test(text) && defsStartTagRegex.test(text) && defsEndTagRegex.test(text);

	return toReturn;
}

export function FindDataDefInsertionPoint (): Position{
	let text: string = window.activeTextEditor.document.getText();
	
	let defsEndTagRegex = /<\/Defs>/mg;
	let tagResultIndex = defsEndTagRegex.exec(text).index;
	
	let newLineRegex = /\n/gm;
	let newlineExec = newLineRegex.exec(text.substr(0, tagResultIndex));

	var lineCounter = 0;
	var match = undefined;
	while ((match = newLineRegex.exec(text.substr(0, tagResultIndex))) != null){
		lineCounter++;
	}

	return new Position(window.activeTextEditor.selection.start.line, 0);
}

export function FindComponentDataInsertionPoint (): Position{
	let text: string = window.activeTextEditor.document.getText();
	let lineIndex = window.activeTextEditor.selection.start.line;

	let lines = text.split('\n');

	let componentsEndTagRegex = /< *\/ *components *>/mg;

	let foundEndTag = componentsEndTagRegex.test(lines[lineIndex]);
	while(!foundEndTag){
		lineIndex++;
		foundEndTag = componentsEndTagRegex.test(lines[lineIndex]);
	}

	return new Position(lineIndex, 0);
}