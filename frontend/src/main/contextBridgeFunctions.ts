import { dialog } from 'electron';
import { BudgetServiceClient, Purchase } from '../generated/budget_service';
import {
  // eslint-disable-next-line camelcase
  HealthCheckResponse_ServingStatus,
  HealthClient,
} from '../generated/health_check';
import {
  isArray,
  isDate,
  isNumber,
  isPurchaseFilter,
  isString,
  isUploadFileRequest,
} from '../helpers/TypeSafety';
import ApiConnected from '../types/ApiConnected';
import { PurchaseFilterType } from '../types/PurchaseFilter';
import { UploadFileResponse, UploadType } from '../types/UploadFile';
import uploadFileRest from '../helpers/RestApiHelper';

async function pollOnline(
  healthCheckService: HealthClient | null,
): Promise<ApiConnected> {
  return new Promise<ApiConnected>((resolve, reject) => {
    if (healthCheckService !== null) {
      healthCheckService.check(
        {
          service: '',
        },
        (err, response) => {
          if (err !== null) {
            reject(
              new Error(
                `An error occured while checking the health of the server: ${err}`,
              ),
            );
          } else if (
            // eslint-disable-next-line camelcase
            response.status === HealthCheckResponse_ServingStatus.SERVING
          ) {
            resolve({
              connected: true,
              message: 'The connection is good!',
            });
          } else {
            resolve({
              connected: false,
              message: `The health check response returned ${response.status} with message`,
            });
          }
        },
      );
    } else {
      resolve({
        connected: false,
        message:
          "The health check service is null, it's likely you have not set an api url",
      });
    }
  });
}

async function getPurchases(
  arg: any,
  budgetService: BudgetServiceClient | null,
): Promise<Purchase[]> {
  return new Promise<Purchase[]>((resolve, reject) => {
    if (budgetService === null) {
      reject(
        new Error(
          "The budget service is null, it's likely you have not set an api url",
        ),
      );
    } else if (isPurchaseFilter(arg)) {
      if (arg.purchaseFilterType === PurchaseFilterType.All) {
        budgetService.getPurchases(
          {
            description: arg.description,
            category: arg.category,
            startTime: arg.startTime,
            endTime: arg.endTime,
          },
          (err, response) => {
            if (err !== null) {
              reject(
                new Error(`An error occured while getting purchases: ${err}`),
              );
            } else {
              resolve(response.purchases);
            }
          },
        );
      } else if (arg.purchaseFilterType === PurchaseFilterType.MostCommon) {
        budgetService.getMostCommonPurchases(
          {
            category: arg.category,
            startTime: arg.startTime,
            endTime: arg.endTime,
            count: arg.count,
          },
          (err, response) => {
            if (err !== null) {
              reject(
                new Error(
                  `An error occured while getting most common purchases: ${err}`,
                ),
              );
            } else {
              resolve(response.purchases);
            }
          },
        );
      } else {
        reject(new Error('The purchase filter type is not supported'));
      }
    } else {
      reject(new Error('The passed argument is not a purchase filter'));
    }
  });
}

async function addPurchase(
  args: any,
  budgetService: BudgetServiceClient | null,
) {
  return new Promise<void>((resolve, reject) => {
    if (budgetService === null) {
      reject(
        new Error(
          "The budget service is null, it's likely you have not set an api url",
        ),
      );
    } else if (
      isArray(args) &&
      args.length >= 4 &&
      isDate(args[0]) &&
      isString(args[1]) &&
      isNumber(args[2]) &&
      isString(args[3])
    ) {
      budgetService.addPurchase(
        {
          date: args[0],
          description: args[1],
          amount: args[2],
          category: args[3],
        },
        (err) => {
          if (err !== null) {
            reject(
              new Error(`An error occured while adding a purchase: ${err}`),
            );
          } else {
            resolve();
          }
        },
      );
    } else {
      reject(new Error('The passed argument is malformed'));
    }
  });
}

async function uploadFile(arg: any, serverName: string | null) {
  return new Promise<UploadFileResponse>((resolve, reject) => {
    if (serverName === null) {
      reject(
        new Error(
          "The server name is null, it's likely you have not set an api url",
        ),
      );
    } else if (isUploadFileRequest(arg)) {
      if (arg.uploadType === UploadType.Purchase) {
        dialog
          .showOpenDialog({
            title: 'Upload File...',
            filters: [
              {
                name: 'Upload file',
                extensions: [arg.fileType],
              },
            ],
            properties: ['openFile'],
          })
          .then((value) => {
            if (!value.canceled) {
              return uploadFileRest(serverName!, value.filePaths[0]);
            }

            return {
              success: false,
              message: 'The user canceled the file dialog',
            };
          })
          .then((value) => resolve(value))
          .catch((err) => reject(err));
      } else {
        reject(
          new Error(`The upload type ${arg.uploadType} is not yet supported`),
        );
      }
    } else {
      reject(new Error('The passed argument is not a upload file request'));
    }
  });
}

async function getCategories(budgetService: BudgetServiceClient | null) {
  return new Promise<string[]>((resolve, reject) => {
    if (budgetService === null) {
      reject(
        new Error(
          "The budget service is null, it's likely you have not set an api url",
        ),
      );
    } else {
      budgetService.getCategories({}, (err, result) => {
        if (err !== null) {
          reject(new Error(`An error occured while adding a category: ${err}`));
        } else {
          resolve(result.categories);
        }
      });
    }
  });
}

async function addCategory(
  arg: any,
  budgetService: BudgetServiceClient | null,
) {
  return new Promise<void>((resolve, reject) => {
    if (budgetService === null) {
      reject(
        new Error(
          "The budget service is null, it's likely you have not set an api url",
        ),
      );
    } else if (isString(arg)) {
      budgetService.addCategory(
        {
          category: arg,
        },
        (err) => {
          if (err !== null) {
            reject(
              new Error(`An error occured while adding a category: ${err}`),
            );
          } else {
            resolve();
          }
        },
      );
    } else {
      reject(new Error('The passed argument is not a string'));
    }
  });
}

export {
  pollOnline,
  getPurchases,
  addPurchase,
  uploadFile,
  addCategory,
  getCategories,
};
