import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let AtmosphereDataPattern : DataDefGenConfig = {
	xsi: "AtmosphereData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "sfxList", default: "\n\t\t\t<AtmosphereSFXData>\n\t\t\t\t<sfxRefs>\n\t\t\t\t\t<sfxRef>sfx_datadef_id</sfxRef>\n\t\t\t\t</sfxRefs>\n\t\t\t\t<ambience>ambience_datadef_id</ambience>\n\t\t\t\t<weight>1</weight>\n\t\t\t\t<centerTime>12</centerTime>\n\t\t\t\t<timeOffset>10</timeOffset>\n\t\t\t</AtmosphereSFXData>\n\t\t"},
	]
}

export function AtmosphereDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(AtmosphereDataPattern, editBuilder);
}