enum UploadType {
  Purchase,
  PayHistory,
}

interface UploadFileRequest {
  fileType: string;
  uploadType: UploadType;
}

interface UploadFileResponse {
  success: boolean;
  message: string;
}

export { UploadType, UploadFileRequest, UploadFileResponse };
