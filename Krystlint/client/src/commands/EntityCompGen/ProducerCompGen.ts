import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let ProducerCompPattern : DataDefGenConfig = {
	xsi: "ProducerComponentData",
	tagHierarchy: [
		{name: "resources", default: "\n\t\t\t\t\t<ResourceProductionData>\n\t\t\t\t\t\t<resourceRef>resource_datadef_id</resourceRef>\n\t\t\t\t\t\t<amount>1</amount>\n\t\t\t\t\t</ResourceProductionData>\n\t\t\t\t"},
	]
}

export function ProducerCompGen(editProducer: TextEditorEdit){
	InsertDataComponentXML(ProducerCompPattern, editProducer);
}