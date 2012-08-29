namespace Ring.io.Messages
{
    using System;
    using System.Collections.Generic;

    public class Message
    {
        public Message()
        {
            this.Id = Guid.NewGuid();
            this.DateTime = DateTime.UtcNow;
            this.Parts = new Dictionary<string, string>();
        }

        public Guid Id { get; set; }
        public Guid? CorrelationId { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public DateTime DateTime { get; set; }
        public Dictionary<string, string> Parts { get; set; }
    }
}
