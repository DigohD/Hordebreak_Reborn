import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML, InsertItemComponentXML, ItemComponentGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let BurnableCompPattern : ItemComponentGenConfig = {
	xsi: "BurnableItemComponentData",
	tagHierarchy: [
		{name: "gradeRef", default: "burnable_grade_datadef_id"},
		{name: "burnTime", default: "15"}
	]
}

export function BurnableCompGen(editBurnable: TextEditorEdit){
	InsertItemComponentXML(BurnableCompPattern, editBurnable);
}