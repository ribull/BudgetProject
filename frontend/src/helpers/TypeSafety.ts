import ApiConnected from '../types/ApiConnected';
import AppConstants from '../types/AppConstants';

function isBoolean(value: unknown): value is boolean {
  return value === false || value === true;
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

export { isBoolean, isAppConstants, isApiConnected };
