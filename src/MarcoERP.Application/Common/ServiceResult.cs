using System.Collections.Generic;
using System.Linq;

namespace MarcoERP.Application.Common
{
    /// <summary>
    /// Represents the result of an application-layer operation.
    /// Encapsulates success/failure state, error messages, and optional payload.
    /// </summary>
    public sealed class ServiceResult
    {
        private ServiceResult(bool isSuccess, IReadOnlyList<string> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors ?? new List<string>();
        }

        /// <summary>True if the operation succeeded.</summary>
        public bool IsSuccess { get; }

        /// <summary>True if the operation failed.</summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>Validation or domain error messages.</summary>
        public IReadOnlyList<string> Errors { get; }

        /// <summary>Combined error message string (pipe-separated).</summary>
        public string ErrorMessage => string.Join(" | ", Errors);

        /// <summary>Creates a successful result.</summary>
        public static ServiceResult Success() => new ServiceResult(true, null);

        /// <summary>Creates a failed result with one error.</summary>
        public static ServiceResult Failure(string error) =>
            new ServiceResult(false, new List<string> { error });

        /// <summary>Creates a failed result with multiple errors.</summary>
        public static ServiceResult Failure(IEnumerable<string> errors) =>
            new ServiceResult(false, errors.ToList());
    }

    /// <summary>
    /// Represents the result of an application-layer operation that returns data.
    /// </summary>
    public sealed class ServiceResult<T>
    {
        private ServiceResult(bool isSuccess, T data, IReadOnlyList<string> errors)
        {
            IsSuccess = isSuccess;
            Data = data;
            Errors = errors ?? new List<string>();
        }

        /// <summary>True if the operation succeeded.</summary>
        public bool IsSuccess { get; }

        /// <summary>True if the operation failed.</summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>The data payload (default when failed).</summary>
        public T Data { get; }

        /// <summary>Validation or domain error messages.</summary>
        public IReadOnlyList<string> Errors { get; }

        /// <summary>Combined error message string (pipe-separated).</summary>
        public string ErrorMessage => string.Join(" | ", Errors);

        /// <summary>Creates a successful result with data.</summary>
        public static ServiceResult<T> Success(T data) =>
            new ServiceResult<T>(true, data, null);

        /// <summary>Creates a failed result with one error.</summary>
        public static ServiceResult<T> Failure(string error) =>
            new ServiceResult<T>(false, default, new List<string> { error });

        /// <summary>Creates a failed result with multiple errors.</summary>
        public static ServiceResult<T> Failure(IEnumerable<string> errors) =>
            new ServiceResult<T>(false, default, errors.ToList());
    }
}
