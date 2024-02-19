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
import { Saved } from '../../generated/budget_service';
import IconButton from '../IconButton';
import BulmaDatepicker from '../BulmaDatepicker';

const { Input } = Form;

interface SavingsProps {
  savings: Saved[];
  maxRows: number;
  saveEditedSaved: (
    savedId: number,
    date: Date,
    description: string,
    amount: number,
  ) => void;
  deleteRow: (savedId: number) => void;
}

export default function SavingsTable({
  savings,
  maxRows,
  saveEditedSaved,
  deleteRow,
}: SavingsProps) {
  const [currentPage, setCurrentPage] = useState(1);
  const [sortColumn, setSortColumn] = useState<string>('Date');
  const [sortAscending, setSortAscending] = useState(false);
  const [editedRow, setEditedRow] = useState<number>();

  const [editDate, setEditDate] = useState(new Date());
  const [editDescription, setEditDescription] = useState('');
  const [editAmount, setEditAmount] = useState('');

  const saveInvestmentCallback = useCallback(
    (savedId: number) => {
      const amt = parseFloat(editAmount);
      if (!Number.isNaN(amt)) {
        saveEditedSaved(savedId, editDate, editDescription, amt);
      }
    },
    [editAmount, editDate, editDescription, saveEditedSaved],
  );

  const isEditAmountValid = !Number.isNaN(parseFloat(editAmount));

  const paritionedSavings = useMemo(() => {
    const sortedSavings = savings.sort((s1, s2) => {
      let result: number = 0;
      if (sortColumn === 'Date') {
        result =
          (s1.date?.getTime() ?? Math.max()) -
          (s2.date?.getTime() ?? Math.max());
      } else if (sortColumn === 'Description') {
        result = s2.description.localeCompare(s1.description);
      } else if (sortColumn === 'Amount') {
        result = s1.amount - s2.amount;
      }

      if (!sortAscending) {
        result *= -1;
      }

      return result;
    });

    const returned: ReactElement[][] = [];
    for (let i = 0; i < sortedSavings.length; i += 1) {
      if (i % maxRows === 0) {
        returned.push([]);
      }

      const notEditing = editedRow !== sortedSavings[i].savedId;

      returned[returned.length - 1].push(
        <tr key={`saved-table-tr-${sortedSavings[i].savedId}`}>
          <td
            key={`saved-table-cell-date-${sortedSavings[i].savedId}`}
            className="td-med"
          >
            {notEditing ? (
              sortedSavings[i].date?.toDateString()
            ) : (
              <BulmaDatepicker
                onSelect={(newDate) => newDate !== null && setEditDate(newDate)}
                initialValue={editDate}
                size="small"
              />
            )}
          </td>
          <td
            key={`saved-table-cell-desc-${sortedSavings[i].savedId}`}
            className="td-lg"
          >
            {notEditing ? (
              sortedSavings[i].description
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
            key={`saved-table-cell-amt-${sortedSavings[i].savedId}`}
            className="td-sm"
          >
            {notEditing ? (
              sortedSavings[i].amount
            ) : (
              <Input
                value={editAmount}
                onChange={(event) => setEditAmount(event.currentTarget.value)}
                color={isEditAmountValid ? 'text' : 'danger'}
                size="small"
              />
            )}
          </td>
          <td className="td-sm">
            {editedRow === undefined && (
              <IconButton
                fontAwesomeIcon={faPencil}
                onClick={() => {
                  setEditedRow(sortedSavings[i].savedId);
                  setEditDate(sortedSavings[i].date ?? new Date());
                  setEditDescription(sortedSavings[i].description);
                  setEditAmount(`${sortedSavings[i].amount}`);
                }}
              />
            )}
            {editedRow === sortedSavings[i].savedId && (
              <>
                <IconButton
                  fontAwesomeIcon={faFloppyDisk}
                  onClick={() => {
                    saveInvestmentCallback(sortedSavings[i].savedId);
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
                    deleteRow(sortedSavings[i].savedId);
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
    editDescription,
    saveInvestmentCallback,
    deleteRow,
  ]);

  const tableHeaders = useMemo(
    () =>
      ['Date', 'Description', 'Amount'].map(
        (headerName) => (
          <th
            key={`saved-history-th-${headerName}`}
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
          {paritionedSavings[currentPage - 1]}
        </tbody>
      </Table>
      <Pagination
        current={currentPage}
        total={paritionedSavings.length}
        showFirstLast
        onChange={(pageNum) => setCurrentPage(pageNum)}
      />
    </div>
  );
}
