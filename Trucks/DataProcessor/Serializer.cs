namespace Trucks.DataProcessor
{
    using AutoMapper;
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Trucks.Data.Models;
    using Trucks.Data.Models.Enums;
    using Trucks.DataProcessor.ExportDto;
    using Trucks.Utilities;

    public class Serializer
    {
        public static string ExportDespatchersWithTheirTrucks(TrucksContext context)
        {
            XmlHelper xmlHelper = new XmlHelper();
            Mapper mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TrucksProfile>();
            }));

            List<Despatcher> despatchers = context.Despatchers
                .Include(d => d.Trucks)
                .Where(d => d.Trucks.Count > 0)
                .OrderByDescending(d => d.Trucks.Count)
                .ThenBy(d => d.Name)
                .AsNoTracking()
                .ToList();

            foreach (Despatcher despatcher in despatchers)
            {
                despatcher.Trucks = despatcher.Trucks
                    .OrderBy(t => t.RegistrationNumber)
                    .ToList();
            }

            List<ExportDespatcherDto> exportDespatchers = mapper.Map<List<ExportDespatcherDto>>(despatchers);
            return xmlHelper.Serialize<List<ExportDespatcherDto>>(exportDespatchers, "Despatchers");
        }

        public static string ExportClientsWithMostTrucks(TrucksContext context, int capacity)
        {
            var allClients = context.Clients
                .Include(c => c.ClientsTrucks)
                .ThenInclude(ct => ct.Truck)
                .AsNoTracking()
                .ToList();

            var validClientsWithTruck = allClients
                .Select(c => new
                {
                    c.Name,
                    Trucks = c.ClientsTrucks
                        .Where(ct => ct.Truck.TankCapacity >= capacity)
                        .Select(t => new
                        {
                            TruckRegistrationNumber = t.Truck.RegistrationNumber,
                            VinNumber = t.Truck.VinNumber,
                            TankCapacity = t.Truck.TankCapacity,
                            CargoCapacity = t.Truck.CargoCapacity,
                            CategoryType = t.Truck.CategoryType.ToString(),
                            MakeType = t.Truck.MakeType.ToString()
                        })
                        .OrderBy(t => t.MakeType)
                        .ThenByDescending(t => t.CargoCapacity)
                        .ToList()
                })
                .Where(c => c.Trucks.Count > 0)
                .OrderByDescending(c => c.Trucks.Count)
                .ThenBy(c => c.Name)
                .Take(10)
                .ToList();

            return JsonConvert.SerializeObject(validClientsWithTruck, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
        }
    }
}
