import { TextEditorEdit, ExtensionContext, Position } from 'vscode';
import { DataDefGenConfig } from '../XMLGen/InsertXML';

import {InsertDataDefXML} from '../XMLGen/InsertXML';

let BuildingQuestDataPattern : DataDefGenConfig = {
	xsi: "QuestData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "followingQuestRef", default: "quest_datadef_id"},
		{name: "questForkRef", default: "quest_datadef_id"},
		{name: "questTypeData", xsi: "BuildingAddonQuestData", default: "\n\t\t\t<buildingRef>building_datadef_id</buildingRef>\n\t\t\t<amount>1</amount>\n\t\t"},
		{name: "buildingUnlockRefs", default: "\n\t\t\t<unlockRef>building_datadef_id</unlockRef>\n\t\t"},
	]
}

let BuildingAddonQuestDataPattern : DataDefGenConfig = {
	xsi: "QuestData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "followingQuestRef", default: "quest_datadef_id"},
		{name: "questForkRef", default: "quest_datadef_id"},
		{name: "questTypeData", xsi: "BuildingAddonQuestData", default: "\n\t\t\t<buildingRef>building_datadef_id</buildingRef>\n\t\t\t<amount>1</amount>\n\t\t"},
		{name: "buildingUnlockRefs", default: "\n\t\t\t<unlockRef>building_datadef_id</unlockRef>\n\t\t"},
	]
}

let CraftingQuestDataPattern : DataDefGenConfig = {
	xsi: "QuestData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "followingQuestRef", default: "quest_datadef_id"},
		{name: "questForkRef", default: "quest_datadef_id"},
		{name: "questTypeData", xsi: "CraftingQuestData", default: "\n\t\t\t<itemRef>item_datadef_id</itemRef>\n\t\t\t<amount>1</amount>\n\t\t"},
		{name: "buildingUnlockRefs", default: "\n\t\t\t<unlockRef>building_datadef_id</unlockRef>\n\t\t"},
	]
}

let ExcavateItemsQuestDataPattern : DataDefGenConfig = {
	xsi: "QuestData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "followingQuestRef", default: "quest_datadef_id"},
		{name: "questForkRef", default: "quest_datadef_id"},
		{name: "questTypeData", xsi: "ExcavateItemsQuestData", default: "\n\t\t\t<itemRef>item_datadef_id</itemRef>\n\t\t\t<amount>1</amount>\n\t\t"},
		{name: "buildingUnlockRefs", default: "\n\t\t\t<unlockRef>building_datadef_id</unlockRef>\n\t\t"},
	]
}

let HarvestCropQuestDataPattern : DataDefGenConfig = {
	xsi: "QuestData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "followingQuestRef", default: "quest_datadef_id"},
		{name: "questForkRef", default: "quest_datadef_id"},
		{name: "questTypeData", xsi: "HarvestCropQuestData", default: "\n\t\t\t<itemRef>item_datadef_id</itemRef>\n\t\t\t<amount>1</amount>\n\t\t"},
		{name: "buildingUnlockRefs", default: "\n\t\t\t<unlockRef>building_datadef_id</unlockRef>\n\t\t"},
	]
}

let RefinementQuestDataPattern : DataDefGenConfig = {
	xsi: "QuestData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "followingQuestRef", default: "quest_datadef_id"},
		{name: "questForkRef", default: "quest_datadef_id"},
		{name: "questTypeData", xsi: "RefinementQuestData", default: "\n\t\t\t<itemRef>item_datadef_id</itemRef>\n\t\t\t<amount>1</amount>\n\t\t"},
		{name: "buildingUnlockRefs", default: "\n\t\t\t<unlockRef>building_datadef_id</unlockRef>\n\t\t"},
	]
}

let ConstructRoomQuestDataPattern : DataDefGenConfig = {
	xsi: "QuestData",
	tagHierarchy: [
		{name: "id", default: "new_datadef_id"},
		{name: "followingQuestRef", default: "quest_datadef_id"},
		{name: "questForkRef", default: "quest_datadef_id"},
		{name: "questTypeData", xsi: "ConstructRoomQuestData", default: "\n\t\t\t<RoomQuestType>Size/Property</RoomQuestType>\n\t\t\t<width>1</width>\n\t\t\t<height>1</height>\n\t\t\t<propertyRef>property_datadef_id</propertyRef>\n\t\t\t<propertyLevel>1</propertyLevel>\n\t\t"},
		{name: "buildingUnlockRefs", default: "\n\t\t\t<unlockRef>building_datadef_id</unlockRef>\n\t\t"},
	]
}

export function QuestDataGen(editBuilder: TextEditorEdit, args: string[]){
	if(args[1] == "Building"){
		InsertDataDefXML(BuildingQuestDataPattern, editBuilder);
	}
	else if(args[1] == "BuildingAddon"){
		InsertDataDefXML(BuildingQuestDataPattern, editBuilder);
	}
	else if(args[1] == "Crafting"){
		InsertDataDefXML(CraftingQuestDataPattern, editBuilder);
	}
	else if(args[1] == "ExcavateItems"){
		InsertDataDefXML(ExcavateItemsQuestDataPattern, editBuilder);
	}
	else if(args[1] == "HarvestCrop"){
		InsertDataDefXML(HarvestCropQuestDataPattern, editBuilder);
	}
	else if(args[1] == "Refinement"){
		InsertDataDefXML(RefinementQuestDataPattern, editBuilder);
	}
	else if(args[1] == "ConstructRoom"){
		InsertDataDefXML(ConstructRoomQuestDataPattern, editBuilder);
	}
}