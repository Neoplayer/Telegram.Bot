﻿using System;
using Telegram.Bot.Types;

namespace Telegram.Bot.Exceptions
{
    /// <summary>
    /// Represents an api error
    /// </summary>
    public class ApiRequestException : Exception
    {
        /// <summary>
        /// Gets the error code.
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Contains information about why a request was unsuccessful.
        /// </summary>
        public ResponseParameters Parameters { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiRequestException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ApiRequestException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiRequestException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="errorCode">The error code.</param>
        public ApiRequestException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiRequestException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ApiRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiRequestException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="innerException">The inner exception.</param>
        public ApiRequestException(string message, int errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Returns a new instance of the <see cref="ApiRequestException"/> class.
        /// </summary>
        /// <param name="apiResponse">The API response.</param>
        /// <returns><see cref="ApiRequestException"/></returns>
        public static ApiRequestException FromApiResponse<T>(ApiResponse<T> apiResponse)
        {
            string message;
            switch (apiResponse.Code)
            {
                //TODO: Add test cases for all those
                case 400:
                    message = apiResponse.Message.Remove(0, "Bad Request: ".Length);
                    switch (message.Trim())
                    {
                        case "chat not found":
                            return new ChatNotFoundException(apiResponse.Message)
                            {
                                Parameters = apiResponse.Parameters
                            };
                        case "have no rights to send a message":
                            return new BotRestrictedException(apiResponse.Message)
                            {
                                Parameters = apiResponse.Parameters
                            };
                        case "not enough rights to restrict/unrestrict chat member":
                            return new NotEnoughRightsException(apiResponse.Message)
                            {
                                Parameters = apiResponse.Parameters
                            };
                        case "user not found":
                            return new UserNotFoundException(apiResponse.Message)
                            {
                                Parameters = apiResponse.Parameters
                            };
                        case "method is available for supergroup and channel chats only":
                            return new WrongChatTypeException(apiResponse.Message)
                            {
                                Parameters = apiResponse.Parameters
                            };
                        default:
                            if (message.EndsWith(" is empty"))
                            {
                                return new MissingParameterException(apiResponse.Message, message.Remove(message.IndexOf(" is empty")))
                                {
                                    Parameters = apiResponse.Parameters
                                };
                            }
                            LogMissingError(apiResponse);
                            return new ApiRequestException(apiResponse.Message, 400)
                            {
                                Parameters = apiResponse.Parameters
                            };
                    }
                case 403:
                    message = apiResponse.Message.Remove(0, "Forbidden: ".Length);
                    switch (message.Trim())
                    {
                        case "bot was blocked by the user":
                            return new BotBlockedException(apiResponse.Message)
                            {
                                Parameters = apiResponse.Parameters
                            };
                        case "bot can't initiate conversation with a user":
                            return new BotNotStartedException(apiResponse.Message)
                            {
                                Parameters = apiResponse.Parameters
                            };
                        default:
                            LogMissingError(apiResponse);
                            return new ApiRequestException(apiResponse.Message, 403)
                            {
                                Parameters = apiResponse.Parameters
                            };
                    }
                default:
                    LogMissingError(apiResponse);
                    return new ApiRequestException(apiResponse.Message, apiResponse.Code)
                    {
                        Parameters = apiResponse.Parameters
                    };
            }
        }

        private static void LogMissingError<T>(ApiResponse<T> apiResponse)
        {
            //const string loggingPhp = "88.198.66.60/logError.php?message=";
            //HttpClient c = new HttpClient();
            //c.GetAsync(loggingPhp + WebUtility.HtmlEncode(apiResponse.Message + "\n" + JsonConvert.SerializeObject(apiResponse.Parameters)));
        }
    }
}
