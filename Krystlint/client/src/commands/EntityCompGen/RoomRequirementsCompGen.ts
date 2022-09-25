import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let RoomRequirementsCompPattern : DataDefGenConfig = {
	xsi: "RoomRequirementsComponentData",
	tagHierarchy: [
		{name: "unsatisfiedMod", default: "0.1"},
		{name: "propertyRequirements", default: "\n\t\t\t\t\t<RoomPropertyRequirementData>\n\t\t\t\t\t\t<propertyRef>property_datadef_id</propertyRef>\n\t\t\t\t\t\t<level>1</level>\n\t\t\t\t\t</RoomPropertyRequirementData>\n\t\t\t\t"},
	]
}

export function RoomRequirementsCompGen(editRoomRequirements: TextEditorEdit){
	InsertDataComponentXML(RoomRequirementsCompPattern, editRoomRequirements);
}