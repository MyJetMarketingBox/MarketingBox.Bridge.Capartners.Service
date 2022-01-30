namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Responses
{
    public class Response<TSuccessResult, TFailedResult>
        where TSuccessResult : class
        where TFailedResult : class
    {
        protected Response(TFailedResult failedResult)
        {
            FailedResult = failedResult;
        }

        protected Response(TSuccessResult successResult)
        {
            SuccessResult = successResult;
        }

        public TSuccessResult SuccessResult { get; }
        public TFailedResult FailedResult { get; }

        public bool IsFailed => FailedResult != null;

        public static Response<TSuccessResult, TFailedResult> CreateFailed(TFailedResult result)
        {
            return new Response<TSuccessResult, TFailedResult>(result);
        }

        public static Response<TSuccessResult, TFailedResult> CreateSuccess(TSuccessResult result)
        {
            return new Response<TSuccessResult, TFailedResult>(result);
        }
    }

    //public class ResponseList<TSuccessResult, TFailedResult>
    //where TSuccessResult : IReadOnlyList<TSuccessResult>
    //where TFailedResult : class
    //{
    //    protected ResponseList(TFailedResult failedResult)
    //    {
    //        FailedResult = failedResult;
    //    }

    //    protected ResponseList(TSuccessResult successResult)
    //    {
    //        SuccessResult = successResult;
    //    }

    //    public IReadOnlyList<TSuccessResult> SuccessResult { get; }
    //    public TFailedResult FailedResult { get; }

    //    public bool IsFailed => FailedResult != null;

    //    public int Count => throw new NotImplementedException();

    //    public static ResponseList<TSuccessResult, TFailedResult> CreateFailed(TFailedResult result)
    //    {
    //        return new ResponseList<TSuccessResult, TFailedResult>(result);
    //    }

    //    public static ResponseList<TSuccessResult, TFailedResult> CreateSuccess(TSuccessResult result)
    //    {
    //        return new ResponseList<TSuccessResult, TFailedResult>(result);
    //    }
    //}
}