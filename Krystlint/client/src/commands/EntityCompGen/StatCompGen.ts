import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let StatCompPattern : DataDefGenConfig = {
	xsi: "StatComponentData",
	tagHierarchy: [
		{name: "startHealth", default: "100.0"},
		{name: "startShields", default: "100.0"},
		{name: "startShieldsRegeneration", default: "0.0"},
		{name: "startArmor", default: "0.0"},
		{name: "defenseTypeRef", default: "defense_type_datadef_id"}
	]
}

export function StatCompGen(editStat: TextEditorEdit){
	InsertDataComponentXML(StatCompPattern, editStat);
}