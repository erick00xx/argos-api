namespace ArgosApi.Shared
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Error { get; }
        public int? StatusCode { get; }

        protected Result(bool isSuccess, T value, string error, int? statusCode)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            StatusCode = statusCode;
        }

        public static Result<T> Ok(T value) => new Result<T>(true, value, null!, null);
        public static Result<T> Fail(string error, int? statusCode = null) => new Result<T>(false, default!, error, statusCode);

        public static implicit operator Result<T>(T value) => Ok(value);
    }
}