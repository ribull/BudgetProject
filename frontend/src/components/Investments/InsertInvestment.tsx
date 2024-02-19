import { useState } from 'react';
import { Button, Form } from 'react-bulma-components';

const { Input, Control, Field, Label, Help } = Form;

interface InsertInvestmentProps {
  onSubmit: (
    description: string,
    currentAmount: number,
    yearlyGrowthRate: number,
  ) => void;
}

export default function InsertInvestment({ onSubmit }: InsertInvestmentProps) {
  const [description, setDescription] = useState('');
  const [currentAmount, setCurrentAmount] = useState('');
  const [yearlyGrowthRate, setYearlyGrowthRate] = useState('');

  const currentAmountIsValid =
    currentAmount !== '' && !Number.isNaN(parseFloat(currentAmount));
  const growthRateIsValid =
    yearlyGrowthRate !== '' && !Number.isNaN(parseFloat(yearlyGrowthRate));

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
        <Label>Current Amount</Label>
        <Control>
          <Input
            value={currentAmount}
            onChange={(event) => setCurrentAmount(event.currentTarget.value)}
            color={currentAmountIsValid ? 'text' : 'danger'}
            size="small"
          />
          <Help invisible={currentAmount === '' || currentAmountIsValid}>
            {currentAmount} is not a number
          </Help>
        </Control>
      </Field>
      <Field>
        <Label>Yearly Growth Rate</Label>
        <Control>
          <Input
            value={yearlyGrowthRate}
            onChange={(event) => setYearlyGrowthRate(event.currentTarget.value)}
            color={growthRateIsValid ? 'text' : 'danger'}
            size="small"
          />
          <Help invisible={yearlyGrowthRate === '' || growthRateIsValid}>
            {yearlyGrowthRate} is not a number
          </Help>
        </Control>
      </Field>
      <Field>
        <Label className="aligning-label">Transparent</Label>
        <Button
          onClick={() => {
            onSubmit(
              description,
              parseFloat(currentAmount),
              parseFloat(yearlyGrowthRate),
            );
            setDescription('');
            setCurrentAmount('');
            setYearlyGrowthRate('');
          }}
        >
          Add Investment
        </Button>
      </Field>
    </Field>
  );
}
