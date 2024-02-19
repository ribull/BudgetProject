import { useState } from 'react';
import { Button, Form } from 'react-bulma-components';

const { Input, Control, Field, Label, Help } = Form;

interface InsertWishlistItemProps {
  onSubmit: (description: string, amount: number, notes: string) => void;
}

export default function InsertWishlistItem({
  onSubmit,
}: InsertWishlistItemProps) {
  const [description, setDescription] = useState('');
  const [amount, setAmount] = useState('');
  const [notes, setNotes] = useState('');

  const amountIsValid = amount !== '' && !Number.isNaN(parseFloat(amount));

  return (
    <Field kind="group">
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
        <Label>Notes</Label>
        <Control>
          <Input
            value={notes}
            onChange={(event) => setNotes(event.currentTarget.value)}
            size="small"
          />
        </Control>
      </Field>
      <Field>
        <Label className="aligning-label">Transparent</Label>
        <Button
          onClick={() => {
            onSubmit(description, parseFloat(amount), notes);
            setDescription('');
            setAmount('');
            setNotes('');
          }}
        >
          Add Wishlist Item
        </Button>
      </Field>
    </Field>
  );
}
