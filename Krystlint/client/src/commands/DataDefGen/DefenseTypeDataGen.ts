import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let DefenseTypeDataPattern : DataDefGenConfig = {
	xsi: "DefenseTypeData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "damagedBy", default: "\n\t\t\t<DamageEntry>\n\t\t\t\t<damageRef>damage_type_datadef_id</damageRef>\n\t\t\t\t<amplifier>1.0</amplifier>\n\t\t\t</DamageEntry>\n\t\t"},
	]
}

export function DefenseTypeDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(DefenseTypeDataPattern, editBuilder);
}