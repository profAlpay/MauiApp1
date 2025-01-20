using SQLite;

namespace MauiApp1.Models
{
    public class TodoItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [NotNull]
        public string Title { get; set; }
        
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        
        [NotNull]
        public DateTime CreatedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
    }
} 