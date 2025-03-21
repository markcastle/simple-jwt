using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Serialization;
using SimpleJwt.Abstractions.Validation;
using SimpleJwt.Core.TokenLifetime;
using SimpleJwt.Newtonsoft.Serialization;

namespace SimpleJwt.Unity
{
    /// <summary>
    /// Provides extensions methods for using SimpleJwt in Unity.
    /// </summary>
    public static class SimpleJwtUnityExtensions
    {
        /// <summary>
        /// Initializes SimpleJwt for use in Unity projects.
        /// </summary>
        /// <param name="useNewtonsoftJson">Whether to use Newtonsoft.Json as the serialization provider. Default is true.</param>
        public static void InitializeForUnity(bool useNewtonsoftJson = true)
        {
            if (useNewtonsoftJson)
            {
                JsonProviderConfiguration.SetProvider(new NewtonsoftJsonProvider());
            }
        }
        
        /// <summary>
        /// Creates a Unity coroutine for validating a JWT token.
        /// </summary>
        /// <param name="validator">The validator.</param>
        /// <param name="token">The token to validate.</param>
        /// <param name="callback">The callback to invoke when validation completes.</param>
        /// <returns>An enumerator for the coroutine.</returns>
        public static IEnumerator ValidateTokenCoroutine(
            this IJwtValidator validator, 
            string token, 
            Action<ValidationResult> callback)
        {
            var operation = new AsyncOperationWrapper<ValidationResult>(
                validator.ValidateAsync(token, CancellationToken.None));
                
            yield return operation;
            
            if (operation.IsCompleted && !operation.IsFaulted)
            {
                callback?.Invoke(operation.Result);
            }
            else if (operation.IsFaulted && operation.Exception != null)
            {
                throw operation.Exception;
            }
        }
        
        /// <summary>
        /// Creates a Unity coroutine for refreshing a JWT token.
        /// </summary>
        /// <param name="refresher">The token refresher.</param>
        /// <param name="accessToken">The access token to refresh.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="callback">The callback to invoke when refreshing completes.</param>
        /// <returns>An enumerator for the coroutine.</returns>
        public static IEnumerator RefreshTokenCoroutine(
            this ITokenRefresher refresher,
            string accessToken,
            string refreshToken,
            Action<RefreshResult> callback)
        {
            var operation = new AsyncOperationWrapper<RefreshResult>(
                refresher.RefreshAsync(accessToken, refreshToken, CancellationToken.None));
                
            yield return operation;
            
            if (operation.IsCompleted && !operation.IsFaulted)
            {
                callback?.Invoke(operation.Result);
            }
            else if (operation.IsFaulted && operation.Exception != null)
            {
                throw operation.Exception;
            }
        }
    }
    
    /// <summary>
    /// Wraps an async operation as a Unity-compatible object that can be yielded in a coroutine.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    public class AsyncOperationWrapper<T>
    {
        private readonly Task<T> _task;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncOperationWrapper{T}"/> class.
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        public AsyncOperationWrapper(Task<T> task)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
        }
        
        /// <summary>
        /// Gets a value indicating whether the task is faulted.
        /// </summary>
        public bool IsFaulted => _task.IsFaulted;
        
        /// <summary>
        /// Gets a value indicating whether the task is completed.
        /// </summary>
        public bool IsCompleted => _task.IsCompleted;
        
        /// <summary>
        /// Gets the exception that caused the task to fault, if any.
        /// </summary>
        public Exception Exception => _task.Exception;
        
        /// <summary>
        /// Gets the result of the task.
        /// </summary>
        public T Result => _task.Result;
        
        /// <summary>
        /// Gets the task being wrapped.
        /// </summary>
        public Task<T> Task => _task;
    }
} 