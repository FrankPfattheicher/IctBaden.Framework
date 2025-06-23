using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace IctBaden.Framework;


/// <summary>
/// General result to be returned from any operations.
/// Supports success, failure and exception results.
/// </summary>
/// <typeparam name="TResult"></typeparam>
public class Result<TResult>
{
    /// <summary>
    /// Signals operation was completed successfully
    /// and Data contains result data.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// List of error messages from operation.
    /// Should be empty on success. 
    /// </summary>
    public string[] ErrorMessages { get; init; } = [];
    
    /// <summary>
    /// List of error messages combined in one text
    /// </summary>
    public string Errors => 
        string.Join("; ", ErrorMessages.Where(m => !string.IsNullOrEmpty(m)));

    /// <summary>
    /// Complete exception information on exception failure.
    /// </summary>
    public Exception? FailureException { get; }

    private TResult? _data;
    /// <summary>
    /// Result data from successful execution.
    /// </summary>
    public TResult Data
    {
        get => _data!;
        init => _data = value;
    }


    /// <summary>
    /// Signals failed execution
    /// </summary>
    public bool IsFailed => !Success;

    /// <summary>
    /// Signals failed execution with exception
    /// </summary>
    public bool IsException => FailureException != null;


    public Result()
    {
        // serialization
    }

    private Result(TResult data)
    {
        Success = true;
        _data = data;
    }

    private Result(string message)
    {
        Success = false;
        ErrorMessages = new[] { message };
    }

    private Result(string[] messages)
    {
        Success = false;
        ErrorMessages = messages;
    }

    private Result(Exception ex)
    {
        Success = false;
        FailureException = ex;
        ErrorMessages = new[] { ex.Message };
    }


   
    /// <summary>
    /// Returns successful result with result data 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Result<TResult> Ok(TResult data) => new(data);
    /// <summary>
    /// Returns default boolean successful result
    /// </summary>
    /// <returns></returns>
    public static Result<bool> Ok() => new(true);

    
    /// <summary>
    /// Returns failed result with error message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Result<TResult> Failed(string message) => new(message);
    /// <summary>
    /// Returns default failed result without error message
    /// </summary>
    /// <returns></returns>
    public static Result<TResult> Failed() => new(string.Empty);
    /// <summary>
    /// Returns failed result with list of error messages
    /// </summary>
    /// <param name="messages"></param>
    /// <returns></returns>
    public static Result<TResult> Failed(string[] messages) => new(messages);
    /// <summary>
    /// Returns exception result with exception 
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static Result<TResult> Exception(Exception ex) => new(ex);

}
