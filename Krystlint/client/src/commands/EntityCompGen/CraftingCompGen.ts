import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let CraftingCompPattern : DataDefGenConfig = {
	xsi: "CraftingComponentData",
	tagHierarchy: [
		{name: "recipes", default: "\n\t\t\t\t\t<recipeRef>recipe_datadef_id</recipeRef>\n\t\t\t\t"},
	]
}

export function CraftingCompGen(editCrafting: TextEditorEdit){
	InsertDataComponentXML(CraftingCompPattern, editCrafting);
}