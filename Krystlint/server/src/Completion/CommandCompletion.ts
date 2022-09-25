import {IsCaretWithinRefTag, IsCaretWithinDataComponentList, IsCaretWithinItemComponentList, IsCaretWithinDataDef} from '../utils/CaretUtils';
import { Position, CompletionItemKind, Command, CompletionItem } from 'vscode-languageserver';
import { IndexBankType } from '../server';
import { stringify } from 'querystring';

interface CommandConfig {
	command: string;
	completionText: string;
	arg: string[];
}

export const KrystDataDefCommandConfigs : CommandConfig[] = [
	// DataDef Generation Commands
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Ambience",
		arg: ["Ambience"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Atmosphere",
		arg: ["Atmosphere"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:BuildingCategory",
		arg: ["BuildingCategory"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Building",
		arg: ["Building"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Burnable",
		arg: ["Burnable"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Color",
		arg: ["Color"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:CraftingRecipe",
		arg: ["CraftingRecipe"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:DamageType",
		arg: ["DamageType"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:DefenseType",
		arg: ["DefenseType"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:EdgeObject",
		arg: ["EdgeObject"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Effect",
		arg: ["Effect"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Effect:Projectile",
		arg: ["Effect", "Projectile"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:EnemyMesh",
		arg: ["EnemyMesh"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Environment",
		arg: ["Environment"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:FNEEntityMesh",
		arg: ["FNEEntityMesh"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:FNEEntityView",
		arg: ["FNEEntityView"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:GameStateMusic",
		arg: ["GameStateMusic"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Item",
		arg: ["Item"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:MountedObject",
		arg: ["MountedObject"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Music",
		arg: ["Music"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:PlayerMesh",
		arg: ["PlayerMesh"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Quest:BuildingAddon",
		arg: ["Quest", "BuildingAddon"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Quest:Building",
		arg: ["Quest", "Building"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Quest:Crafting",
		arg: ["Quest", "Crafting"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Quest:ExcavateItems",
		arg: ["Quest", "ExcavateItems"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Quest:HarvestCrop",
		arg: ["Quest", "HarvestCrop"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Quest:Refinement",
		arg: ["Quest", "Refinement"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Quest:ConstructRoom",
		arg: ["Quest", "ConstructRoom"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:RefinementRecipe",
		arg: ["RefinementRecipe"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:RoomProperty",
		arg: ["RoomProperty"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:RoomResource",
		arg: ["RoomResource"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:SFX",
		arg: ["SFX"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Sprite",
		arg: ["Sprite"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Site",
		arg: ["Site"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:String",
		arg: ["String"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:Tile",
		arg: ["Tile"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:TileObject",
		arg: ["TileObject"]
	},
	{
		command: "KrystLint.DataDefGen",
		completionText: "KrystGen:DataDef:VFX",
		arg: ["VFX"]
	},
]

export const KrystComponentDataCommandConfigs : CommandConfig[] = [
	// Data Component Generation Commands
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:Builder",
		arg: ["Builder"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:BuildingAddon",
		arg: ["BuildingAddon"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:Consumer",
		arg: ["Consumer"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:Crafting",
		arg: ["Crafting"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:Crop",
		arg: ["Crop"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:Door",
		arg: ["Door"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:Environment",
		arg: ["Environment"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:Excavatable",
		arg: ["Excavatable"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:Inventory",
		arg: ["Inventory"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:ItemTransfer",
		arg: ["ItemTransfer"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:Producer",
		arg: ["Producer"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:Refinement",
		arg: ["Refinement"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:RoomRequirements",
		arg: ["RoomRequirements"]
	},
	{
		command: "KrystLint.ComponentDataGen",
		completionText: "KrystGen:ComponentData:Stat",
		arg: ["Stat"]
	},
]

export const KrystItemComponentCommandConfigs : CommandConfig[] = [
	// Item Component Generation Commands
	{
		command: "KrystLint.ItemComponentGen",
		completionText: "KrystGen:ItemComponent:Burnable",
		arg: ["Burnable"]
	},
	{
		command: "KrystLint.ItemComponentGen",
		completionText: "KrystGen:ItemComponent:Clothing",
		arg: ["Clothing"]
	},
	{
		command: "KrystLint.ItemComponentGen",
		completionText: "KrystGen:ItemComponent:Consumable",
		arg: ["Consumable"]
	},
	{
		command: "KrystLint.ItemComponentGen",
		completionText: "KrystGen:ItemComponent:Fuel",
		arg: ["Fuel"]
	},
	{
		command: "KrystLint.ItemComponentGen",
		completionText: "KrystGen:ItemComponent:Weapon",
		arg: ["Weapon"]
	},
	{
		command: "KrystLint.ItemComponentGen",
		completionText: "KrystGen:ItemComponent:WeaponMod",
		arg: ["WeaponMod"]
	},
]

export function GetCommandCompletionList(
	text: string,
	caretPos: Position,
) : CompletionItem[]{
	let Allcommands : CompletionItem[] = [];

	let IsWithinDataDef = IsCaretWithinDataDef(
		text,
		caretPos.line,
		caretPos.character
	);

	if(!IsWithinDataDef){
		Allcommands = KrystDataDefCommandConfigs.map((command, index) => {
			return {
				label: command.completionText,
				kind: CompletionItemKind.Snippet,
				data: "DataDef::" + index,
				insertText: "\t",
				command: {
					title: command.completionText, 
					command: command.command,
					arguments: command.arg
				}
			};
		})
	}

	let isWithinDataComponentList = IsCaretWithinDataComponentList(
		text,
		caretPos.line,
		caretPos.character
	);

	if(isWithinDataComponentList){
		let allComponentDataCommands : CompletionItem[] = KrystComponentDataCommandConfigs.map((command, index) => {
			return {
				label: command.completionText,
				kind: CompletionItemKind.Snippet,
				data: "ComponentData::" + index,
				insertText: "\t",
				command: {
					title: command.completionText, 
					command: command.command,
					arguments: command.arg
				}
			};
		})
		Allcommands = [...Allcommands, ...allComponentDataCommands];
	}

	let isWithinItemComponentList = IsCaretWithinItemComponentList(
		text,
		caretPos.line,
		caretPos.character
	);

	if(isWithinItemComponentList){
		let allItemComponentCommands : CompletionItem[] = KrystItemComponentCommandConfigs.map((command, index) => {
			return {
				label: command.completionText,
				kind: CompletionItemKind.Snippet,
				data: "ItemComponent::" + index,
				insertText: "\t",
				command: {
					title: command.completionText, 
					command: command.command,
					arguments: command.arg
				}
			};
		})
		Allcommands = [...Allcommands, ...allItemComponentCommands];
	}

	return Allcommands;
}