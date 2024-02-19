import { useState } from 'react';
import { Button, Form } from 'react-bulma-components';
import BulmaDatepicker from '../BulmaDatepicker';

const { Input, Control, Field, Label, Help } = Form;

interface InsertSavedProps {
  onSubmit: (date: Date, description: string, amount: number) => void;
}

export default function InsertSaved({ onSubmit }: InsertSavedProps) {
  const [date, setDate] = useState(new Date());
  const [description, setDescription] = useState('');
  const [amount, setAmount] = useState('');

  const amountIsValid = amount !== '' && !Number.isNaN(parseFloat(amount));

  return (
    <Field kind="group">
      <Field>
        <Label>Date</Label>
        <BulmaDatepicker
          size="small"
          onSelect={(newDate) => newDate !== null && setDate(newDate)}
          initialValue={date}
        />
      </Field>
      <Field>
        <Label>Description</Label>
        <Input
          value={description}
          onChange={(event) => setDescription(event.currentTarget.value)}
          size="small"
        />
      </Field>
      <Field>
        <Label>Amount</Label>
        <Control>
          <Input
            value={amount}
            onChange={(event) => setAmount(event.currentTarget.value)}
            color={amountIsValid ? 'text' : 'danger'}
            size="small"
          />
          <Help invisible={amount === '' || amountIsValid}>
            {amount} is not a number
          </Help>
        </Control>
      </Field>
      <Field>
        <Label className="aligning-label">Transparent</Label>
        <Button
          onClick={() => {
            onSubmit(
              date,
              description,
              parseFloat(amount),
            );
            setDescription('');
            setAmount('');
            setDate(new Date());
          }}
        >
          Add Saving
        </Button>
      </Field>
    </Field>
  );
}
