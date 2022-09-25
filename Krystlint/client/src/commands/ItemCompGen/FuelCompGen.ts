import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML, InsertItemComponentXML, ItemComponentGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let FuelCompPattern : ItemComponentGenConfig = {
	xsi: "FuelItemComponentData",
	tagHierarchy: [
		{name: "fuelValue", default: "1"},
	]
}

export function FuelCompGen(editFuel: TextEditorEdit){
	InsertItemComponentXML(FuelCompPattern, editFuel);
}