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
import { FuturePurchase } from '../../generated/budget_service';
import IconButton from '../IconButton';
import BulmaDatepicker from '../BulmaDatepicker';
import AutoComplete from '../Autocomplete';

const { Input } = Form;

interface FuturePurchasesTableProps {
  futurePurchases: FuturePurchase[];
  maxRows: number;
  existingCategories: string[];
  saveEditedPurchase: (
    futurePurchaseId: number,
    date: Date,
    description: string,
    amount: number,
    category: string,
  ) => void;
  deleteRow: (futurePurchaseId: number) => void;
}

export default function FuturePurchasesTable({
  futurePurchases,
  maxRows,
  existingCategories,
  saveEditedPurchase,
  deleteRow,
}: FuturePurchasesTableProps) {
  const [currentPage, setCurrentPage] = useState(1);
  const [sortColumn, setSortColumn] = useState<string>('Date');
  const [sortAscending, setSortAscending] = useState(true);
  const [editedRow, setEditedRow] = useState<number>();

  const [editDate, setEditDate] = useState(new Date());
  const [editDescription, setEditDescription] = useState('');
  const [editAmount, setEditAmount] = useState('');
  const [editCategory, setEditCategory] = useState('');

  function savePurchaseCallback(futurePurchaseId: number) {
    const amt = parseFloat(editAmount);
    if (!Number.isNaN(amt)) {
      saveEditedPurchase(
        futurePurchaseId,
        editDate,
        editDescription,
        parseFloat(editAmount),
        editCategory,
      );
    }
  }

  const isEditAmountValid = !Number.isNaN(parseFloat(editAmount));
  const editCategoryIsWarn = existingCategories.includes(editCategory);

  const partitionedPurchases = useMemo(() => {
    const sortedPurchases = futurePurchases.sort((p1, p2) => {
      let result: number = 0;
      if (sortColumn === 'Date') {
        result =
          (p1.date?.getTime() ?? Math.max()) -
          (p2.date?.getTime() ?? Math.max());
      } else if (sortColumn === 'Description') {
        result = p2.description.localeCompare(p1.description);
      } else if (sortColumn === 'Amount') {
        result = p1.amount - p2.amount;
      } else if (sortColumn === 'Category') {
        result = p2.category.localeCompare(p1.category);
      }

      if (!sortAscending) {
        result *= -1;
      }

      return result;
    });

    const returned: ReactElement[][] = [];
    for (let i = 0; i < sortedPurchases.length; i += 1) {
      if (i % maxRows === 0) {
        returned.push([]);
      }

      const notEditing = editedRow !== sortedPurchases[i].futurePurchaseId;

      returned[returned.length - 1].push(
        <tr key={`purchases-table-tr-${sortedPurchases[i].futurePurchaseId}`}>
          <td
            key={`purchases-table-cell-date-${sortedPurchases[i].futurePurchaseId}`}
            className="td-med"
          >
            {notEditing ? (
              sortedPurchases[i].date?.toDateString()
            ) : (
              <BulmaDatepicker
                onSelect={(newDate) => newDate !== null && setEditDate(newDate)}
                initialValue={editDate}
                size="small"
              />
            )}
          </td>
          <td
            key={`purchases-table-cell-desc-${sortedPurchases[i].futurePurchaseId}`}
            className="td-lg"
          >
            {notEditing ? (
              sortedPurchases[i].description
            ) : (
              <Input
                value={editDescription}
                onChange={(event) => setEditDescription(event.currentTarget.value)}
                size="small"
              />
            )}
          </td>
          <td
            key={`purchases-table-cell-amt-${sortedPurchases[i].futurePurchaseId}`}
            className="td-sm"
          >
            {notEditing ? (
              sortedPurchases[i].amount
            ) : (
              <Input
                value={editAmount}
                onChange={(event) => setEditAmount(event.currentTarget.value)}
                color={isEditAmountValid ? 'text' : 'danger'}
                size="small"
              />
            )}
          </td>
          <td
            key={`purchases-table-cell-cat-${sortedPurchases[i].futurePurchaseId}`}
            className="td-med"
          >
            {notEditing ? (
              sortedPurchases[i].category
            ) : (
              <AutoComplete
                value={editCategory}
                possibleMatches={existingCategories}
                onSelection={(selection) => setEditCategory(selection)}
                color={editCategoryIsWarn ? 'text' : 'warning'}
              />
            )}
          </td>
          <td className="td-sm">
            {editedRow === undefined && (
              <IconButton
                fontAwesomeIcon={faPencil}
                onClick={() => {
                  setEditedRow(sortedPurchases[i].futurePurchaseId);
                  setEditDate(sortedPurchases[i].date ?? new Date());
                  setEditDescription(sortedPurchases[i].description);
                  setEditAmount(`${sortedPurchases[i].amount}`);
                  setEditCategory(sortedPurchases[i].category);
                }}
              />
            )}
            {editedRow === sortedPurchases[i].futurePurchaseId && (
              <>
                <IconButton
                  fontAwesomeIcon={faFloppyDisk}
                  onClick={() => {
                    savePurchaseCallback(sortedPurchases[i].futurePurchaseId);
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
                    deleteRow(sortedPurchases[i].futurePurchaseId);
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
    futurePurchases,
    sortColumn,
    sortAscending,
    maxRows,
    editedRow,
    editDate,
    editDescription,
    editAmount,
    editCategory,
    deleteRow,
    saveEditedPurchase,
  ]);

  const tableHeaders = useMemo(
    () =>
      ['Date', 'Description', 'Amount', 'Category'].map((headerName) => (
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
        <tbody className="purchases-body">
          {partitionedPurchases[currentPage - 1]}
        </tbody>
      </Table>
      <Pagination
        current={currentPage}
        total={partitionedPurchases.length}
        showFirstLast
        onChange={(pageNum) => setCurrentPage(pageNum)}
      />
    </div>
  );
}
