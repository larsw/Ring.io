namespace Ring.io
{
    using System;

    public class HashTableEntry
    {
        public string NodeId { get; set; }
        public string RingToken { get; set; }
        public string Address { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
