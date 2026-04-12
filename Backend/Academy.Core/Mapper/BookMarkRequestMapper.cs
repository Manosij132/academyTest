using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Core.Mapper
{
    public static class BookMarkRequestMapper
    {        
        public static BookMarkRequest ToBookMarkRequest(BookMarkTemplates entity)
        {
            if (entity == null) return null;

            return new BookMarkRequest
            {
                BookMarkId = entity.BookMarkId,
                BookMarkName = entity.BookMarkName,
                Trainings = ToIntList(entity.Trainings),
                Community = ToStringList(entity.Communities),
                GroupByColumns = ToIntList(entity.GroupByColumns),
                Projects = ToStringList(entity.Projects),
                Statuses = ToIntList(entity.Statuses),
                activityOptions = ToIntList(entity.ActivitieOptions),
                PrimaryActivities = ToIntList(entity.PrimaryActivities),
                TDC = ToStringList(entity.TDC),
                ReportType = entity.ReportType,
                Seniorities = ToIntList(entity.Seniorities),
                SelectColumns = ToIntList(entity.ConfigureColumns),
                EmailTo = entity.To,
                EmailCC = entity.CC,
                EmailSubject = entity.Subject,
                EmailBody = entity.Body,
                EmployeeId = ToIntList(entity.EmployeeId),
                DateTypeFilter = entity.DateTypeFilter,
                FromDate = entity.FromDate,
                ToDate = entity.ToDate,
                AreaPaths = ToIntList(entity.AreaPaths),
                Client = ToStringList(entity.Client)
            };
        }

        private static List<int> ToIntList(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? new List<int>()
                : input.Split(',')
                       .Select(s => s.Trim())
                       .Where(s => int.TryParse(s, out _))
                       .Select(int.Parse)
                       .ToList();
        }

        private static List<string> ToStringList(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? new List<string>()
                : input.Split(',')
                       .Select(s => s.Trim())
                       .Where(s => !string.IsNullOrEmpty(s))
                       .ToList();
        }

    }
}
