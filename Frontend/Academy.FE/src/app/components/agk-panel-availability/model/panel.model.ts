export class Panel {
  id: number = 0;
  name: string = "";

  constructor(obj?: any){
    Object.assign(this,obj);
  }
}