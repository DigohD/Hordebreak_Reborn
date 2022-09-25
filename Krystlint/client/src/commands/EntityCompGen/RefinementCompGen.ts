import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig, InsertDataComponentXML } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let RefinementCompPattern : DataDefGenConfig = {
	xsi: "RefinementComponentData",
	tagHierarchy: [
		{name: "recipes", default: "\n\t\t\t\t\t<recipeRef>refinement_recipe_datadef_id</recipeRef>\n\t\t\t\t"},
		{name: "burnGradeRef", default: "burnable_grade_datadef_id"},
		{name: "startSFXRef", default: "sfx_datadef_id"},
		{name: "stopSFXRef", default: "sfx_datadef_id"},
		{name: "activeSFXLoopRef", default: "sfx_datadef_id"},
	]
}

export function RefinementCompGen(editRefinement: TextEditorEdit){
	InsertDataComponentXML(RefinementCompPattern, editRefinement);
}