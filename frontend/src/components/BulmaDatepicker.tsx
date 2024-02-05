import React, { useEffect } from 'react';
import bulmaCalendar from 'bulma-calendar';

import { Form } from 'react-bulma-components';

const { Input } = Form;

interface BulmaDatepickerProps {
  onSelect: (date: Date) => void;
  initialValue?: Date;
}

export default function BulmaDatepicker({
  onSelect,
  initialValue,
}: BulmaDatepickerProps) {
  useEffect(() => {
    const calendars = bulmaCalendar.attach('[type="date"]', {
      startDate: initialValue,
    });

    for (let i = 0; i < calendars.length; i += 1) {
      calendars[i].on('select', (date) => {
        onSelect(new Date(date.timeStamp));
      });
    }
  }, []); // Empty dependency array ensures the effect runs only once on component mount

  return (
    <div>
      <Input type="date" />
    </div>
  );
}
