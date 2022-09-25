import { callbackify } from 'util';
import { resolve } from 'dns';

const fs = require('fs');

export function FindAllXMLFilePaths(rootPath: string, AllXMLPaths: string[]){
	let files = fs.readdirSync(rootPath);

	files.forEach((file: string) => {
		if(file.indexOf('.xml') > -1 && file.indexOf('.meta') < 0)
		{
			AllXMLPaths.push(rootPath + "\\" + file);
		}
		if(file.indexOf('.') < 0){
			if(fs.lstatSync(rootPath + "\\" + file).isDirectory()){
				FindAllXMLFilePaths(rootPath + "\\" + file, AllXMLPaths);
			}
		}
	});
}