import axios, { isCancel, AxiosRequestConfig } from 'axios';

export async function uploadFile(uri: string, file: Blob): Promise<string> {
  let data: FormData = new FormData();
  data.append('file', file);

  try {
    await axios.post(uri, data, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });

    return Promise.resolve('');
  }
  catch (error) {
    if (axios.isAxiosError(error)) {
      return Promise.reject(`An error has occured in the API call: ${error.message}`);
    }

    return Promise.reject('AHHHHHHHHHHHHHHHHHHHH');
  }
}