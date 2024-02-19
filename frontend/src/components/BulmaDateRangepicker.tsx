import { forwardRef } from 'react';

import { Size } from 'react-bulma-components/src/components';
import ReactDatePicker from 'react-datepicker';

import 'react-datepicker/dist/react-datepicker.css';
import { start } from 'repl';

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

interface BulmaDateRangepickerProps {
  onSelect: (startDate: Date | null, endDate: Date | null) => void;
  size: Size;
  initialStartValue: Date;
  initialEndValue: Date;
}

export default function BulmaDateRangepicker({
  onSelect,
  size,
  initialStartValue,
  initialEndValue,
}: BulmaDateRangepickerProps) {
  function onChange(dates: (Date | null)[]) {
    const [startDate, endDate] = dates;
    if (endDate !== null) {
      console.log(dates);
      onSelect(startDate, endDate);
    }
  }

  return (
    <ReactDatePicker
      startDate={initialStartValue}
      endDate={initialEndValue}
      onChange={(dates) => onChange(dates)}
      customInput={<RefInput size={size} />}
      selectsRange
    />
  );
}
