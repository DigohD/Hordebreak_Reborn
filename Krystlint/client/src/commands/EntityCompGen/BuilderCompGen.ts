import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let BuilderCompPattern : DataDefGenConfig = {
	xsi: "BuilderComponentData",
	tagHierarchy: []
}

export function BuilderCompGen(editBuilder: TextEditorEdit){
	InsertDataComponentXML(BuilderCompPattern, editBuilder);
}