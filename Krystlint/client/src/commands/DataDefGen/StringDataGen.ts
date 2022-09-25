import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let StringDataPattern : DataDefGenConfig = {
	xsi: "StringData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "sv", default: "sv_translation"},
		{name: "en", default: "en_translation"},
	]
}

export function StringDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(StringDataPattern, editBuilder);
}