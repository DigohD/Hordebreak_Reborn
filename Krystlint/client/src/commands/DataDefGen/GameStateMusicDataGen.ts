import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let GameStateMusicDataPattern : DataDefGenConfig = {
	xsi: "GameStateMusicData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "music", default: "\n\t\t\t<sfxRef>music_sfx_datadef_id</sfxRef>\n\t\t"},
		{name: "ambience", default: "\n\t\t\t<sfxRef>ambience_sfx_datadef_id</sfxRef>\n\t\t"},
	]
}

export function GameStateMusicDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(GameStateMusicDataPattern, editBuilder);
}