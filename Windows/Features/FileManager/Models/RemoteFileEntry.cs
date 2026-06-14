namespace LIAF.Windows.Features.FileManager.Models;

public sealed record RemoteFileEntry(string Path, bool IsDirectory, long? Size, DateTimeOffset? ModifiedAt);
