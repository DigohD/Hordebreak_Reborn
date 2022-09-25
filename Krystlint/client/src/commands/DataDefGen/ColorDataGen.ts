import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let ColorDataPattern : DataDefGenConfig = {
	xsi: "ColorData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "colorCode", default: "#FFFFFF"},
	]
}

export function ColorDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(ColorDataPattern, editBuilder);
}