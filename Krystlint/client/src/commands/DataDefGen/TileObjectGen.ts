import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let TileObjectPattern : DataDefGenConfig = {
	xsi: "FNEEntityData",
	entityType: "TileObject",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "blocking", default: "true"},
		{name: "smallCollisionBox", default: "false"},
		{name: "pathingCost", default: "100"},
		{name: "seeThrough", default: "true"},
		{name: "hittable", default: "true"},
		{name: "editorName", default: "editor_label"},
		{name: "editorCategoryName", default: "editor_category"},
		{name: "components", default: "\n\n\t\t"},
		{name: "entityViewVariations", default: "\n\t\t\t<viewRef>entity_view_id</viewRef>\n\t\t"},
	]
}

export function TileObjectGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(TileObjectPattern, editBuilder);
}