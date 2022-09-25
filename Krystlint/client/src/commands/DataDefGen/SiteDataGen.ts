import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let SiteDataPattern : DataDefGenConfig = {
	xsi: "SiteData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "filePath", default: "file_path"},
		{name: "width", default: "0"},
		{name: "height", default: "0"},
	]
}

export function SiteDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(SiteDataPattern, editBuilder);
}