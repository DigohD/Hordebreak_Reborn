import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let BuildingCategoryDataPattern : DataDefGenConfig = {
	xsi: "BuildingCategoryData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "nameRef", default: "datadef_id"},
		{name: "iconRef", default: "datadef_id"},
		{name: "preferredIndex", default: "0"}
	]
}

export function BuildingCategoryDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(BuildingCategoryDataPattern, editBuilder);
}