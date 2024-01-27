function isBoolean(value: unknown): value is boolean {
  return value === false || value === true;
}

export default isBoolean;