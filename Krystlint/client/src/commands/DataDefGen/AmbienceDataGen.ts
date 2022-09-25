import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let AmbienceDataPattern : DataDefGenConfig = {
	xsi: "AmbienceData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "filePath", default: "file_path"}
	]
}

export function AmbienceDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(AmbienceDataPattern, editBuilder);
}