using System;

namespace VRTown.Service
{
    //
    // Summary:
    //     For logging messages.
    public interface ILogger
    {
        //
        // Summary:
        //     Returns a logger associated with the specified type.
        ILogger ForType<T>();

        //
        // Summary:
        //     Logs a message with severity Log.
        void Log(string message, string hexColor = "#ccccff");

        bool DebugAssert(bool condition, string message);

        //
        // Summary:
        //     Logs a message with severity Warning.
        void Warning(string message, string hexColor = "#ccccff");

        //
        // Summary:
        //     Logs a message and an associated exception with severity Warning.
        void Warning(Exception exception, string message, string hexColor = "#ccccff");

        //
        // Summary:
        //     Logs a message with severity Error.
        void Error(string message, string hexColor = "#ccccff");

        //
        // Summary:
        //     Logs a message and an associated exception with severity Error.
        void Error(Exception exception, string message, string hexColor = "#ccccff");
    }
}