export function DoesIdExistMoreThanOnce (array: string[], toFind: string, start: number, end: number) : boolean { 
    if (start > end) return false; 
   
    let mid=Math.floor((start + end)/2); 
    
    if (array[mid] === toFind){

		if(mid + 1 < array.length && array[mid + 1] === toFind)
			return true;
		if(mid - 1 >= 0 && array[mid - 1] === toFind)
			return true; 
		
		return false;
	} 
          
    if(array[mid] > toFind)  {
		return DoesIdExistMoreThanOnce(array, toFind, start, mid-1); 
	}
    else{
        return DoesIdExistMoreThanOnce(array, toFind, mid+1, end); 
	}   
} 

export function DoesIdExist (array: string[], toFind: string, start: number, end: number) : boolean { 
    if (start > end) return false; 
   
    let mid=Math.floor((start + end)/2); 
    
    if (array[mid] === toFind){
		return true;
	} 
          
    if(array[mid] > toFind)  {
		return DoesIdExist(array, toFind, start, mid-1); 
	}
    else{
        return DoesIdExist(array, toFind, mid+1, end); 
	}   
} 