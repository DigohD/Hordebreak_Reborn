const fs = require('fs');

import {IndexIds} from './Ids';
import {IndexBankType} from '../server';

export function IndexAllXMLFiles(filePaths: string[], IndexBank: IndexBankType){
	IndexBank.Ids.splice(0, IndexBank.Ids.length);

	filePaths.forEach((path: string) => {
		let data = fs.readFileSync(path, {encoding:'utf8', flag:'r'});

		IndexFile(data, IndexBank);
	});

	IndexBank.Ids.sort();
}

function IndexFile(content: string, IndexBank: IndexBankType){
	IndexIds(content, IndexBank.Ids);
}