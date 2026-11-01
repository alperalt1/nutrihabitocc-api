namespace Nutrihabitocc.Domain.Dtos;

public class S3ObjectResult
{
    public Stream Stream { get; set; } = Stream.Null;
    public string ContentType { get; set; } = "application/octet-stream";
}
