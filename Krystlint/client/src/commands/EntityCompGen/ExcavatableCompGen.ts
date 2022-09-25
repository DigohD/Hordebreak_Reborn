import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let ExcavatableCompPattern : DataDefGenConfig = {
	xsi: "ExcavatableComponentData",
	tagHierarchy: [
		{name: "totalHits", default: "5"},
		{name: "hitEffectRef", default: "effect_datadef_id"},
		{name: "deathEffectRef", default: "effect_datadef_id"},
		{name: "HitLootTable", default: "\n\t\t\t\t\t<minRolls>0</minRolls>\n\t\t\t\t\t<maxRolls>0</maxRolls>\n\t\t\t\t\t<table>\n\t\t\t\t\t\t<LootEntry>\n\t\t\t\t\t\t\t<itemRef>item_datadef_id</itemRef>\n\t\t\t\t\t\t\t<probability>10</probability>\n\t\t\t\t\t\t\t<guaranteed>false</guaranteed>\n\t\t\t\t\t\t\t<unique>false</unique>\n\t\t\t\t\t\t</LootEntry>\n\t\t\t\t\t</table>\n\t\t\t\t"},
		{name: "DestroyLootTable", default: "\n\t\t\t\t\t<minRolls>0</minRolls>\n\t\t\t\t\t<maxRolls>0</maxRolls>\n\t\t\t\t\t<table>\n\t\t\t\t\t\t<LootEntry>\n\t\t\t\t\t\t\t<itemRef>item_datadef_id</itemRef>\n\t\t\t\t\t\t\t<probability>10</probability>\n\t\t\t\t\t\t\t<guaranteed>false</guaranteed>\n\t\t\t\t\t\t\t<unique>false</unique>\n\t\t\t\t\t\t</LootEntry>\n\t\t\t\t\t</table>\n\t\t\t\t"},
	]
}

export function ExcavatableCompGen(editExcavatable: TextEditorEdit){
	InsertDataComponentXML(ExcavatableCompPattern, editExcavatable);
}