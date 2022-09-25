import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let DoorCompPattern : DataDefGenConfig = {
	xsi: "DoorComponentData",
	tagHierarchy: [
		{name: "openAnimationName", default: "animation_name"},
		{name: "closeAnimationName", default: "animation_name"},
		{name: "openSFXRef", default: "sfx_datadef_id"},
		{name: "closeSFXRef", default: "sfx_datadef_id"}
	]
}

export function DoorCompGen(editDoor: TextEditorEdit){
	InsertDataComponentXML(DoorCompPattern, editDoor);
}