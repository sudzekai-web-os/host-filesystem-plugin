using Application.DI;
using Sudzekai.LinCtrl.Abi.Managed;
using Microsoft.Extensions.DependencyInjection;
using Sudzekai.LinCtrl.Assembly;
using Sudzekai.LinCtrl.Core;

namespace FilesystemPlugin
{
    [PluginAssembly]
    public partial class FilesystemPlugin : IPluginAssembly
    {
        public void Configure(AssemblyHostBuilder builder)
        {
            LinCtrlContext.AddTypeInfoResolver(FilesystemJsonContext.Default);

            builder.Services.AddApplicationServices();
            builder.Services.AddTransient<IRequestHandler, GetDirectoryDetailsHandler>();
            builder.Services.AddTransient<IRequestHandler, GetDirectorySecurityHandler>();
            builder.Services.AddTransient<IRequestHandler, GetDirectoriesHandler>();
            builder.Services.AddTransient<IRequestHandler, GetFilesHandler>();
            builder.Services.AddTransient<IRequestHandler, CreateDirectoryHandler>();
            builder.Services.AddTransient<IRequestHandler, DeleteDirectoryHandler>();
            builder.Services.AddTransient<IRequestHandler, CopyDirectoryHandler>();
            builder.Services.AddTransient<IRequestHandler, MoveDirectoryHandler>();
            builder.Services.AddTransient<IRequestHandler, GetFileDetailsHandler>();
            builder.Services.AddTransient<IRequestHandler, GetFileSecurityHandler>();
            builder.Services.AddTransient<IRequestHandler, ReadTextHandler>();
            builder.Services.AddTransient<IRequestHandler, ImportFileHandler>();
            builder.Services.AddTransient<IRequestHandler, ExportFileHandler>();
            builder.Services.AddTransient<IRequestHandler, CreateFileHandler>();
            builder.Services.AddTransient<IRequestHandler, CopyFileHandler>();
            builder.Services.AddTransient<IRequestHandler, MoveFileHandler>();
            builder.Services.AddTransient<IRequestHandler, DeleteFileHandler>();
            builder.Services.AddTransient<IRequestHandler, WriteTextHandler>();
            builder.Services.AddTransient<IRequestHandler, ReplaceLinesHandler>();
        }
    }
}
