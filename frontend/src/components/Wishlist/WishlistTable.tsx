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
import { WishlistItem } from '../../generated/budget_service';
import IconButton from '../IconButton';

const { Input } = Form;

interface WishlistTableProps {
  wishlist: WishlistItem[];
  maxRows: number;
  saveEditedWishlistItem: (
    wishlistItemId: number,
    description: string,
    amount: number,
    notes: string,
  ) => void;
  deleteRow: (investmentId: number) => void;
}

export default function WishlistTable({
  wishlist,
  maxRows,
  saveEditedWishlistItem,
  deleteRow,
}: WishlistTableProps) {
  const [currentPage, setCurrentPage] = useState(1);
  const [sortColumn, setSortColumn] = useState<string>('');
  const [sortAscending, setSortAscending] = useState(false);
  const [editedRow, setEditedRow] = useState<number>();

  const [editDescription, setEditDescription] = useState('');
  const [editAmount, setEditAmount] = useState('');
  const [editNotes, setEditNotes] = useState('');

  const saveWishlistItemCallback = useCallback(
    (wishlistItemId: number) => {
      const amt = parseFloat(editAmount);
      if (!Number.isNaN(amt)) {
        saveEditedWishlistItem(wishlistItemId, editDescription, amt, editNotes);
      }
    },
    [editAmount, editDescription, editNotes, saveEditedWishlistItem],
  );

  const isEditAmountValid = !Number.isNaN(parseFloat(editAmount));

  const partitionedWishlist = useMemo(() => {
    const sortedWishlist = wishlist.sort((w1, w2) => {
      let result: number = 0;
      if (sortColumn === 'Description') {
        result = w2.description.localeCompare(w1.description);
      } else if (sortColumn === 'Amount') {
        result =
          (w1.amount ?? Number.MAX_VALUE) - (w2.amount ?? Number.MAX_VALUE);
      }

      if (!sortAscending) {
        result *= -1;
      }

      return result;
    });

    const returned: ReactElement[][] = [];
    for (let i = 0; i < sortedWishlist.length; i += 1) {
      if (i % maxRows === 0) {
        returned.push([]);
      }

      const notEditing = editedRow !== sortedWishlist[i].wishlistItemId;

      returned[returned.length - 1].push(
        <tr key={`wishlist-table-tr-${sortedWishlist[i].wishlistItemId}`}>
          <td
            key={`wishlist-table-cell-desc-${sortedWishlist[i].wishlistItemId}`}
            className="td-lg"
          >
            {notEditing ? (
              sortedWishlist[i].description
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
            key={`wishlist-table-cell-curr-amt-${sortedWishlist[i].wishlistItemId}`}
            className="td-sm"
          >
            {notEditing ? (
              sortedWishlist[i].amount
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
            key={`wishlist-table-cell-notes-${sortedWishlist[i].notes}`}
            className="td-med"
          >
            {notEditing ? (
              sortedWishlist[i].notes
            ) : (
              <Input
                value={editNotes}
                onChange={(event) => setEditNotes(event.currentTarget.value)}
                size="small"
              />
            )}
          </td>
          <td className="td-sm">
            {editedRow === undefined && (
              <IconButton
                fontAwesomeIcon={faPencil}
                onClick={() => {
                  setEditedRow(sortedWishlist[i].wishlistItemId);
                  setEditDescription(sortedWishlist[i].description);
                  setEditAmount(`${sortedWishlist[i].amount}`);
                  setEditNotes(sortedWishlist[i].notes);
                }}
              />
            )}
            {editedRow === sortedWishlist[i].wishlistItemId && (
              <>
                <IconButton
                  fontAwesomeIcon={faFloppyDisk}
                  onClick={() => {
                    saveWishlistItemCallback(sortedWishlist[i].wishlistItemId);
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
                    deleteRow(sortedWishlist[i].wishlistItemId);
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
    deleteRow,
    editAmount,
    editDescription,
    editNotes,
    editedRow,
    isEditAmountValid,
    maxRows,
    saveWishlistItemCallback,
    sortAscending,
    sortColumn,
    wishlist,
  ]);

  const tableHeaders = useMemo(
    () =>
      ['Description', 'Amount', 'Notes'].map((headerName) => (
        <th
          key={`wishlist-history-th-${headerName}`}
          onClick={() => {
            if (headerName === sortColumn) {
              setSortAscending(!sortAscending);
            } else if (headerName !== 'Notes') {
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
          {partitionedWishlist[currentPage - 1]}
        </tbody>
      </Table>
      <Pagination
        current={currentPage}
        total={partitionedWishlist.length}
        showFirstLast
        onChange={(pageNum) => setCurrentPage(pageNum)}
      />
    </div>
  );
}
