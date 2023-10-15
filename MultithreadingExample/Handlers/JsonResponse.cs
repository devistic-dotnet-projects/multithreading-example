using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultithreadingExample.Handlers
{
    public class JsonResponse<T>
    {
        public T data { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
    }
}