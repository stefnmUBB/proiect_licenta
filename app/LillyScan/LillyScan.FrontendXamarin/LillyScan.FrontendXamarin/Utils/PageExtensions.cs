using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LillyScan.FrontendXamarin.Utils
{
    internal static class PageExtensions
    {
        public static async Task<bool> PromptYesNo(this Page page, string message)
        {
            string promptResponse = string.Empty;
            await MainThread.InvokeOnMainThreadAsync(async ()
                => promptResponse = await page.DisplayActionSheet(message, null, null, Strings.Yes, Strings.No));            
            return promptResponse == Strings.Yes;
        }        
    }
}
