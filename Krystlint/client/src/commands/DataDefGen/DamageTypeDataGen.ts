import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let DamageTypeDataPattern : DataDefGenConfig = {
	xsi: "DamageTypeData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
	]
}

export function DamageTypeDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(DamageTypeDataPattern, editBuilder);
}