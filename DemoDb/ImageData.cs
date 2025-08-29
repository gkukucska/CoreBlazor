using System.ComponentModel.DataAnnotations;

namespace DemoDb;

public class ImageData
{
    public int Id { get; set; }

    [Required]
    public byte[] Data{ get; set; }
}