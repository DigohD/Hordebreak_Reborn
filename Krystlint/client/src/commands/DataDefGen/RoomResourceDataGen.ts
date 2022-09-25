import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let RoomResourceDataPattern : DataDefGenConfig = {
	xsi: "RoomResourceData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "nameRef", default: "string_datadef_id"},
		{name: "iconRef", default: "icon_datadef_id"},
	]
}

export function RoomResourceDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(RoomResourceDataPattern, editBuilder);
}