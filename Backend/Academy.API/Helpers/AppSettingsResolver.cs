using Academy.Core.Abstraction.Infrastructure;
using Academy.Domain.Entities;
using Academy.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace Academy.API.Helpers
{
    public class AppSettingsResolver
    {
        private readonly IAcademyDbContext _academyDbContext;
        private readonly IConfiguration _configuration;
        private readonly string _environment;

        public AppSettingsResolver(IAcademyDbContext academyDbContext, IConfiguration configuration)
        {
            _configuration = configuration;
            _academyDbContext = academyDbContext;
            _environment = _configuration["Environment"].ToString() ?? "dev";
        }

        public async Task ResolveAsync(object settings)
        {
            if (settings == null)
                return;

            var properties = settings.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(settings);

                if(typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && 
                    !(property.PropertyType.IsPrimitive || property.PropertyType.Name == "String"))
                {
                    var collection = property.GetValue(settings) as IEnumerable;

                    if (collection != null)
                    {
                        // Iterate through each item in the collection
                        foreach (var item in collection)
                        {
                            // Recursively resolve settings for each item in the collection
                            await ResolveAsync(item);
                        }
                    }
                }
                else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    await ResolveAsync(value);
                }
                else if (value is string strValue && strValue.StartsWith("##") && strValue.EndsWith("##"))
                {
                    // Extract the key name
                    var keyName = strValue.TrimStart('#').TrimEnd('#');
                    // Fetch the value from the database
                    var dbValue = await FetchFromDatabase(keyName);
                    // Optionally set the value back to the property
                    property.SetValue(settings, dbValue);
                }
            }
        }

        private async Task<string> FetchFromDatabase(string keyName)
        {
            Configuration configEntity = await _academyDbContext.Configurations
                                                .FirstOrDefaultAsync(x => x.Environment.ToLower() == _environment.ToLower() &&
                                                                            x.Key.ToLower() == keyName.ToLower());

            if (configEntity != null)
            {
                return configEntity.Value;
            }

            throw new Exception(string.Format(Messages.ERROR_CONFIG_KEY_NOT_FOUND, keyName));
        }
    }
}
