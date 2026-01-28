export interface PaginatedResult<T> {
  index: number;
  size: number;
  count: number;
  data: T[];
}
