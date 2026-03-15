using System.ComponentModel.DataAnnotations;

namespace friendout_backend.RequestModels.Comment;

public class UpdateCommentRequest
{
    [Required]
    [MinLength(1)]
    public string Content { get; set; } = null!;
}

