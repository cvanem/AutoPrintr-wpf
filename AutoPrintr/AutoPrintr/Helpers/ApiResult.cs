﻿using System.Net;

namespace AutoPrintr.Helpers
{
    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public T Result { get; set; }
        public HttpStatusCode? StatusCode { get; set; }
    }
}