import { useState } from 'react';
import { Button, Form } from 'react-bulma-components';

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

  console.log(dateRange);

  return (
    <Field kind="group">
      <Field>
        <Label>Date Range</Label>
        <BulmaDateRangepicker
          onSelect={(newStartDate, newEndDate) => newStartDate !== null && newEndDate !== null && setDateRange([newStartDate, newEndDate])}
          initialStartValue={dateRange[0]}
          initialEndValue={dateRange[1]}
          size="small"
        />
      </Field>
      <Field>
        <Label>Earnings</Label>
        <Control>
          <Input
            value={earnings}
            onChange={(event) => setEarnings(event.currentTarget.value)}
            size="small"
          />
        </Control>
      </Field>
      <Field>
        <Label>Pre Tax Deductions</Label>
        <Control>
          <Input
            value={preTax}
            onChange={(event) => setPreTaxDeductions(event.currentTarget.value)}
            size="small"
          />
        </Control>
      </Field>
      <Field>
        <Label>Taxes</Label>
        <Control>
          <Input
            value={taxes}
            onChange={(event) => setTaxes(event.currentTarget.value)}
            size="small"
          />
        </Control>
      </Field>
      <Field>
        <Label>Post Tax Deductions</Label>
        <Control>
          <Input
            value={postTax}
            onChange={(event) => setPostTaxDeductions(event.currentTarget.value)}
            size="small"
          />
        </Control>
      </Field>
      <Field>
        <Label className="aligning-label">Transparent</Label>
        <Button onClick={() => {
          onSubmit(dateRange, parseFloat(earnings), parseFloat(preTax), parseFloat(taxes), parseFloat(postTax));
          setEarnings('');
          setPreTaxDeductions('');
          setTaxes('');
          setPostTaxDeductions('');
        }}>Add Pay History</Button>
      </Field>
    </Field>
  );
}
