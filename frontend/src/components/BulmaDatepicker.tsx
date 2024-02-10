import React, { useEffect } from 'react';
import bulmaCalendar from 'bulma-calendar';

import { Form } from 'react-bulma-components';
import { Size } from 'react-bulma-components/src/components';

const { Input } = Form;

interface BulmaDatepickerProps {
  onSelect: (date: Date) => void;
  size?: Size;
  initialValue?: Date;
}

export default function BulmaDatepicker({
  onSelect,
  size,
  initialValue,
}: BulmaDatepickerProps) {
  useEffect(() => {
    const calendars = bulmaCalendar.attach('[type="date"]', {
      startDate: initialValue,
    });

    for (let i = 0; i < calendars.length; i += 1) {
      calendars[i].on('select', (date) => {
        if (date.data.date.start !== undefined) {
          onSelect(date.data.date.start);
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
