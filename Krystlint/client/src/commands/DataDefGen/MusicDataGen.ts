import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let MusicDataPattern : DataDefGenConfig = {
	xsi: "MusicData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "filePath", default: "file_path"},
	]
}

export function MusicDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(MusicDataPattern, editBuilder);
}