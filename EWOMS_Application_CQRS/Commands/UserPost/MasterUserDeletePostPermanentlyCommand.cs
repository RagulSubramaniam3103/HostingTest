using System;

namespace EWOMS_Application_CQRS.Commands.UserPost
{
    public class MasterUserDeletePostPermanentlyCommand
    {
        public int SNo { get; set; }
        public string AdminId { get; set; }
    }
}
