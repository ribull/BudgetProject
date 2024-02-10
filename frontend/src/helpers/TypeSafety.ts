import { PayHistory, Purchase } from '../generated/budget_service';
import ApiConnected from '../types/ApiConnected';
import AppConstants from '../types/AppConstants';
import { PurchaseFilter, PurchaseFilterType } from '../types/PurchaseFilter';
import {
  UploadFileRequest,
  UploadFileResponse,
  UploadType,
} from '../types/UploadFile';

function isArray(object: unknown): object is unknown[] {
  return Array.isArray(object);
}

function isBoolean(value: unknown): value is boolean {
  return value === false || value === true;
}

function isDate(value: unknown): value is Date {
  return value instanceof Date && !Number.isNaN(value);
}

function isNumber(value: unknown): value is number {
  return typeof value === 'number' && !Number.isNaN(value);
}

function isAppConstants(value: unknown): value is AppConstants {
  return (value as AppConstants).apiUrl !== undefined;
}

function isApiConnected(value: unknown): value is ApiConnected {
  const convertedValue = value as ApiConnected;
  return (
    convertedValue.connected !== undefined &&
    convertedValue.message !== undefined
  );
}

function isUploadType(value: unknown): value is UploadType {
  return (<any>Object).values(UploadType).includes(value);
}

function isUploadFileRequest(value: unknown): value is UploadFileRequest {
  const convertedValue = value as UploadFileRequest;
  return (
    convertedValue.fileType !== undefined &&
    convertedValue.uploadType !== undefined &&
    isUploadType(convertedValue.uploadType)
  );
}

function isUploadFileResponse(value: unknown): value is UploadFileResponse {
  const convertedValue = value as UploadFileResponse;
  return (
    convertedValue.success !== undefined && convertedValue.message !== undefined
  );
}

function isPurchaseFilterType(value: unknown): value is PurchaseFilterType {
  return (<any>Object).values(PurchaseFilterType).includes(value);
}

function isPurchaseFilter(value: unknown): value is PurchaseFilter {
  const convertedValue = value as PurchaseFilter;
  return (
    convertedValue.purchaseFilterType !== undefined &&
    isPurchaseFilterType(convertedValue.purchaseFilterType)
  );
}

function isString(value: unknown): value is string {
  return typeof value === 'string' || value instanceof String;
}

function isStringArray(value: unknown): value is string[] {
  if (isArray(value)) {
    return value.every((val) => isString(val));
  }

  return false;
}

function isPurchase(value: unknown): value is Purchase {
  const convertedValue = value as Purchase;
  return (
    convertedValue.amount !== undefined &&
    convertedValue.category !== undefined &&
    convertedValue.date !== undefined &&
    convertedValue.description !== undefined &&
    convertedValue.purchaseId !== undefined
  );
}

function isPurchaseArray(value: unknown): value is Purchase[] {
  if (isArray(value)) {
    return value.every((val) => isPurchase(val));
  }

  return false;
}

function isPayHistory(value: unknown): value is PayHistory {
  const convertedValue = value as PayHistory;
  return (
    convertedValue.payPeriodStartDate !== undefined &&
    convertedValue.payPeriodEndDate !== undefined &&
    convertedValue.earnings !== undefined &&
    convertedValue.preTaxDeductions !== undefined &&
    convertedValue.taxes !== undefined &&
    convertedValue.postTaxDeductions !== undefined
  );
}

function isPayHistoryArray(value: unknown): value is PayHistory[] {
  if (isArray(value)) {
    return value.every((val) => isPayHistory(val));
  }

  return false;
}

export {
  isArray,
  isBoolean,
  isDate,
  isNumber,
  isAppConstants,
  isApiConnected,
  isUploadType,
  isUploadFileRequest,
  isUploadFileResponse,
  isPurchaseFilterType,
  isPurchaseFilter,
  isString,
  isStringArray,
  isPurchase,
  isPurchaseArray,
  isPayHistoryArray,
};
