import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let SFXDataPattern : DataDefGenConfig = {
	xsi: "SFXData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "filePath", default: "file_path"},
		{name: "distance", default: "1.0"},
	]
}

export function SFXDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(SFXDataPattern, editBuilder);
}