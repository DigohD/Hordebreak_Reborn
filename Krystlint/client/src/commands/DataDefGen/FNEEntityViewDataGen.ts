import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let FNEEntityViewDataPattern : DataDefGenConfig = {
	xsi: "FNEEntityViewData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "viewIsGameObject", default: "true"},
		{name: "entityMeshData", default: "entity_mesh_datadef_id"},
		{name: "scaleMod", default: "1.0"},
		{name: "emissiveColor", default: "#FFFFFF"},
		{name: "isTransparent", default: "false"},
		{name: "onHitEffectRef", default: "effect_datadef_id"},
		{name: "onDeathEffectRef", default: "effect_datadef_id"},
		{name: "entityLightSourceData", default: "\n\t\t\t<intensity>1.0</intensity>\n\t\t\t<range>1.0</range>\n\t\t\t<color>#FFFFFF</color>\n\t\t\t<offsetX>0.0</offsetX>\n\t\t\t<offsetY>1.0</offsetY>\n\t\t\t<offsetZ>1.0</offsetZ>\n\t\t"},
		{name: "entityVfxData", default: "\n\t\t\t<vfxRef>vfx_datadef_id</vfxRef>\n\t\t\t<alwaysOn>false</alwaysOn>\n\t\t\t<scaleMod>1.0</scaleMod>\n\t\t\t<offsetX>0.0</offsetX>\n\t\t\t<offsetY>1.0</offsetY>\n\t\t\t<offsetZ>1.0</offsetZ>\n\t\t"},
		{name: "animations", default: "\n\t\t\t<AnimationData>\n\t\t\t\t<animPath>building_datadef_id</animPath>\n\t\t\t</AnimationData>\n\t\t"},
	]
}

export function FNEEntityViewDataGen(editBuilder: TextEditorEdit){
	InsertDataDefXML(FNEEntityViewDataPattern, editBuilder);
}