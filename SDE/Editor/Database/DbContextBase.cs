using System;
using ErrorManager;
using SDE.ApplicationConfiguration;
using SDE.Core;
using SDE.Databases;
using SDE.Editor.Generic.Parsers.Generic;

namespace SDE.Editor.Database {
	public enum SaveContextState {
		Valid,
		InvalidFileType,
		OriginalFileNotFound,
		TableDisabled,
		TableNotModified,
	}

	public abstract class DbContextBase {
		public const int MaximumNumberOfAllowedExceptions = 10;

		public int NumberOfErrors { get; protected set; }
		public bool IsRenewal { get; protected set; }
		public bool IsClipboard { get; set; }
		public string FilePath { get; set; }
		public string OldPath { get; protected set; }
		public DataSource Source { get; set; }
		public FileType FileType { get; set; }

		public MergedTable MetaMobDb;
		public MergedTable MetaItemDb;

		protected abstract BaseDatabase _bdb { get; }

		public bool ReportException(Exception err) {
			DbIOErrorHandler.HandleLoader(err, err.Message);
			NumberOfErrors--;

			if (NumberOfErrors < 0) {
				DbIOErrorHandler.FailedToReadTooManyItems(err: err);
				return false;
			}

			return true;
		}

		public void ReportIdException(string exception, object item, ErrorLevel errorLevel = ErrorLevel.Warning) {
			DbIOErrorHandler.Handle(StackTraceException.GetStrackTraceException(), exception, item.ToString(), errorLevel);
			DbDebugHelper.OnExceptionThrown(Source, FilePath, _bdb);
		}

		public bool ReportIdExceptionWithError(string exception, object item, int line = -1) {
			DbIOErrorHandler.Handle(StackTraceException.GetStrackTraceException(), exception, item.ToString(), line);
			DbDebugHelper.OnExceptionThrown(Source, FilePath, _bdb);
			NumberOfErrors--;

			if (NumberOfErrors < 0) {
				DbIOErrorHandler.FailedToReadTooManyItems(Source);
				DbDebugHelper.OnStoppedLoading(Source, FilePath, _bdb);
				return false;
			}

			return true;
		}

		public bool ReportIdException(object item) {
			return ReportIdException(item, -1);
		}

		public bool ReportIdException(object item, int line) {
			DbIOErrorHandler.Handle(StackTraceException.GetStrackTraceException(), "Failed to read an item.", item.ToString(), line);
			DbDebugHelper.OnExceptionThrown(Source, FilePath, _bdb);
			NumberOfErrors--;

			if (NumberOfErrors < 0) {
				DbIOErrorHandler.FailedToReadTooManyItems(Source);
				DbDebugHelper.OnStoppedLoading(Source, FilePath, _bdb);
				return false;
			}

			return true;
		}

		public bool ReportIdException(FileParserException fpe, object item) {
			DbIOErrorHandler.Handle(fpe, fpe.Reason, (item ?? "#").ToString(), fpe.Line);
			DbDebugHelper.OnExceptionThrown(Source, fpe.File, _bdb);
			NumberOfErrors--;

			if (NumberOfErrors < 0) {
				DbIOErrorHandler.FailedToReadTooManyItems(Source);
				DbDebugHelper.OnStoppedLoading(Source, FilePath, _bdb);
				return false;
			}

			return true;
		}

		public bool ReportException(string item) {
			DbIOErrorHandler.Handle(StackTraceException.GetStrackTraceException(), item);
			DbDebugHelper.OnExceptionThrown(Source, FilePath, _bdb);
			NumberOfErrors--;

			if (NumberOfErrors < 0) {
				DbIOErrorHandler.FailedToReadTooManyItems(Source);
				DbDebugHelper.OnStoppedLoading(Source, FilePath, _bdb);
				return false;
			}

			return true;
		}
	}
}
