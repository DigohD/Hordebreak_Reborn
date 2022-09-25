import { window, TextEditorEdit, ExtensionContext, Position, Range, TextEditorRevealType } from 'vscode';
import {FindDataDefInsertionPoint, IsXMLDocumentDefined, FindComponentDataInsertionPoint} from '../XMLGen/XMLUtils';

export interface DataDefGenConfig{
	xsi: string;
	entityType?: string;
	tagHierarchy: {
		name: string;
		default: string;
		xsi?: string;
	}[]
}

export interface ComponentDataGenConfig{
	xsi: string;
	entityType?: string;
	tagHierarchy: {
		name: string;
		default: string;
	}[]
}

export interface ItemComponentGenConfig{
	xsi: string;
	entityType?: string;
	tagHierarchy: {
		name: string;
		default: string;
	}[]
}

export function InsertXMLDefinition(editBuilder: TextEditorEdit, _optionalContent: string = undefined){
	editBuilder.insert(new Position(0, 0), "<?xml version='1.0' encoding=\"UTF-8\"?>\n");
	editBuilder.insert(new Position(1, 0), "<Defs xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n\n");

	if(_optionalContent){
		editBuilder.insert(new Position(2, 0), _optionalContent);
		editBuilder.insert(new Position(3, 0), "</Defs>");
	}else{
		editBuilder.insert(new Position(2, 0), "</Defs>");
	}
}

export function InsertDataDefXML(
	config: DataDefGenConfig,
	editBuilder: TextEditorEdit
){
	let isXMLDefined : boolean = IsXMLDocumentDefined();

	window.showInformationMessage("XML DEFINED IN FILE: " + isXMLDefined);

	let FullXMLInsertion = "";

	if(!config.entityType){
		FullXMLInsertion += `\t<DataDef xsi:type=\"${config.xsi}\">\n`;
	}
	else{
		FullXMLInsertion += `\t<DataDef entityType=\"${config.entityType}\" xsi:type=\"${config.xsi}\">\n`;
	}

	config.tagHierarchy.forEach((entry, index) => {
		let xsiInsert = "";
		if(entry.xsi){
			xsiInsert= ` xsi:type=\"${entry.xsi}\"`;
		}
		FullXMLInsertion += `\t\t<${entry.name + xsiInsert}>${entry.default}</${entry.name}>`;
		if(index < config.tagHierarchy.length - 1){
			FullXMLInsertion += "\n";
		}
	});

	FullXMLInsertion += `\n\t</DataDef>\n`;

	let insertionPoint : Position = new Position(0, 0);
	if(!isXMLDefined){
		InsertXMLDefinition(editBuilder, FullXMLInsertion);
		insertionPoint = new Position(2, 0);
	}else{
		insertionPoint = FindDataDefInsertionPoint();
		editBuilder.insert(insertionPoint, FullXMLInsertion);
	}
}

export function InsertDataComponentXML(
	config: ComponentDataGenConfig,
	editBuilder: TextEditorEdit
){
	let FullXMLInsertion = "";

	if(config.tagHierarchy.length == 0){
		FullXMLInsertion += `\t\t\t<DataComponent xsi:type=\"${config.xsi}\" />\n`;
	}else{
		FullXMLInsertion += `\t\t\t<DataComponent xsi:type=\"${config.xsi}\">\n`;
		config.tagHierarchy.forEach((entry, index) => {
			FullXMLInsertion += `\t\t\t\t<${entry.name}>${entry.default}</${entry.name}>`;
			if(index < config.tagHierarchy.length - 1){
				FullXMLInsertion += "\n";
			}
		});
		FullXMLInsertion += `\n\t\t\t</DataComponent>\n`;
	}

	let insertionPoint : Position = new Position(0, 0);

	insertionPoint = FindComponentDataInsertionPoint();
	editBuilder.insert(insertionPoint, FullXMLInsertion);

	window.activeTextEditor.revealRange(new Range(insertionPoint, insertionPoint), TextEditorRevealType.InCenter);
}

export function InsertItemComponentXML(
	config: ItemComponentGenConfig,
	editBuilder: TextEditorEdit
){
	let FullXMLInsertion = "";

	if(config.tagHierarchy.length == 0){
		FullXMLInsertion += `\t\t\t<ItemComponentData xsi:type=\"${config.xsi}\" />\n`;
	}else{
		FullXMLInsertion += `\t\t\t<ItemComponentData xsi:type=\"${config.xsi}\">\n`;
		config.tagHierarchy.forEach((entry, index) => {
			FullXMLInsertion += `\t\t\t\t<${entry.name}>${entry.default}</${entry.name}>`;
			if(index < config.tagHierarchy.length - 1){
				FullXMLInsertion += "\n";
			}
		});
		FullXMLInsertion += `\n\t\t\t</ItemComponentData>\n`;
	}

	let insertionPoint : Position = new Position(0, 0);

	insertionPoint = FindComponentDataInsertionPoint();
	editBuilder.insert(insertionPoint, FullXMLInsertion);

	window.activeTextEditor.revealRange(new Range(insertionPoint, insertionPoint), TextEditorRevealType.InCenter);
}