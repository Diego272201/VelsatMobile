using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatMobile.Model.NotificacionesMobile
{
    public class UserFCMToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FCMToken { get; set; }
        public string Platform { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
