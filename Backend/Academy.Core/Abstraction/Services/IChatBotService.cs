using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Arch.EntityFrameworkCore.UnitOfWork.Collections;

namespace Academy.Core.Abstraction.Services
{
    public interface IChatBotService
    {
        Task<ChatboartServiceResponse> ExecuteChatBotTrainingAssignment(string userEmail, string trainingName);
        Task<List<Academy.Shared.DTO.EmployeeTrainingsResponse>> GetEmployeeTrainings(string email);
        IEnumerable<Employee> GetEmployees(string name);
        Task<List<Dictionary<string, string>>> ExecuteDynamicQuery(string prompt);
    }
}
