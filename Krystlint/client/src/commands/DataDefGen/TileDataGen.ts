import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let TileDataPattern : DataDefGenConfig = {
	xsi: "TileData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "category", default: "category"},
		{name: "albedopath", default: "file_path"},
		{name: "normalpath", default: "file_path"},
		{name: "maskmappath", default: "file_path"},
		{name: "emissivepath", default: "file_path"},
		{name: "textureSheet", default: "NOT_USED"},
		{name: "textureSheetIndex", default: "NOT_USED"},
		{name: "chanceToSpawnTileObject", default: "10"},
		{name: "hardEdges", default: "false"},
		{name: "overlapPriority", default: "1"},
		{name: "tileObjectGenDataList", default: "\n\t\t\t<TileObjectGenData>\n\t\t\t\t<objectRef>entity_datadef_id</objectRef>\n\t\t\t\t<weight>1</weight>\n\t\t\t\t<tileObjectGenClusterData>\n\t\t\t\t\t<radius>1.0</radius>\n\t\t\t\t\t<density>10</density>\n\t\t\t\t</tileObjectGenClusterData>\n\t\t\t</TileObjectGenData>\n\t\t"},
		{name: "timeEffectFrequency", default: "5.0"},
		{name: "timeEffects", default: "\n\t\t\t<TileTimeEffectData>\n\t\t\t\t<effectRef>effect_datadef_id</effectRef>\n\t\t\t\t<weight>1</weight>\n\t\t\t\t<centerTime>12</centerTime>\n\t\t\t\t<timeOffset>10</timeOffset>\n\t\t\t</TileTimeEffectData>\n\t\t"},
		{name: "mapColor", default: "#FFFFFF"},
		{name: "roomPropertyRefs", default: "\n\t\t\t<propertyRef>property_datadef_id</propertyRef>\n\t\t"},
		{name: "atmosphereRef", default: "atmosphere_datadef_id"},
	]
}

export function TileDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(TileDataPattern, editBuilder);
}