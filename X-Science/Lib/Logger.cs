using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScienceChecklist {
	/// <summary>
	/// Class for helping with log messages.
	/// </summary>
	internal sealed class Logger {
		/// <summary>
		/// Creates a new instance of the Logger class.
		/// </summary>
		/// <param name="parent">The owner of this logger.</param>
		public Logger (object parent) {
			_ownerName = parent.GetType().Name;
		}

		/// <summary>
		/// Creates a new instance of the Logger class to be used by static classes.
		/// </summary>
		/// <param name="parentName">The name of the parent.</param>
		public Logger (string parentName) {
			_ownerName = parentName;
		}

		#region METHODS (PUBLIC)

		/// <summary>
		/// Writes a log message at the Fatal level.
		/// </summary>
		/// <param name="message">The message to write.</param>
		public void Fatal (string message) {
			WriteMessage(message, LogLevel.Fatal);
		}

		/// <summary>
		/// Writes a log message at the Error level.
		/// </summary>
		/// <param name="message">The message to write.</param>
		public void Error (string message) {
			WriteMessage(message, LogLevel.Error);
		}

		/// <summary>
		/// Writes a log message at the Warning level.
		/// </summary>
		/// <param name="message">The message to write.</param>
		public void Warning (string message) {
			WriteMessage(message, LogLevel.Warning);
		}

		/// <summary>
		/// Writes a log message at the Info level.
		/// </summary>
		/// <param name="message">The message to write.</param>
		public void Info (string message) {
			WriteMessage(message, LogLevel.Info);
		}

		/// <summary>
		/// Writes a log message at the Debug level.
		/// </summary>
		/// <param name="message">The message to write.</param>
		public void Debug (string message) {
			WriteMessage(message, LogLevel.Debug);
		}

		/// <summary>
		/// Writes a log message at the Trace level.
		/// </summary>
		/// <param name="message">The message to write.</param>
		public void Trace (string message) {
			WriteMessage(message, LogLevel.Trace);
		}

		#endregion

		#region METHODS (PRIVATE)

		/// <summary>
		/// Writes the specified log message to the UnityEngine Debug log at the specified log level.
		/// </summary>
		/// <param name="message">The message to write.</param>
		/// <param name="logLevel">The log level at which the log should be written.</param>
		private void WriteMessage (string message, LogLevel logLevel) {
			if (logLevel > MaxLogLevel) {
				return;
			}
			var msg = GetMessage (message, logLevel);
			switch (logLevel) {
				case LogLevel.Fatal:
				case LogLevel.Error:
					UnityEngine.Debug.LogError(msg);
					break;
				case LogLevel.Warning:
				case LogLevel.Info:
					UnityEngine.Debug.LogWarning(msg);
					break;
				case LogLevel.Debug:
				case LogLevel.Trace:
					UnityEngine.Debug.Log(msg);
					break;
			}
		}

		/// <summary>
		/// Formats a log message with the mod name, owner name, log level, and specified message.
		/// </summary>
		/// <param name="message">The log message to be written.</param>
		/// <param name="logLevel">The log level at which the log should be written.</param>
		/// <returns>The formatted message.</returns>
		private string GetMessage (string message, LogLevel logLevel) {
			return string.Format("[{0} [x] Science!]: <{1}> ({2}) - {3}", DateTime.Now, logLevel, _ownerName, message);
		}

		#endregion

		#region FIELDS

		private readonly string _ownerName;

		private const LogLevel MaxLogLevel = LogLevel.Trace;

		#endregion

		#region LogLevel

		private enum LogLevel
		{
			Fatal = 0,
			Error = 1,
			Warning = 2,
			Info = 3,
			Debug = 4,
			Trace = 5,
		}

		#endregion
	}
}
