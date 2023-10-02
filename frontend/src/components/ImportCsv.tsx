import Button from 'react-bootstrap/Button';
import Form from 'react-bootstrap/Form';
import { useState, useCallback } from 'react';

import { ApiStatus } from '../model/ApiStatus';
import { uploadFile } from '../helpers/RestApiHelper';
import * as constants from '../constants';

enum UploadType {
  Purchases = "Purchases",
  WorkdayPayHistory = "WorkdayPayHistory"
}

export default function ImportCsv() {
  const [purchases, setPurchases] = useState<File>();

  async function handleSubmit(event: React.ChangeEvent<HTMLInputElement>) {
    event.preventDefault();

    setStatus(ApiStatus.Submitting);
    try {
      if (file !== undefined)
      {
        await submitForm(file, uploadType);
        setStatus(ApiStatus.Submitting);
      }
    } catch (err: any) {
      setStatus(ApiStatus.Error);
      setError(err);
    }                                     
  }

  function handleUploadTypeChange(event: React.ChangeEvent<HTMLInputElement>) {
    setUploadType(event.target.value as UploadType);
  }

  function handleFileChange(event: React.ChangeEvent<HTMLInputElement>) {
    const { files } = event.target;
    const selectedFiles = files as FileList;
    setFile(selectedFiles?.[0]);
  }

  return (
    <Form>
      <Form.Label>Import a file to the database</Form.Label>
      <Form.Group controlId="formFile" className="mb-3">
        <Form.Select aria-label="Upload Type" disabled={status == ApiStatus.Submitting} onChange={handleUploadTypeChange}>
          <option>Upload type</option>
          <option value={UploadType.Purchases}>Purchases</option>
          <option value={UploadType.WorkdayPayHistory}>Workday Pay History</option>
        </Form.Select>
        <Form.Label>Select File...</Form.Label>
        <Form.Control type="file" onChange={handleFileChange} disabled={status == ApiStatus.Submitting}/>
      </Form.Group>
      <Button type="submit" disabled={status == ApiStatus.Empty || status == ApiStatus.Submitting}>Import</Button>
    </Form>
  )
}

async function submitForm(file: File, uploadType: UploadType): Promise<string> {
  let endpoint: string;
  switch (uploadType) {
    case UploadType.Purchases:
      endpoint = '/importpurchasesfromcsv';
      break;
    case UploadType.WorkdayPayHistory:
      endpoint = '/importpayhistoryfromworkdaycsv';
      break;
  }

  let uri: string = `${constants.API_BASE_URL}/api/fileimport/${endpoint}`;
  return uploadFile(uri, file);
}