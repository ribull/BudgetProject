import { useState } from 'react';
import { Button, Form } from 'react-bulma-components';

import { Purchase } from '../../generated/budget_service';
import BulmaDateRangepicker from '../BulmaDateRangepicker';

const { Input, Control, Field, Label, Help } = Form;

interface InsertPayHistoryProps {
  onSubmit: (dateRange: Date[], earnings: number, preTax: number, tax: number, postTax: number) => void;
}

export default function InsertPayHistory({ onSubmit }: InsertPayHistoryProps) {
  const [dateRange, setDateRange] = useState([new Date(), new Date()]);
  const [earnings, setEarnings] = useState('');
  const [preTax, setPreTaxDeductions] = useState('');
  const [taxes, setTaxes] = useState('');
  const [postTax, setPostTaxDeductions] = useState('');

  return (
    <Field kind="group">
      <Field>
        <Label>Date Range</Label>
        <BulmaDateRangepicker
          onSelect={(newStartDate, newEndDate) => setDateRange([newStartDate, newEndDate])}
          initialStartValue={dateRange[0]}
          initialEndValue={dateRange[1]}
        />
      </Field>
      <Field>
        <Label>Earnings</Label>
        <Control>
          <Input
            value={earnings}
            onChange={(event) => setEarnings(event.currentTarget.value)}
          />
        </Control>
      </Field>
      <Field>
        <Label>Pre Tax Deductions</Label>
        <Control>
          <Input
            value={preTax}
            onChange={(event) => setPreTaxDeductions(event.currentTarget.value)}
          />
        </Control>
      </Field>
      <Field>
        <Label>Taxes</Label>
        <Control>
          <Input
            value={taxes}
            onChange={(event) => setTaxes(event.currentTarget.value)}
          />
        </Control>
      </Field>
      <Field>
        <Label>Post Tax Deductions</Label>
        <Control>
          <Input
            value={postTax}
            onChange={(event) => setPostTaxDeductions(event.currentTarget.value)}
          />
        </Control>
      </Field>
      <Field>
        <Label className="aligning-label">Transparent</Label>
        <Button onClick={() => {
          onSubmit(dateRange, parseFloat(earnings), parseFloat(preTax), parseFloat(taxes), parseFloat(postTax));
          setDateRange([new Date(), new Date()]);
          setEarnings('');
          setPreTaxDeductions('');
          setTaxes('');
          setPostTaxDeductions('');
        }}>Add Pay History</Button>
      </Field>
    </Field>
  );
}
