export function IndexIds(fileContent: string, Ids: string[]){
	var regex = /<id>(.*?)<\/id>/gm;

	var match;
	while ((match = regex.exec(fileContent)) != null) {
		Ids.push(match[1]);
	}
}