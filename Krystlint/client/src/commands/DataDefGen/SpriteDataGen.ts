import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let SpriteDataPattern : DataDefGenConfig = {
	xsi: "SpriteData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "spritePath", default: "file_path"},
		{name: "spriteCategory", default: "default"},
	]
}

export function SpriteDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(SpriteDataPattern, editBuilder);
}