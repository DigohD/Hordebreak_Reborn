import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let BuildingAddonCompPattern : DataDefGenConfig = {
	xsi: "BuildingAddonComponentData",
	tagHierarchy: [
		{name: "addonRefs", default: "\n\t\t\t\t\t<addonRef>building_addon_id</addonRef>\n\t\t\t\t"},
	]
}

export function BuildingAddonCompGen(editBuildingAddon: TextEditorEdit){
	InsertDataComponentXML(BuildingAddonCompPattern, editBuildingAddon);
}