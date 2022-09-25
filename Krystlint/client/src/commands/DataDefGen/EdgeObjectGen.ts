import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let EdgeObjectPattern : DataDefGenConfig = {
	xsi: "FNEEntityData",
	entityType: "EdgeObject",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "blocking", default: "true"},
		{name: "smallCollisionBox", default: "false"},
		{name: "pathingCost", default: "100"},
		{name: "seeThrough", default: "true"},
		{name: "hittable", default: "true"},
		{name: "editorName", default: "editor_label"},
		{name: "isMountable", default: "false"},
		{name: "editorCategoryName", default: "editor_category"},
		{name: "components", default: "\n\n\t\t"},
		{name: "entityViewVariations", default: "\n\t\t\t<viewRef>entity_view_id</viewRef>\n\t\t"},
		{name: "roomPropertyRefs", default: "\n\t\t\t<propertyRef>property_datadef_id</propertyRef>\n\t\t"}
	]
}

export function EdgeObjectGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(EdgeObjectPattern, editBuilder);
}