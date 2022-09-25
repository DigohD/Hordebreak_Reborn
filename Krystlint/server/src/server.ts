/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
import {
	createConnection,
	TextDocuments,
	Diagnostic,
	DiagnosticSeverity,
	ProposedFeatures,
	InitializeParams,
	DidChangeConfigurationNotification,
	CompletionItem,
	CompletionItemKind,
	TextDocumentPositionParams,
	TextDocumentSyncKind,
	InitializeResult
} from 'vscode-languageserver';

import {
	TextDocument, Position
} from 'vscode-languageserver-textdocument';

const fs = require('fs');

import {FindAllXMLFilePaths} from './utils/ScanAllFiles';
import {IndexAllXMLFiles} from './index/IndexMain';

import {ValidateFileIds} from './Validation/IdValidation';
import {ValidateFileRefs} from './Validation/RefsValidation';
import { GetRefCompletionList } from './Completion/RefCompletion';
import { GetCommandCompletionList } from './Completion/CommandCompletion';

// Create a connection for the server, using Node's IPC as a transport.
// Also include all preview / proposed LSP features.
let connection = createConnection(ProposedFeatures.all);

// Create a simple text document manager. 
let documents: TextDocuments<TextDocument> = new TextDocuments(TextDocument);

let hasConfigurationCapability: boolean = false;
let hasWorkspaceFolderCapability: boolean = false;
let hasDiagnosticRelatedInformationCapability: boolean = false;

let root = "";

export interface IndexBankIdEntry{
	Id: string,
	filePath: string,
	line: number
}

export interface IndexBankType {
	Ids: []
}

let IndexBank: IndexBankType = {
	Ids: []
};

const IndexWorkspace = (rootPath: string, AllXMLPaths: string[]) => {
	FindAllXMLFilePaths(rootPath, AllXMLPaths);
	IndexAllXMLFiles(AllXMLPaths, IndexBank);
}

connection.onInitialize((params: InitializeParams) => {
	let capabilities = params.capabilities;

	if(params.rootPath){
		root = params.rootPath;
		let AllXMLPaths: string[] = [];
		IndexWorkspace(params.rootPath, AllXMLPaths);
	}

	// Does the client support the `workspace/configuration` request?
	// If not, we fall back using global settings.
	hasConfigurationCapability = !!(
		capabilities.workspace && !!capabilities.workspace.configuration
	);
	hasWorkspaceFolderCapability = !!(
		capabilities.workspace && !!capabilities.workspace.workspaceFolders
	);
	hasDiagnosticRelatedInformationCapability = !!(
		capabilities.textDocument &&
		capabilities.textDocument.publishDiagnostics &&
		capabilities.textDocument.publishDiagnostics.relatedInformation
	);

	const result: InitializeResult = {
		capabilities: {
			textDocumentSync: TextDocumentSyncKind.Incremental,
			// Tell the client that this server supports code completion.
			completionProvider: {
				resolveProvider: true
			}
		}
	};
	if (hasWorkspaceFolderCapability) {
		result.capabilities.workspace = {
			workspaceFolders: {
				supported: true
			}
		};
	}
	return result;
});

connection.onInitialized(() => {
	if (hasConfigurationCapability) {
		// Register for all configuration changes.
		connection.client.register(DidChangeConfigurationNotification.type, undefined);
	}
	if (hasWorkspaceFolderCapability) {
		connection.workspace.onDidChangeWorkspaceFolders(_event => {
			connection.console.log('Workspace folder change event received.');
		});
	}
});

// The example settings
export interface ExampleSettings {
	maxNumberOfProblems: number;
}

// The global settings, used when the `workspace/configuration` request is not supported by the client.
// Please note that this is not the case when using this server with the client provided in this example
// but could happen with other clients.
const defaultSettings: ExampleSettings = { maxNumberOfProblems: 1000 };
let globalSettings: ExampleSettings = defaultSettings;

// Cache the settings of all open documents
let documentSettings: Map<string, Thenable<ExampleSettings>> = new Map();

connection.onDidChangeConfiguration(change => {
	if (hasConfigurationCapability) {
		// Reset all cached document settings
		documentSettings.clear();
	} else {
		globalSettings = <ExampleSettings>(
			(change.settings.languageServerExample || defaultSettings)
		);
	}

	// Revalidate all open text documents
	documents.all().forEach(validateTextDocument);
});

function getDocumentSettings(resource: string): Thenable<ExampleSettings> {
	if (!hasConfigurationCapability) {
		return Promise.resolve(globalSettings);
	}
	let result = documentSettings.get(resource);
	if (!result) {
		result = connection.workspace.getConfiguration({
			scopeUri: resource,
			section: 'languageServerExample'
		});
		documentSettings.set(resource, result);
	}
	return result;
}

// Only keep settings for open documents
documents.onDidClose(e => {
	documentSettings.delete(e.document.uri);
});

// The content of a text document has changed. This event is emitted
// when the text document first opened or when its content has changed.
documents.onDidChangeContent(change => {
	validateTextDocument(change.document);
});

documents.onDidSave(save => {
	let AllXMLPaths: string[] = [];
	IndexWorkspace(root, AllXMLPaths);

	validateTextDocument(save.document);
});

async function validateTextDocument(textDocument: TextDocument): Promise<void> {
	// In this simple example we get the settings for every validate run.
	let settings = await getDocumentSettings(textDocument.uri);

	// The validator creates diagnostics for all uppercase words length 2 and more
	let text = textDocument.getText();
	let diagnostics: Diagnostic[] = [];
	let problems = 0;
	
	ValidateFileIds(
		text,
		diagnostics,
		textDocument,
		IndexBank
	);

	ValidateFileRefs(
		text,
		diagnostics,
		textDocument,
		IndexBank
	);

	// Send the computed diagnostics to VSCode.
	connection.sendDiagnostics({ uri: textDocument.uri, diagnostics });
}

connection.onDidChangeWatchedFiles(_change => {
	// Monitored files have change in VSCode
	connection.console.log('We received an file change event');
});

// This handler provides the initial list of the completion items.
connection.onCompletion(
	(_textDocumentPosition: TextDocumentPositionParams): CompletionItem[] => {
		var doc : TextDocument | undefined = documents.get(_textDocumentPosition.textDocument.uri);
		var text : string = "";
		if(doc){
			text = doc.getText();
		}
		var caretPos: Position = _textDocumentPosition.position;
		
		let AllSuggestions : CompletionItem[]  = [];

		let RefSuggestionList = GetRefCompletionList(
			text,
			caretPos,
			IndexBank
		);
		AllSuggestions = [...RefSuggestionList]

		let CommandSuggestionList = GetCommandCompletionList(
			text,
			caretPos
		);
		AllSuggestions= [...AllSuggestions, ...CommandSuggestionList];

		return AllSuggestions;
	}
);

// This handler resolves additional information for the item selected in
// the completion list.
connection.onCompletionResolve(
	(item: CompletionItem): CompletionItem => {
		let data : string = item.data;
		if (data.indexOf("IDREF::") > -1) {
			item.detail = 'DataDef id reference.';
		} 
		return item;
	}
);

// Make the text document manager listen on the connection
// for open, change and close text document events
documents.listen(connection);

// Listen on the connection
connection.listen();
