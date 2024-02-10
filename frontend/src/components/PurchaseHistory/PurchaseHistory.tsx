import { useCallback, useEffect, useState } from 'react';
import { ElectronHandler } from '../../main/preload';
import { PurchaseFilter, PurchaseFilterType } from '../../types/PurchaseFilter';
import { isPurchaseArray, isStringArray } from '../../helpers/TypeSafety';
import InsertPurchase from './InsertPurchase';
import { Purchase } from '../../generated/budget_service';
import PurchasesTable from './PurchasesTable';
import UploadCsv from './UploadCsv';
import { UploadType } from '../../types/UploadFile';

interface PurchaseHistoryProps {
  isConnected: boolean;
  contextBridge: ElectronHandler;
}

export default function PurchaseHistory({
  isConnected,
  contextBridge,
}: PurchaseHistoryProps) {
  const [recentPurchases, setRecentPurchases] = useState<Purchase[]>([]);
  const [topPurchases, setTopPurchases] = useState<Purchase[]>([]);
  const [existingCategories, setExistingCategories] = useState<string[]>([]);

  async function addPurchase(
    date: Date,
    description: string,
    amount: number,
    category: string,
  ) {
    if (!existingCategories.includes(category)) {
      await contextBridge.ipcRenderer.invoke('add-category', category);
    }

    await contextBridge.ipcRenderer.invoke('add-purchase', [
      date,
      description,
      amount,
      category,
    ]);
  }

  const editPurchase = useCallback(
    async (
      purchaseId: number,
      date: Date,
      description: string,
      amount: number,
      category: string,
    ) => {
      if (!existingCategories.includes(category)) {
        await contextBridge.ipcRenderer.invoke('add-category', category);
      }

      await contextBridge.ipcRenderer.invoke('edit-purchase', [
        purchaseId,
        date,
        description,
        amount,
        category,
      ]);
    },
    [contextBridge],
  );

  const getRecentPurchases = useCallback(
    async (purchaseFilter: PurchaseFilter): Promise<Purchase[]> => {
    if (isConnected) {
      const purchasesResp = await contextBridge.ipcRenderer.invoke('get-purchases', purchaseFilter);
      if (isPurchaseArray(purchasesResp)) {
        return purchasesResp;
      }
    }

    return [];
  }, [contextBridge, isConnected]);

  const refreshPurchaseData = useCallback(() => {
    console.log('refreshing...');
    const startTime = new Date();
    startTime.setMonth(startTime.getMonth() - 3);
    const getRecentPurchasesRequest: PurchaseFilter = {
      startTime,
      purchaseFilterType: PurchaseFilterType.All,
    };

    getRecentPurchases(getRecentPurchasesRequest)
      .then((purchases) => setRecentPurchases(purchases))
      .catch((err) =>
        console.log(`Couldn't retrieve recent purchases: ${err}`),
      );

    const getTopPurchasesRequest: PurchaseFilter = {
      count: 40,
      purchaseFilterType: PurchaseFilterType.MostCommon,
    };

    getRecentPurchases(getTopPurchasesRequest)
      .then((purchases) => setTopPurchases(purchases))
      .catch((err) => console.log(`Couldn't retrieve top purchases: ${err}`));

    contextBridge.ipcRenderer
      .invoke('get-categories')
      .then((result) => {
        if (isStringArray(result)) {
          setExistingCategories(result);
        }

        return true;
      })
      .catch((err) => console.log(`Couldn't retrieve categories: ${err}`));
  }, [contextBridge, getRecentPurchases]);

  useEffect(() => {
    if (isConnected) {
      refreshPurchaseData();
    }
  }, [contextBridge, refreshPurchaseData, isConnected]);

  return (
    <div>
      <InsertPurchase
        onSubmit={(date, description, amount, category) =>
          addPurchase(date, description, amount, category)
            .then(() => refreshPurchaseData())
            .catch((err) =>
              console.log(`An error occured while inserting purchase: ${err}`),
            )
        }
        topPurchases={topPurchases}
        existingCategories={existingCategories}
      />
      <UploadCsv
        onSubmit={() =>
          contextBridge.ipcRenderer
            .invoke('upload-file', {
              fileType: 'csv',
              uploadType: UploadType.Purchase,
            })
            .then(() => refreshPurchaseData())
            .catch((err) =>
              console.log(`An error occurred while upload the csv: ${err}`),
            )
        }
      />
      <PurchasesTable
        maxRows={15}
        purchases={recentPurchases}
        topPurchases={topPurchases}
        existingCategories={existingCategories}
        saveEditedPurchase={(purchaseId, date, description, amount, category) =>
          editPurchase(purchaseId, date, description, amount, category)
            .then(() => refreshPurchaseData())
            .catch((err) =>
              console.log(
                `An error occured while editing the purchase: ${err}`,
              ),
            )
        }
        deleteRow={(purchaseId) =>
          contextBridge.ipcRenderer
            .invoke('delete-purchase', purchaseId)
            .then(() => refreshPurchaseData())
            .catch((err) => console.log(err))
        }
        requestOlderPurchases={() => console.log('requesting more purchases')}
      />
    </div>
  );
}
