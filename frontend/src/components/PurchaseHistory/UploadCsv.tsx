import { Button, Form } from 'react-bulma-components';

const { Field } = Form;

interface UploadCsvProps {
  onSubmit: () => void;
}

export default function UploadCsv({ onSubmit }: UploadCsvProps) {
  return (
    <Field kind="group">
      <Button onClick={() => onSubmit()}>Upload CSV</Button>
    </Field>
  );
}
