import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML, InsertItemComponentXML, ItemComponentGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let ClothingCompPattern : ItemComponentGenConfig = {
	xsi: "ClothingItemComponentData",
	tagHierarchy: [
		{name: "itemMeshRef", default: "fneentitymesh_datadef_id"},
		{name: "statModList", default: "\n\t\t\t\t\t<StatModData>\n\t\t\t\t\t\t<modType>MaxHealth/MaxShields/Armor</modType>\n\t\t\t\t\t\t<amount>1</amount>\n\t\t\t\t\t</StatModData>\n\t\t\t\t"},
		{name: "triggersPerMinute", default: "100"},
		{name: "isAutequipmentTypeomatic", default: "Head/Torso/Legs/Waist/Hands/Back/Feet"},
	]
}

export function ClothingCompGen(editClothing: TextEditorEdit){
	InsertItemComponentXML(ClothingCompPattern, editClothing);
}