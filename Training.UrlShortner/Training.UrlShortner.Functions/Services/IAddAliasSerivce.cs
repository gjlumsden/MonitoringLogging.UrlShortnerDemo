using System;
using System.Threading.Tasks;
using Training.UrlShortner.Functions.Functions;

namespace Training.UrlShortner.Functions.Services
{
    public interface IAddAliasSerivce
    {
        Task ExecuteAsync(AddAliasProperties requestContent, DateTime enqueuedTimeUtc);
    }
}