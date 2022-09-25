import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let EnemyMeshDataPattern : DataDefGenConfig = {
	xsi: "FNEEntityMeshData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "path", default: "file_path"},
		{name: "albedoPath", default: "file_path"},
		{name: "maskMapPath", default: "file_path"},
		{name: "normalPath", default: "file_path"},
		{name: "emissivePath", default: "file_path"},
	]
}

export function EnemyMeshDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(EnemyMeshDataPattern, editBuilder);
}