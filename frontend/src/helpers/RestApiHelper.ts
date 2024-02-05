import * as fs from 'fs';
import { UploadFileResponse } from '../types/UploadFile';

export default async function uploadFileRest(
  serverName: string,
  fileName: string,
): Promise<UploadFileResponse> {
  const formData = new FormData();
  formData.append('csvFile', new Blob([fs.readFileSync(fileName)]), fileName);

  const endpoint = `http://${serverName}/api/fileimport/uploadpurchasecsv`;
  const response = await fetch(endpoint, {
    method: 'POST',
    body: formData,
  });

  if (response.ok) {
    return {
      success: true,
      message: 'Successfully uploaded file',
    };
  }

  return {
    success: false,
    message: await response.text(),
  };
}
