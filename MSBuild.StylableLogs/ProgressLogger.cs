using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSBuild.StylableLogs
{
    internal class ProgressLogger
    {
        private ProgressInfo? progressInfo;
        private Timer timer;

        public ProgressLogger()
        {
            timer = new Timer(Refresh, null, Timeout.Infinite, Timeout.Infinite);
            AnsiConsole.Cursor.Hide();
        }

        public ProgressInfo GetProgressInfo(string statusMessage)
        {
            if (progressInfo is null)
            {
                progressInfo = new ProgressInfo(this, statusMessage);
                timer.Change(250, 250);
            }

            return progressInfo;
        }

        internal void EndProgress()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            Refresh(null);
            progressInfo = null;
        }

        public void Refresh(object? stateInfo)
        {
            if (progressInfo is null || progressInfo.TaskOperations.Count == 0)
            {
                return;
            }

            AnsiConsole.Cursor.SetPosition(0, 0);

            // Creating new array to avoid concurrency issues (only for prototype purposes)
            foreach (var taskOperation in progressInfo.TaskOperations.ToArray())
            {
                switch (taskOperation.Status)
                {
                    case TaskOperationStatus.InProgress:
                        AnsiConsole.Markup($"[grey]⌛ {taskOperation.Message}[/]");
                        Console.WriteLine("\x1b[K");
                        break;
                    case TaskOperationStatus.Failed:
                        AnsiConsole.Markup($"[red]❌ {taskOperation.Message}[/]");
                        Console.WriteLine("\x1b[K");
                        break;
                }
            }

            int completed = progressInfo.TaskOperations.Count(t => t.Status == TaskOperationStatus.CompletedSucessfully);
            int failed = progressInfo.TaskOperations.Count(t => t.Status == TaskOperationStatus.Failed);
            int skipped = progressInfo.TaskOperations.Count(t => t.Status == TaskOperationStatus.Skipped);

            int finished = completed + failed + skipped;

            AnsiConsole.Write("\x1b[K\n");
            AnsiConsole.Write($"{progressInfo.StatusMessage}");

            if (completed > 0)
            {
                AnsiConsole.MarkupInterpolated($"[green]{completed}[/] completed");
            }
            if (failed > 0)
            {
                AnsiConsole.MarkupInterpolated($" [red]{failed}[/] failed");
            }
            if (skipped > 0)
            {
                AnsiConsole.MarkupInterpolated($" [grey]{skipped}[/] skipped");
            }

            if (progressInfo.TotalOperations.HasValue)
            {
                int percentage = (int)((finished / (double)progressInfo.TotalOperations) * 100);
                int progressBarPercentage = (int)((finished / (double)progressInfo.TotalOperations) * 50); // we want shorter progress bar

                AnsiConsole.Write("\x1b[K\n");
                AnsiConsole.Markup($"[blue]{new string('█', progressBarPercentage)}[/][grey15]{new string('█', 50 - progressBarPercentage)}[/]");
                AnsiConsole.Write($" {percentage}%\x1b[K");
            }
        }
    }

    public class ProgressInfo : IDisposable
    {
        private readonly ProgressLogger logger;

        internal List<TaskOperation> TaskOperations { get; } = new();

        public string StatusMessage { get; set; }

        public int? TotalOperations { get; set; }

        internal ProgressInfo(ProgressLogger logger, string statusMessage)
        {
            this.logger = logger;
            StatusMessage = statusMessage;

        }

        public TaskOperation CreateTaskOperation(string message)
        {
            TaskOperation operation = new TaskOperation(message);
            TaskOperations.Add(operation);

            return operation;
        }

        public void Dispose()
        {
            logger.EndProgress();
        }
    }

    public class TaskOperation
    {
        public TaskOperationStatus Status { get; private set; }

        public string Message { get; private set; }

        internal TaskOperation(string message)
        {
            Message = message;
        }

        public void Completed()
        {
            Status = TaskOperationStatus.CompletedSucessfully;
        }

        public void Skipped()
        {
            Status = TaskOperationStatus.Skipped;
        }

        public void Failed(string errorMessage)
        {
            Message = errorMessage;
            Status = TaskOperationStatus.Failed;
        }
    }

    public enum TaskOperationStatus
    {
        InProgress = 0,
        CompletedSucessfully,
        CompletedWithWarning,
        Skipped,
        Failed
    }
}
