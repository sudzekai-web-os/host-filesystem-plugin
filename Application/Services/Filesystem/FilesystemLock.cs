namespace Application.Services.Filesystem
{
    internal sealed class FilesystemLock
    {
        private readonly object _sync = new();

        public TResult Execute<TResult>(Func<TResult> action)
        {
            lock (_sync)
                return action();
        }
    }
}
