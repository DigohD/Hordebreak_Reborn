import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML, InsertItemComponentXML, ItemComponentGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let WeaponCompPattern : ItemComponentGenConfig = {
	xsi: "WeaponItemComponentData",
	tagHierarchy: [
		{name: "itemMeshRef", default: "fneentitymesh_datadef_id"},
		{name: "statModList", default: "\n\t\t\t\t\t<StatModData>\n\t\t\t\t\t\t<modType>MaxHealth/MaxShields/Armor</modType>\n\t\t\t\t\t\t<amount>1</amount>\n\t\t\t\t\t</StatModData>\n\t\t\t\t"},
		{name: "triggersPerMinute", default: "100"},
		{name: "isAutomatic", default: "false"},
		{name: "reloadTimeInSeconds", default: "1.0"},
		{name: "soundRange", default: "15"},
		{name: "ammoClipSize", default: "10"},
		{name: "reloadEffectRef", default: "effect_datadef_id"},
		{name: "effectRef", default: "effect_datadef_id"},
		{name: "weaponPosture", default: "Rifle/Heavy/Light"},
		{name: "muzzleOffsetForward", default: "0.0"},
		{name: "muzzleOffsetRight", default: "0.0"},
		{name: "muzzleOffsetUp", default: "0.0"},
		{name: "scaleMod", default: "1.0"},
		{name: "iconOffsetRight", default: "0.0"},
		{name: "iconOffsetUp", default: "0.0"},
		{name: "iconScaleMod", default: "1.0"},
	]
}

export function WeaponCompGen(editWeapon: TextEditorEdit){
	InsertItemComponentXML(WeaponCompPattern, editWeapon);
}