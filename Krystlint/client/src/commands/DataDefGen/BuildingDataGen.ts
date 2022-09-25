import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let BuildingDataPattern : DataDefGenConfig = {
	xsi: "BuildingData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "productRef", default: "item_datadef_id"},
		{name: "categoryRef", default: "category_datadef_id"},
		{name: "nameRef", default: "string_datadef_id"},
		{name: "descriptionRef", default: "string_datadef_id"},
		{name: "iconRef", default: "icon_datadef_id"},
		{name: "isWallAddon", default: "false"},
		{name: "requiredMaterials", default: "\n\t\t\t<MaterialDef>\n\t\t\t\t<itemRef>item_datadef_id</itemRef>\n\t\t\t\t<amount>1</amount>\n\t\t\t</MaterialDef>\n\t\t"},
		{name: "unlockRefs", default: "\n\t\t\t<unlockRef>building_datadef_id</unlockRef>\n\t\t"},
	]
}

export function BuildingDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(BuildingDataPattern, editBuilder);
}