namespace ArgosApi.Shared
{
    public class PagedResult<T> : Result<T>
    {
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalPages { get; }
        public int TotalRecords { get; }

        private PagedResult(bool isSuccess, T value, string error, int? statusCode, 
                          int pageNumber, int pageSize, int totalRecords)
            : base(isSuccess, value, error, statusCode)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        }

        public static PagedResult<T> Ok(T value, int pageNumber, int pageSize, int totalRecords) 
            => new PagedResult<T>(true, value, null!, null, pageNumber, pageSize, totalRecords);
        public static new PagedResult<T> Fail(string error, int? statusCode = null) 
            => new PagedResult<T>(false, default!, error, statusCode, 0, 0, 0);
    }
}