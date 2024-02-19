import { useCallback, useEffect, useState } from 'react';
import { ElectronHandler } from '../../main/preload';
import { isSavings } from '../../helpers/TypeSafety';
import { Saved } from '../../generated/budget_service';
import InsertSaved from './InsertSaved';
import SavingsTable from './SavingsTable';

interface SavingsProps {
  isConnected: boolean;
  contextBridge: ElectronHandler;
}

export default function Savings({
  isConnected,
  contextBridge,
}: SavingsProps) {
  const [savings, setSavings] = useState<Saved[]>([]);

  async function addSaved(date: Date, description: string, amount: number) {
    await contextBridge.ipcRenderer.invoke('add-saved', [
      date,
      description,
      amount,
    ]);
  }

  const editSaved = useCallback(
    async (
      savedId: number,
      date: Date,
      description: string,
      amount: number,
    ) => {
      await contextBridge.ipcRenderer.invoke('edit-saved', [
        savedId,
        date,
        description,
        amount,
      ]);
    },
    [contextBridge],
  );

  const getSavings = useCallback(async (): Promise<Saved[]> => {
    if (isConnected) {
      const savingsResp = await contextBridge.ipcRenderer.invoke('get-savings');
      if (isSavings(savingsResp)) {
        return savingsResp;
      }
    }

    return [];
  }, [contextBridge, isConnected]);

  const refreshSavingsData = useCallback(() => {
    console.log('refreshing...');

    getSavings()
      .then((savings) => setSavings(savings))
      .catch((err) => console.log(`Couldn't retrieve savings: ${err}`));
  }, [contextBridge, getSavings]);

  useEffect(() => {
    if (isConnected) {
      refreshSavingsData();
    }
  }, [contextBridge, refreshSavingsData, isConnected]);

  return (
    <div>
      <InsertSaved
        onSubmit={(date, description, currentAmount) =>
          addSaved(date, description, currentAmount)
            .then(() => refreshSavingsData())
            .catch((err) =>
              console.log(
                `An error occured while inserting a savings: ${err}`,
              ),
            )
        }
      />
      <SavingsTable
        maxRows={15}
        savings={savings}
        saveEditedSaved={(savedId, date, description, currentAmount) =>
          editSaved(savedId, date, description, currentAmount)
            .then(() => refreshSavingsData())
            .catch((err) =>
              console.log(`An error occured while editing the saved: ${err}`),
            )
        }
        deleteRow={(savedId) =>
          contextBridge.ipcRenderer
            .invoke('delete-saved', savedId)
            .then(() => refreshSavingsData())
            .catch((err) => console.log(err))
        }
      />
    </div>
  );
}
