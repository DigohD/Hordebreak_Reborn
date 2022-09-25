import { TextEditorEdit } from 'vscode';
import { BurnableCompGen } from './BurnableCompGen';
import { WeaponCompGen } from './WeaponCompGen';
import { ClothingCompGen } from './ClothingCompGen';
import { ConsumableCompGen } from './ConsumableCompGen';
import { FuelCompGen } from './FuelCompGen';
import { WeaponModCompGen } from './WeaponModCompGen';
export function ItemComponentGenCommand(editBuilder: TextEditorEdit, args: any){
	switch(args[0]){
		case "Burnable":
			BurnableCompGen(editBuilder)
			break;
		case "Clothing":
			ClothingCompGen(editBuilder)
			break;
		case "Consumable":
			ConsumableCompGen(editBuilder)
			break;
		case "Consumable":
			ConsumableCompGen(editBuilder)
			break;
		case "Fuel":
			FuelCompGen(editBuilder)
			break;
		case "Weapon":
			WeaponCompGen(editBuilder)
			break;
		case "WeaponMod":
			WeaponModCompGen(editBuilder)
			break;
	}
}