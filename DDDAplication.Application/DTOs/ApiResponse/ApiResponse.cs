using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.DTOs.ApiResponse
{
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates whether the request was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message providing details about the status of the request.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// List of errors, if any.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Additional data that may be sent as part of the response.
        /// </summary>
        public T Data { get; set; }

        public ApiResponse() { }

        public ApiResponse(bool success, string message, T data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public ApiResponse(bool success, string message, List<string> errors)
        {
            Success = success;
            Message = message;
            Errors = errors ?? new List<string>();
        }

        public static ApiResponse<T> CreateErrorResponse(string message, List<string> errors = null)
        {
            return new ApiResponse<T>(false, message, errors);
        }

        public static ApiResponse<T> CreateSuccessResponse(string message, T data = default)
        {
            return new ApiResponse<T>(true, message, data);
        }
    }

}
