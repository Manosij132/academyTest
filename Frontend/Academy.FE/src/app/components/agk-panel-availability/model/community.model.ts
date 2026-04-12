export class Community {
  communityId: number = 0;
  communityName: string = '';
  
  constructor(obj?: any){
    Object.assign(this,obj);
  }
}
