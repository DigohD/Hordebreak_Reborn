import { TextEditorEdit } from 'vscode';
import { BuilderCompGen } from './BuilderCompGen';
import { BuildingAddonCompGen } from './BuildingAddonCompGen';
import { ConsumerCompGen } from './ConsumerCompGen';
import { CraftingCompGen } from './CraftingCompGen';
import { CropCompGen } from './CropCompGen';
import { DoorCompGen } from './DoorCompGen';
import { EnvironmentCompGen } from './EnvironmentCompGen';
import { ExcavatableCompGen } from './ExcavatableCompGen';
import { InventoryCompGen } from './InventoryCompGen';
import { ItemTransferCompGen } from './ItemTransferCompGen';
import { ProducerCompGen } from './ProducerCompGen';
import { RefinementCompGen } from './RefinementCompGen';
import { RoomRequirementsCompGen } from './RoomRequirementsCompGen';
import { StatCompGen } from './StatCompGen';

export function ComponentDataGenCommand(editBuilder: TextEditorEdit, args: any){
	switch(args[0]){
		case "Builder":
			BuilderCompGen(editBuilder)
			break;
		case "BuildingAddon":
			BuildingAddonCompGen(editBuilder)
			break;
		case "Consumer":
			ConsumerCompGen(editBuilder)
			break;
		case "Crafting":
			CraftingCompGen(editBuilder)
			break;
		case "Crop":
			CropCompGen(editBuilder)
			break;
		case "Door":
			DoorCompGen(editBuilder)
			break;
		case "Environment":
			EnvironmentCompGen(editBuilder)
			break;
		case "Excavatable":
			ExcavatableCompGen(editBuilder)
			break;
		case "Inventory":
			InventoryCompGen(editBuilder)
			break;
		case "ItemTransfer":
			ItemTransferCompGen(editBuilder)
			break;
		case "Producer":
			ProducerCompGen(editBuilder)
			break;
		case "Refinement":
			RefinementCompGen(editBuilder)
			break;
		case "RoomRequirements":
			RoomRequirementsCompGen(editBuilder)
			break;
		case "Stat":
			StatCompGen(editBuilder)
			break;
	}
}