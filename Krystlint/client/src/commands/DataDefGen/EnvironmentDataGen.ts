import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let EnvironmentDataPattern : DataDefGenConfig = {
	xsi: "EnvironmentData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "nameRef", default: "string_datadef_id"},
		{name: "iconRef", default: "icon_datadef_id"},
	]
}

export function EnvironmentDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(EnvironmentDataPattern, editBuilder);
}