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
import { Purchase } from '../../generated/budget_service';
import IconButton from '../IconButton';
import BulmaDatepicker from '../BulmaDatepicker';
import AutoComplete from '../Autocomplete';

const { Input } = Form;

interface PurchasesTableProps {
  purchases: Purchase[];
  maxRows: number;
  topPurchases: Purchase[];
  existingCategories: string[];
  requestOlderPurchases: () => void;
  saveEditedPurchase: (
    purchaseId: number,
    date: Date,
    description: string,
    amount: number,
    category: string,
  ) => void;
  deleteRow: (purchaseId: number) => void;
}

export default function PurchasesTable({
  purchases,
  maxRows,
  topPurchases,
  existingCategories,
  requestOlderPurchases,
  saveEditedPurchase,
  deleteRow,
}: PurchasesTableProps) {
  const [currentPage, setCurrentPage] = useState(1);
  const [sortColumn, setSortColumn] = useState<string>('Date');
  const [sortAscending, setSortAscending] = useState(false);
  const [editedRow, setEditedRow] = useState<number>();

  const [editDate, setEditDate] = useState(new Date());
  const [editDescription, setEditDescription] = useState('');
  const [editAmount, setEditAmount] = useState('');
  const [editCategory, setEditCategory] = useState('');

  const savePurchaseCallback = useCallback(
    (purchaseId: number) => {
      saveEditedPurchase(
        purchaseId,
        editDate,
        editDescription,
        parseFloat(editAmount),
        editCategory,
      );
    },
    [editDate, editDescription, editAmount, editCategory, saveEditedPurchase],
  );

  const isEditAmountValid = !Number.isNaN(parseFloat(editAmount));
  const editCategoryIsWarn = existingCategories.includes(editCategory);

  const partitionedPurchases = useMemo(() => {
    const sortedPurchases = purchases.sort((p1, p2) => {
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

      const notEditing = editedRow !== sortedPurchases[i].purchaseId;

      returned[returned.length - 1].push(
        <tr key={`purchases-table-tr-${sortedPurchases[i].purchaseId}`}>
          <td
            key={`purchases-table-cell-date-${sortedPurchases[i].purchaseId}`}
            className="td-med"
          >
            {notEditing ? (
              sortedPurchases[i].date?.toDateString()
            ) : (
              <BulmaDatepicker
                onSelect={(newDate) => setEditDate(newDate)}
                initialValue={editDate}
                size="small"
              />
            )}
          </td>
          <td
            key={`purchases-table-cell-desc-${sortedPurchases[i].purchaseId}`}
            className="td-lg"
          >
            {notEditing ? (
              sortedPurchases[i].description
            ) : (
              <AutoComplete
                value={editDescription}
                possibleMatches={topPurchases.map((purchase) => purchase.description)}
                color="text"
                onSelection={(selection, autoCompleted) => {
                  // Find the purchase it autocompleted to if autocompleted
                  if (autoCompleted) {
                    const purchase = topPurchases.find(
                      (p) => p.description === selection,
                    );

                    if (purchase !== undefined) {
                      setEditAmount(`${purchase.amount}`);
                      setEditCategory(purchase.category);
                    }
                  }

                  setEditDescription(selection);
                }}
              />
            )}
          </td>
          <td
            key={`purchases-table-cell-amt-${sortedPurchases[i].purchaseId}`}
            className="td-sm"
          >
            {notEditing ? (
              sortedPurchases[i].amount
            ) : (
              <Input
                value={editAmount}
                onChange={(event) => setEditAmount(event.currentTarget.value)}
                color={isEditAmountValid ? 'text' : 'danger'}
              />
            )}
          </td>
          <td
            key={`purchases-table-cell-cat-${sortedPurchases[i].purchaseId}`}
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
                  setEditedRow(sortedPurchases[i].purchaseId);
                  setEditDate(sortedPurchases[i].date ?? new Date());
                  setEditDescription(sortedPurchases[i].description);
                  setEditAmount(`${sortedPurchases[i].amount}`);
                  setEditCategory(sortedPurchases[i].category);
                }}
              />
            )}
            {editedRow === sortedPurchases[i].purchaseId && (
              <>
                <IconButton
                  fontAwesomeIcon={faFloppyDisk}
                  onClick={() =>
                    savePurchaseCallback(sortedPurchases[i].purchaseId)
                  }
                />
                <IconButton
                  fontAwesomeIcon={faXmark}
                  onClick={() => setEditedRow(undefined)}
                />
                <IconButton
                  fontAwesomeIcon={faTrash}
                  onClick={() => {
                    deleteRow(sortedPurchases[i].purchaseId);
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
    purchases,
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
        onChange={(pageNum) => {
          setCurrentPage(pageNum);
          if (pageNum + 1 >= partitionedPurchases.length) {
            // If it's one of the last two
            requestOlderPurchases();
          }
        }}
      />
    </div>
  );
}
