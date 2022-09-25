import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let CropCompPattern : DataDefGenConfig = {
	xsi: "CropComponentData",
	tagHierarchy: [
		{name: "consumedOnHarvest", default: "false"},
		{name: "transformsOnHarvest", default: "false"},
		{name: "transformedEntityRef", default: "entity_datadef_id"},
		{name: "growthTimeTicks", default: "1000"},
		{name: "matureEntityRef", default: "entity_datadef_id"},
		{name: "harvestableViewRef", default: "entity_view_datadef_id"},
		{name: "harvestEffectRef", default: "effect_datadef_id"},
		{name: "environmentSpans", default: "\n\t\t\t\t\t<EnvironmentSpanData>\n\t\t\t\t\t\t<environmentRef>environment_datadef_id</environmentRef>\n\t\t\t\t\t\t<lowPoint>5.0</lowPoint>\n\t\t\t\t\t\t<highPoint>5.0</highPoint>\n\t\t\t\t\t</EnvironmentSpanData>\n\t\t\t\t"},
		{name: "produceLootTable", default: "\n\t\t\t\t\t<minRolls>0</minRolls>\n\t\t\t\t\t<maxRolls>0</maxRolls>\n\t\t\t\t\t<table>\n\t\t\t\t\t\t<LootEntry>\n\t\t\t\t\t\t\t<itemRef>item_datadef_id</itemRef>\n\t\t\t\t\t\t\t<probability>10</probability>\n\t\t\t\t\t\t\t<guaranteed>false</guaranteed>\n\t\t\t\t\t\t\t<unique>false</unique>\n\t\t\t\t\t\t</LootEntry>\n\t\t\t\t\t</table>\n\t\t\t\t"},
	]
}

export function CropCompGen(editCrop: TextEditorEdit){
	InsertDataComponentXML(CropCompPattern, editCrop);
}