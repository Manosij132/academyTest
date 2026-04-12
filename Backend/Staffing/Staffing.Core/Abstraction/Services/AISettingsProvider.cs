using Staffing.Core.Abstraction.Infrastructure;
using Staffing.Core.Abstraction.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Staffing.Core.Abstraction.Services
{
    public class AISettingsProvider : IAISettingsProvider
    {
        private readonly AIConnection _connection;

        public AISettingsProvider(IOptions<AIConnection> settings)
        {
            _connection = settings.Value;
            
        }

        public AIConnection GetAIConnection() => _connection;
        public string GetAIModel() => _connection.AIModel;
        public string GetAIService() => _connection.AIService;
    }
}
