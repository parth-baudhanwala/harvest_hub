namespace BuildingBlocks.Pagination;

public class PaginatedResult<TEntity>(int index, int size, long count, IEnumerable<TEntity> data)
    where TEntity : class
{
    public int Index { get; } = index;
    public int Size { get; } = size;
    public long Count { get; } = count;
    public IEnumerable<TEntity> Data { get; } = data;
}
