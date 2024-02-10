import { useCallback, useEffect, useState } from 'react';
import { ElectronHandler } from '../../main/preload';
import { PayHistory } from '../../generated/budget_service';
import InsertPayHistory from './InsertPayHistory';
import PayHistoryTable from './PayHistoryTable';
import { isPayHistoryArray } from '../../helpers/TypeSafety';

interface PayHistoryHistoryProps {
  isConnected: boolean;
  contextBridge: ElectronHandler;
}

export default function PayHistoryHistory({
  isConnected,
  contextBridge,
}: PayHistoryHistoryProps) {
  const [payHistories, setPayHistories] = useState<PayHistory[]>([]);

  async function addPayHistory(
    startDate: Date,
    endDate: Date,
    earnings: number,
    preTax: number,
    taxes: number,
    postTax: number,
  ) {
    await contextBridge.ipcRenderer.invoke('add-pay-history', [
      startDate,
      endDate,
      earnings,
      preTax,
      taxes,
      postTax,
    ]);
  }

  const editPayHistory = useCallback(
    async (
      payHistoryId: number,
      startDate: Date,
      endDate: Date,
      earnings: number,
      preTax: number,
      taxes: number,
      postTax: number,
    ) => {
      await contextBridge.ipcRenderer.invoke('edit-pay-history', [
        payHistoryId,
        startDate,
        endDate,
        earnings,
        preTax,
        taxes,
        postTax,
      ]);
    },
    [contextBridge],
  );

  const getPayHistories = useCallback(async (): Promise<PayHistory[]> => {
    if (isConnected) {
      const payHistoriesResponse =
        await contextBridge.ipcRenderer.invoke('get-pay-histories');
      if (isPayHistoryArray(payHistoriesResponse)) {
        return payHistoriesResponse;
      }
    }

    return [];
  }, [contextBridge, isConnected]);

  const refreshPayHistoryData = useCallback(() => {
    getPayHistories()
      .then((ph) => setPayHistories(ph))
      .catch((err) => console.log(`Couldn't retrieve pay histories: ${err}`));
  }, [getPayHistories]);

  useEffect(() => {
    if (isConnected) {
      refreshPayHistoryData();
    }
  }, [contextBridge, refreshPayHistoryData, isConnected]);

  return (
    <div>
      <InsertPayHistory
        onSubmit={(dateRange, earnings, preTax, taxes, postTax) =>
          addPayHistory(
            dateRange[0],
            dateRange[1],
            earnings,
            preTax,
            taxes,
            postTax,
          )
            .then(() => refreshPayHistoryData())
            .catch((err) =>
              console.log(`An error occured while inserting a pay history: ${err}`),
            )
        }
      />
      <PayHistoryTable
        maxRows={15}
        payHistories={payHistories}
        saveEditedPayHistory={(
          payHistoryId,
          startDate,
          endDate,
          earnings,
          preTax,
          taxes,
          postTax,
        ) =>
          editPayHistory(
            payHistoryId,
            startDate,
            endDate,
            earnings,
            preTax,
            taxes,
            postTax,
          )
            .then(() => refreshPayHistoryData())
            .catch((err) =>
              console.log(
                `An error occured while editing the pay history: ${err}`,
              ),
            )
        }
        deleteRow={(payHistoryId) =>
          contextBridge.ipcRenderer
            .invoke('delete-pay-history', payHistoryId)
            .then(() => refreshPayHistoryData())
            .catch((err) => console.log(err))
        }
      />
    </div>
  );
}
