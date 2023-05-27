using Microsoft.AspNetCore.Identity;

namespace BuDiTest.EmailSender
{
    public interface IMailSender
    {
        public void MessageSend(Message message);
        
    }
}
