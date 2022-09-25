import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let RoomPropertyDataPattern : DataDefGenConfig = {
	xsi: "RoomPropertyData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "displayNameRef", default: "string_datadef_id"},
		{name: "iconRef", default: "icon_datadef_id"},
		{name: "absencePropertyRef", default: "room_property_datadef_id"},
	]
}

export function RoomPropertyDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(RoomPropertyDataPattern, editBuilder);
}