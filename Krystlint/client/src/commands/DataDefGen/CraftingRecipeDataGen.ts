import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let CraftingRecipeDataPattern : DataDefGenConfig = {
	xsi: "CraftingRecipeData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "productRef", default: "item_datadef_id"},
		{name: "productAmount", default: "1"},
		{name: "craftSFXRef", default: "sfx_datadef_id"},
		{name: "requiredMaterials", default: "\n\t\t\t<MaterialDef>\n\t\t\t\t<itemRef>item_datadef_id</itemRef>\n\t\t\t\t<amount>1</amount>\n\t\t\t</MaterialDef>\n\t\t"},
	]
}

export function CraftingRecipeDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(CraftingRecipeDataPattern, editBuilder);
}