namespace Domain.Models.Configurations;

public class BlobSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DefaultContainer { get; set; } = null!;
    public string UploadsContainer { get; set; } = null!;
}
