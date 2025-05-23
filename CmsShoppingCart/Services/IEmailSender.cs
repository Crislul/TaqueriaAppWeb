using System.Threading.Tasks;

namespace CmsShoppingCart.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
