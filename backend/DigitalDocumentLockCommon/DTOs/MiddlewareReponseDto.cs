using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DigitalDocumentLockCommon.DTOs
{
    public class MiddlewareResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }

        public MiddlewareResponseDto(T data, string message = "Success")
        {
            Success = true;
            Message = message;
            Data = data;
        }

        public MiddlewareResponseDto(string errorMessage)
        {
            Success = false;
            Message = errorMessage;
        }
    }
}
