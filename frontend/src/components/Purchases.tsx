import Table from 'react-bootstrap/Table';
import { useState, useCallback } from 'react';

import { ApiStatus } from '../model/ApiStatus';
import { uploadFile } from '../helpers/RestApiHelper';
import * as constants from '../constants';
import { BudgetServiceClient } from '../jsproto/Budget_serviceServiceClientPb';
import { GetPurchasesRequest, GetPurchasesResponse, Purchase } from '../jsproto/budget_service_pb';

export default function Purchases() {
  const [purchases, setPurchases] = useState<Array<Purchase>>();

  return (
    <Table>
      <thead>
        <tr>
          <th>Date</th>
          <th>Description</th>
          <th>Amount</th>
          <th>Category</th>
        </tr>
      </thead>
      <tbody>
        {purchases?.map(purchase => (
          <tr>
            <td>{purchase.getDate()}</td>
            <td>{purchase.getDescription()}</td>
            <td>{purchase.getAmount()}</td>
            <td>{purchase.getCategory()}</td>
          </tr>
        ))}
      </tbody>
    </Table>
  )
}