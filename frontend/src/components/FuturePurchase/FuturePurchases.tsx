import { useCallback, useEffect, useState } from 'react';
import { ElectronHandler } from '../../main/preload';
import { isFuturePurchaseArray, isStringArray } from '../../helpers/TypeSafety';
import { FuturePurchase } from '../../generated/budget_service';
import InsertFuturePurchase from './InsertFuturePurchase';
import FuturePurchasesTable from './FuturePurchasesTable';

interface FuturePurchasesProps {
  isConnected: boolean;
  contextBridge: ElectronHandler;
}

export default function FuturePurchases({
  isConnected,
  contextBridge,
}: FuturePurchasesProps) {
  const [futurePurchases, setFuturePurchases] = useState<FuturePurchase[]>([]);
  const [existingCategories, setExistingCategories] = useState<string[]>([]);

  async function addFuturePurchase(
    date: Date,
    description: string,
    amount: number,
    category: string,
  ) {
    if (!existingCategories.includes(category)) {
      await contextBridge.ipcRenderer.invoke('add-category', category);
    }

    await contextBridge.ipcRenderer.invoke('add-future-purchase', [
      date,
      description,
      amount,
      category,
    ]);
  }

  const editFuturePurchase = useCallback(
    async (
      futurePurchaseId: number,
      date: Date,
      description: string,
      amount: number,
      category: string,
    ) => {
      if (!existingCategories.includes(category)) {
        await contextBridge.ipcRenderer.invoke('add-category', category);
      }

      await contextBridge.ipcRenderer.invoke('edit-future-purchase', [
        futurePurchaseId,
        date,
        description,
        amount,
        category,
      ]);
    },
    [contextBridge, existingCategories],
  );

  const getFuturePurchases = useCallback(async (): Promise<
    FuturePurchase[]
  > => {
    if (isConnected) {
      const purchasesResp = await contextBridge.ipcRenderer.invoke('get-future-purchases');
      if (isFuturePurchaseArray(purchasesResp)) {
        return purchasesResp;
      }
    }

    return [];
  }, [contextBridge, isConnected]);

  const refreshFuturePurchaseData = useCallback(() => {
    console.log('refreshing...');

    getFuturePurchases()
      .then((purchases) => setFuturePurchases(purchases))
      .catch((err) =>
        console.log(`Couldn't retrieve future purchases: ${err}`),
      );

    contextBridge.ipcRenderer
      .invoke('get-categories')
      .then((result) => {
        if (isStringArray(result)) {
          setExistingCategories(result);
        }

        return true;
      })
      .catch((err) => console.log(`Couldn't retrieve categories: ${err}`));
  }, [contextBridge, getFuturePurchases]);

  useEffect(() => {
    if (isConnected) {
      refreshFuturePurchaseData();
    }
  }, [contextBridge, refreshFuturePurchaseData, isConnected]);

  return (
    <div>
      <InsertFuturePurchase
        onSubmit={(date, description, amount, category) =>
          addFuturePurchase(date, description, amount, category)
            .then(() => refreshFuturePurchaseData())
            .catch((err) =>
              console.log(
                `An error occured while inserting future purchase: ${err}`,
              ),
            )
        }
        existingCategories={existingCategories}
      />
      <FuturePurchasesTable
        maxRows={15}
        futurePurchases={futurePurchases}
        existingCategories={existingCategories}
        saveEditedPurchase={(purchaseId, date, description, amount, category) =>
          editFuturePurchase(purchaseId, date, description, amount, category)
            .then(() => refreshFuturePurchaseData())
            .catch((err) =>
              console.log(
                `An error occured while editing the purchase: ${err}`,
              ),
            )
        }
        deleteRow={(purchaseId) =>
          contextBridge.ipcRenderer
            .invoke('delete-future-purchase', purchaseId)
            .then(() => refreshFuturePurchaseData())
            .catch((err) => console.log(err))
        }
      />
    </div>
  );
}
