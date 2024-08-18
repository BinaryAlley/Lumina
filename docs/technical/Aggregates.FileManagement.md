- [Lumina Domain](#lumina-domain)
  - [File Management Aggregate](#file-management-aggregate)
    - [File Info](#file-info)
    - [Stream Info](#stream-info)

# [Lumina Domain](#lumina-domain)

## [File Management Aggregate](#file-management-aggregate)

### [File Info](#file-info)

```csharp
class FileInfo
{
    ErrorOr<FileInfo> Create(string path, string fileExtension, long sizeInBytes, DateTime lastModified, Optional<string> mimeType);
}
```

```json
{
    "path": "/media/movies/The Lord of the Rings - The Fellowship of the Ring.mkw",
    "fileExtension": "mkv",
    "sizeInBytes": 15797942704,
    "lastModified": "2024-08-18",
    "mimeType": "video/x-matroska"   
}
```

### [Stream Info](#stream-info)

```csharp
class StreamInfo
{
    ErrorOr<StreamInfo> Create(string streamId, string mimeType, int bitrate, string codec,
        Optional<string> resolution = default, Optional<float> frameRate = default, 
        Optional<int> sampleRate = default, Optional<int> channels = default);
}
```

```json
{
    "streamId": "stream_001",
    "mimeType": "video/mp4",
    "bitrate": 5000000,
    "codec": "h264",
    "resolution": "1920x1080",
    "frameRate": 29.97,
    "sampleRate": 48000,
    "channels": 2  
}
```