import { useState } from 'react';
import { Button, Dropdown, Form } from 'react-bulma-components';
import AutoComplete from '../Autocomplete';

import BulmaDatepicker from '../BulmaDatepicker';
import { isString } from '../../helpers/TypeSafety';

const { Input, Control, Field, Label, Help } = Form;

interface InsertFuturePurchaseProps {
  onSubmit: (date: Date, description: string, amount: number, category: string) => void;
  existingCategories: string[];
}

export default function InsertFuturePurchase({ onSubmit, existingCategories }: InsertFuturePurchaseProps) {
  const [date, setDate] = useState(new Date());
  const [description, setDescription] = useState('');
  const [amount, setAmount] = useState('');
  const [category, setCategory] = useState('');
  const [repeating, setRepeating] = useState('None');

  const categoryIsWarn = !existingCategories.includes(category);
  const amountIsValid = amount !== '' && !Number.isNaN(parseFloat(amount));

  return (
    <Field kind="group">
      <Field>
        <Label>Date</Label>
        <BulmaDatepicker
          onSelect={(newDate) => newDate !== null && setDate(newDate)}
          initialValue={date}
          size="small"
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
        <Label>Repeating</Label>
        <Dropdown label={repeating} onChange={(event) => isString(event) && setRepeating(event)}>
          <Dropdown.Item value="None">
            None
          </Dropdown.Item>
          <Dropdown.Item value="Weekly">
            Weekly
          </Dropdown.Item>
          <Dropdown.Item value="Monthly">
            Monthly
          </Dropdown.Item>
          <Dropdown.Item value="Yearly">
            Yearly
          </Dropdown.Item>
        </Dropdown>
      </Field>
      <Field>
        <Label className="aligning-label">Transparent</Label>
        <Button onClick={() => {
          onSubmit(date, description, parseFloat(amount), category);
          setDate(new Date());
          setDescription('');
          setAmount('');
          setCategory('');
          setRepeating('None');
        }}>Add Future Purchase</Button>
      </Field>
    </Field>
  );
}
