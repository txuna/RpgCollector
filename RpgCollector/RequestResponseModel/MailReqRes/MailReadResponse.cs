﻿using RpgCollector.Models.MailModel;

namespace RpgCollector.RequestResponseModel.MailReqRes
{
    public class MailReadResponse
    {
        public ErrorCode Error { get; set; }
        public int MailId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string SendDate { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public int HasReceived { get; set; }
    }
}
