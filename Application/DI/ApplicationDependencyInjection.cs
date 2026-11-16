using Application.Dispatchering.Commands.Directories;
using Application.Dispatchering.Commands.Files;
using Application.Dispatchering.Dispatchers;
using Application.Dispatchering.Handlers;
using Application.Dispatchering.Handlers.Directories;
using Application.Dispatchering.Handlers.Files;
using Application.Dto;
using Application.Services.Database;
using Application.Services.Filesystem;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DI
{
    public static class ApplicationDependencyInjection
    {
        extension (IServiceCollection services)
        {
            public IServiceCollection AddApplicationServices()
            {
                services.AddSingleton<DatabaseApiClient>();
                services.AddSingleton<SystemAuthenticationService>();
                services.AddSingleton(provider => new DatabaseHistoryService(
                    provider.GetRequiredService<DatabaseApiClient>(),
                    provider.GetRequiredService<SystemAuthenticationService>()));
                services.AddSingleton<FilesystemPathResolver>();
                services.AddSingleton<FilesystemLock>();
                services.AddSingleton<DirectoryService>();
                services.AddSingleton<FileService>();
                services.AddSingleton<IDispatcher, Dispatcher>();
                services.AddTransient<ICommandHandler<ImportFileCommand, FileDetails>, ImportFileCommandHandler>();
                services.AddTransient<ICommandHandler<ExportFileCommand, string>, ExportFileCommandHandler>();

                services.AddTransient<ICommandHandler<GetDirectoriesCommand, DirectoryEntry[]>, GetDirectoriesCommandHandler>();
                services.AddTransient<ICommandHandler<GetDirectoryDetailsCommand, DirectoryDetails>, GetDirectoryDetailsCommandHandler>();
                services.AddTransient<ICommandHandler<GetDirectorySecurityCommand, object>, GetDirectorySecurityCommandHandler>();
                services.AddTransient<ICommandHandler<GetFilesCommand, FileDetails[]>, GetFilesCommandHandler>();
                services.AddTransient<ICommandHandler<CreateDirectoryCommand, DirectoryEntry>, CreateDirectoryCommandHandler>();
                services.AddTransient<ICommandHandler<DeleteDirectoryCommand, object?>, DeleteDirectoryCommandHandler>();
                services.AddTransient<ICommandHandler<CopyDirectoryCommand, DirectoryEntry>, CopyDirectoryCommandHandler>();
                services.AddTransient<ICommandHandler<MoveDirectoryCommand, DirectoryEntry>, MoveDirectoryCommandHandler>();
                services.AddTransient<ICommandHandler<GetFileDetailsCommand, FileDetails>, GetFileDetailsCommandHandler>();
                services.AddTransient<ICommandHandler<GetFileSecurityCommand, object>, GetFileSecurityCommandHandler>();
                services.AddTransient<ICommandHandler<ReadTextCommand, string>, ReadTextCommandHandler>();
                services.AddTransient<ICommandHandler<CreateFileCommand, FileDetails>, CreateFileCommandHandler>();
                services.AddTransient<ICommandHandler<CopyFileCommand, FileDetails>, CopyFileCommandHandler>();
                services.AddTransient<ICommandHandler<MoveFileCommand, FileDetails>, MoveFileCommandHandler>();
                services.AddTransient<ICommandHandler<DeleteFileCommand, object?>, DeleteFileCommandHandler>();
                services.AddTransient<ICommandHandler<WriteTextCommand, object?>, WriteTextCommandHandler>();
                services.AddTransient<ICommandHandler<ReplaceLinesCommand, object?>, ReplaceLinesCommandHandler>();

                return services;
            }
        }
    }
}
