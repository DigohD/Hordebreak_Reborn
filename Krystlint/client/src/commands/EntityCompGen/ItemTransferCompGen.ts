import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let ItemTransferCompPattern : DataDefGenConfig = {
	xsi: "ItemTransferComponentData",
	tagHierarchy: [
		{name: "transferIntervalTicks", default: "15"},
	]
}

export function ItemTransferCompGen(editItemTransfer: TextEditorEdit){
	InsertDataComponentXML(ItemTransferCompPattern, editItemTransfer);
}