namespace DDOT.MPS.Communication.Model.Responses
{
    public class BaseResponse<T> : InitialResponse
    {
        public string Message { get; set; }
        public T? Data { get; set; }
    }

    public class InitialResponse
    {
        public bool Success { get; set; } = false;
    }

    public class CoreResponse : InitialResponse
    {
        public string Response { get; set; }
    }

    public class ResponseData<T>
    {
        public string Message { get; set; }
        public Result<T> Result { get; set; }
    }

    public class Result<T>
    {
        public T[] Entities { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class Pagination
    {
        public long Length { get; set; }
        public int PageSize { get; set; }
    }
}
