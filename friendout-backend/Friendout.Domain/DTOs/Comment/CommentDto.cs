namespace Friendout.Domain.DTOs.Comment;

public class CommentDto
{
    public required string CommentId { get; set; }

    public required string SendBy { get; set; }

    public required string UserId { get; set; }
    
    public required string Content { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
}