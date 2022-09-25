import { window, commands, ExtensionContext, TextEditorEdit, Position } from 'vscode';
import { downloadAndUnzipVSCode } from 'vscode-test';

import { DataDefGenCommand } from './DataDefGen/Main';
import { ComponentDataGenCommand } from './EntityCompGen/Main';
import { ItemComponentGenCommand } from './ItemCompGen/Main';

export function RegisterClientCommands(context: ExtensionContext){
	context.subscriptions.push(commands.registerCommand("KrystLint.DataDefGen", (...args: any) => {
		window.activeTextEditor.edit((editBuilder: TextEditorEdit) => {
			DataDefGenCommand(editBuilder, args);
		});
	}));

	context.subscriptions.push(commands.registerCommand("KrystLint.ComponentDataGen", (...args: any) => {
		window.activeTextEditor.edit((editBuilder: TextEditorEdit) => {
			ComponentDataGenCommand(editBuilder, args);
		});
	}));

	context.subscriptions.push(commands.registerCommand("KrystLint.ItemComponentGen", (...args: any) => {
		window.activeTextEditor.edit((editBuilder: TextEditorEdit) => {
			ItemComponentGenCommand(editBuilder, args);
		});
	}));
}