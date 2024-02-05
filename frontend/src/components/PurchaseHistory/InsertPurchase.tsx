import { useState } from 'react';
import { Button, Form } from 'react-bulma-components';
import AutoComplete from '../Autocomplete';

import { Purchase } from '../../generated/budget_service';
import BulmaDatepicker from '../BulmaDatepicker';

const { Input, Control, Field, Label, Help } = Form;

interface InsertPurchaseProps {
  onSubmit: (date: Date, description: string, amount: number, category: string) => void;
  topPurchases: Purchase[];
  existingCategories: string[];
}

export default function InsertPurchase({ onSubmit, topPurchases, existingCategories }: InsertPurchaseProps) {
  const [date, setDate] = useState(new Date());
  const [description, setDescription] = useState('');
  const [amount, setAmount] = useState('');
  const [category, setCategory] = useState('');

  const categoryIsWarn = !existingCategories.includes(category);
  const amountIsValid = amount !== '' && !Number.isNaN(parseFloat(amount));

  return (
    <Field kind="group">
      <Field>
        <Label>Date</Label>
        <BulmaDatepicker
          onSelect={(newDate) => setDate(newDate)}
          initialValue={new Date()}
        />
      </Field>
      <Field>
        <Label>Description</Label>
        <AutoComplete
          value={description}
          possibleMatches={topPurchases.map((purchase) => purchase.description)}
          color="text"
          onSelection={(selection, autoCompleted) => {
            // Find the purchase it autocompleted to if autocompleted
            if (autoCompleted) {
              const purchase = topPurchases.find(
                (p) => p.description === selection,
              );

              if (purchase !== undefined) {
                setAmount(`${purchase.amount}`);
                setCategory(purchase.category);
              }
            }

            setDescription(selection);
          }}
        />
      </Field>
      <Field>
        <Label>Amount</Label>
        <Control>
          <Input
            value={amount}
            onChange={(event) => setAmount(event.currentTarget.value)}
            color={amountIsValid ? 'text' : 'danger'}
          />
          <Help invisible={amount === '' || amountIsValid}>{amount} is not a number</Help>
        </Control>
      </Field>
      <Field>
        <Label>Category</Label>
        <AutoComplete
          value={category}
          possibleMatches={existingCategories}
          onSelection={(selection) => setCategory(selection)}
          color={categoryIsWarn ? 'text' : 'warning'}
        />
        <Help invisible={
          category === ''
          || !categoryIsWarn
        }>
          This category does not currently exist. It will be created.
        </Help>
      </Field>
      <Field>
        <Label className="aligning-label">Transparent</Label>
        <Button onClick={() => onSubmit(date, description, parseFloat(amount), category)}>Add Purchase</Button>
      </Field>
    </Field>
  );
}
