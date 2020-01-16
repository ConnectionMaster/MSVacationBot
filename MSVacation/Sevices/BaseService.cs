using Microsoft.Bot.Builder;

namespace CoreBot.MSVacation.Services
{
    public class BaseService
    {
        private readonly IStorage _storage;

        public IStorage Storage { get => _storage; }

        public BaseService(IStorage storage)
        {
            _storage = storage;
        }
    }
}