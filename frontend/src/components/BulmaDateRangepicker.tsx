import React, { useEffect } from 'react';
import bulmaCalendar from 'bulma-calendar';

import { Form } from 'react-bulma-components';
import { Size } from 'react-bulma-components/src/components';

const { Input } = Form;

interface BulmaDateRangepickerProps {
  onSelect: (startDate: Date, endDate: Date) => void;
  size?: Size;
  initialStartValue?: Date;
  initialEndValue?: Date;
}

export default function BulmaDateRangepicker({
  onSelect,
  size,
  initialStartValue,
  initialEndValue,
}: BulmaDateRangepickerProps) {
  useEffect(() => {
    const calendars = bulmaCalendar.attach('[type="date"]', {
      startDate: initialStartValue,
      endDate: initialEndValue,
      isRange: true,
    });

    for (let i = 0; i < calendars.length; i += 1) {
      calendars[i].on('select', (date) => {
        if (
          date.data.date.start !== undefined &&
          date.data.date.end !== undefined
        ) {
          onSelect(date.data.date.start, date.data.date.end);
        }
      });
    }
  }); // Empty dependency array ensures the effect runs only once on component mount

  return (
    <div>
      <Input type="date" size={size} />
    </div>
  );
}
