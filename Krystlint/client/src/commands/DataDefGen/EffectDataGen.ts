import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let EffectDataPattern : DataDefGenConfig = {
	xsi: "EffectData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "delay", default: "0.0"},
		{name: "repetitions", default: "0"},
		{name: "repetitionTime", default: "0.0"},
		{name: "screenShake", default: "0.0"},
		{name: "vfxRef", default: "vfx_datadef_id"},
		{name: "sfxRef", default: "sfx_datadef_id"},
		{name: "onBirthEffectRef", default: "effect_datadef_id"},
	]
}

let ProjectileEffectDataPattern : DataDefGenConfig = {
	xsi: "EffectData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "delay", default: "0.0"},
		{name: "repetitions", default: "0"},
		{name: "repetitionTime", default: "0.0"},
		{name: "screenShake", default: "0.0"},
		{name: "vfxRef", default: "vfx_datadef_id"},
		{name: "sfxRef", default: "sfx_datadef_id"},
		{name: "onBirthEffectRef", default: "effect_datadef_id"},
		{name: "realEffectData", xsi: "ProjectileEffectData", default: "\n\t\t\t<lifetime>1.0</lifetime>\n\t\t\t<speed>10.0</speed>\n\t\t\t<pellets>1</pellets>\n\t\t\t<damage>5.0</damage>\n\t\t\t<healing>0.0</healing>\n\t\t\t<inaccuracy>5.0</inaccuracy>\n\t\t\t<invertDirection>false</invertDirection>\n\t\t\t<targetsPlayers>false</targetsPlayers>\n\t\t\t<targetsEnemies>true</targetsEnemies>\n\t\t\t<projectileVfxRef>vfx_datadef_id</projectileVfxRef>\n\t\t\t<onDeathEffectRef>effect_datadef_id</onDeathEffectRef>\n\t\t\t<onHitEffectRef>effect_datadef_id</onHitEffectRef>\n\t\t\t<damageTypeRef>damage_type_Datadef_id</damageTypeRef>\n\t\t", },
	]
}

export function EffectDataGen(editBuilder: TextEditorEdit, args: string[]){
	if(!args[1]){
		InsertDataDefXML(EffectDataPattern, editBuilder);
	}
	else if(args[1] == "Projectile"){
		InsertDataDefXML(ProjectileEffectDataPattern, editBuilder);
	}
}