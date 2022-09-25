import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let RefinementRecipeDataPattern : DataDefGenConfig = {
	xsi: "RefinementRecipeData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "processNameRef", default: "string_datadef_id"},
		{name: "processDescriptionRef", default: "string_datadef_id"},
		{name: "processIconRef", default: "sprite_datadef_id"},
		{name: "processTime", default: "15"},
		{name: "requiredMaterials", default: "\n\t\t\t<MaterialDef>\n\t\t\t\t<itemRef>item_datadef_id</itemRef>\n\t\t\t\t<amount>1</amount>\n\t\t\t</MaterialDef>\n\t\t"},
		{name: "producedMaterials", default: "\n\t\t\t<MaterialDef>\n\t\t\t\t<itemRef>item_datadef_id</itemRef>\n\t\t\t\t<amount>1</amount>\n\t\t\t</MaterialDef>\n\t\t"},
	]
}

export function RefinementRecipeDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(RefinementRecipeDataPattern, editBuilder);
}