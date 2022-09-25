import { TextEditorEdit } from 'vscode';
import { TileObjectGen } from './TileObjectGen';
import { EdgeObjectGen } from './EdgeObjectGen';
import { AmbienceDataGen } from './AmbienceDataGen';
import { AtmosphereDataGen } from './AtmosphereDataGen';
import { BuildingCategoryDataGen } from './BuildingCategoryDataGen';
import { BuildingDataGen } from './BuildingDataGen';
import { BurnableDataGen } from './BurnableDataGen';
import { ColorDataGen } from './ColorDataGen';
import { CraftingRecipeDataGen } from './CraftingRecipeDataGen';
import { DamageTypeDataGen } from './DamageTypeDataGen';
import { DefenseTypeDataGen } from './DefenseTypeDataGen';
import { EffectDataGen } from './EffectDataGen';
import { EnemyMeshDataGen } from './EnemyMeshDataGen';
import { EnvironmentDataGen } from './EnvironmentDataGen';
import { FNEEntityMeshDataGen } from './FNEEntityMeshDataGen';
import { FNEEntityViewDataGen } from './FNEEntityViewDataGen';
import { GameStateMusicDataGen } from './GameStateMusicDataGen';
import { ItemDataGen } from './ItemDataGen';
import { MountedObjectDataGen } from './MountedObjectDataGen';
import { MusicDataGen } from './MusicDataGen';
import { PlayerMeshDataGen } from './PlayerMeshDataGen';
import { QuestDataGen } from './QuestDataGen';
import { RefinementRecipeDataGen } from './RefinementRecipeDataGen';
import { RoomPropertyDataGen } from './RoomPropertyDataGen';
import { RoomResourceDataGen } from './RoomResourceDataGen';
import { SFXDataGen } from './SFXDataGen';
import { SpriteDataGen } from './SpriteDataGen';
import { StringDataGen } from './StringDataGen';
import { TileDataGen } from './TileDataGen';
import { VFXDataGen } from './VFXDataGen';
import { SiteDataGen } from './SiteDataGen';

export function DataDefGenCommand(editBuilder: TextEditorEdit, args: any){
	switch(args[0]){
		case "Ambience":
			AmbienceDataGen(editBuilder)
			break;
		case "Atmosphere":
			AtmosphereDataGen(editBuilder)
			break;
		case "BuildingCategory":
			BuildingCategoryDataGen(editBuilder)
			break;
		case "Building":
			BuildingDataGen(editBuilder)
			break;
		case "Burnable":
			BurnableDataGen(editBuilder)
			break;
		case "Color":
			ColorDataGen(editBuilder)
			break;
		case "CraftingRecipe":
			CraftingRecipeDataGen(editBuilder)
			break;
		case "DamageType":
			DamageTypeDataGen(editBuilder)
			break;
		case "DefenseType":
			DefenseTypeDataGen(editBuilder)
			break;
		case "EdgeObject":
			EdgeObjectGen(editBuilder)
			break;
		case "Effect":
			EffectDataGen(editBuilder, args)
			break;
		case "EnemyMesh":
			EnemyMeshDataGen(editBuilder)
			break;
		case "Environment":
			EnvironmentDataGen(editBuilder)
			break;
		case "FNEEntityMesh":
			FNEEntityMeshDataGen(editBuilder)
			break;
		case "FNEEntityView":
			FNEEntityViewDataGen(editBuilder)
			break;
		case "GameStateMusic":
			GameStateMusicDataGen(editBuilder)
			break;
		case "Item":
			ItemDataGen(editBuilder)
			break;
		case "MountedObject":
			MountedObjectDataGen(editBuilder)
			break;
		case "Music":
			MusicDataGen(editBuilder)
			break;
		case "PlayerMesh":
			PlayerMeshDataGen(editBuilder)
			break;
		case "Quest":
			QuestDataGen(editBuilder, args)
			break;
		case "RefinementRecipe":
			RefinementRecipeDataGen(editBuilder)
			break;
		case "RoomProperty":
			RoomPropertyDataGen(editBuilder)
			break;
		case "RoomResource":
			RoomResourceDataGen(editBuilder)
			break;
		case "SFX":
			SFXDataGen(editBuilder)
			break;
		case "Site":
			SiteDataGen(editBuilder)
			break;
		case "Sprite":
			SpriteDataGen(editBuilder)
			break;
		case "String":
			StringDataGen(editBuilder)
			break;
		case "Tile":
			TileDataGen(editBuilder)
			break;
		case "TileObject":
			TileObjectGen(editBuilder)
			break;
		case "VFX":
			VFXDataGen(editBuilder)
			break;
	}
}