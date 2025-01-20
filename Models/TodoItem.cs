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

        // Timer özellikleri
        public bool IsTimerRunning { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public DateTime? TimerStartedAt { get; set; }
        
        // Yeni özellikler
        public TimeSpan TargetDuration { get; set; } // Hedef süre
        public TimeSpan RemainingTime { get; set; }  // Kalan süre
        public bool IsCountdown { get; set; }        // Geri sayım modu aktif mi?
    }
} 