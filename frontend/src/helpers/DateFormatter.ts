export default function formatDate(date?: Date) {
  if (date === undefined) {
    return '';
  }

  return `${date.getUTCDay()}/${date.getUTCMonth()}/${date.getUTCFullYear()}`;
}
