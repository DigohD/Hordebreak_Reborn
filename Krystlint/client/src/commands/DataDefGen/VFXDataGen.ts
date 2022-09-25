import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let VFXDataPattern : DataDefGenConfig = {
	xsi: "VFXData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "prefabPath", default: "NOT_USED"},
		{name: "assetBundlePath", default: "file_path"},
		{name: "assetBundleName", default: "name"},
		{name: "effectName", default: "name"},
		{name: "heightPosition", default: "1.0"},
		{name: "effectScale", default: "1.0"},
		{name: "lifetime", default: "1.0"},
	]
}

export function VFXDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(VFXDataPattern, editBuilder);
}