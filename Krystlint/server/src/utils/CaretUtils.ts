export function IsCaretWithinRefTag(text: string, caretLine: number, caretCharacter: number){
	var lines : string[] = text.split('\n');

	var beforeCaret = lines[caretLine].substr(0, caretCharacter);
	var afterCaret = lines[caretLine].substr(caretCharacter, lines[caretLine].length - caretCharacter);

	var regexStartTag = /< *(?:ref|siteRef|transformedEntityRef|recipeRef|buildingRef|viewRef|nameRef|iconRef|itemRef|productRef|descriptionRef|categoryRef|unlockRef|damageRef|vfxRef|sfxRef|defenseTypeRef|openSFXRef|closeSFXRef|enemyMeshRef|accessoryMeshRef|resourceRef|typeRef|headRef|hairRef|torsoRef|handsRef|legsRef|feetRef|burnGradeRef|startSFXRef|stopSFXRef|activeSFXLoopRef|propertyRef|hitEffectRef|deathEffectRef|harvestEffectRef|harvestableViewRef|environmentRef|matureEntityRef|pickupSoundRef|laydownSoundRef|gradeRef|craftSFXRef|infoRef|processNameRef|processDescriptionRef|processIconRef|effectRef|reloadEffectRef|projectileVfxRef|onDeathEffectRef|damageTypeRef|followingQuestRef|tileRef|objectRef|atmosphereRef|displayNameRef|absencePropertyRef) *>/gm;
	var regexEndTag = /<\/ *(?:ref|siteRef|transformedEntityRef|recipeRef|buildingRef|viewRef|nameRef|iconRef|itemRef|productRef|descriptionRef|categoryRef|unlockRef|damageRef|vfxRef|sfxRef|defenseTypeRef|openSFXRef|closeSFXRef|enemyMeshRef|accessoryMeshRef|resourceRef|typeRef|headRef|hairRef|torsoRef|handsRef|legsRef|feetRef|burnGradeRef|startSFXRef|stopSFXRef|activeSFXLoopRef|propertyRef|hitEffectRef|deathEffectRef|harvestEffectRef|harvestableViewRef|environmentRef|matureEntityRef|pickupSoundRef|laydownSoundRef|gradeRef|craftSFXRef|infoRef|processNameRef|processDescriptionRef|processIconRef|effectRef|reloadEffectRef|projectileVfxRef|onDeathEffectRef|damageTypeRef|followingQuestRef|tileRef|objectRef|atmosphereRef|displayNameRef|absencePropertyRef) *>/gm;
	
	let foundBeforeTag = regexStartTag.test(beforeCaret);
	let foundAfterTag = regexEndTag.test(afterCaret);

	if(!foundBeforeTag && caretLine > 0){
		// check previous line for start tag and no end tag
		foundBeforeTag = (regexStartTag.test(lines[caretLine - 1]) && !regexEndTag.test(lines[caretLine - 1]));
	}

	if(!foundAfterTag && caretLine < lines.length - 1){
		// check next line for end tag and no start tag
		foundAfterTag = (!regexStartTag.test(lines[caretLine + 1]) && regexEndTag.test(lines[caretLine + 1]));
	}

	return foundBeforeTag && foundAfterTag;
}

export function IsCaretWithinDataDef(
	text: string, 
	caretLine: number, 
	caretCharacter: number,
	xsi: string = ""
){
	var lines : string[] = text.split('\n');

	var beforeCaret = "";
	var afterCaret = "";

	var regexStartDataDefTag = /< *DataDef.*>/gm;
	var regexEndDataDefTag = /<\/ *DataDef *>/gm;

	let foundEndTag = false;
	let foundStartTag = false;

	let dataDefStartLineCounter = caretLine;
	while(!foundStartTag){
		beforeCaret = lines[dataDefStartLineCounter];
		foundStartTag = regexStartDataDefTag.test(beforeCaret);
		foundEndTag = regexEndDataDefTag.test(beforeCaret);
		dataDefStartLineCounter--;
		if(dataDefStartLineCounter < 0){
			return false;
		}
		if(foundEndTag){
			return false;
		}
		if(foundStartTag && xsi){
			let xsiRegex = new RegExp(xsi, "mg");
			let xsiExists = xsiRegex.test(beforeCaret);
			if(!xsiExists){
				return false;
			}
		}
	}

	let dataDefEndLineCounter = caretLine;
	while(!foundEndTag){
		afterCaret = lines[dataDefEndLineCounter];
		foundEndTag = regexEndDataDefTag.test(afterCaret);
		foundStartTag = regexStartDataDefTag.test(afterCaret);
		dataDefEndLineCounter++;
		if(dataDefEndLineCounter == lines.length){
			return false;
		}
		if(foundStartTag){
			return false;
		}
	}

	return true;
}

export function IsCaretWithinComponentsTag(
	text: string, 
	caretLine: number, 
	caretCharacter: number
){
	var lines : string[] = text.split('\n');

	var beforeCaret = "";
	var afterCaret = "";

	var regexStartComponentsTag = /< *components *>/gm;
	var regexEndComponentsTag = /<\/ *components *>/gm;

	let foundEndTag = false;
	let foundStartTag = false;

	let dataDefStartLineCounter = caretLine;
	while(!foundStartTag){
		beforeCaret = lines[dataDefStartLineCounter];
		foundStartTag = regexStartComponentsTag.test(beforeCaret);
		foundEndTag = regexEndComponentsTag.test(beforeCaret);
		dataDefStartLineCounter--;
		if(dataDefStartLineCounter < 0){
			return false;
		}
		if(foundEndTag){
			return false;
		}
	}

	let dataDefEndLineCounter = caretLine;
	while(!foundEndTag){
		afterCaret = lines[dataDefEndLineCounter];
		foundEndTag = regexEndComponentsTag.test(afterCaret);
		foundStartTag = regexStartComponentsTag.test(afterCaret);
		dataDefEndLineCounter++;
		if(dataDefEndLineCounter == lines.length){
			return false;
		}
		if(foundStartTag){
			return false;
		}
	}

	return true;
}

export function IsCaretWithinDataComponentList(
	text: string, 
	caretLine: number, 
	caretCharacter: number
){
	let isWithinDataDef : boolean = IsCaretWithinDataDef(text, caretLine, caretCharacter, "FNEEntityData");

	if(!isWithinDataDef){
		return false;
	}

	let isWithinComponentsList = IsCaretWithinComponentsTag(text, caretLine, caretCharacter);

	if(!isWithinComponentsList){
		return false;
	}

	return true;
}

export function IsCaretWithinItemComponentList(
	text: string, 
	caretLine: number, 
	caretCharacter: number
){
	let isWithinDataDef : boolean = IsCaretWithinDataDef(text, caretLine, caretCharacter, "ItemData");

	if(!isWithinDataDef){
		return false;
	}

	let isWithinComponentsList = IsCaretWithinComponentsTag(text, caretLine, caretCharacter);

	if(!isWithinComponentsList){
		return false;
	}

	return true;
}