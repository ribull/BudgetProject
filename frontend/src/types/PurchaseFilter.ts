enum PurchaseFilterType {
  All,
  MostCommon,
}

interface PurchaseFilter {
  purchaseFilterType: PurchaseFilterType;
  description?: string;
  category?: string;
  startTime?: Date;
  endTime?: Date;
  count?: number;
}

export { PurchaseFilter, PurchaseFilterType };
