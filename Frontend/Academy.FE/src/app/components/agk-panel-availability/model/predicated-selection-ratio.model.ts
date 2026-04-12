export class PredictedSelectionRatioModel
{
    tdc: string | undefined = "";
    communityId: number | undefined
    startDate : Date  = new Date;
    endDate : Date | undefined;
    l1SelectionRatio: number | undefined;
    gkSelectionRatio: number | undefined;
}
