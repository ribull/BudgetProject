import { ReactElement, useCallback, useMemo, useState } from 'react';
import { Pagination, Table, Form } from 'react-bulma-components';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faChevronDown,
  faChevronUp,
  faFloppyDisk,
  faPencil,
  faTrash,
  faXmark,
} from '@fortawesome/free-solid-svg-icons';
import { Investment } from '../../generated/budget_service';
import IconButton from '../IconButton';

const { Input } = Form;

interface InvestmentsTableProps {
  investments: Investment[];
  maxRows: number;
  saveEditedInvestment: (
    investmentId: number,
    description: string,
    currentAmount: number,
    yearlyGrowthRate: number,
  ) => void;
  deleteRow: (investmentId: number) => void;
}

export default function FuturePurchasesTable({
  investments,
  maxRows,
  saveEditedInvestment,
  deleteRow,
}: InvestmentsTableProps) {
  const [currentPage, setCurrentPage] = useState(1);
  const [sortColumn, setSortColumn] = useState<string>('Current Amount');
  const [sortAscending, setSortAscending] = useState(false);
  const [editedRow, setEditedRow] = useState<number>();

  const [editDescription, setEditDescription] = useState('');
  const [editCurrentAmount, setEditCurrentAmount] = useState('');
  const [editYearlyGrowthRate, setEditYearlyGrowthRate] = useState('');

  const saveInvestmentCallback = useCallback(
    (investmentId: number) => {
      const currAmt = parseFloat(editCurrentAmount);
      const yGR = parseFloat(editYearlyGrowthRate);
      if (!Number.isNaN(currAmt) && !Number.isNaN(yGR)) {
        saveEditedInvestment(investmentId, editDescription, currAmt, yGR);
      }
    },
    [
      editCurrentAmount,
      editDescription,
      editYearlyGrowthRate,
      saveEditedInvestment,
    ],
  );

  const isEditCurrentAmountValid = !Number.isNaN(parseFloat(editCurrentAmount));
  const isEditYearlyGrowthRateValid = !Number.isNaN(
    parseFloat(editYearlyGrowthRate),
  );

  const partitionedInvestments = useMemo(() => {
    const sortedInvestments = investments.sort((i1, i2) => {
      let result: number = 0;
      if (sortColumn === 'Description') {
        result = i2.description.localeCompare(i1.description);
      } else if (sortColumn === 'Current Amount') {
        result = i1.currentAmount - i2.currentAmount;
      } else if (sortColumn === 'Yearly Growth Rate') {
        result = i1.currentAmount - i2.currentAmount;
      }

      if (!sortAscending) {
        result *= -1;
      }

      return result;
    });

    const returned: ReactElement[][] = [];
    for (let i = 0; i < sortedInvestments.length; i += 1) {
      if (i % maxRows === 0) {
        returned.push([]);
      }

      const notEditing = editedRow !== sortedInvestments[i].investmentId;

      returned[returned.length - 1].push(
        <tr key={`investments-table-tr-${sortedInvestments[i].investmentId}`}>
          <td
            key={`investments-table-cell-desc-${sortedInvestments[i].investmentId}`}
            className="td-lg"
          >
            {notEditing ? (
              sortedInvestments[i].description
            ) : (
              <Input
                value={editDescription}
                onChange={(event) =>
                  setEditDescription(event.currentTarget.value)
                }
                size="small"
              />
            )}
          </td>
          <td
            key={`investments-table-cell-curr-amt-${sortedInvestments[i].investmentId}`}
            className="td-sm"
          >
            {notEditing ? (
              sortedInvestments[i].currentAmount
            ) : (
              <Input
                value={editCurrentAmount}
                onChange={(event) =>
                  setEditCurrentAmount(event.currentTarget.value)
                }
                color={isEditCurrentAmountValid ? 'text' : 'danger'}
                size="small"
              />
            )}
          </td>
          <td
            key={`investments-table-cell-ygr-${sortedInvestments[i].investmentId}`}
            className="td-med"
          >
            {notEditing ? (
              sortedInvestments[i].yearlyGrowthRate
            ) : (
              <Input
                value={editYearlyGrowthRate}
                onChange={(event) =>
                  setEditYearlyGrowthRate(event.currentTarget.value)
                }
                color={isEditYearlyGrowthRateValid ? 'text' : 'danger'}
                size="small"
              />
            )}
          </td>
          <td className="td-sm">
            {editedRow === undefined && (
              <IconButton
                fontAwesomeIcon={faPencil}
                onClick={() => {
                  setEditedRow(sortedInvestments[i].investmentId);
                  setEditDescription(sortedInvestments[i].description);
                  setEditCurrentAmount(`${sortedInvestments[i].currentAmount}`);
                  setEditYearlyGrowthRate(
                    `${sortedInvestments[i].yearlyGrowthRate}`,
                  );
                }}
              />
            )}
            {editedRow === sortedInvestments[i].investmentId && (
              <>
                <IconButton
                  fontAwesomeIcon={faFloppyDisk}
                  onClick={() => {
                    saveInvestmentCallback(sortedInvestments[i].investmentId);
                    setEditedRow(undefined);
                  }}
                />
                <IconButton
                  fontAwesomeIcon={faXmark}
                  onClick={() => setEditedRow(undefined)}
                />
                <IconButton
                  fontAwesomeIcon={faTrash}
                  onClick={() => {
                    deleteRow(sortedInvestments[i].investmentId);
                    setEditedRow(undefined);
                  }}
                />
              </>
            )}
          </td>
        </tr>,
      );
    }

    return returned;
  }, [
    investments,
    sortColumn,
    sortAscending,
    maxRows,
    editedRow,
    editDescription,
    editCurrentAmount,
    isEditCurrentAmountValid,
    editYearlyGrowthRate,
    isEditYearlyGrowthRateValid,
    saveInvestmentCallback,
    deleteRow,
  ]);

  const tableHeaders = useMemo(
    () =>
      ['Description', 'Current Amount', 'Yearly Growth Rate'].map(
        (headerName) => (
          <th
            key={`investment-history-th-${headerName}`}
            onClick={() => {
              if (headerName === sortColumn) {
                setSortAscending(!sortAscending);
              } else {
                setSortColumn(headerName);
                setSortAscending(false);
              }
            }}
          >
            {headerName}{' '}
            {headerName === sortColumn && (
              <FontAwesomeIcon
                icon={sortAscending ? faChevronUp : faChevronDown}
              />
            )}
          </th>
        ),
      ),
    [sortAscending, sortColumn],
  );

  return (
    <div>
      <Table hoverable striped>
        <thead>
          <tr>
            {tableHeaders}
            <th>Filter</th>
          </tr>
        </thead>
        <tbody className="purchases-body">
          {partitionedInvestments[currentPage - 1]}
        </tbody>
      </Table>
      <Pagination
        current={currentPage}
        total={partitionedInvestments.length}
        showFirstLast
        onChange={(pageNum) => setCurrentPage(pageNum)}
      />
    </div>
  );
}
