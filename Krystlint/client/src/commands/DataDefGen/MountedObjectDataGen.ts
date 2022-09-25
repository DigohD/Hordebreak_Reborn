import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let MountedObjectDataPattern : DataDefGenConfig = {
	xsi: "MountedObjectData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "mayGenerateFromOutdoors", default: "false"},
		{name: "viewVariations", default: "\n\t\t\t<viewRef>entity_view_id</viewRef>\n\t\t"},
		{name: "environmentTransfers", default: "\n\t\t\t<EnvironmentEffectData>\n\t\t\t\t<typeRef>environment_type_datadef_id</typeRef>\n\t\t\t\t<amount>1</amount>\n\t\t\t</EnvironmentEffectData>\n\t\t"},
		{name: "resourceTransfers", default: "\n\t\t\t<ResourceProductionData>\n\t\t\t\t<typeRef>environment_type_datadef_id</typeRef>\n\t\t\t\t<amount>1</amount>\n\t\t\t</ResourceProductionData>\n\t\t"},
	]
}

export function MountedObjectDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(MountedObjectDataPattern, editBuilder);
}