import { ReactElement, useMemo, useState } from 'react';
import { Pagination, Table } from 'react-bulma-components';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faChevronDown,
  faChevronUp,
  faPencil,
} from '@fortawesome/free-solid-svg-icons';
import { Purchase } from '../../generated/budget_service';

interface PurchasesTableProps {
  purchases: Purchase[];
  maxRows: number;
  requestOlderPurchases: () => void;
}

export default function PurchasesTable({
  purchases,
  maxRows,
  requestOlderPurchases,
}: PurchasesTableProps) {
  const [currentPage, setCurrentPage] = useState(1);
  const [sortColumn, setSortColumn] = useState<string>('Date');
  const [sortAscending, setSortAscending] = useState(false);

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

      returned[returned.length - 1].push(
        <tr key={`purchases-table-tr-${sortedPurchases[i].purchaseId}`}>
          <td
            key={`purchases-table-cell-date-${sortedPurchases[i].purchaseId}`}
          >
            {sortedPurchases[i].date?.toDateString()}
          </td>
          <td
            key={`purchases-table-cell-desc-${sortedPurchases[i].purchaseId}`}
          >
            {sortedPurchases[i].description}
          </td>
          <td key={`purchases-table-cell-amt-${sortedPurchases[i].purchaseId}`}>
            {sortedPurchases[i].amount}
          </td>
          <td key={`purchases-table-cell-cat-${sortedPurchases[i].purchaseId}`}>
            {sortedPurchases[i].category}
          </td>
          <td>
            <FontAwesomeIcon icon={faPencil} />
          </td>
        </tr>,
      );
    }

    return returned;
  }, [purchases, sortColumn, sortAscending, maxRows]);

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
    <>
      <Table hoverable striped>
        <thead>
          <tr>
            {tableHeaders}
            <th>Filter</th>
          </tr>
        </thead>
        <tbody>{partitionedPurchases[currentPage - 1]}</tbody>
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
    </>
  );
}
