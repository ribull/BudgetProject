import { useCallback, useEffect, useState } from 'react';
import { ElectronHandler } from '../../main/preload';
import { isInvestmentArray } from '../../helpers/TypeSafety';
import { Investment } from '../../generated/budget_service';
import FuturePurchasesTable from './InvestmentsTable';
import InsertInvestment from './InsertInvestment';

interface InvestmentsProps {
  isConnected: boolean;
  contextBridge: ElectronHandler;
}

export default function Investments({
  isConnected,
  contextBridge,
}: InvestmentsProps) {
  const [investments, setInvestments] = useState<Investment[]>([]);

  async function addInvestment(
    description: string,
    currentAmount: number,
    yearlyGrowthRate: number,
  ) {
    await contextBridge.ipcRenderer.invoke('add-investment', [
      description,
      currentAmount,
      yearlyGrowthRate,
    ]);
  }

  const editInvestment = useCallback(
    async (
      investmentId: number,
      description: string,
      currentAmount: number,
      yearlyGrowthRate: number,
    ) => {
      await contextBridge.ipcRenderer.invoke('edit-investment', [
        investmentId,
        description,
        currentAmount,
        yearlyGrowthRate,
      ]);
    },
    [contextBridge],
  );

  const getInvestments = useCallback(async (): Promise<Investment[]> => {
    if (isConnected) {
      const investmentsResp =
        await contextBridge.ipcRenderer.invoke('get-investments');
      if (isInvestmentArray(investmentsResp)) {
        return investmentsResp;
      }
    }

    return [];
  }, [contextBridge, isConnected]);

  const refreshInvestmentData = useCallback(() => {
    console.log('refreshing...');

    getInvestments()
      .then((newInvestments) => setInvestments(newInvestments))
      .catch((err) => console.log(`Couldn't retrieve investments: ${err}`));
  }, [getInvestments]);

  useEffect(() => {
    if (isConnected) {
      refreshInvestmentData();
    }
  }, [contextBridge, refreshInvestmentData, isConnected]);

  return (
    <div>
      <InsertInvestment
        onSubmit={(description, currentAmount, yearlyGrowthRate) =>
          addInvestment(description, currentAmount, yearlyGrowthRate)
            .then(() => refreshInvestmentData())
            .catch((err) =>
              console.log(
                `An error occured while inserting an investment: ${err}`,
              ),
            )
        }
      />
      <FuturePurchasesTable
        maxRows={15}
        investments={investments}
        saveEditedInvestment={(
          investmentId,
          description,
          currentAmount,
          yearlyGrowthRate,
        ) =>
          editInvestment(
            investmentId,
            description,
            currentAmount,
            yearlyGrowthRate,
          )
            .then(() => refreshInvestmentData())
            .catch((err) =>
              console.log(
                `An error occured while editing the investment: ${err}`,
              ),
            )
        }
        deleteRow={(investmentId) =>
          contextBridge.ipcRenderer
            .invoke('delete-investment', investmentId)
            .then(() => refreshInvestmentData())
            .catch((err) => console.log(err))
        }
      />
    </div>
  );
}
