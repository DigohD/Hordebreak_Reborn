import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML, InsertItemComponentXML, ItemComponentGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let ConsumableCompPattern : ItemComponentGenConfig = {
	xsi: "ConsumableItemComponentData",
	tagHierarchy: [
		{name: "buff", default: "HealthGain/HealthLoss"},
		{name: "amount", default: "1"},
		{name: "effectRef", default: "effect_datadef_id"},
	]
}

export function ConsumableCompGen(editConsumable: TextEditorEdit){
	InsertItemComponentXML(ConsumableCompPattern, editConsumable);
}