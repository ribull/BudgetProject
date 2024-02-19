import { forwardRef } from 'react';

import { Size } from 'react-bulma-components/src/components';
import ReactDatePicker from 'react-datepicker';

import 'react-datepicker/dist/react-datepicker.css';

interface DatePickerInputProps {
  onClick: () => void;
  value: string;
  size: Size;
}

const RefInput = forwardRef<HTMLInputElement, DatePickerInputProps>(
  ({ onClick, value, size }: DatePickerInputProps, ref) => (
    <input
      className={`input is-${size}`}
      ref={ref}
      onClick={onClick}
      value={value}
    />
  ),
);

interface BulmaDatepickerProps {
  onSelect: (date: Date | null) => void;
  size: Size;
  initialValue: Date;
}

export default function BulmaDatepicker({
  onSelect,
  size,
  initialValue,
}: BulmaDatepickerProps) {
  return (
    <ReactDatePicker
      selected={initialValue}
      onChange={(newDate) => onSelect(newDate)}
      customInput={<RefInput size={size} />}
    />
  );
}
