using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Staffing.Shared
{
    public class ServiceResponse<T>(bool success, T data, string? errorMessage = null)
    {
        public T Data { get; set; } = data;
        public bool Success { get; set; } = success;
        public string? ErrorMessage { get; set; } = errorMessage;
    }
}
