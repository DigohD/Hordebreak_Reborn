import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML, InsertItemComponentXML, ItemComponentGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let WeaponModCompPattern : ItemComponentGenConfig = {
	xsi: "WeaponModItemComponentData",
	tagHierarchy: [
		{name: "modColor", default: "White/Black/Green/Yellow/Red/Blue/Brown"},
		{name: "modBuffs", default: "\n\t\t\t\t\t<ModBuff>\n\t\t\t\t\t\t<buffType>Damage/FireRate/ClipSize/ReloadTime/ProjectileSpeed</buffType>\n\t\t\t\t\t\t<amount>1.0</amount>\n\t\t\t\t\t</ModBuff>\n\t\t\t\t"},
	]
}

export function WeaponModCompGen(editWeaponMod: TextEditorEdit){
	InsertItemComponentXML(WeaponModCompPattern, editWeaponMod);
}