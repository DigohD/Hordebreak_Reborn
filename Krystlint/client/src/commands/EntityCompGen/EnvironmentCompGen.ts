import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let EnvironmentCompPattern : DataDefGenConfig = {
	xsi: "EnvironmentComponentData",
	tagHierarchy: [
		{name: "environment", default: "\n\t\t\t\t\t<EnvironmentEffectData>\n\t\t\t\t\t\t<typeRef>environment_datadef_id</typeRef>\n\t\t\t\t\t\t<amount>1</amount>\n\t\t\t\t\t</EnvironmentEffectData>\n\t\t\t\t"},
	]
}

export function EnvironmentCompGen(editEnvironment: TextEditorEdit){
	InsertDataComponentXML(EnvironmentCompPattern, editEnvironment);
}