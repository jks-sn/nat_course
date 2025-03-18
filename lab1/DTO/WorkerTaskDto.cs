namespace Dto;

public class WorkerTaskDto
{
    public string RequestId { get; set; }
    public string Hash { get; set; }
    public int MaxLength { get; set; }
    public int PartNumber { get; set; }
    public int PartCount { get; set; }
}