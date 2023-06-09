﻿namespace RpgCollector.Models.MailModel
{
    public class Mailbox
    {
        public int MailId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string SendDate { get; set; }
        public int IsRead { get; set; }
        public int IsDeleted { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public int HasReceived { get; set; }
        public string ExpireDate { get; set; }
    }
}
