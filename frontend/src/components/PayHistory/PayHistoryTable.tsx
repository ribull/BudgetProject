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
import { PayHistory } from '../../generated/budget_service';
import IconButton from '../IconButton';
import BulmaDatepicker from '../BulmaDatepicker';

const { Input } = Form;

interface PayHistoryTableProps {
  payHistories: PayHistory[];
  maxRows: number;
  saveEditedPayHistory: (
    payHistoryId: number,
    startDate: Date,
    endDate: Date,
    earnings: number,
    preTax: number,
    taxes: number,
    postTax: number,
  ) => void;
  deleteRow: (payHistoryId: number) => void;
}

export default function PurchasesTable({
  payHistories,
  maxRows,
  saveEditedPayHistory,
  deleteRow,
}: PayHistoryTableProps) {
  const [currentPage, setCurrentPage] = useState(1);
  const [sortColumn, setSortColumn] = useState<string>('End Date');
  const [sortAscending, setSortAscending] = useState(false);
  const [editedRow, setEditedRow] = useState<number>();

  const [editStartDate, setEditStartDate] = useState(new Date());
  const [editEndDate, setEditEndDate] = useState(new Date());
  const [editEarnings, setEditEarnings] = useState('');
  const [editPreTax, setEditPreTaxDeductions] = useState('');
  const [editTaxes, setEditTaxes] = useState('');
  const [editPostTax, setEditPostTaxDeductions] = useState('');

  function savePayHistoryCallback(payHistoryId: number) {
    saveEditedPayHistory(
      payHistoryId,
      editStartDate,
      editEndDate,
      parseFloat(editEarnings),
      parseFloat(editPreTax),
      parseFloat(editTaxes),
      parseFloat(editPostTax),
    );
  }

  const partitionedPayHistories = useMemo(() => {
    const sortedPayHistories = payHistories.sort((p1, p2) => {
      let result: number = 0;

      switch (sortColumn) {
        case 'Start Date':
          result =
            (p1.payPeriodStartDate?.getTime() ?? Math.max()) -
            (p2.payPeriodStartDate?.getTime() ?? Math.max());
          break;
        case 'End Date':
          result =
            (p1.payPeriodEndDate?.getTime() ?? Math.max()) -
            (p2.payPeriodEndDate?.getTime() ?? Math.max());
          break;
        case 'Earnings':
          result = p1.earnings - p2.earnings;
          break;
        case 'Pre Tax Deductions':
          result = p1.preTaxDeductions - p2.preTaxDeductions;
          break;
        case 'Taxes':
          result = p1.taxes - p2.taxes;
          break;
        case 'Post Tax Deductions':
          result = p1.postTaxDeductions - p2.postTaxDeductions;
          break;
      }

      if (!sortAscending) {
        result *= -1;
      }

      return result;
    });

    const returned: ReactElement[][] = [];
    for (let i = 0; i < sortedPayHistories.length; i += 1) {
      if (i % maxRows === 0) {
        returned.push([]);
      }

      const notEditing = editedRow !== sortedPayHistories[i].payHistoryId;

      returned[returned.length - 1].push(
        <tr key={`pay-history-table-tr-${sortedPayHistories[i].payHistoryId}`}>
          <td
            key={`pay-history-table-cell-sdate-${sortedPayHistories[i].payHistoryId}`}
          >
            {notEditing ? (
              sortedPayHistories[i].payPeriodStartDate?.toLocaleDateString()
            ) : (
              <BulmaDatepicker
                onSelect={(newDate) => setEditStartDate(newDate)}
                initialValue={editStartDate}
                size="small"
              />
            )}
          </td>
          <td
            key={`pay-history-table-cell-edate-${payHistories[i].payHistoryId}`}
          >
            {notEditing ? (
              payHistories[i].payPeriodEndDate?.toDateString()
            ) : (
              <BulmaDatepicker
                onSelect={(newDate) => setEditEndDate(newDate)}
                initialValue={editEndDate}
                size="small"
              />
            )}
          </td>
          <td
            key={`pay-history-table-cell-earn-${payHistories[i].earnings}`}
          >
            {notEditing ? (
              payHistories[i].earnings
            ) : (
              <Input
                value={editEarnings}
                onChange={(event) => setEditEarnings(event.currentTarget.value)}
              />
            )}
          </td>
          <td
            key={`pay-history-table-cell-pre-${payHistories[i].preTaxDeductions}`}
          >
            {notEditing ? (
              payHistories[i].preTaxDeductions
            ) : (
              <Input
                value={editPreTax}
                onChange={(event) => setEditPreTaxDeductions(event.currentTarget.value)}
              />
            )}
          </td>
          <td
            key={`pay-history-table-cell-tax-${payHistories[i].taxes}`}
          >
            {notEditing ? (
              payHistories[i].taxes
            ) : (
              <Input
                value={editTaxes}
                onChange={(event) => setEditTaxes(event.currentTarget.value)}
              />
            )}
          </td>
          <td
            key={`pay-history-table-cell-post-${payHistories[i].postTaxDeductions}`}
          >
            {notEditing ? (
              payHistories[i].postTaxDeductions
            ) : (
              <Input
                value={editPostTax}
                onChange={(event) => setEditPostTaxDeductions(event.currentTarget.value)}
              />
            )}
          </td>
          <td className="td-sm">
            {editedRow === undefined && (
              <IconButton
                fontAwesomeIcon={faPencil}
                onClick={() => {
                  setEditedRow(payHistories[i].payHistoryId);
                  setEditStartDate(payHistories[i].payPeriodStartDate ?? new Date());
                  setEditEndDate(payHistories[i].payPeriodEndDate ?? new Date());
                  setEditEarnings(`${payHistories[i].earnings}`);
                  setEditPreTaxDeductions(`${payHistories[i].preTaxDeductions}`);
                  setEditTaxes(`${payHistories[i].taxes}`);
                  setEditPostTaxDeductions(`${payHistories[i].postTaxDeductions}`);
                }}
              />
            )}
            {editedRow === payHistories[i].payHistoryId && (
              <>
                <IconButton
                  fontAwesomeIcon={faFloppyDisk}
                  onClick={() => {
                    savePayHistoryCallback(payHistories[i].payHistoryId);
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
                    deleteRow(payHistories[i].payHistoryId);
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
    sortColumn,
    sortAscending,
    maxRows,
    editedRow,
    deleteRow,
    editStartDate,
    editEndDate,
    editEarnings,
    editPreTax,
    editTaxes,
    editPostTax,
  ]);

  const tableHeaders = useMemo(
    () =>
      [
        'Start Date',
        'End Date',
        'Earnings',
        'Pre Tax Deductions',
        'Taxes',
        'Post Tax Deductions'
      ].map((headerName) => (
        <th
          key={`purchase-history-th-${headerName}`}
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
      )),
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
        <tbody className="pay-histories-body">
          {partitionedPayHistories[currentPage - 1]}
        </tbody>
      </Table>
      <Pagination
        current={currentPage}
        total={partitionedPayHistories.length}
        showFirstLast
        onChange={(pageNum) => setCurrentPage(pageNum)}
      />
    </div>
  );
}
