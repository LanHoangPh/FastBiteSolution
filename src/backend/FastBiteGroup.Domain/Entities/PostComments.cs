namespace FastBiteGroup.Domain.Entities;

public class PostComments : IDateTracking, ISoftDelete
{
    public int CommentID { get; set; }
    public int PostID { get; set; }
    public Guid UserID { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? ParentCommentID { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedByUserID { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public virtual Posts? Post { get; set; }
    public virtual PostComments? ParentComment { get; set; }
    public virtual ICollection<PostComments> Replies { get; set; } = new HashSet<PostComments>();
}

