import { Diagnostic, DiagnosticSeverity, TextDocument } from 'vscode-languageserver';
import { ExampleSettings, IndexBankType } from '../server';

import {DoesIdExist} from '../utils/ListSearch';

// nameRef|iconRef|itemRef|productRef|descriptionRef|categoryRef|unlockRef|damageRef|vfxRef|sfxRef|defenseTypeRef|openSFXRef|closeSFXRef|enemyMeshRef|accessoryMeshRef|resourceRef|typeRef|headRef|hairRef|torsoRef|handsRef|legsRef|feetRef|burnGradeRef|startSFXRef|stopSFXRef|activeSFXLoopRef|propertyRef|hitEffectRef|deathEffectRef|harvestEffectRef|harvestableViewRef|environmentRef|matureEntityRef|pickupSoundRef|laydownSoundRef|gradeRef|craftSFXRef|infoRef|processNameRef|processDescriptionRef|processIconRef|effectRef|reloadEffectRef|projectileVfxRef|onDeathEffectRef|damageTypeRef|followingQuestRef|tileRef|objectRef|atmosphereRef|displayNameRef|absencePropertyRef
export function ValidateFileRefs(
	text: string, 
	diagnostics: Diagnostic[], 
	textDocument: TextDocument,
	IndexBank: IndexBankType
){
	// Mighty regex!
	var regex = /< *(ref|siteRef|recipeRef|transformedEntityRef|buildingRef|viewRef|nameRef|iconRef|itemRef|productRef|descriptionRef|categoryRef|unlockRef|damageRef|vfxRef|sfxRef|defenseTypeRef|openSFXRef|closeSFXRef|enemyMeshRef|accessoryMeshRef|resourceRef|typeRef|headRef|hairRef|torsoRef|handsRef|legsRef|feetRef|burnGradeRef|startSFXRef|stopSFXRef|activeSFXLoopRef|propertyRef|hitEffectRef|deathEffectRef|harvestEffectRef|harvestableViewRef|environmentRef|matureEntityRef|pickupSoundRef|laydownSoundRef|gradeRef|craftSFXRef|infoRef|processNameRef|processDescriptionRef|processIconRef|effectRef|reloadEffectRef|projectileVfxRef|onDeathEffectRef|damageTypeRef|followingQuestRef|tileRef|objectRef|atmosphereRef|displayNameRef|absencePropertyRef) *>[\r\n]*(.*?)[\r\n]*<\/ *(?:ref|siteRef|recipeRef|transformedEntityRef|buildingRef|viewRef|nameRef|iconRef|itemRef|productRef|descriptionRef|categoryRef|unlockRef|damageRef|vfxRef|sfxRef|defenseTypeRef|openSFXRef|closeSFXRef|enemyMeshRef|accessoryMeshRef|resourceRef|typeRef|headRef|hairRef|torsoRef|handsRef|legsRef|feetRef|burnGradeRef|startSFXRef|stopSFXRef|activeSFXLoopRef|propertyRef|hitEffectRef|deathEffectRef|harvestEffectRef|harvestableViewRef|environmentRef|matureEntityRef|pickupSoundRef|laydownSoundRef|gradeRef|craftSFXRef|infoRef|processNameRef|processDescriptionRef|processIconRef|effectRef|reloadEffectRef|projectileVfxRef|onDeathEffectRef|damageTypeRef|followingQuestRef|tileRef|objectRef|atmosphereRef|displayNameRef|absencePropertyRef) *>/gm;
	let m: RegExpExecArray | null;

	while ((m = regex.exec(text))) {
		m[1] = m[1].trim();

		var doesStringExist = DoesIdExist(
			IndexBank.Ids,
			m[2],
			0,
			IndexBank.Ids.length - 1
		);
		if(!doesStringExist){
			let diagnostic: Diagnostic = {
				severity: DiagnosticSeverity.Error,
				range: {
					start: textDocument.positionAt(m.index),
					end: textDocument.positionAt(m.index + m[0].length)
				},
				message: `There is no DataDef with Id '${m[2]}'.`,
				source: 'Krystlint'
			};

			diagnostics.push(diagnostic);
		}
	}
}