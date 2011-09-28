using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.StorageClient;

namespace WebRole.Models
{
    public class Compliment : TableServiceEntity
    {
        public string Text { get; set; }
        public string AudioBlob { get; set; }
        public bool Approved { get; set; }
        public Compliment() { }
        public Compliment(string text, string audioBlob)
            : base(string.Empty, (DateTime.MaxValue - DateTime.UtcNow).Ticks.ToString("d19"))
        {
            Text = text;
            AudioBlob = audioBlob;
        }
    }
}