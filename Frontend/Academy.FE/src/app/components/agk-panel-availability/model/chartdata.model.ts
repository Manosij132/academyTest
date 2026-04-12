export class ChartDataModel
{
    name: string = '';
    value: number = 0;
    constructor(obj?: any){
        Object.assign(this,obj);
    }
}
