import { useEffect, useState } from 'react';
import { Notification, Button } from 'react-bulma-components';
import { ElectronHandler } from '../../main/preload';
import { PurchaseFilter, PurchaseFilterType } from '../../types/PurchaseFilter';
import { isPurchaseArray, isStringArray } from '../../helpers/TypeSafety';
import InsertPurchase from './InsertPurchase';
import { Purchase } from '../../generated/budget_service';
import PurchasesTable from './PurchasesTable';

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

  const [addPurchaseError, setAddPurchaseError] = useState('');

  useEffect(() => {
    if (isConnected) {
      const startTime = new Date();
      startTime.setMonth(startTime.getMonth() - 3);
      const getRecentPurchasesRequest: PurchaseFilter = {
        startTime,
        purchaseFilterType: PurchaseFilterType.All,
      };

      contextBridge.ipcRenderer
        .invoke('get-purchases', getRecentPurchasesRequest)
        .then((result) => {
          if (isPurchaseArray(result)) {
            setRecentPurchases(result);
          }

          return true;
        })
        .catch((err) =>
          console.log(`Couldn't retrieve recent purchases: ${err}`),
        );

      const getTopPurchasesRequest: PurchaseFilter = {
        count: 40,
        purchaseFilterType: PurchaseFilterType.MostCommon,
      };

      contextBridge.ipcRenderer
        .invoke('get-purchases', getTopPurchasesRequest)
        .then((result) => {
          if (isPurchaseArray(result)) {
            setTopPurchases(result);
          }

          return true;
        })
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
    }
  }, [contextBridge, isConnected]);

  return (
    <div>
      <InsertPurchase
        onSubmit={(date, description, amount, category) => {
          if (!existingCategories.includes(category)) {
            contextBridge.ipcRenderer
              .invoke('add-category', category)
              .then(async () =>
                contextBridge.ipcRenderer.invoke('add-purchase', [
                  date,
                  description,
                  amount,
                  category,
                ]),
              )
              .catch((err) => setAddPurchaseError(err));
          } else {
            contextBridge.ipcRenderer
              .invoke('add-purchase', [date, description, amount, category])
              .catch((err) => setAddPurchaseError(err));
          }
        }}
        topPurchases={topPurchases}
        existingCategories={existingCategories}
      />
      {addPurchaseError !== '' && (
        <Notification color="warning">
          {`An error occured while adding a purchase: ${addPurchaseError}`}
          <Button remove />
        </Notification>
      )}
      <PurchasesTable
        maxRows={15}
        purchases={recentPurchases}
        topPurchases={topPurchases}
        existingCategories={existingCategories}
        saveEditedPurchase={(purchaseId, date, description, amount, category) => {
          if (!existingCategories.includes(category)) {
            contextBridge.ipcRenderer
              .invoke('add-category', category)
              .then(async () =>
                contextBridge.ipcRenderer.invoke('edit-purchase', [
                  purchaseId,
                  date,
                  description,
                  amount,
                  category,
                ]),
              )
              .catch((err) => console.log(err));
          } else {
            contextBridge.ipcRenderer
              .invoke('edit-purchase', [purchaseId, date, description, amount, category])
              .catch((err) => console.log(err));
          }
        }}
        deleteRow={(purchaseId) => contextBridge.ipcRenderer.invoke('delete-purchase', purchaseId).catch(err => console.log(err))}
        requestOlderPurchases={() => console.log('requesting more purchases')}
      />
    </div>
  );
}
