export class DataRequestOptions {
  SearchText: string;
  PagingOptions: PagingOption;
  FilterOptions: FilterOption[];
  SortOptions: SortOption;

  constructor() {
    this.SearchText = "";
    this.PagingOptions = new PagingOption();
    this.FilterOptions = [];
    this.SortOptions = new SortOption();
  }
}

export class PagingOption {
  PageSize: number = 20;
  PageIndex: number = 0;
}

export class FilterOption {
  FilterBy: string = "";
  FilterValue: string = "";
}

export class SortOption {
  SortBy: string = "";
  SortByDescending: boolean = false;
}
