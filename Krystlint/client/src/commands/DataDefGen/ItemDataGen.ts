import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let ItemDataPattern : DataDefGenConfig = {
	xsi: "ItemData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "nameRef", default: "string_datadef_id"},
		{name: "infoRef", default: "string_datadef_id"},
		{name: "iconRef", default: "icon_datadef_id"},
		{name: "width", default: "1"},
		{name: "height", default: "1"},
		{name: "maxStackSize", default: "1"},
		{name: "pickupSoundRef", default: "sfx_datadef_id"},
		{name: "laydownSoundRef", default: "sfx_datadef_id"},
		{name: "components", default: "\n\n\t\t"},
	]
}

export function ItemDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(ItemDataPattern, editBuilder);
}